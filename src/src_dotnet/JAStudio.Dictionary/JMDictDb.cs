using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SQLite;

namespace JAStudio.Dictionary;

sealed class JMDictDb : DataConnection
{
   JMDictDb(DataOptions options) : base(options) {}

   public ITable<WordKanjiRow> WordKanjis => this.GetTable<WordKanjiRow>();
   public ITable<WordReadingRow> WordReadings => this.GetTable<WordReadingRow>();
   public ITable<WordSenseRow> WordSenses => this.GetTable<WordSenseRow>();

   public ITable<NameKanjiRow> NameKanjis => this.GetTable<NameKanjiRow>();
   public ITable<NameReadingRow> NameReadings => this.GetTable<NameReadingRow>();
   public ITable<NameTranslationRow> NameTranslations => this.GetTable<NameTranslationRow>();

   /// <summary>The options for opening the database at <paramref name="dbPath"/> read-only, made once and used for every connection to it.</summary>
   public static DataOptions ReadOnlyOptionsFor(string dbPath) => OptionsFor($"Data Source={dbPath};Mode=ReadOnly");

   public static JMDictDb Open(DataOptions options) => new(options);

   public static JMDictDb Open(string dbPath) => new(OptionsFor($"Data Source={dbPath}"));

   /// <remarks>
   /// Names Microsoft.Data.Sqlite, the SQLite provider JAStudio uses. Left to detect the provider, LinqToDB first tried to load System.Data.SQLite,<br/>
   /// which isn't there, and caught the <see cref="System.IO.FileNotFoundException"/>: 2,032 of them in the first full rebuild after a start (2026-11-12).
   /// </remarks>
   static DataOptions OptionsFor(string connectionString) => new DataOptions().UseSQLite(connectionString, SQLiteProvider.Microsoft);
}
