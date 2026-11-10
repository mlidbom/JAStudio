using System;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SQLite;
using Microsoft.Data.Sqlite;
// ReSharper disable UnusedMember.Global

namespace JAStudio.Anki;

sealed class AnkiDb : DataConnection
{
   AnkiDb(DataOptions options) : base(options) {}

   public ITable<NoteRow> Notes => this.GetTable<NoteRow>();
   public ITable<CardRow> Cards => this.GetTable<CardRow>();
   public ITable<NoteTypeRow> NoteTypes => this.GetTable<NoteTypeRow>();
   public ITable<FieldRow> Fields => this.GetTable<FieldRow>();
   public ITable<TemplateRow> Templates => this.GetTable<TemplateRow>();
   public ITable<DeckRow> Decks => this.GetTable<DeckRow>();
   public ITable<DeckConfigRow> DeckConfigs => this.GetTable<DeckConfigRow>();
   public ITable<ConfigRow> Config => this.GetTable<ConfigRow>();
   public ITable<ReviewLogRow> ReviewLog => this.GetTable<ReviewLogRow>();
   public ITable<GraveRow> Graves => this.GetTable<GraveRow>();
   public ITable<TagRow> Tags => this.GetTable<TagRow>();
   public ITable<CollectionRow> Collection => this.GetTable<CollectionRow>();

   /// <summary>
   /// Opens the Anki database <paramref name="dbFilePath"/> for reading, through a connection of its own that closes when the returned<br/>
   /// <see cref="AnkiDb"/> is disposed.
   /// </summary>
   /// <remarks>
   /// It opens the file as immutable (<c>immutable=1</c>), as if it could never change, since nothing else opens it while Anki has it open: Anki<br/>
   /// holds an exclusive lock on it. Yet Anki keeps changing it, and a connection to an immutable file never notices: it keeps using the pages it<br/>
   /// has cached. So no connection may outlive its read: none is pooled, and each closes with its <see cref="AnkiDb"/>. A connection reused once<br/>
   /// Anki had changed the file read wrong data, or failed with "database disk image is malformed" from then on, until Anki restarted<br/>
   /// (2026-11-10: each connection was left open, and Microsoft.Data.Sqlite's pool took it back once it was garbage collected).
   /// </remarks>
   public static AnkiDb OpenReadOnly(string dbFilePath)
   {
      var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = $"file:{dbFilePath}?immutable=1", Pooling = false }.ToString());
      connection.Open();

      // Register Anki's custom "unicase" collation (Unicode-aware case-insensitive comparison). Anki registers this via apsw; we approximate it with .NET's OrdinalIgnoreCase.
      connection.CreateCollation("unicase", (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase));

      return new AnkiDb(new DataOptions().UseConnection(SQLiteTools.GetDataProvider(SQLiteProvider.Microsoft), connection, disposeConnection: true));
   }
}
