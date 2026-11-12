using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace.Editors.MatchConditions;

namespace JAStudio.UI.ViewModels.Workspace.Editors.MatchConditions;

/// <summary>
/// The tree editor of a vocab's match-condition, on its Matching tab (<see cref="MatchConditionEditor"/>): the match-condition being edited (an<br/>
/// <see cref="EditableMatchCondition"/>), shown as a tree of <see cref="Lines"/>, and written to the vocab after every change.
/// </summary>
/// <remarks>
/// It's edited as the note search's tree is: the operations act on the <see cref="SelectedLine"/>, and the view calls them from its keys and menus.<br/>
/// The add menu offers the conditions there are where a condition goes: on a word at the root, and on a token inside <c>PrecedingToken.AnyOf(</c>.
/// </remarks>
partial class MatchConditionEditorViewModel : ObservableObject
{
   const string HowToEditTheTree = "+ adds a condition or a group, or type its key from the + menu, such as p for PrecedingToken. "
                                 + "- negates, Del removes, Enter or a double-click changes the texts, Ctrl+Z undoes. Right-click for more.";

   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<VocabNote> _currentVocab;
   readonly Action _changeMade;
   readonly EditableMatchCondition _matchCondition;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while a condition's dialog is open.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   /// <param name="changeMade">Called after every change the tree editor writes to the vocab.</param>
   public MatchConditionEditorViewModel(IFatalErrorHandler fatalErrorHandler, Func<VocabNote> currentVocab, Action changeMade)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _currentVocab = currentVocab;
      _changeMade = changeMade;
      _matchCondition = new EditableMatchCondition(VocabsMatchCondition);
      ShowTheMatchCondition(ConditionPosition.Root);
   }

   /// <summary>The tree's lines: just the root, with every other line under it.</summary>
   [ObservableProperty] IReadOnlyList<MatchConditionLineViewModel> _lines = [];

   /// <summary>The selected line, which the keys and the right-click menu act on. The tree sets it when a line is clicked.</summary>
   [ObservableProperty] MatchConditionLineViewModel? _selectedLine;

   /// <summary>
   /// Raised after every change the tree editor made, with the tree shown anew, but not after one made elsewhere, such as by a rule's editor: the view<br/>
   /// puts the keyboard back on the selected line then, so that the next key acts on it.
   /// </summary>
   public event Action? ChangedByTheTreeEditor;

   /// <summary>How to edit the tree, shown under it.</summary>
   public string HowToEdit => HowToEditTheTree;

   MatchConditionLineViewModel Root => Lines[0];

   /// <summary>The line the operations act on: the selected one, or the root when none is.</summary>
   MatchConditionLineViewModel Selected => SelectedLine ?? Root;

   AllOfGroup<CandidateWordVariant> VocabsMatchCondition => _currentVocab().MatchingConfiguration.MatchCondition;

   /// <summary>
   /// Shows the vocab's match-condition as it is now, when something else may have changed it, such as a rule edited rule by rule on the same tab:<br/>
   /// a change like the tree editor's own, which undo takes back.
   /// </summary>
   public void Refresh()
   {
      if(VocabsMatchCondition == _matchCondition.Current) return;

      var selected = Selected.Position;
      _matchCondition.ChangedElsewhere(VocabsMatchCondition);
      ShowTheMatchCondition(selected);
   }

   /// <summary>Adds <paramref name="condition"/> to the selected group, or to the group the selected line is in, and selects it.</summary>
   internal void AddCondition(Condition condition) => ChangeTheMatchCondition(_matchCondition.AddCondition(Selected.GroupToAddTo.ConditionPosition, condition));

   /// <summary>Opens <paramref name="editor"/>, for a new condition, through <paramref name="editInADialog"/>, and adds the condition it returns, if any.</summary>
   internal void AddAConditionThroughAnEditor(ConditionTextsEditorViewModel editor, Func<ConditionTextsEditorViewModel, Task<Condition?>> editInADialog) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await editInADialog(editor) is {} condition)
            AddCondition(condition);
      });

   /// <summary>An editor filled in with the texts of the selected line's condition, or null when its condition takes no texts, as a fact or a group doesn't.</summary>
   internal ConditionTextsEditorViewModel? EditorForTheSelectedCondition()
   {
      var selected = Selected;
      return selected.Level.TestsWithTexts.FirstOrDefault(it => it.Made(selected.Condition)) is {} test
                ? ConditionTextsEditorViewModel.ForChanging(selected.Condition, test, title: selected.CodeOnOneLine)
                : null;
   }

   /// <summary>
   /// Opens an editor filled in with the texts of the selected line's condition through <paramref name="editInADialog"/>, and puts the condition it returns,<br/>
   /// if any, in its place, inside the negations and conditions on subjects around it. Does nothing when the condition takes no texts.
   /// </summary>
   internal void ChangeTheSelectedConditionThroughAnEditor(Func<ConditionTextsEditorViewModel, Task<Condition?>> editInADialog)
   {
      if(EditorForTheSelectedCondition() is not {} editor) return;

      var selected = Selected;
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await editInADialog(editor) is not {} changed) return;

         _matchCondition.ReplaceCondition(selected.ConditionPosition, changed);
         ChangeTheMatchCondition(selected.Position);
      });
   }

   /// <summary>Removes the selected line, and a group with everything in it. The root can't be removed, so removing it does nothing.</summary>
   internal void RemoveTheSelectedLine()
   {
      if(Selected.IsRoot) return;

      var removed = Selected.Position;
      _matchCondition.RemoveCondition(removed);
      ChangeTheMatchCondition(PositionToSelectAfterRemoving(removed));
   }

   /// <summary>Negates the selected line's whole condition, or takes its negation away. The root is never negated, so negating it does nothing.</summary>
   internal void ToggleNegationOfTheSelectedLine()
   {
      if(Selected.IsRoot) return;

      var selected = Selected.Position;
      _matchCondition.ToggleNegation(selected);
      ChangeTheMatchCondition(selected);
   }

   /// <summary>Switches the selected group between "all of" and "any of". Does nothing when the selected line isn't a group, or is the root, which is always "all of".</summary>
   internal void SwitchTheSelectedGroup()
   {
      if(!CanSwitchTheSelectedGroup) return;

      var selected = Selected;
      _matchCondition.SwitchGroup(selected.ConditionPosition);
      ChangeTheMatchCondition(selected.Position);
   }

   /// <summary>Undoes the last change to the match-condition, if there is one.</summary>
   internal void Undo()
   {
      if(!_matchCondition.CanUndo) return;

      var selected = Selected.Position;
      _matchCondition.Undo();
      ChangeTheMatchCondition(selected);
   }

   /// <summary>
   /// The add menu: the conditions there are where a condition added on the selected line goes, each picked by a single key. Under a subject,<br/>
   /// such as <c>PrecedingToken</c>, the conditions on it, so p, f, g adds <c>PrecedingToken.IsAGodanVerb</c>. A condition that takes texts,<br/>
   /// such as <c>EndsWithOneOf</c>, opens its editor.
   /// </summary>
   /// <param name="addAConditionThrough">Opens the editor for the picked condition, and adds the condition it returns.</param>
   internal List<SpecMenuItem> AddConditionMenu(Action<ConditionTextsEditorViewModel> addAConditionThrough)
   {
      return TheConditionsOn(Selected.GroupToAddTo.Level, placed: it => it, subjectsSoFar: "");

      List<SpecMenuItem> TheConditionsOn(IConditionLevel level, Func<Condition, Condition> placed, string subjectsSoFar)
      {
         return AutomaticMnemonics.WithKeysPicked(
         [
            ..TheFactsSubmenuIfThereAreAny(),
            SpecMenuItem.Submenu("Group", TheGroups()),
            ..level.Subjects.Select(TheConditionsOnTheSubject),
            ..level.TestsWithTexts.Select(TheEditorFor)
         ]);

         IEnumerable<SpecMenuItem> TheFactsSubmenuIfThereAreAny() => level.Facts.Count > 0 ? [SpecMenuItem.Submenu("Fact", TheFacts())] : [];

         List<SpecMenuItem> TheFacts() => AutomaticMnemonics.WithKeysPicked([..level.Facts.Select(fact => SpecMenuItem.Command(fact.ToString(), () => AddCondition(placed(fact))))]);

         List<SpecMenuItem> TheGroups() =>
         [
            SpecMenuItem.Command(ShortcutFinger.Mnemonic('a', "All of"), () => AddCondition(placed(level.NewAllOfGroup())), 'a'),
            SpecMenuItem.Command(ShortcutFinger.Mnemonic('o', "Any of"), () => AddCondition(placed(level.NewAnyOfGroup())), 'o')
         ];

         SpecMenuItem TheConditionsOnTheSubject(IConditionSubject subject) =>
            SpecMenuItem.Submenu(subject.Name, TheConditionsOn(subject.Level, it => placed(subject.Meets(it)), $"{subjectsSoFar}{subject.Name}."));

         SpecMenuItem TheEditorFor(ConditionTestWithTexts test) =>
            SpecMenuItem.Command(test.Name, () => addAConditionThrough(ConditionTextsEditorViewModel.ForANewCondition(subjectsSoFar + test.Name, test, placed)));
      }
   }

   /// <summary>The selected line's right-click menu: everything the editor can do to it, each picked by a single key.</summary>
   /// <param name="openTheAddMenu">Opens the <see cref="AddConditionMenu"/>.</param>
   /// <param name="changeTheSelectedCondition">Opens the editor filled in with the texts of the selected line's condition.</param>
   internal List<SpecMenuItem> SelectedLineMenu(Action openTheAddMenu, Action changeTheSelectedCondition) =>
   [
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('a', "Add a condition (+)"), openTheAddMenu, 'a'),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('c', "Change the texts (Enter, double-click)"), changeTheSelectedCondition, 'c', enabled: EditorForTheSelectedCondition() is not null),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('n', Selected.IsNegated ? "Remove the negation (-)" : "Negate (-)"), ToggleNegationOfTheSelectedLine, 'n', enabled: !Selected.IsRoot),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('s', Selected.IsAnAnyOfGroup ? "Switch to all of" : "Switch to any of"), SwitchTheSelectedGroup, 's',
                           enabled: CanSwitchTheSelectedGroup),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('r', "Remove (Del)"), RemoveTheSelectedLine, 'r', enabled: !Selected.IsRoot),
      SpecMenuItem.Separator(),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('u', "Undo (Ctrl+Z)"), Undo, 'u', enabled: _matchCondition.CanUndo)
   ];

   /// <summary>Whether the selected line is a group that can be switched between "all of" and "any of": any but the root, which is always "all of".</summary>
   bool CanSwitchTheSelectedGroup => Selected is { IsGroup: true, IsRoot: false };

   /// <summary>Writes the changed match-condition to the vocab, shows it with the line at <paramref name="positionToSelect"/> selected, and says a change was made.</summary>
   void ChangeTheMatchCondition(ConditionPosition positionToSelect)
   {
      _currentVocab().MatchingConfiguration.SetMatchCondition(_matchCondition.Current);
      ShowTheMatchCondition(positionToSelect);
      ChangedByTheTreeEditor?.Invoke();
      _changeMade();
   }

   void ShowTheMatchCondition(ConditionPosition positionToSelect)
   {
      Lines = [MatchConditionLineViewModel.RootOf(_matchCondition)];
      SelectedLine = Root.ThisAndAllBelow().FirstOrDefault(it => it.IsAt(positionToSelect)) ?? Root;
   }

   /// <summary>The removed line's next sibling, which now has its position, or else its previous sibling, or else its group.</summary>
   ConditionPosition PositionToSelectAfterRemoving(ConditionPosition removed)
   {
      var group = _matchCondition.Current.ConditionAt(removed.Parent);
      return removed.IndexInParent < group.Children.Count ? removed
             : removed.IndexInParent > 0                  ? removed.Parent.Child(removed.IndexInParent - 1)
                                                          : removed.Parent;
   }
}
