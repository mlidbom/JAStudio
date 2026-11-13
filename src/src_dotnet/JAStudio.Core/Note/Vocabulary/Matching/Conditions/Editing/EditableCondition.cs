using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;

/// <summary>
/// A condition as the tree editor builds it, a vocab's match-condition or a shared-condition's definition (<see cref="IEditedCondition"/>): the<br/>
/// <see cref="Current"/> tree of conditions, the changes the editor makes to it, and undo. Every change leaves a valid tree: an "all of" group at the<br/>
/// root, on the subject of the <see cref="Level"/>, and each condition on the subject of the place it's in.
/// </summary>
/// <remarks>
/// Conditions are values, so each change makes a new tree, and undo goes back to the tree before the change. Conditions are found by their<br/>
/// <see cref="ConditionPosition"/>.
/// </remarks>
public class EditableCondition
{
   //refactor: EditableSearch is this class for a search: the same changes by position, and the same undo. One generic class could serve both
   //   once NoteCondition and Condition share the tree's shape, which is the design document's open question of the note search's tree
   //   becoming a Condition<JPNote>.
   readonly Stack<Condition> _earlierTrees = new();

   /// <param name="tree">The condition as it is now, an "all of" group on <paramref name="level"/>'s subject: the <see cref="Current"/> one, with no change to undo.</param>
   /// <param name="level">The conditions there are at the root: those on a word, for a vocab's match-condition.</param>
   public EditableCondition(Condition tree, IConditionLevel level)
   {
      AssertIsAnAllOfGroup(tree);
      Current = tree;
      Level = level;
   }

   /// <summary>The conditions there are at the root: those on a word, for a vocab's match-condition, and those on its subject, for a shared-condition's definition.</summary>
   public IConditionLevel Level { get; }

   /// <summary>The tree as it is now: an "all of" group on the <see cref="Level"/>'s subject.</summary>
   public Condition Current { get; private set; }

   /// <summary>Whether there's a change to undo.</summary>
   public bool CanUndo => _earlierTrees.Count > 0;

   /// <summary>
   /// The conditions there are where the condition at <paramref name="position"/> is: the <see cref="Level"/>'s at the root, and inside a condition on a<br/>
   /// subject, such as the preceding token, those on the subject.
   /// </summary>
   public IConditionLevel LevelAt(ConditionPosition position)
   {
      var level = Level;
      var condition = Current;
      foreach(var index in position.IndexesFromTheRoot)
      {
         if(condition is ISubjectCondition onASubject) level = onASubject.Subject.Level;
         condition = condition.Children[index];
      }

      return level;
   }

   /// <summary>
   /// The conditions a shared-condition can be made of on the line of the condition at <paramref name="position"/>, from the outermost in: the condition<br/>
   /// there, and each that the negations and conditions on subjects around it hold, each on its subject, those worth naming<br/>
   /// (<see cref="ConditionWithItsLevel.IsWorthNaming"/>). <c>!PrecedingToken.Surface.EndsWithOneOf("ん")</c> gives <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c><br/>
   /// on a word, <c>Surface.EndsWithOneOf("ん")</c> on a token and <c>EndsWithOneOf("ん")</c> on a text, and <c>PrecedingToken.IsAGodanVerb</c> none, since it's a<br/>
   /// name already. The root gives itself when it holds two conditions or more.
   /// </summary>
   public IReadOnlyList<ConditionWithItsLevel> ConditionsToMakeASharedConditionOfAt(ConditionPosition position)
   {
      return [.. ThePositionsInward().Select(it => new ConditionWithItsLevel(Current.ConditionAt(it), LevelAt(it))).Where(it => it.IsWorthNaming)];

      IEnumerable<ConditionPosition> ThePositionsInward()
      {
         for(var inward = position;; inward = inward.Child(0))
         {
            yield return inward;
            if(Current.ConditionAt(inward) is not (INegatedCondition or ISubjectCondition)) yield break;
         }
      }
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
      if(removed.IsRoot) throw new InvalidOperationException("The root of the tree can't be removed.");

      Change(removed.Parent, GroupAt(removed.Parent).WithoutConditionAt(removed.IndexInParent));
   }

   /// <summary>Negates the condition at <paramref name="position"/>, or takes its negation away if it is a negation. The root is always an "all of" group, never negated.</summary>
   public void ToggleNegation(ConditionPosition position)
   {
      if(position.IsRoot) throw new InvalidOperationException("The root of the tree is always an \"all of\" group, never negated.");

      var condition = Current.ConditionAt(position);
      Change(position, condition is INegatedCondition negation ? negation.Condition : condition.Negated());
   }

   /// <summary>Switches the group at <paramref name="groupPosition"/> between "all of" and "any of", keeping its conditions. The root is always "all of".</summary>
   public void SwitchGroup(ConditionPosition groupPosition)
   {
      if(groupPosition.IsRoot) throw new InvalidOperationException("The root of the tree is always an \"all of\" group.");

      Change(groupPosition, GroupAt(groupPosition).SwitchedBetweenAllOfAndAnyOf());
   }

   /// <summary>
   /// Takes in the condition as something else changed it, such as making a shared-condition, or the MCP server: a change like the editor's own, which<br/>
   /// undo takes back. Nothing, when it's the <see cref="Current"/> tree.
   /// </summary>
   /// <param name="tree">The condition as it is now, an "all of" group on the <see cref="Level"/>'s subject.</param>
   public void ChangedElsewhere(Condition tree)
   {
      AssertIsAnAllOfGroup(tree);
      if(tree == Current) return;

      _earlierTrees.Push(Current);
      Current = tree;
   }

   /// <summary>Goes back to the tree as it was before the last change. Only when <see cref="CanUndo"/>.</summary>
   public void Undo() => Current = CanUndo ? _earlierTrees.Pop() : throw new InvalidOperationException("There's no change to undo.");

   void Change(ConditionPosition position, Condition replacement)
   {
      _earlierTrees.Push(Current);
      Current = Current.WithConditionAt(position, replacement);
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

   static void AssertIsAnAllOfGroup(Condition tree) =>
      Contract.Argument.Assert(tree is IConditionGroup { IsAnyOf: false }, () => $"The tree editor's tree has an \"all of\" group at its root, not: {tree}");
}
