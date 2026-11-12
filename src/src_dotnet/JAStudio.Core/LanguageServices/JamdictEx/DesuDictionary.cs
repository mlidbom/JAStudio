using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Compze.Internals.Logging;
using Compze.Threading;
using JAStudio.Dictionary;

namespace JAStudio.Core.LanguageServices.JamdictEx;

class DictionaryProvider
{
   static readonly IMonitor Monitor = IMonitor.New();
   static DictionaryProvider? _instance;

   public static DictionaryProvider GetInstance() => Monitor.DoubleCheckedLocking(ref _instance, () => new DictionaryProvider());

   public static void ShutDown() => Monitor.Locked(() =>
   {
      var old = _instance;
      _instance = null;
      //refactor: this catch swallows every exception Dispose throws, with no evidence of one that isn't a bug. Delete it, and catch only an
      //   exception seen in real use, if one turns up.
      try { old?._db.Dispose(); } catch { /* Connection may already be closed */ }
   });

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

   /// <summary><see cref="AllWordForms"/> in order, for telling whether any word form starts with a text.</summary>
   public FormsInOrder WordFormsInOrder { get; private set; } = null!;

   void LoadAllForms()
   {
      var stopwatch = Stopwatch.StartNew();
      AllWordForms = _db.LoadAllWordForms();
      AllNameForms = _db.LoadAllNameForms();
      WordFormsInOrder = new FormsInOrder(AllWordForms);
      this.Log().Info($"Loaded {AllWordForms.Count} word forms and {AllNameForms.Count} name forms in {stopwatch.ElapsedMilliseconds}ms");
   }
}
