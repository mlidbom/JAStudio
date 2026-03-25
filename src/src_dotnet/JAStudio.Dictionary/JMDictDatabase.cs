using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LinqToDB;

namespace JAStudio.Dictionary;

public sealed class JMDictDatabase : IDisposable
{
   readonly string _dbPath;

   JMDictDatabase(string dbPath) => _dbPath = dbPath;

   public static JMDictDatabase Open(string dbPath) => new(dbPath);

   public static JMDictDatabase OpenOrGenerate(string dbPath, Action<string>? log = null)
   {
      if(File.Exists(dbPath))
      {
         var version = ReadSchemaVersion(dbPath);
         if(version < JMDictDatabaseGenerator.SchemaVersion)
         {
            log?.Invoke($"Database schema version {version} is outdated (current: {JMDictDatabaseGenerator.SchemaVersion}), regenerating...");
            File.Delete(dbPath);
         }
      }

      if(!File.Exists(dbPath))
         JMDictDatabaseGenerator.Generate(dbPath, log);

      return Open(dbPath);
   }

   static long ReadSchemaVersion(string dbPath)
   {
      using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
      connection.Open();
      using var cmd = connection.CreateCommand();
      cmd.CommandText = "PRAGMA user_version";
      var result = (long)cmd.ExecuteScalar()!;
      connection.Close();
      Microsoft.Data.Sqlite.SqliteConnection.ClearPool(connection);
      return result;
   }

   JMDictDb OpenDb() => JMDictDb.OpenReadOnly(_dbPath);

   public List<WordEntry> LookupWord(string text)
   {
      using var db = OpenDb();
      var entryIds = LookupWordEntryIds(db, text);
      return entryIds.Count == 0 ? [] : LoadWordEntries(db, entryIds);
   }

   public List<NameEntry> LookupName(string text)
   {
      using var db = OpenDb();
      var entryIds = LookupNameEntryIds(db, text);
      return entryIds.Count == 0 ? [] : LoadNameEntries(db, entryIds);
   }

   public HashSet<string> LoadAllWordForms()
   {
      using var db = OpenDb();
      var forms = new HashSet<string>(
         db.WordKanjis.Select(k => k.Text)
           .Union(db.WordReadings.Select(r => r.Text))
           .ToList());
      return forms;
   }

   public HashSet<string> LoadAllNameForms()
   {
      using var db = OpenDb();
      var forms = new HashSet<string>(
         db.NameKanjis.Select(k => k.Text)
           .Union(db.NameReadings.Select(r => r.Text))
           .ToList());
      return forms;
   }

   static List<long> LookupWordEntryIds(JMDictDb db, string text) =>
      db.WordKanjis.Where(k => k.Text == text).Select(k => k.EntryId)
        .Union(db.WordReadings.Where(r => r.Text == text).Select(r => r.EntryId))
        .ToList();

   static List<long> LookupNameEntryIds(JMDictDb db, string text) =>
      db.NameKanjis.Where(k => k.Text == text).Select(k => k.EntryId)
        .Union(db.NameReadings.Where(r => r.Text == text).Select(r => r.EntryId))
        .ToList();

   static List<WordEntry> LoadWordEntries(JMDictDb db, List<long> entryIds)
   {
      var kanjis = db.WordKanjis
                     .Where(k => entryIds.Contains(k.EntryId))
                     .OrderBy(k => k.EntryId).ThenBy(k => k.Id)
                     .ToList()
                     .ToLookup(k => k.EntryId);

      var readings = db.WordReadings
                       .Where(r => entryIds.Contains(r.EntryId))
                       .OrderBy(r => r.EntryId).ThenBy(r => r.Id)
                       .ToList()
                       .ToLookup(r => r.EntryId);

      var senses = db.WordSenses
                     .Where(s => entryIds.Contains(s.EntryId))
                     .OrderBy(s => s.EntryId).ThenBy(s => s.Id)
                     .ToList()
                     .ToLookup(s => s.EntryId);

      return entryIds.Select(id => new WordEntry(
         id,
         kanjis[id].Select(r => new WordKanji(r)).ToList(),
         readings[id].Select(r => new WordReading(r)).ToList(),
         senses[id].Select(r => new WordSense(r)).ToList())).ToList();
   }

   static List<NameEntry> LoadNameEntries(JMDictDb db, List<long> entryIds)
   {
      var kanjis = db.NameKanjis
                     .Where(k => entryIds.Contains(k.EntryId))
                     .OrderBy(k => k.EntryId).ThenBy(k => k.Id)
                     .ToList()
                     .ToLookup(k => k.EntryId);

      var readings = db.NameReadings
                       .Where(r => entryIds.Contains(r.EntryId))
                       .OrderBy(r => r.EntryId).ThenBy(r => r.Id)
                       .ToList()
                       .ToLookup(r => r.EntryId);

      var translations = db.NameTranslations
                           .Where(t => entryIds.Contains(t.EntryId))
                           .OrderBy(t => t.EntryId).ThenBy(t => t.Id)
                           .ToList()
                           .ToLookup(t => t.EntryId);

      return entryIds.Select(id => new NameEntry(
         id,
         kanjis[id].Select(r => new NameKanji(r)).ToList(),
         readings[id].Select(r => new NameReading(r)).ToList(),
         translations[id].Select(r => new NameTranslation(r)).ToList())).ToList();
   }

   public void Dispose() {}
}
