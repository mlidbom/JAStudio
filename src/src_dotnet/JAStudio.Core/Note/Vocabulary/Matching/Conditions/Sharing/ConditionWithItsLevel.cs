using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

/// <summary>
/// A condition in a tree of conditions, such as a vocab's match-condition, with its <see cref="Level"/>: the conditions on the subject it's on, which<br/>
/// tell whether it's a fact, and which a shared-condition defined as it would be on. <see cref="Candidates.SharedConditionCandidates"/> looks for the<br/>
/// conditions vocab share among these, and a line of the tree editor can be made a shared-condition of one (<see cref="SharedConditions.MakeOf"/>).
/// </summary>
/// <param name="Condition">The condition.</param>
/// <param name="Level">The conditions on the subject <paramref name="Condition"/> is on.</param>
public sealed record ConditionWithItsLevel(Condition Condition, IConditionLevel Level)
{
   /// <summary>This condition and every condition it holds, all the way down, each with its level.</summary>
   internal IEnumerable<ConditionWithItsLevel> ItAndEveryConditionItHolds => [this, .. TheConditionsItHolds.SelectMany(it => it.ItAndEveryConditionItHolds)];

   /// <summary>How many conditions its tree has, itself among them: the larger of two candidates held by as many vocab is listed first.</summary>
   internal int Size => ItAndEveryConditionItHolds.Count();

   /// <summary>The conditions of its tree that are worth giving a name as a shared-condition, itself among them, in the order of the tree.</summary>
   internal IEnumerable<ConditionWithItsLevel> ConditionsWorthNamingInIt => ItAndEveryConditionItHolds.Where(it => it.IsWorthNaming);

   /// <summary>
   /// The combinations of some of the conditions of each group in its tree (<see cref="IConditionGroup.CombinationsOfSomeOfItsConditions"/>), itself among<br/>
   /// them, each on its group's level: those of each group of at most <see cref="Candidates.SharedConditionCandidates.MostConditionsOfAGroupToCombine"/>.
   /// </summary>
   internal IEnumerable<ConditionWithItsLevel> CombinationsOfSomeOfTheConditionsOfEachGroupInIt => ItAndEveryConditionItHolds.SelectMany(it => it.CombinationsOfSomeOfItsConditions);

   /// <summary>
   /// Whether this condition is found within <paramref name="larger"/>: whether <paramref name="larger"/>, or one of the conditions it holds, means the same<br/>
   /// (<see cref="Condition.MeansTheSameAs"/>), or is a group holding conditions meaning the same as each of this one's (<see cref="IConditionGroup.HoldsTheConditionsOf"/>).
   /// </summary>
   internal bool IsFoundWithin(ConditionWithItsLevel larger) => larger.ItAndEveryConditionItHolds.Any(it => it.MeansTheSameAsOrHoldsTheConditionsOf(Condition));

   bool MeansTheSameAsOrHoldsTheConditionsOf(Condition other) => Condition.MeansTheSameAs(other) || Condition is IConditionGroup group && group.HoldsTheConditionsOf(other);

   /// <summary>
   /// Whether it's worth giving a name: not empty, not a name already (<see cref="IsAName"/>), not a negation, since naming what it negates names it too,<br/>
   /// and not a group of fewer than two conditions.
   /// </summary>
   public bool IsWorthNaming => !Condition.IsEmpty && !IsAName && Condition is not (INegatedCondition or IConditionGroup { Conditions.Count: < 2 });

   /// <summary>Whether it's a name already: a fact or a shared-condition, on a subject or negated, such as <c>!PrecedingToken.IsAnIchidanVerb</c>.</summary>
   bool IsAName => Condition switch
   {
      ISharedCondition => true,
      INegatedCondition or ISubjectCondition => TheConditionsItHolds.Single().IsAName,
      _ => Level.Facts.Contains(Condition)
   };

   /// <summary>
   /// When it's a group of at most <see cref="Candidates.SharedConditionCandidates.MostConditionsOfAGroupToCombine"/> conditions, the combinations of some of them<br/>
   /// (<see cref="IConditionGroup.CombinationsOfSomeOfItsConditions"/>) but the empty ones, on its level: none for any other condition.
   /// </summary>
   IEnumerable<ConditionWithItsLevel> CombinationsOfSomeOfItsConditions =>
      Condition is IConditionGroup { Conditions.Count: <= Candidates.SharedConditionCandidates.MostConditionsOfAGroupToCombine } group
         ? group.CombinationsOfSomeOfItsConditions().Where(it => !it.IsEmpty).Select(it => new ConditionWithItsLevel(it, Level))
         : [];

   /// <summary>The conditions directly under this one in the tree (<see cref="Condition.Children"/>), each with its level.</summary>
   IEnumerable<ConditionWithItsLevel> TheConditionsItHolds =>
      //review:matching: ConditionWithItsLevel was a tuple of a condition and its level, passed around SharedConditionCandidates. The level goes with the
      //   condition only because a Condition doesn't say its own: each kind of subject has one level (WordConditions.Level, TokenConditions.Level,
      //   TextConditions.Level), so a Condition<TSubject> could say it, and this type would go.
      Condition.Children.Select(it => new ConditionWithItsLevel(it, LevelOfTheConditionsItHolds));

   /// <summary>The level of the conditions it holds: its subject's, for a condition on a subject (<see cref="ISubjectCondition"/>), else its own.</summary>
   IConditionLevel LevelOfTheConditionsItHolds => Condition is ISubjectCondition onASubject ? onASubject.Subject.Level : Level;
}
