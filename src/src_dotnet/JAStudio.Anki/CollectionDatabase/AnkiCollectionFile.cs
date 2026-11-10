using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace JAStudio.Anki.CollectionDatabase;

/// <summary>
/// An Anki collection's database file, <c>collection.anki2</c>, read directly, as the headless JAStudio reads the collection of an Anki profile,<br/>
/// while Anki may be running. Anki's latest changes may still be in the database's write-ahead log then, which reading the file doesn't see.
/// </summary>
/// <remarks>
/// Each read opens the file as immutable (<c>immutable=1</c>), as if it could never change, which is the only way past the exclusive lock Anki<br/>
/// holds on it. Yet Anki keeps changing it, and a connection to an immutable file never notices: it keeps using the pages it has cached. So no<br/>
/// connection outlives its read: none is pooled, and each closes when its read is done. A connection reused once Anki had changed the file read<br/>
/// wrong data, or failed with "database disk image is malformed" from then on, until Anki restarted (2026-11-10: each connection was left open,<br/>
/// and Microsoft.Data.Sqlite's pool took it back once it was garbage collected).
/// </remarks>
public sealed class AnkiCollectionFile : AnkiCollectionDatabase
{
   readonly FileInfo _file;

   public AnkiCollectionFile(FileInfo file) => _file = file;

   protected override string TheJsonTextOf(string select)
   {
      using var connection = OpenedForOneRead();
      using var command = connection.CreateCommand();
      command.CommandText = select;
      return command.ExecuteScalar() as string ?? throw new InvalidOperationException($"The SELECT returned no JSON text: {select}");
   }

   /// <summary>A connection of its own, for one read, with the collation and the SQL function of Anki's that the SELECTs use.</summary>
   SqliteConnection OpenedForOneRead()
   {
      var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = $"file:{_file.FullName}?immutable=1", Pooling = false }.ToString());
      connection.Open();
      AddAnkisUnicaseCollationTo(connection);
      AddAnkisFieldAtIndexFunctionTo(connection);
      return connection;
   }

   /// <summary>
   /// Adds to <paramref name="connection"/> Anki's SQL collation <c>unicase</c>, by which Anki compares the names of note types, fields and<br/>
   /// templates ignoring case, and which the indexes of those names use.
   /// </summary>
   /// <remarks>Anki's folds Unicode case; ignoring case ordinally comes close.</remarks>
   static void AddAnkisUnicaseCollationTo(SqliteConnection connection) => connection.CreateCollation("unicase", (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase));

   /// <summary>
   /// Adds to <paramref name="connection"/> Anki's SQL function <c>field_at_index</c>, as Anki's own: the value of the field at an index, from 0,<br/>
   /// of a note's fields (<see cref="NoteFieldsAsAnkiStoresThem.ValueAt"/>).
   /// </summary>
   static void AddAnkisFieldAtIndexFunctionTo(SqliteConnection connection) =>
      connection.CreateFunction("field_at_index", (string fldsColumn, int ordinal) => new NoteFieldsAsAnkiStoresThem(fldsColumn).ValueAt(ordinal), isDeterministic: true);
}
