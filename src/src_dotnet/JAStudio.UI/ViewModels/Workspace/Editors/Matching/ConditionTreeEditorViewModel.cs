using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// The tree editor (<see cref="ConditionTreeEditor"/>) of a vocab's match-condition, on its Matching tab, or of a shared-condition's definition, in its<br/>
/// window: the condition being edited (an <see cref="EditableCondition"/>), shown as a tree of <see cref="Lines"/>, and stored where it's kept<br/>
/// (<see cref="IEditedCondition"/>) after every change.
/// </summary>
/// <remarks>
/// It's edited as the note search's tree is: the operations act on the <see cref="SelectedLine"/>, and the view calls them from its keys and menus.<br/>
/// The add menu offers the conditions there are where a condition goes: on a word at the root of a match-condition, and on a token inside<br/>
/// <c>PrecedingToken.AnyOf(</c>.
/// </remarks>
partial class ConditionTreeEditorViewModel : ObservableObject
{
   const string HowToEditTheTree = "+ adds a condition or a group, or type its key from the + menu, such as p for PrecedingToken. "
                                 + "- negates, Del removes, Enter or a double-click changes the texts, Ctrl+Z undoes. "
                                 + "F12 or Ctrl+click opens a shared-condition's definition. Right-click for more.";

   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly IEditedCondition _edited;
   readonly NoteServices _noteServices;
   readonly Action _changeMade;
   readonly Action<ISharedCondition> _openTheDefinitionOf;
   readonly EditableCondition _tree;

