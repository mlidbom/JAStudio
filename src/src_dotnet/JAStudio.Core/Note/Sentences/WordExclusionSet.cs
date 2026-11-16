using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;

namespace JAStudio.Core.Note.Sentences;

public class WordExclusionSet
{
   /// <summary>Makes a change to the set: a sentence's set makes it as a change of the sentence, under its guard, reparsing it after.</summary>
   readonly Action<Action> _makeTheChange;

   readonly HashSet<WordExclusion> _exclusions;
   HashSet<string> _excludedWords;

   /// <param name="makeTheChange">Makes a change to the set, given as an action: a sentence's set makes it as a change of the sentence.</param>
   /// <param name="exclusions">The set's exclusions.</param>
   public WordExclusionSet(Action<Action> makeTheChange, List<WordExclusion> exclusions)
   {
      _makeTheChange = makeTheChange;
      _exclusions = [.. exclusions];
      _excludedWords = ExtractWords();
   }

   /// <summary>A set of no sentence's, which simply makes each change.</summary>
   public WordExclusionSet(List<WordExclusion> exclusions) : this(change => change(), exclusions) {}

   public static WordExclusionSet Empty() => new([]);

   public static WordExclusionSet Empty(Action<Action> makeTheChange) => new(makeTheChange, []);

   public bool IsEmpty() => _exclusions.Count == 0;

   public IEnumerable<WordExclusion> Get() => _exclusions;

   HashSet<string> ExtractWords() => [.. _exclusions.Select(it => it.Word)];

   public HashSet<string> Words() => _excludedWords;

   public void Reset() => _makeTheChange(() =>
   {
      _exclusions.Clear();
      _excludedWords.Clear();
   });

   public void AddGlobal(string vocab)
   {
      Add(WordExclusion.Global(vocab));
   }

   public void Add(WordExclusion exclusion) => _makeTheChange(() =>
   {
      _exclusions.Add(exclusion);
      _excludedWords = ExtractWords();
   });

   public void Remove(WordExclusion exclusion) => _makeTheChange(() =>
   {
      _exclusions.Remove(exclusion);
      _excludedWords = ExtractWords();
   });

   /// <summary>Adds <paramref name="match"/>, at its position in the sentence only (<see cref="Match.ToExclusion"/>).</summary>
   public void Add(Match match) => Add(match.ToExclusion());

   /// <summary>Whether the set contains <paramref name="match"/>: its form at its position, or its form everywhere in the sentence.</summary>
   public bool Contains(Match match) => ExcludesAtIndex(match.ExclusionForm, match.StartIndex);

   /// <summary>
   /// Whether the set contains <paramref name="match"/> by its form everywhere in the sentence (<see cref="WordExclusion.IsGlobal"/>), so that removing<br/>
   /// it removes the form at every position.
   /// </summary>
   public bool ContainsItsFormEverywhereInTheSentence(Match match) => ExclusionsExcluding(match).Any(it => it.IsGlobal);

   /// <summary>
   /// Removes <paramref name="match"/>: every exclusion that excludes it, the one of its form everywhere in the sentence too, if there is one<br/>
   /// (<see cref="ContainsItsFormEverywhereInTheSentence"/>).
   /// </summary>
   public void Remove(Match match)
   {
      var exclusionsExcludingIt = ExclusionsExcluding(match);
      Contract.Argument.Assert(exclusionsExcludingIt.Count > 0, () => $"The set contains no {match.ExclusionForm} at {match.StartIndex} to remove: {this}");

      _makeTheChange(() =>
      {
         _exclusions.ExceptWith(exclusionsExcludingIt);
         _excludedWords = ExtractWords();
      });
   }

   /// <summary>The exclusions that exclude <paramref name="match"/>: its form at its position, and its form everywhere in the sentence.</summary>
   List<WordExclusion> ExclusionsExcluding(Match match) => [.. _exclusions.Where(it => it.ExcludesFormAtIndex(match.ExclusionForm, match.StartIndex))];

   public void RemoveString(string toRemove) => _makeTheChange(() =>
   {
      _exclusions.RemoveWhere(it => it.Word == toRemove);
      _excludedWords = ExtractWords();
   });

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
