using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Grouping;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>
/// One line in the search's condition tree: a condition on a property, or a group with its conditions under it.<br/>
/// A <see cref="NegatedCondition"/> isn't a line of its own: it's shown as "Not: " on the line of the condition it negates.
/// </summary>
class SearchConditionNodeViewModel
{
   readonly SearchConditionNodeViewModel? _parent;

   /// <param name="shownCondition">The condition shown on this line, with any negation around it.</param>
   /// <param name="position">Where <paramref name="shownCondition"/> is in the search.</param>
   /// <param name="parent">The group this line is in, or null for the root.</param>
   public SearchConditionNodeViewModel(NoteCondition shownCondition, ConditionPosition position, SearchConditionNodeViewModel? parent)
   {
      _parent = parent;
      Position = position;
      Description = shownCondition.DescriptionLines().First();
      (Condition, ConditionPosition) = TheConditionInsideAnyNegation(shownCondition, position);
      Children = Condition is ConditionGroup group
                    ? [..group.Conditions.Select((condition, index) => new SearchConditionNodeViewModel(condition, ConditionPosition.Child(index), this))]
                    : [];
   }

   /// <summary>The line as the editor shows it, such as "Tags include source::anime" or "Not: All of:".</summary>
   public string Description { get; }

   /// <summary>The lines of the conditions in this line's group. None unless <see cref="IsGroup"/>.</summary>
   public IReadOnlyList<SearchConditionNodeViewModel> Children { get; }

   /// <summary>Where this line's condition is in the search, with any negation around it: what removing the line, or negating it, works on.</summary>
   public ConditionPosition Position { get; }

   /// <summary>This line's condition without the negation around it, if there is one: what editing the line edits.</summary>
   public NoteCondition Condition { get; }

   /// <summary>Where <see cref="Condition"/> is in the search: inside any negation around it.</summary>
   public ConditionPosition ConditionPosition { get; }

   public bool IsGroup => Condition is ConditionGroup;

   /// <summary>Whether the line's condition is negated: its <see cref="Condition"/> is inside a <see cref="NegatedCondition"/>.</summary>
   public bool IsNegated => Position != ConditionPosition;

   public bool IsRoot => _parent is null;

   /// <summary>The group a condition added on this line goes into: this line's group, or the group this line is in.</summary>
   public ConditionPosition GroupToAddTo =>
      IsGroup ? ConditionPosition : _parent?.ConditionPosition ?? throw new InvalidOperationException("A search's root is always a group.");

   /// <summary>This line and every line under it, in the order the tree shows them.</summary>
   public IEnumerable<SearchConditionNodeViewModel> ThisAndAllBelow() => [this, ..Children.SelectMany(it => it.ThisAndAllBelow())];

   /// <summary><paramref name="shownCondition"/>, or the condition inside the negations around it, with its position.</summary>
   static (NoteCondition Condition, ConditionPosition Position) TheConditionInsideAnyNegation(NoteCondition shownCondition, ConditionPosition position)
   {
      while(shownCondition is NegatedCondition negated)
      {
         shownCondition = negated.Condition;
         position = position.Child(0);
      }

      return (shownCondition, position);
   }
}
