using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>
/// A group of conditions, <see cref="AllOfGroup{TSubject}"/> or <see cref="AnyOfGroup{TSubject}"/>, as the match-condition editor changes it, whatever the<br/>
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
}
