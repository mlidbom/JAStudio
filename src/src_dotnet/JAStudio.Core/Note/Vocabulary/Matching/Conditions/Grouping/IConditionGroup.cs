using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>
/// A group of conditions, <see cref="AllOfGroup{TSubject}"/> or <see cref="AnyOfGroup{TSubject}"/>, as the tree editor changes it, whatever the<br/>
/// subject of its conditions.
/// </summary>
public interface IConditionGroup
{
   /// <summary>The conditions in the group, in the order the editor shows them: the group's <see cref="Condition.Children"/>.</summary>
   IReadOnlyList<Condition> Conditions { get; }

   /// <summary>Whether the group is met when any of its conditions is (OR), rather than all of them (AND).</summary>
   bool IsAnyOf { get; }

   /// <summary>This group with <paramref name="condition"/>, a condition on the group's subject, added after its other conditions.</summary>
   Condition WithConditionAdded(Condition condition);

   /// <summary>This group without its condition at <paramref name="index"/>.</summary>
   Condition WithoutConditionAt(int index);

   /// <summary>An "any of" group with this group's conditions, if this is an "all of" group, and the other way around.</summary>
   Condition SwitchedBetweenAllOfAndAnyOf();

   /// <summary>
   /// Whether <paramref name="condition"/> is a group of the same kind, each of whose conditions means the same as a different one of this group's<br/>
   /// (<see cref="Condition.MeansTheSameAs"/>): <c>AllOf(A, B, C)</c> holds the conditions of <c>AllOf(B, A)</c>.
   /// </summary>
   internal bool HoldsTheConditionsOf(Condition condition);

   /// <summary>
   /// Each combination of some of the group's conditions, two or more of them but not all, as a group of the same kind with them in their order:<br/>
   /// <c>AllOf(A, B, C)</c> gives <c>AllOf(A, B)</c>, <c>AllOf(A, C)</c> and <c>AllOf(B, C)</c>. None for a group of fewer than three. A group of<br/>
   /// <c>n</c> conditions has nearly <c>2^n</c>.
   /// </summary>
   /// <remarks>
   /// In the order counting in binary gives, the first condition the lowest bit, as <see cref="SysUtils.Collections.Generic.SubsetsExtensions"/> gives<br/>
   /// a list's subsets. <see cref="Sharing.Candidates.SharedConditionCandidates"/> keeps a condition as the first vocab found holding it holds it, so the<br/>
   /// order decides which of two combinations meaning the same it keeps.
   /// </remarks>
   internal IEnumerable<Condition> CombinationsOfSomeOfItsConditions();
}
