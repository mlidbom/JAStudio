using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Core.Note.Sentences;

public class WordExclusionSet
{
   readonly Action _saveCallback;
   readonly HashSet<WordExclusion> _exclusions;
   HashSet<string> _excludedWords;

   public WordExclusionSet(Action saveCallback, List<WordExclusion> exclusions)
   {
      _saveCallback = saveCallback;
      _exclusions = new HashSet<WordExclusion>(exclusions);
      _excludedWords = ExtractWords();
   }

   public static WordExclusionSet Empty()
   {
      return new WordExclusionSet(() => {}, []);
   }

   public static WordExclusionSet Empty(Action saveCallback) => new(saveCallback, []);

   public bool IsEmpty() => !_exclusions.Any();

   public IEnumerable<WordExclusion> Get() => _exclusions;

   HashSet<string> ExtractWords()
   {
      return _exclusions.Select(e => e.Word).ToHashSet();
   }

   public HashSet<string> Words() => _excludedWords;

   public void Reset()
   {
      _exclusions.Clear();
      _excludedWords.Clear();
      _saveCallback();
   }

   public void AddGlobal(string vocab)
   {
      Add(WordExclusion.Global(vocab));
   }

   public void Add(WordExclusion exclusion)
   {
      _exclusions.Add(exclusion);
      _excludedWords = ExtractWords();
      _saveCallback();
   }

   public void Remove(WordExclusion exclusion)
   {
      _exclusions.Remove(exclusion);
      _excludedWords = ExtractWords();
      _saveCallback();
   }

   public void RemoveString(string toRemove)
   {
      var toRemoveList = _exclusions.Where(ex => ex.Word == toRemove).ToList();
      foreach(var exclusion in toRemoveList)
      {
         _exclusions.Remove(exclusion);
      }

      _excludedWords = ExtractWords();
      _saveCallback();
   }

   /// <remarks>A loop, not LINQ's <c>Any</c>, whose lambda would allocate a closure at every call, exclusions or not: parsing calls it millions of times a pass.</remarks>
   public bool ExcludesAtIndex(string word, int index)
   {
      foreach(var exclusion in _exclusions)
      {
         if(exclusion.ExcludesFormAtIndex(word, index)) return true;
      }

      return false;
   }

   public override string ToString()
   {
      return string.Join(", ", _exclusions.Select(e => e.ToString()));
   }
}
