using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;

/// <summary>
/// A vocab's match-condition as the match-condition editor builds it: the <see cref="Current"/> tree of conditions, the changes the editor makes to it,<br/>
/// and undo. Every change leaves a valid match-condition: an "all of" group at the root, and each condition on the subject of the place it's in.
/// </summary>
/// <remarks>
/// Conditions are values, so each change makes a new tree, and undo goes back to the tree before the change. Conditions are found by their<br/>
/// <see cref="ConditionPosition"/>.
/// </remarks>
public class EditableMatchCondition
{
   //refactor: EditableSearch is this class for a search: the same changes by position, and the same undo. One generic class could serve both
   //   once NoteCondition and Condition share the tree's shape, which is the design document's open question of the note search's tree
   //   becoming a Condition<JPNote>.
   readonly Stack<AllOfGroup<CandidateWordVariant>> _earlierMatchConditions = new();

   /// <param name="matchCondition">The vocab's match-condition as it is now: the <see cref="Current"/> one, with no change to undo.</param>
   public EditableMatchCondition(AllOfGroup<CandidateWordVariant> matchCondition) => Current = matchCondition;

   /// <summary>The match-condition as it is now.</summary>
   public AllOfGroup<CandidateWordVariant> Current { get; private set; }

   /// <summary>Whether there's a change to undo.</summary>
   public bool CanUndo => _earlierMatchConditions.Count > 0;

   /// <summary>
   /// The conditions there are where the condition at <paramref name="position"/> is: those on a word at the root, and inside a condition on a<br/>
   /// subject, such as the preceding token, those on the subject.
   /// </summary>
   public IConditionLevel LevelAt(ConditionPosition position)
   {
      IConditionLevel level = WordConditions.Level;
      Condition condition = Current;
      foreach(var index in position.IndexesFromTheRoot)
      {
         if(condition is ISubjectCondition onASubject) level = onASubject.Subject.Level;
         condition = condition.Children[index];
      }

      return level;
   }

   /// <summary>Adds <paramref name="condition"/> after the other conditions in the group at <paramref name="groupPosition"/>. It must be a condition on the group's subject.</summary>
   /// <returns>The position of the added condition.</returns>
   public ConditionPosition AddCondition(ConditionPosition groupPosition, Condition condition)
   {
      var group = GroupAt(groupPosition);
      Change(groupPosition, group.WithConditionAdded(condition));
      return groupPosition.Child(group.Conditions.Count);
   }

   /// <summary>Replaces the condition at <paramref name="position"/> with <paramref name="replacement"/>, as when a condition is changed. It must be on the same subject.</summary>
   public void ReplaceCondition(ConditionPosition position, Condition replacement) => Change(position, replacement);

   /// <summary>
   /// Removes the condition at <paramref name="position"/>, a group with everything in it, and the negations and conditions on subjects that hold<br/>
   /// only it: removing <c>IsAGodanVerb</c> from <c>!PrecedingToken.IsAGodanVerb</c> removes the whole line. The root can't be removed.
   /// </summary>
   public void RemoveCondition(ConditionPosition position)
   {
      var removed = TheConditionWithWhatHoldsOnlyIt(position);
      if(removed.IsRoot) throw new InvalidOperationException("The root of a match-condition can't be removed.");

      Change(removed.Parent, GroupAt(removed.Parent).WithoutConditionAt(removed.IndexInParent));
   }

   /// <summary>Negates the condition at <paramref name="position"/>, or takes its negation away if it is a negation. The root is always an "all of" group, never negated.</summary>
   public void ToggleNegation(ConditionPosition position)
   {
      if(position.IsRoot) throw new InvalidOperationException("The root of a match-condition is always an \"all of\" group, never negated.");

      var condition = Current.ConditionAt(position);
      Change(position, condition is INegatedCondition negation ? negation.Condition : condition.Negated());
   }

   /// <summary>Switches the group at <paramref name="groupPosition"/> between "all of" and "any of", keeping its conditions. The root is always "all of".</summary>
   public void SwitchGroup(ConditionPosition groupPosition)
   {
      if(groupPosition.IsRoot) throw new InvalidOperationException("The root of a match-condition is always an \"all of\" group.");

      Change(groupPosition, GroupAt(groupPosition).SwitchedBetweenAllOfAndAnyOf());
   }

   /// <summary>
   /// Takes in the vocab's match-condition as something else changed it, such as the Matching tab's rule-by-rule editors: a change like the editor's own,<br/>
   /// which undo takes back. Nothing, when it's the <see cref="Current"/> one.
   /// </summary>
   public void ChangedElsewhere(AllOfGroup<CandidateWordVariant> matchCondition)
   {
      if(matchCondition == Current) return;

      _earlierMatchConditions.Push(Current);
      Current = matchCondition;
   }

   /// <summary>Goes back to the match-condition as it was before the last change. Only when <see cref="CanUndo"/>.</summary>
   public void Undo() => Current = CanUndo ? _earlierMatchConditions.Pop() : throw new InvalidOperationException("There's no change to undo.");

   void Change(ConditionPosition position, Condition replacement)
   {
      _earlierMatchConditions.Push(Current);
      Current = (AllOfGroup<CandidateWordVariant>)Current.WithConditionAt(position, replacement);
   }

   IConditionGroup GroupAt(ConditionPosition position) =>
      Current.ConditionAt(position) as IConditionGroup ?? throw new ArgumentException($"The condition at {position} is not a group.", nameof(position));

   /// <summary><paramref name="position"/>, or the position of the outermost negation or condition on a subject that holds only the condition there.</summary>
   ConditionPosition TheConditionWithWhatHoldsOnlyIt(ConditionPosition position)
   {
      while(!position.IsRoot && Current.ConditionAt(position.Parent) is INegatedCondition or ISubjectCondition)
         position = position.Parent;
      return position;
   }
}
