using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing.Candidates;

/// <summary>
/// A condition worth naming that vocab's match-conditions hold, found while looking for the conditions vocab share (<see cref="ConditionsHeldByVocab"/>),<br/>
/// with the vocab found holding it so far: a <see cref="SharedConditionCandidate"/> once enough vocab hold it.
/// </summary>
sealed class ConditionHeldByVocab
{
   /// <summary>The condition, as the first vocab found holding it holds it.</summary>
   readonly ConditionWithItsLevel _asFirstFound;

   /// <summary>Whether it was first found as a combination of some of a group's conditions, rather than as a condition in a tree.</summary>
   readonly bool _isACombination;

   /// <summary>The vocab found holding it, each once, however often its match-condition holds it.</summary>
   readonly HashSet<VocabNote> _vocab = [];

   internal ConditionHeldByVocab(ConditionWithItsLevel asFirstFound, bool isACombination)
   {
      _asFirstFound = asFirstFound;
      _isACombination = isACombination;
   }

   /// <summary>
   /// Counts it as held by <paramref name="vocab"/> too, once however often its match-condition holds it, such as in two groups of an "any of", or as a<br/>
   /// condition and as a combination of some of a group's conditions.
   /// </summary>
   internal void CountAsHeldBy(VocabNote vocab) => _vocab.Add(vocab);

   /// <summary>How many vocab hold it.</summary>
   internal int HowManyVocabHoldIt => _vocab.Count;

   /// <summary>How many conditions its tree has (<see cref="ConditionWithItsLevel.Size"/>).</summary>
   internal int Size => _asFirstFound.Size;

   /// <summary>The condition as code, on one line.</summary>
   internal string Code => ConditionCode.OnOneLine(_asFirstFound.Condition);

   /// <summary>
   /// Whether it's only ever found within another of <paramref name="found"/>: within a larger one (<see cref="ConditionWithItsLevel.IsFoundWithin"/>),<br/>
   /// held by the same vocab, such as the <c>Surface.EndsWithOneOf("ん")</c> of <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c>.
   /// </summary>
   internal bool IsOnlyEverFoundWithinAnotherOf(IEnumerable<ConditionHeldByVocab> found) => found.Any(IsOnlyEverFoundWithin);

   bool IsOnlyEverFoundWithin(ConditionHeldByVocab larger) => larger != this && IsHeldByTheSameVocabAs(larger) && _asFirstFound.IsFoundWithin(larger._asFirstFound);

   bool IsHeldByTheSameVocabAs(ConditionHeldByVocab other) => other._vocab.SetEquals(_vocab);

   /// <summary>It as a candidate for a shared-condition, with the vocab found holding it.</summary>
   internal SharedConditionCandidate AsACandidate => new(_asFirstFound.Condition, _asFirstFound.Level, _isACombination, [.. _vocab]);
}
