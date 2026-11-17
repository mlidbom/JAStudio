using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// A set of <see cref="WordExclusion"/>s, each a form excluded at one position in a sentence or everywhere in it: a sentence's incorrect matches, or its<br/>
/// hidden matches (<see cref="SentenceNoteConfiguration"/>). Parsing asks it whether it excludes a form at a position.
/// </summary>
public class WordExclusionSet
{
   /// <summary>
   /// Changes the set it's given, this one, to the exclusions it's given: a sentence's set publishes them as a tevent of the sentence, whose applier<br/>
   /// applies them (<see cref="Apply"/>), and a set of no sentence's applies them at once.
   /// </summary>
   readonly Action<WordExclusionSet, IReadOnlySet<WordExclusion>> _changeTo;

   /// <remarks>A <see cref="HashSet{T}"/>, not an interface, so that <see cref="ExcludesAtIndex"/> loops over it without allocating an enumerator.</remarks>
   HashSet<WordExclusion> _exclusions;
   HashSet<string> _excludedWords;

   /// <param name="exclusions">The set's exclusions.</param>
   /// <param name="changeTo">Changes the set it's given, this one, to the exclusions it's given: a sentence's set publishes them as a tevent of the sentence.</param>
   public WordExclusionSet(IEnumerable<WordExclusion> exclusions, Action<WordExclusionSet, IReadOnlySet<WordExclusion>> changeTo) : this(exclusions) => _changeTo = changeTo;

   /// <summary>A set of no sentence's, which applies each change at once.</summary>
   public WordExclusionSet(IEnumerable<WordExclusion> exclusions)
   {
      _changeTo = static (set, changed) => set.Apply(changed);
      _exclusions = [.. exclusions];
      _excludedWords = ExtractWords();
   }

   public static WordExclusionSet Empty() => new([]);

   public bool IsEmpty() => _exclusions.Count == 0;

   public IReadOnlySet<WordExclusion> Get() => _exclusions;

   HashSet<string> ExtractWords() => [.. _exclusions.Select(it => it.Word)];

   public HashSet<string> Words() => _excludedWords;

   public void Reset() => ChangeTo([]);

   public void AddGlobal(string vocab) => Add(WordExclusion.Global(vocab));

   public void Add(WordExclusion exclusion) => ChangeTo([.. _exclusions, exclusion]);

   public void Remove(WordExclusion exclusion) => ChangeTo(_exclusions.Where(it => !it.Equals(exclusion)));

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

      ChangeTo(_exclusions.Except(exclusionsExcludingIt));
   }

   /// <summary>The exclusions that exclude <paramref name="match"/>: its form at its position, and its form everywhere in the sentence.</summary>
   List<WordExclusion> ExclusionsExcluding(Match match) => [.. _exclusions.Where(it => it.ExcludesFormAtIndex(match.ExclusionForm, match.StartIndex))];

   public void RemoveString(string toRemove) => ChangeTo(_exclusions.Where(it => it.Word != toRemove));

   /// <summary>Makes <paramref name="exclusions"/> the set's exclusions: what their tevent's applier does.</summary>
   internal void Apply(IReadOnlySet<WordExclusion> exclusions)
   {
      _exclusions = [.. exclusions];
      _excludedWords = ExtractWords();
   }

   void ChangeTo(IEnumerable<WordExclusion> exclusions) => _changeTo(this, exclusions.ToHashSet());

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
