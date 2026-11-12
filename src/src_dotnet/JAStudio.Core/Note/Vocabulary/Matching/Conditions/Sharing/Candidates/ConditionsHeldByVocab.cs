using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE.CollectionsCE.GenericCE;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing.Candidates;

/// <summary>
/// The conditions worth naming that the match-conditions of some vocab hold (<see cref="ConditionWithItsLevel.ConditionsWorthNamingInIt"/>), and the<br/>
/// combinations of some of the conditions of their groups, each found once by what it means (<see cref="ConditionMeaningComparer"/>), with the vocab<br/>
/// holding it (<see cref="ConditionHeldByVocab"/>).
/// </summary>
sealed class ConditionsHeldByVocab
{
   readonly Dictionary<Condition, ConditionHeldByVocab> _byMeaning = new(ConditionMeaningComparer.Instance);

   /// <summary>The conditions <paramref name="vocab"/>'s match-conditions hold, each kept as the first of them found holding it holds it.</summary>
   internal ConditionsHeldByVocab(IEnumerable<VocabNote> vocab)
   {
      //review:matching: this and ConditionHeldByVocab were a dictionary in SharedConditionCandidates.FoundIn and its private class Found, which made its
      //   SharedConditionCandidate at once, around the list of vocab it went on adding to. "Held by vocab", as the tool's description says a match-condition
      //   holds a condition; a ConditionHeldByVocab becomes a candidate only once enough vocab hold it.
      foreach(var note in vocab) FindTheConditionsHeldBy(note);
   }

   /// <summary>Those held by at least <paramref name="count"/> vocab, in the order they were first found.</summary>
   internal IReadOnlyList<ConditionHeldByVocab> HeldByAtLeast(int count) => [.. _byMeaning.Values.Where(it => it.HowManyVocabHoldIt >= count)];

   /// <summary>
   /// Counts each condition worth naming that <paramref name="vocab"/>'s match-condition holds as held by it: first the conditions in its tree, then the<br/>
   /// combinations of some of a group's conditions.
   /// </summary>
   void FindTheConditionsHeldBy(VocabNote vocab)
   {
      var matchCondition = new ConditionWithItsLevel(vocab.MatchingConfiguration.MatchCondition, WordConditions.Level);
      foreach(var condition in matchCondition.ConditionsWorthNamingInIt) TheOneMeaningTheSameAs(condition, isACombination: false).CountAsHeldBy(vocab);
      foreach(var combination in matchCondition.CombinationsOfSomeOfTheConditionsOfEachGroupInIt) TheOneMeaningTheSameAs(combination, isACombination: true).CountAsHeldBy(vocab);
   }

   /// <summary>The condition found meaning the same as <paramref name="condition"/>: made from it when it's the first found that means this.</summary>
   ConditionHeldByVocab TheOneMeaningTheSameAs(ConditionWithItsLevel condition, bool isACombination) =>
      _byMeaning.GetOrAdd(condition.Condition, () => new ConditionHeldByVocab(condition, isACombination));
}
