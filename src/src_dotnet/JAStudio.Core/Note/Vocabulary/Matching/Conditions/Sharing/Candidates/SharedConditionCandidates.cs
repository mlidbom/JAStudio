using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing.Candidates;

/// <summary>
/// Finds the conditions that the match-conditions of several vocab hold (<see cref="SharedConditionCandidate"/>), which shared-conditions could give names:<br/>
/// the conditions in their trees, and the combinations of some of the conditions in a group, such as two conditions that several match-conditions<br/>
/// require among others. Conditions are compared by what they mean (<see cref="Condition.MeansTheSameAs"/>), so the order of a group's conditions doesn't<br/>
/// matter.
/// </summary>
/// <remarks>
/// It proposes; a mind decides. A shared-condition is mostly its name, and two vocab holding the same condition may hold it for different reasons, which one<br/>
/// shared-condition would tie together. So a shared-condition is made from what it finds only once someone has named it and decided it's one concept.
/// </remarks>
/// <remarks>
/// Left out: what's a name already, a fact or a shared-condition, on a subject or negated, such as <c>!PrecedingToken.IsAnIchidanVerb</c>; a negation, since<br/>
/// naming what it negates names it too, and what it negates counts as held by the vocab holding it; a group of fewer than two conditions; and a condition<br/>
/// that's only ever found within a larger one, held by the same vocab, such as the <c>Surface.EndsWithOneOf("ん")</c> of<br/>
/// <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c>. The combinations of a group's conditions are looked at for a group of at most<br/>
/// <see cref="MostConditionsOfAGroupToCombine"/>, since a group of n has 2^n.
/// </remarks>
public static class SharedConditionCandidates
{
   /// <summary>The most conditions a group may have for the combinations of some of them to be looked at.</summary>
   public const int MostConditionsOfAGroupToCombine = 8;

   /// <summary>
   /// The conditions the match-conditions of at least <paramref name="heldByAtLeast"/> of <paramref name="vocab"/> hold, those held by the most vocab first,<br/>
   /// then the largest, then by their code in ordinal order.
   /// </summary>
   public static IReadOnlyList<SharedConditionCandidate> FoundIn(IEnumerable<VocabNote> vocab, int heldByAtLeast)
   {
      //review:matching: the namespace Sharing.Candidates is new: the finder's five types made Sharing ten, in two groups. SharedConditionCandidate and
      //   SharedConditionCandidates keep their names, which say "candidate" as the namespace does, since "shared-condition candidate" is the concept's name.
      var heldOftenEnough = new ConditionsHeldByVocab(vocab).HeldByAtLeast(heldByAtLeast);
      var notOnlyEverFoundWithinALargerOne = heldOftenEnough.Where(it => !it.IsOnlyEverFoundWithinAnotherOf(heldOftenEnough));
      return [.. InTheOrderListed(notOnlyEverFoundWithinALargerOne).Select(it => it.AsACandidate)];

      static IEnumerable<ConditionHeldByVocab> InTheOrderListed(IEnumerable<ConditionHeldByVocab> found) =>
         found.OrderByDescending(it => it.HowManyVocabHoldIt).ThenByDescending(it => it.Size).ThenBy(it => it.Code, StringComparer.Ordinal);
   }
}
