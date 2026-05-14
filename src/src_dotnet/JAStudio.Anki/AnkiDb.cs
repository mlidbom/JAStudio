using System;
using LinqToDB;
using LinqToDB.Data;
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
   /// Open the Anki database for reading as an immutable snapshot by using a SQLite URI with immutable=1, because we have been unable to open the DB while Anki is running in any other way.
   /// </summary>
   public static AnkiDb OpenReadOnly(string dbFilePath)
   {
      var uri = $"file:{dbFilePath}?immutable=1";
      var connection = new SqliteConnection($"Data Source={uri}");
      connection.Open();

      // Register Anki's custom "unicase" collation (Unicode-aware case-insensitive comparison). Anki registers this via apsw; we approximate it with .NET's OrdinalIgnoreCase.
      connection.CreateCollation("unicase", (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase));

      return new AnkiDb(new DataOptions().UseSQLite().UseConnection(connection));
   }
}
