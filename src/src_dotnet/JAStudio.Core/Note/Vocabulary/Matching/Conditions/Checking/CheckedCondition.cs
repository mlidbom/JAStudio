using System.Collections.Generic;
using JAStudio.Core.Note.ConditionTrees;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Checking;

/// <summary>
/// A condition checked against a subject (<see cref="Condition{TSubject}.CheckedAgainst"/>), such as a vocab's match-condition against a word in a<br/>
/// sentence: whether the subject meets it, and each of the conditions it holds, checked against the subject it holds it for, such as a condition on the<br/>
/// token before the word against that token. It shows which of a tree's conditions decided whether a word matches.
/// </summary>
public sealed class CheckedCondition
{
   internal CheckedCondition(bool isMet, IReadOnlyList<CheckedCondition> children)
   {
      IsMet = isMet;
      Children = children;
   }

   /// <summary>Whether the subject meets the condition.</summary>
   public bool IsMet { get; }

   /// <summary>The conditions the condition holds, each checked: one for each of its <see cref="Condition.Children"/>, in the same order.</summary>
   public IReadOnlyList<CheckedCondition> Children { get; }

   /// <summary>
   /// The check of the condition at <paramref name="position"/> in the tree whose root this is the check of, as <see cref="Condition.ConditionAt"/> finds<br/>
   /// the condition.
   /// </summary>
   public CheckedCondition At(ConditionPosition position) => position.IsRoot ? this : Children[position.FirstIndex].At(position.BelowTheFirstChild);
}
