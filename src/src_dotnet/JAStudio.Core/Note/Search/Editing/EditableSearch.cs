using System;
using System.Collections.Generic;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Search.Grouping;

namespace JAStudio.Core.Note.Search.Editing;

/// <summary>
/// A search as the search editor builds it: the <see cref="Current"/> tree of conditions, the changes the editor makes to it, and undo.<br/>
/// It starts as an empty "all of" group, which finds every note. Every change leaves a valid search.
/// </summary>
/// <remarks>
/// Conditions are values, so each change makes a new tree, and undo goes back to the tree before the change.<br/>
/// Conditions are found by their <see cref="ConditionPosition"/>.
/// </remarks>
public class EditableSearch
{
   readonly Stack<NoteCondition> _earlierSearches = new();

   /// <summary>The search as it is now.</summary>
   public NoteCondition Current { get; private set; } = ConditionGroup.AllOf();

   /// <summary>Whether there's a change to undo.</summary>
   public bool CanUndo => _earlierSearches.Count > 0;

   /// <summary>Adds <paramref name="condition"/> after the other conditions in the group at <paramref name="groupPosition"/>.</summary>
   /// <returns>The position of the added condition.</returns>
   public ConditionPosition AddCondition(ConditionPosition groupPosition, NoteCondition condition)
   {
      var group = GroupAt(groupPosition);
      Change(Current.WithConditionAt(groupPosition, group.WithConditionAdded(condition)));
      return groupPosition.Child(group.Conditions.Count);
   }

   /// <summary>
   /// Replaces the whole search with an "all of" group holding only <paramref name="condition"/>, as searching for a tag from the tag tree does.<br/>
   /// The group lets conditions added later narrow the search down. Undo goes back to the search before.
   /// </summary>
   /// <returns>The position of <paramref name="condition"/>.</returns>
   public ConditionPosition ReplaceTheWholeSearchWith(NoteCondition condition)
   {
      Change(ConditionGroup.AllOf(condition));
      return ConditionPosition.Root.Child(0);
   }

   /// <summary>Replaces the condition at <paramref name="position"/> with <paramref name="replacement"/>, as when a condition is edited.</summary>
   public void ReplaceCondition(ConditionPosition position, NoteCondition replacement) => Change(Current.WithConditionAt(position, replacement));

   /// <summary>
   /// Removes the condition at <paramref name="position"/>, and a group with everything in it.<br/>
   /// A negation goes with the condition it negates. The root can't be removed.
   /// </summary>
   public void RemoveCondition(ConditionPosition position)
   {
      var removed = TheConditionWithItsNegations(position);
      if(removed.IsRoot) throw new InvalidOperationException("The root of a search can't be removed.");

      Change(Current.WithConditionAt(removed.Parent, GroupAt(removed.Parent).WithoutConditionAt(removed.IndexInParent)));
   }

   /// <summary>Negates the condition at <paramref name="position"/>, or takes its negation away if it is a <see cref="NegatedCondition"/>.</summary>
   public void ToggleNegation(ConditionPosition position) =>
      Change(Current.WithConditionAt(position,
                                     Current.ConditionAt(position) is NegatedCondition negated
                                        ? negated.Condition
                                        : ConditionGroup.Not(Current.ConditionAt(position))));

   /// <summary>Switches the group at <paramref name="groupPosition"/> between "all of" and "any of", keeping its conditions.</summary>
   public void SwitchGroup(ConditionPosition groupPosition) =>
      Change(Current.WithConditionAt(groupPosition, GroupAt(groupPosition).SwitchedBetweenAllOfAndAnyOf()));

   /// <summary>Starts over from <paramref name="search"/>, as from the search the workspace was last left with: there's no change to undo.</summary>
   public void StartFrom(NoteCondition search)
   {
      _earlierSearches.Clear();
      Current = search;
   }

   /// <summary>Goes back to the search as it was before the last change. Only when <see cref="CanUndo"/>.</summary>
   public void Undo() => Current = CanUndo ? _earlierSearches.Pop() : throw new InvalidOperationException("There's no change to undo.");

   void Change(NoteCondition changed)
   {
      _earlierSearches.Push(Current);
      Current = changed;
   }

   ConditionGroup GroupAt(ConditionPosition position) =>
      Current.ConditionAt(position) as ConditionGroup ?? throw new ArgumentException($"The condition at {position} is not a group.", nameof(position));

   /// <summary><paramref name="position"/>, or the position of the outermost <see cref="NegatedCondition"/> directly around it.</summary>
   ConditionPosition TheConditionWithItsNegations(ConditionPosition position)
   {
      while(!position.IsRoot && Current.ConditionAt(position.Parent) is NegatedCondition)
         position = position.Parent;
      return position;
   }
}
