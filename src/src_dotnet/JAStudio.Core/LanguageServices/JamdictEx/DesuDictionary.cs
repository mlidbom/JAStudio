using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Compze.Internals.Logging;
using JAStudio.Dictionary;

namespace JAStudio.Core.LanguageServices.JamdictEx;

class DictionaryProvider
{
   static readonly object Lock = new();
   static DictionaryProvider? _instance;

   public static DictionaryProvider GetInstance()
   {
      // ReSharper disable once InconsistentlySynchronizedField
      if(_instance != null) return _instance;
      lock(Lock)
      {
         return _instance ??= new DictionaryProvider();
      }
   }

   public static void ShutDown()
   {
      lock(Lock)
      {
         var old = _instance;
         _instance = null;
         try { old?._db.Dispose(); } catch { /* Connection may already be closed */ }
      }
   }

   readonly JMDictDatabase _db;

   DictionaryProvider()
   {
      var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JAStudio");
      Directory.CreateDirectory(dir);
      var dbPath = Path.Combine(dir, "jmdict.db");

      var stopwatch = Stopwatch.StartNew();
      this.Log().Info("Opening JMDict database...");
      _db = JMDictDatabase.OpenOrGenerate(dbPath, msg => this.Log().Info(msg));
      this.Log().Info($"JMDict database ready in {stopwatch.ElapsedMilliseconds}ms");
      LoadAllForms();
   }

   public List<DictEntry> LookupWord(string word) =>
      _db.LookupWord(word).Select(DictEntry.FromWord).ToList();

   public List<DictEntry> LookupName(string word) =>
      _db.LookupName(word).Select(DictEntry.FromName).ToList();

   public HashSet<string> AllWordForms { get; private set; } = null!;
   public HashSet<string> AllNameForms { get; private set; } = null!;

   void LoadAllForms()
   {
      var stopwatch = Stopwatch.StartNew();
      AllWordForms = _db.LoadAllWordForms();
      AllNameForms = _db.LoadAllNameForms();
      this.Log().Info($"Loaded {AllWordForms.Count} word forms and {AllNameForms.Count} name forms in {stopwatch.ElapsedMilliseconds}ms");
   }
}
