using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices.JamdictEx;

public class DictLookup
{
   readonly JPCollection _collection;

   internal DictLookup(JPCollection collection)
   {
      _collection = collection;
      _dictionaryProvider = new LazyCE<DictionaryProvider>(() => DictionaryProvider.GetInstance());
   }

   public static void ShutDownJamdict() => DictionaryProvider.ShutDown();

   readonly LazyCE<DictionaryProvider> _dictionaryProvider;

   // Cache dictionaries for @cache decorator equivalent
   static readonly ConcurrentDictionary<(string, string), DictLookupResult> TryLookupWithReadingCache = new();
   static readonly ConcurrentDictionary<string, List<DictEntry>> LookupWordRawCache = new();
   static readonly ConcurrentDictionary<string, List<DictEntry>> LookupNameRawCache = new();
   static readonly ConcurrentDictionary<string, bool> IsWordCache = new();

   HashSet<string> AllWordForms() => CoreApp.IsTesting ? [] : _dictionaryProvider.Value.AllWordForms;
   HashSet<string> AllNameForms() => CoreApp.IsTesting ? [] : _dictionaryProvider.Value.AllNameForms;

   public DictLookupResult LookupVocabWordOrName(VocabNote vocab)
   {
      if(vocab.GetReadings().Any())
      {
         return LookupWordOrNameWithMatchingReading(
            vocab.Question.WithoutNoiseCharacters,
            vocab.GetReadings()
         );
      }

      return LookupWordOrName(vocab.Question.WithoutNoiseCharacters);
   }

   public DictLookupResult LookupWordOrNameWithMatchingReading(string word, List<string> readings)
   {
      if(readings.Count == 0)
      {
         throw new ArgumentException("readings may not be empty. If you want to match without filtering on reading, use LookupWordOrName instead");
      }

      return TryLookupWordOrNameWithMatchingReading(word, readings);
   }

   DictLookupResult TryLookupWordOrNameWithMatchingReading(string word, List<string> readings)
   {
      if(!MightBeEntry(word))
      {
         return DictLookupResult.Failed();
      }

      // Create cache key from readings
      var readingsKey = string.Join(",", readings.OrderBy(x => x));
      var cacheKey = (word, readingsKey);

      return TryLookupWithReadingCache.GetOrAdd(cacheKey,
                                                _ =>
                                                   TryLookupWordOrNameWithMatchingReadingInner(word, readings));
   }

   DictLookupResult TryLookupWordOrNameWithMatchingReadingInner(string word, List<string> readings)
   {
      List<DictEntry> KanjiFormMatches(List<DictEntry> lookup)
      {
         return lookup
               .Where(entry => readings.Any(entry.HasMatchingKanaForm) &&
                               entry.KanjiForms.Any() &&
                               entry.HasMatchingKanjiForm(word))
               .ToList();
      }

      List<DictEntry> AnyKanaOnlyMatches(List<DictEntry> lookup)
      {
         return lookup
               .Where(entry => readings.Any(reading => entry.HasMatchingKanaForm(reading)) &&
                               entry.IsKanaOnly())
               .ToList();
      }

      var lookup = LookupWordRaw(word);
      if(!lookup.Any())
      {
         lookup = LookupNameRaw(word);
      }

      var matching = KanaUtils.IsOnlyKana(word)
                        ? AnyKanaOnlyMatches(lookup)
                        : KanjiFormMatches(lookup);

      return new DictLookupResult(matching, word, readings);
   }

   public DictLookupResult LookupWordOrName(string word)
   {
      if(!MightBeEntry(word))
      {
         return DictLookupResult.Failed();
      }

      var wordHit = LookupWord(word);
      if(wordHit.FoundWords())
      {
         return wordHit;
      }

      return LookupName(word);
   }

   public DictLookupResult LookupWord(string word)
   {
      if(!MightBeWord(word))
      {
         return DictLookupResult.Failed();
      }

      var entries = LookupWordRaw(word);
      return new DictLookupResult(entries, word, []);
   }

   public DictLookupResult LookupName(string word)
   {
      if(!MightBeWord(word))
      {
         return DictLookupResult.Failed();
      }

      var entries = LookupNameRaw(word);
      return new DictLookupResult(entries, word, []);
   }

   List<DictEntry> LookupWordRaw(string word)
   {
      if(!MightBeWord(word))
      {
         return [];
      }

      return LookupWordRawCache.GetOrAdd(word, LookupWordRawInner);
   }

   List<DictEntry> LookupWordRawInner(string word)
   {
      var entries = _dictionaryProvider.Value.LookupWord(word);

      if(!KanaUtils.IsOnlyKana(word))
      {
         return entries;
      }

      return entries.Where(ent => ent.IsKanaOnly()).ToList();
   }

   List<DictEntry> LookupNameRaw(string word)
   {
      if(!MightBeName(word))
      {
         return [];
      }

      return LookupNameRawCache.GetOrAdd(word, LookupNameRawInner);
   }

   List<DictEntry> LookupNameRawInner(string word)
   {
      return _dictionaryProvider.Value.LookupName(word);
   }

   public bool MightBeWord(string word) => CoreApp.IsTesting || AllWordForms().Contains(word);

   public bool MightBeName(string word) => CoreApp.IsTesting || AllNameForms().Contains(word);

   public bool MightBeEntry(string word) => MightBeWord(word) || MightBeName(word);

   public bool IsWord(string word)
   {
      if(!MightBeWord(word))
      {
         return false;
      }

      return IsWordCache.GetOrAdd(word, IsWordInner);
   }

   bool IsWordInner(string word) => LookupWord(word).FoundWords();

   public bool IsDictionaryOrCollectionWord(string word) => _collection.Vocab.IsWord(word) || IsWord(word);
}
