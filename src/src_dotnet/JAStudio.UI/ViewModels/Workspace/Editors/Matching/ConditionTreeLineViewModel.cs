using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// One line in the tree editor (<see cref="ConditionTreeEditorViewModel"/>): a condition, or a group with its conditions under it, written as code<br/>
/// (<see cref="ConditionCode"/>). Negations and conditions on a subject aren't lines of their own: they're shown in front of what they hold, on its<br/>
/// line, as in <c>!PrecedingToken.IsAGodanVerb</c> or <c>FollowingToken.AnyOf(</c>.
/// </summary>
class ConditionTreeLineViewModel
{
   readonly ConditionTreeLineViewModel? _parent;

   /// <param name="shownCondition">The condition shown on this line, with the negations and conditions on subjects around it.</param>
   /// <param name="position">Where <paramref name="shownCondition"/> is in the tree.</param>
   /// <param name="tree">The tree the line is in.</param>
   /// <param name="codeLines">The tree's code, by the position of the condition each line is of.</param>
   /// <param name="parent">The group this line is in, or null for the root.</param>
   ConditionTreeLineViewModel(Condition shownCondition,
                               ConditionPosition position,
                               EditableCondition tree,
                               ILookup<ConditionPosition, CodeLine> codeLines,
                               ConditionTreeLineViewModel? parent)
   {
      _parent = parent;
      Position = position;
      CodeLines = [.. codeLines[position]];
      CodeColumn = parent is { OpensWhatItHolds: true } ? parent.ColumnAfterTheParenthesis : 0;
      IsNegated = shownCondition is INegatedCondition;
      (Condition, ConditionPosition) = TheConditionInsideWhatHoldsOnlyIt(shownCondition, position);
      Level = tree.LevelAt(ConditionPosition);
      Children = Condition is IConditionGroup group
                    ? [..group.Conditions.Select((condition, index) => new ConditionTreeLineViewModel(condition, ConditionPosition.Child(index), tree, codeLines, this))]
                    : [];
   }

   /// <summary>
   /// The root line of <paramref name="tree"/>'s <see cref="EditableCondition.Current"/> tree, with every other line under it, its code written as<br/>
   /// <paramref name="edited"/> writes it.
   /// </summary>
   public static ConditionTreeLineViewModel RootOf(EditableCondition tree, IEditedCondition edited) =>
      new(tree.Current,
          ConditionPosition.Root,
          tree,
          edited.LinesOf(tree.Current).ToLookup(it => it.Position),
          parent: null);

   /// <summary>
   /// The line's code, such as <c>!PrecedingToken.IsAGodanVerb</c> or <c>AnyOf(</c>: one line of code, or several for a condition that lists values one to a<br/>
   /// line. The root's is the comment heading the tree, saying what it is (<see cref="IEditedCondition.LinesOf"/>).
   /// </summary>
   public IReadOnlyList<CodeLine> CodeLines { get; }

   /// <summary>
   /// The column, in characters of the code's monospace font, the line's code starts at in the tree: right after the parenthesis of the group it's in, as<br/>
   /// code is indented (<see cref="CodeLayout.ForTheTreeEditor"/>), or 0 for the root and its conditions, the statements under the heading.
   /// </summary>
   public int CodeColumn { get; }

   /// <summary>The line's code as text, as the tree shows it: starting at the <see cref="CodeColumn"/>, with its comment.</summary>
   public string Text => ConditionCode.AsText(CodeLines, CodeLayout.ForTheTreeEditor, CodeColumn);

   /// <summary>The line's code on one line, without its comments, such as <c>!LastToken.InflectedForm.In(連用タ接続, 連用形)</c>: what titles the editor changing it.</summary>
   public string CodeOnOneLine => ConditionCode.OnOneLine(CodeLines);

