using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.SysUtils.Data.Sqlite;
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
      var collectionEntry = CollectionEntryIn(archive);
      using var collection = collectionEntry.Open();
      return InATemporaryCopyOf(collection, collectionEntry.Name);
   }

   /// <summary>The zip entry of the collection database to read in <paramref name="package"/>, an Anki package's zip: the newest format it holds.</summary>
   public static ZipArchiveEntry CollectionEntryIn(ZipArchive package) => CollectionFileNamesNewestFirst.Select(package.GetEntry).OfType<ZipArchiveEntry>().First();

   /// <summary>
   /// The database in <paramref name="collectionFile"/>, a collection database taken out of a package, named as in the package: one of<br/>
   /// <see cref="CollectionFileNamesNewestFirst"/>.
   /// </summary>
   public static AnkiPackageDatabase InCollectionFile(FileInfo collectionFile)
   {
      if(collectionFile.Name != CompressedCollectionFileName) return new AnkiPackageDatabase(collectionFile, databaseFileIsATemporaryCopy: false);

      using var collection = collectionFile.OpenRead();
      return InATemporaryCopyOf(collection, collectionFile.Name);
   }

   /// <summary>Every note type of the collection that has notes.</summary>
   public IReadOnlyList<AnkiNoteType> NoteTypes { get; }

   /// <summary>The notes of <paramref name="noteType"/>, one of <see cref="NoteTypes"/>, read as they're enumerated.</summary>
   public IEnumerable<RawNote> NotesOf(AnkiNoteType noteType)
   {
      using var connection = OpenedReadOnly();
      foreach(var note in connection.RowsReadAsEnumerated("select id, flds, tags from notes where mid = $noteTypeId",
                                                          reader => new RawNote(reader.GetInt64(0), FieldsByName(reader.GetString(1)), reader.GetString(2).Split(' ', StringSplitOptions.RemoveEmptyEntries)),
                                                          ("$noteTypeId", _idsOfNoteTypes[noteType])))
         yield return note;

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

   /// <summary>The database whose file <paramref name="collection"/> reads, copied to a temporary file, and decompressed when it's in the compressed format.</summary>
   static AnkiPackageDatabase InATemporaryCopyOf(Stream collection, string collectionFileName)
   {
      var copy = new FileInfo(Path.Combine(Path.GetTempPath(), $"jastudio-anki-package-{Guid.NewGuid():N}.sqlite"));
      using(var copyStream = copy.Create())
      {
         if(collectionFileName == CompressedCollectionFileName)
         {
            using var decompressing = new DecompressionStream(collection);
            decompressing.CopyTo(copyStream);
         } else
         {
            collection.CopyTo(copyStream);
         }
      }

      return new AnkiPackageDatabase(copy, databaseFileIsATemporaryCopy: true);
   }

   IReadOnlyDictionary<AnkiNoteType, long> ReadNoteTypes()
   {
      using var connection = OpenedReadOnly();
      var keepsNoteTypesInTables = KeepsNoteTypesInTablesOfTheirOwn(connection);
      return connection.Column<long>("select distinct mid from notes")
                       .ToDictionary(id => new AnkiNoteType(connection.Column<string>(SelectTheName(id)).Single(), connection.Column<string>(SelectTheFieldNames(id))));

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
      AddAnkisUnicaseCollationTo(connection);
      return connection;
   }

   /// <summary>Whether the collection keeps its note types in tables of their own, as <c>collection.anki21b</c> does, rather than as JSON in the <c>col</c> table.</summary>
   static bool KeepsNoteTypesInTablesOfTheirOwn(SqliteConnection connection) =>
      connection.ScalarAsLong("select count(*) from sqlite_master where type = 'table' and name = 'notetypes'") > 0;

   /// <summary>Anki's tables compare names with its own collation, <c>unicase</c>, which SQLite must know of to read them.</summary>
   static void AddAnkisUnicaseCollationTo(SqliteConnection connection)
   {
      //refactor: JAStudio.Anki's AnkiCollectionFile.AddAnkisUnicaseCollationTo adds the same collation. Both read Anki's databases with
      //   Microsoft.Data.Sqlite; one home in Core for opening an Anki database, used by both, would end the duplication.
      connection.CreateCollation("unicase", (left, right) => string.Compare(left, right, StringComparison.OrdinalIgnoreCase));
   }
}
