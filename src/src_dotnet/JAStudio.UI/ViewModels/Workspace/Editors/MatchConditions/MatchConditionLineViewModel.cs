using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.UI.ViewModels.Workspace.Editors.MatchConditions;

/// <summary>
/// One line in the tree editor of a vocab's match-condition: a condition, or a group with its conditions under it.<br/>
/// Negations and conditions on a subject aren't lines of their own: they're shown in front of what they hold, on its line, as in<br/>
/// "Not: The preceding token: is a godan verb" or "The preceding token: Any of:".
/// </summary>
class MatchConditionLineViewModel
{
   readonly MatchConditionLineViewModel? _parent;

   /// <param name="shownCondition">The condition shown on this line, with the negations and conditions on subjects around it.</param>
   /// <param name="position">Where <paramref name="shownCondition"/> is in the match-condition.</param>
   /// <param name="matchCondition">The match-condition the line is in.</param>
   /// <param name="parent">The group this line is in, or null for the root.</param>
   MatchConditionLineViewModel(Condition shownCondition, ConditionPosition position, EditableMatchCondition matchCondition, MatchConditionLineViewModel? parent)
   {
      _parent = parent;
      Position = position;
      Description = shownCondition.DescriptionLines().First();
      IsNegated = shownCondition is INegatedCondition;
      (Condition, ConditionPosition) = TheConditionInsideWhatHoldsOnlyIt(shownCondition, position);
      Level = matchCondition.LevelAt(ConditionPosition);
      Children = Condition is IConditionGroup group
                    ? [..group.Conditions.Select((condition, index) => new MatchConditionLineViewModel(condition, ConditionPosition.Child(index), matchCondition, this))]
                    : [];
   }

   /// <summary>The root line of <paramref name="matchCondition"/>'s <see cref="EditableMatchCondition.Current"/> tree, with every other line under it.</summary>
   public static MatchConditionLineViewModel RootOf(EditableMatchCondition matchCondition) =>
      new(matchCondition.Current, ConditionPosition.Root, matchCondition, parent: null);

   /// <summary>The line as the editor shows it, such as "Not: The preceding token: is a godan verb" or "All of:".</summary>
   public string Description { get; }

   /// <summary>The lines of the conditions in this line's group. None unless <see cref="IsGroup"/>.</summary>
   public IReadOnlyList<MatchConditionLineViewModel> Children { get; }

   /// <summary>Where the line's whole condition is, with the negations and conditions on subjects around it: what removing the line, or negating it, works on.</summary>
   public ConditionPosition Position { get; }

   /// <summary>The line's condition inside the negations and conditions on subjects around it: what changing the line changes, and a group's conditions go into.</summary>
   public Condition Condition { get; }

   /// <summary>Where <see cref="Condition"/> is in the match-condition.</summary>
   public ConditionPosition ConditionPosition { get; }

   /// <summary>The conditions there are on the subject of <see cref="Condition"/>: those the add menu offers in this line's group, if it is one.</summary>
   public IConditionLevel Level { get; }

   public bool IsGroup => Condition is IConditionGroup;

   /// <summary>Whether the line is an "any of" group, so that switching it makes it an "all of" group.</summary>
   public bool IsAnAnyOfGroup => Condition is IConditionGroup { IsAnyOf: true };

   /// <summary>Whether the line's whole condition is a negation, so that negating it takes the negation away.</summary>
   public bool IsNegated { get; }

   public bool IsRoot => _parent is null;

   /// <summary>Whether the condition at <paramref name="position"/> is this line's: its whole condition (<see cref="Position"/>), or the <see cref="Condition"/> inside.</summary>
   public bool IsAt(ConditionPosition position) => Position == position || ConditionPosition == position;

   /// <summary>The group a condition added on this line goes into: this line's group, or the group this line is in.</summary>
   public MatchConditionLineViewModel GroupToAddTo => IsGroup ? this : _parent ?? throw new InvalidOperationException("A match-condition's root is always a group.");

   /// <summary>This line and every line under it, in the order the tree shows them.</summary>
   public IEnumerable<MatchConditionLineViewModel> ThisAndAllBelow() => [this, ..Children.SelectMany(it => it.ThisAndAllBelow())];

   /// <summary><paramref name="shownCondition"/>, or the condition inside the negations and conditions on subjects around it, with its position.</summary>
   static (Condition Condition, ConditionPosition Position) TheConditionInsideWhatHoldsOnlyIt(Condition shownCondition, ConditionPosition position)
   {
      while(shownCondition is INegatedCondition or ISubjectCondition)
      {
         shownCondition = shownCondition.Children[0];
         position = position.Child(0);
      }

      return (shownCondition, position);
   }
}