   /// <summary>
   /// The line's code as the tree shows it, starting at the <see cref="CodeColumn"/>, in pieces each shown as its kind should be, such as a negation in a<br/>
   /// color that stands out.
   /// </summary>
   public IReadOnlyList<CodeLineViewModel> ShownCode => CodeLineViewModel.LinesOf(CodeLines, CodeLayout.ForTheTreeEditor, CodeColumn);

   /// <summary>The lines of the conditions in this line's group. None unless <see cref="IsGroup"/>.</summary>
   public IReadOnlyList<ConditionTreeLineViewModel> Children { get; }

   /// <summary>Where the line's whole condition is, with the negations and conditions on subjects around it: what removing the line, or negating it, works on.</summary>
   public ConditionPosition Position { get; }

   /// <summary>The line's condition inside the negations and conditions on subjects around it: what changing the line changes, and a group's conditions go into.</summary>
   public Condition Condition { get; }

   /// <summary>Where <see cref="Condition"/> is in the tree.</summary>
   public ConditionPosition ConditionPosition { get; }

   /// <summary>The conditions there are on the subject of <see cref="Condition"/>: those the add menu offers in this line's group, if it is one.</summary>
   public IConditionLevel Level { get; }

   public bool IsGroup => Condition is IConditionGroup;

   /// <summary>The line's condition when it's a shared-condition, used by its name, such as <c>FollowsAPastTenseStem</c>: none when it isn't one.</summary>
   public ISharedCondition? SharedCondition => Condition as ISharedCondition;

   /// <summary>Whether the line's condition is a shared-condition, used by its name, such as <c>FollowsAPastTenseStem</c>, whose definition the line can show.</summary>
   public bool IsASharedCondition => SharedCondition is not null;

   /// <summary>
   /// The definition of the shared-condition the line is, as code in pieces, as it's read, which the line shows under it when expanded, indented under the<br/>
   /// line's code: none when it isn't one.
   /// </summary>
   public IReadOnlyList<CodeLineViewModel> ShownDefinitionOfTheSharedCondition =>
      SharedCondition is {} sharedCondition
         ? CodeLineViewModel.LinesOf(ConditionCode.LinesOf(sharedCondition.Definition), CodeLayout.ForReading, CodeColumn + IndentationOfAShownDefinition)
         : [];

   /// <summary>How many characters further in than the line's code the definition of the shared-condition it is shows, when expanded.</summary>
   const int IndentationOfAShownDefinition = 3;

   /// <summary>Whether the line opens the conditions under it with a parenthesis, as a group does, such as <c>PrecedingToken.AnyOf(</c>.</summary>
   bool OpensWhatItHolds => CodeLines is [{ OpensWhatFollows: true }];

   /// <summary>The column right after the line's parenthesis, where the conditions under it start, when it <see cref="OpensWhatItHolds"/>.</summary>
   int ColumnAfterTheParenthesis => CodeColumn + CodeLines[0].Code.Length;

   /// <summary>Whether the line is an "any of" group, so that switching it makes it an "all of" group.</summary>
   public bool IsAnAnyOfGroup => Condition is IConditionGroup { IsAnyOf: true };

   /// <summary>Whether the line's whole condition is a negation, so that negating it takes the negation away.</summary>
   public bool IsNegated { get; }

   public bool IsRoot => _parent is null;

   /// <summary>Whether the condition at <paramref name="position"/> is this line's: its whole condition (<see cref="Position"/>), or the <see cref="Condition"/> inside.</summary>
   public bool IsAt(ConditionPosition position) => Position == position || ConditionPosition == position;

   /// <summary>The group a condition added on this line goes into: this line's group, or the group this line is in.</summary>
   public ConditionTreeLineViewModel GroupToAddTo => IsGroup ? this : _parent ?? throw new InvalidOperationException("The tree's root is always a group.");

   /// <summary>This line and every line under it, in the order the tree shows them.</summary>
   public IEnumerable<ConditionTreeLineViewModel> ThisAndAllBelow() => [this, ..Children.SelectMany(it => it.ThisAndAllBelow())];

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
