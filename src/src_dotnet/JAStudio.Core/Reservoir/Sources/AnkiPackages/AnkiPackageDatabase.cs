using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using Microsoft.Data.Sqlite;
using ZstdSharp;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// The collection database of an Anki package, an .apkg file: a zip holding the database and the media files. It's read directly, with no Anki,<br/>
/// as the raw notes (<see cref="RawNote"/>) of its note types (<see cref="AnkiNoteType"/>), the same raw notes an import takes from Anki.
/// </summary>
/// <remarks>
/// Anki has written three formats over the years, and a package holds the newest it was exported in. <c>collection.anki21b</c> is compressed with<br/>
/// zstd and keeps the note types in tables of their own; a package with it also holds a <c>collection.anki2</c> that only says to update Anki.<br/>
/// <c>collection.anki21</c> and <c>collection.anki2</c> keep the note types as JSON in the <c>col</c> table. A compressed database is decompressed to a<br/>
/// temporary file, which <see cref="Dispose"/> deletes.
/// </remarks>
public sealed class AnkiPackageDatabase : IDisposable
{
   /// <summary>The names a package's collection database can have, newest format first: the first a package holds is the one to read.</summary>
   public static readonly IReadOnlyList<string> CollectionFileNamesNewestFirst = ["collection.anki21b", "collection.anki21", "collection.anki2"];

   const string CompressedCollectionFileName = "collection.anki21b";
   const char FieldSeparator = '\x1f';

   readonly FileInfo _databaseFile;
   readonly bool _databaseFileIsATemporaryCopy;
   readonly IReadOnlyDictionary<AnkiNoteType, long> _idsOfNoteTypes;

   AnkiPackageDatabase(FileInfo databaseFile, bool databaseFileIsATemporaryCopy)
   {
      _databaseFile = databaseFile;
      _databaseFileIsATemporaryCopy = databaseFileIsATemporaryCopy;
      _idsOfNoteTypes = ReadNoteTypes();
      NoteTypes = [.. _idsOfNoteTypes.Keys];
   }

   /// <summary>The database in the package <paramref name="package"/>, an .apkg file.</summary>
   public static AnkiPackageDatabase InPackage(FileInfo package)
   {
      using var archive = ZipFile.OpenRead(package.FullName);
      var collectionEntry = CollectionFileNamesNewestFirst.Select(archive.GetEntry).OfType<ZipArchiveEntry>().First();
      var copy = TemporaryFile();
      using(var source = collectionEntry.Open())
         CopyDecompressingIfNeeded(source, collectionEntry.Name, copy);
      return new AnkiPackageDatabase(copy, databaseFileIsATemporaryCopy: true);
   }

   /// <summary>
   /// The database in <paramref name="collectionFile"/>, a collection database taken out of a package, named as in the package: one of<br/>
   /// <see cref="CollectionFileNamesNewestFirst"/>.
   /// </summary>
   public static AnkiPackageDatabase InCollectionFile(FileInfo collectionFile)
   {
      if(collectionFile.Name != CompressedCollectionFileName) return new AnkiPackageDatabase(collectionFile, databaseFileIsATemporaryCopy: false);

      var copy = TemporaryFile();
      using(var source = collectionFile.OpenRead())
         CopyDecompressingIfNeeded(source, collectionFile.Name, copy);
      return new AnkiPackageDatabase(copy, databaseFileIsATemporaryCopy: true);
   }

   /// <summary>Every note type of the collection that has notes.</summary>
   public IReadOnlyList<AnkiNoteType> NoteTypes { get; }

   /// <summary>The notes of <paramref name="noteType"/>, one of <see cref="NoteTypes"/>, read as they're enumerated.</summary>
   public IEnumerable<RawNote> NotesOf(AnkiNoteType noteType)
   {
      using var connection = OpenedReadOnly();
      using var command = connection.CreateCommand();
      command.CommandText = "select id, flds, tags from notes where mid = $noteTypeId";
      command.Parameters.AddWithValue("$noteTypeId", _idsOfNoteTypes[noteType]);
      using var reader = command.ExecuteReader();
      while(reader.Read())
         yield return new RawNote(reader.GetInt64(0), FieldsByName(reader.GetString(1)), reader.GetString(2).Split(' ', StringSplitOptions.RemoveEmptyEntries));

      // A note with fewer values than its note type has fields, as a deck made outside Anki can have, gets empty ones, as Anki shows it.
      IReadOnlyDictionary<string, string> FieldsByName(string fields)
      {
         var values = fields.Split(FieldSeparator);
         return noteType.FieldNames.Select((name, index) => (name, value: index < values.Length ? values[index] : "")).ToDictionary(it => it.name, it => it.value);
      }
   }

   public void Dispose()
   {
      if(_databaseFileIsATemporaryCopy) _databaseFile.Delete();
   }

   IReadOnlyDictionary<AnkiNoteType, long> ReadNoteTypes()
   {
      using var connection = OpenedReadOnly();
      var keepsNoteTypesInTables = Column<long>(connection, "select count(*) from sqlite_master where type = 'table' and name = 'notetypes'").Single() > 0;
      var idsWithNotes = Column<long>(connection, "select distinct mid from notes");
      return idsWithNotes.ToDictionary(id => new AnkiNoteType(Column<string>(connection, SelectTheName(id)).Single(), Column<string>(connection, SelectTheFieldNames(id))));

      string SelectTheName(long id) => keepsNoteTypesInTables
                                          ? $"select name from notetypes where id = {id}"
                                          : $"select json_extract(value, '$.name') from json_each((select models from col)) where key = '{id}'";

      string SelectTheFieldNames(long id) => keepsNoteTypesInTables
                                                ? $"select name from fields where ntid = {id} order by ord"
                                                : $"""
                                                   select json_extract(field.value, '$.name')
                                                   from json_each((select models from col)) as noteType, json_each(json_extract(noteType.value, '$.flds')) as field
                                                   where noteType.key = '{id}'
                                                   order by json_extract(field.value, '$.ord')
                                                   """;
   }

   SqliteConnection OpenedReadOnly()
   {
      var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _databaseFile.FullName, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
      connection.Open();
      // Anki's tables compare names with its own collation, which SQLite must know of to read them.
      connection.CreateCollation("unicase", (left, right) => string.Compare(left, right, StringComparison.OrdinalIgnoreCase));
      return connection;
   }

   static List<T> Column<T>(SqliteConnection connection, string select)
   {
      using var command = connection.CreateCommand();
      command.CommandText = select;
      using var reader = command.ExecuteReader();
      var values = new List<T>();
      while(reader.Read()) values.Add(reader.GetFieldValue<T>(0));
      return values;
   }

   static FileInfo TemporaryFile() => new(Path.Combine(Path.GetTempPath(), $"jastudio-anki-package-{Guid.NewGuid():N}.sqlite"));

   static void CopyDecompressingIfNeeded(Stream source, string collectionFileName, FileInfo target)
   {
      using var targetStream = target.Create();
      if(collectionFileName == CompressedCollectionFileName)
      {
         using var decompressing = new DecompressionStream(source);
         decompressing.CopyTo(targetStream);
      } else
      {
         source.CopyTo(targetStream);
      }
   }
}