   /// <summary>The code of the tree as the <see cref="Lines"/> show it: when a shared-condition it uses is renamed, the code changes though the tree doesn't.</summary>
   string _shownCode = "";

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while a condition's dialog is open.</param>
   /// <param name="edited">The condition edited, and where each change is stored.</param>
   /// <param name="noteServices">The corpus's notes and shared-conditions, which making a shared-condition of a line changes.</param>
   /// <param name="changeMade">Called after every change the tree editor stores.</param>
   /// <param name="openTheDefinitionOf">Opens a shared-condition's window, where its definition is edited.</param>
   public ConditionTreeEditorViewModel(IFatalErrorHandler fatalErrorHandler,
                                       IEditedCondition edited,
                                       NoteServices noteServices,
                                       Action changeMade,
                                       Action<ISharedCondition> openTheDefinitionOf)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _edited = edited;
      _noteServices = noteServices;
      _changeMade = changeMade;
      _openTheDefinitionOf = openTheDefinitionOf;
      _tree = new EditableCondition(edited.Stored, edited.Level);
      ShowTheTree(ConditionPosition.Root);
   }

   /// <summary>The tree's lines: just the root, with every other line under it.</summary>
   [ObservableProperty] IReadOnlyList<ConditionTreeLineViewModel> _lines = [];

   /// <summary>The selected line, which the keys and the right-click menu act on. The tree sets it when a line is clicked.</summary>
   [ObservableProperty] ConditionTreeLineViewModel? _selectedLine;

   /// <summary>
   /// Raised after every change the tree editor made, with the tree shown anew, but not after one made elsewhere, such as by a rule's editor: the view<br/>
   /// puts the keyboard back on the selected line then, so that the next key acts on it.
   /// </summary>
   public event Action? ChangedByTheTreeEditor;

   /// <summary>How to edit the tree, shown under it.</summary>
   public string HowToEdit => HowToEditTheTree;

   ConditionTreeLineViewModel Root => Lines[0];

   /// <summary>The line the operations act on: the selected one, or the root when none is.</summary>
   ConditionTreeLineViewModel Selected => SelectedLine ?? Root;

   /// <summary>
   /// Shows the condition as it's stored now, when something else may have changed it, such as a rule edited rule by rule on the same tab: a change<br/>
   /// like the tree editor's own, which undo takes back. And shows it anew when only its code changed, as when a shared-condition it uses was renamed.
   /// </summary>
   public void Refresh()
   {
      var selected = Selected.Position;
      if(_edited.Stored != _tree.Current) _tree.ChangedElsewhere(_edited.Stored);
      else if(CodeOfTheTree() == _shownCode) return;

      ShowTheTree(selected);
   }

   /// <summary>Adds <paramref name="condition"/> to the selected group, or to the group the selected line is in, and selects it.</summary>
   internal void AddCondition(Condition condition) => StoreTheChangedTree(_tree.AddCondition(Selected.GroupToAddTo.ConditionPosition, condition));

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

         _tree.ReplaceCondition(selected.ConditionPosition, changed);
         StoreTheChangedTree(selected.Position);
      });
   }

   /// <summary>Removes the selected line, and a group with everything in it. The root can't be removed, so removing it does nothing.</summary>
   internal void RemoveTheSelectedLine()
   {
      if(Selected.IsRoot) return;

      var removed = Selected.Position;
      _tree.RemoveCondition(removed);
      StoreTheChangedTree(PositionToSelectAfterRemoving(removed));
   }

   /// <summary>Negates the selected line's whole condition, or takes its negation away. The root is never negated, so negating it does nothing.</summary>
   internal void ToggleNegationOfTheSelectedLine()
   {
      if(Selected.IsRoot) return;

      var selected = Selected.Position;
      _tree.ToggleNegation(selected);
      StoreTheChangedTree(selected);
   }

   /// <summary>Switches the selected group between "all of" and "any of". Does nothing when the selected line isn't a group, or is the root, which is always "all of".</summary>
   internal void SwitchTheSelectedGroup()
   {
      if(!CanSwitchTheSelectedGroup) return;

      var selected = Selected;
      _tree.SwitchGroup(selected.ConditionPosition);
      StoreTheChangedTree(selected.Position);
   }

   /// <summary>Undoes the last change to the condition, if there is one.</summary>
   internal void Undo()
   {
      if(!_tree.CanUndo) return;

      var selected = Selected.Position;
      _tree.Undo();
      StoreTheChangedTree(selected);
   }

   /// <summary>
   /// The dialog that makes a shared-condition of the selected line, offering the conditions on it worth naming<br/>
   /// (<see cref="EditableCondition.ConditionsToMakeASharedConditionOfAt"/>), or null when there are none, as on a line that's a name already.
   /// </summary>
   internal NewSharedConditionDialogViewModel? DialogMakingASharedConditionOfTheSelectedLine() =>
      _tree.ConditionsToMakeASharedConditionOfAt(Selected.Position) is { Count: > 0 } definitions ? new NewSharedConditionDialogViewModel(definitions, _noteServices.SharedConditions) : null;

   /// <summary>
   /// Opens the dialog that makes a shared-condition of the selected line through <paramref name="inADialog"/>, then makes the shared-condition it closes<br/>
   /// with, if any, and uses it wherever its definition is, in every vocab and definition (<see cref="SharedConditions.MakeOf"/>): the line too. Does nothing<br/>
   /// when the line has no condition to make one of.
   /// </summary>
   internal void MakeASharedConditionOfTheSelectedLineThroughADialog(Func<NewSharedConditionDialogViewModel, Task<NewSharedCondition?>> inADialog)
   {
      if(DialogMakingASharedConditionOfTheSelectedLine() is not {} dialog) return;

      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await inADialog(dialog) is not {} toMake) return;

         _noteServices.SharedConditions.MakeOf(toMake.Definition, toMake.Name, _noteServices.Collection.Vocab.All());
         Refresh();
      });
   }

   /// <summary>Opens the window of the shared-condition the selected line is, where its definition is edited. Does nothing when the line isn't one.</summary>
   internal void OpenTheDefinitionOfTheSelectedLine()
   {
      if(Selected.SharedCondition is {} sharedCondition)
         _openTheDefinitionOf(sharedCondition);
   }

   /// <summary>
   /// The add menu: the conditions there are where a condition added on the selected line goes, each picked by a single key. Under a subject,<br/>
   /// such as <c>PrecedingToken</c>, the conditions on it, so p, f, g adds <c>PrecedingToken.IsAGodanVerb</c>. A condition that takes texts,<br/>
   /// such as <c>EndsWithOneOf</c>, opens its editor. The corpus's shared-conditions on the subject are a submenu of their own.
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
            ..TheSharedConditionsSubmenuIfThereAreAny(),
            SpecMenuItem.Submenu("Group", TheGroups()),
            ..level.Subjects.Select(TheConditionsOnTheSubject),
            ..level.TestsWithTexts.Select(TheEditorFor)
         ]);

         IEnumerable<SpecMenuItem> TheFactsSubmenuIfThereAreAny() => level.Facts.Count > 0 ? [SpecMenuItem.Submenu("Fact", TheFacts())] : [];

         List<SpecMenuItem> TheFacts() => AutomaticMnemonics.WithKeysPicked([..level.Facts.Select(fact => SpecMenuItem.Command(fact.ToString(), () => AddCondition(placed(fact))))]);

         IEnumerable<SpecMenuItem> TheSharedConditionsSubmenuIfThereAreAny() =>
            _edited.SharedConditionsOfferedOn(level) is { Count: > 0 } sharedConditions ? [SpecMenuItem.Submenu("Shared", TheSharedConditions(sharedConditions))] : [];

         List<SpecMenuItem> TheSharedConditions(IReadOnlyList<ISharedCondition> sharedConditions) =>
            AutomaticMnemonics.WithKeysPicked([..sharedConditions.Select(it => SpecMenuItem.Command(it.Name, () => AddCondition(placed(it.AsACondition))))]);

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
   /// <param name="makeASharedConditionOfTheSelectedLine">Opens the dialog that makes a shared-condition of the selected line.</param>
   internal List<SpecMenuItem> SelectedLineMenu(Action openTheAddMenu, Action changeTheSelectedCondition, Action makeASharedConditionOfTheSelectedLine) =>
   [
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('a', "Add a condition (+)"), openTheAddMenu, 'a'),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('c', "Change the texts (Enter, double-click)"), changeTheSelectedCondition, 'c', enabled: EditorForTheSelectedCondition() is not null),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('d', "Open its definition (F12, Ctrl+click)"), OpenTheDefinitionOfTheSelectedLine, 'd', enabled: Selected.IsASharedCondition),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('m', "Make a shared-condition of it…"), makeASharedConditionOfTheSelectedLine, 'm',
                           enabled: DialogMakingASharedConditionOfTheSelectedLine() is not null),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('n', Selected.IsNegated ? "Remove the negation (-)" : "Negate (-)"), ToggleNegationOfTheSelectedLine, 'n', enabled: !Selected.IsRoot),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('s', Selected.IsAnAnyOfGroup ? "Switch to all of" : "Switch to any of"), SwitchTheSelectedGroup, 's',
                           enabled: CanSwitchTheSelectedGroup),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('r', "Remove (Del)"), RemoveTheSelectedLine, 'r', enabled: !Selected.IsRoot),
      SpecMenuItem.Separator(),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('u', "Undo (Ctrl+Z)"), Undo, 'u', enabled: _tree.CanUndo)
   ];

   /// <summary>Whether the selected line is a group that can be switched between "all of" and "any of": any but the root, which is always "all of".</summary>
   bool CanSwitchTheSelectedGroup => Selected is { IsGroup: true, IsRoot: false };

   /// <summary>Stores the changed tree where the condition is kept, shows it with the line at <paramref name="positionToSelect"/> selected, and says a change was made.</summary>
   void StoreTheChangedTree(ConditionPosition positionToSelect)
   {
      _edited.Store(_tree.Current);
      ShowTheTree(positionToSelect);
      ChangedByTheTreeEditor?.Invoke();
      _changeMade();
   }

   void ShowTheTree(ConditionPosition positionToSelect)
   {
      _shownCode = CodeOfTheTree();
      Lines = [ConditionTreeLineViewModel.RootOf(_tree, _edited)];
      SelectedLine = Root.ThisAndAllBelow().FirstOrDefault(it => it.IsAt(positionToSelect)) ?? Root;
   }

   string CodeOfTheTree() => ConditionCode.AsText(_edited.LinesOf(_tree.Current));

   /// <summary>The removed line's next sibling, which now has its position, or else its previous sibling, or else its group.</summary>
   ConditionPosition PositionToSelectAfterRemoving(ConditionPosition removed)
   {
      var group = _tree.Current.ConditionAt(removed.Parent);
      return removed.IndexInParent < group.Children.Count ? removed
             : removed.IndexInParent > 0                  ? removed.Parent.Child(removed.IndexInParent - 1)
                                                          : removed.Parent;
   }
}
