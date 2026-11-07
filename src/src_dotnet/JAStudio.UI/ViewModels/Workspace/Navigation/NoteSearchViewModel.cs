using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Editing;
using JAStudio.Core.Note.Search.Grouping;
using JAStudio.Core.Note.Search.NoteTypeProperties;
using JAStudio.Core.Note.Search.TextProperties;
using JAStudio.Core.Note.Search.TrueOrFalseProperties;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Navigation.Search;
using JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;
using JAStudio.UI.Views.Workspace.Navigation;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// A workspace's note search, the "Search" tab of its <see cref="NavigationPaneViewModel"/> (<see cref="NoteSearchView"/>):<br/>
/// the search being built (an <see cref="EditableSearch"/>), shown as a tree of <see cref="Nodes"/>, and the notes it finds, which open in the workspace's tabs.
/// </summary>
/// <remarks>
/// Every change to the search runs it again, in the background. The operations act on the <see cref="SelectedNode"/>,<br/>
/// and the view calls them from its keys and menus. See <c>dev_docs/note-search.md</c>.
/// </remarks>
/// <remarks>
/// Between changes, the results keep showing the same notes, kept up to date, even when the search would no longer find them.<br/>
/// <see cref="RefreshCommand"/>, on F5, runs the same search again.
/// </remarks>
partial class NoteSearchViewModel : ObservableObject
{
   const string HowToSearch = "+ adds a condition or a group, or type its key from the + menu, such as s for searchable text or t for tags. "
                            + "- negates, Del removes, Enter or a double-click changes, Ctrl+Z undoes. Right-click for more.";

   readonly JPCollection _collection;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly EditableSearch _search = new();
   int _searchesStarted;

   public NoteSearchViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services)
   {
      _collection = services.CoreApp.Collection;
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      Results = new SearchResultsViewModel(workspace, services);
      ShowTheSearch(ConditionPosition.Root);
   }

   /// <summary>The notes the search found.</summary>
   public SearchResultsViewModel Results { get; }

   /// <summary>The condition tree's lines: just the root, with every other line under it.</summary>
   [ObservableProperty] IReadOnlyList<SearchConditionNodeViewModel> _nodes = [];

   /// <summary>The selected line, which the keys and the right-click menu act on. The tree sets it when a line is clicked.</summary>
   [ObservableProperty] SearchConditionNodeViewModel? _selectedNode;

   [ObservableProperty] string _statusText = HowToSearch;

   [ObservableProperty] bool _isSearching;

   SearchConditionNodeViewModel Root => Nodes[0];

   /// <summary>The line the operations act on: the selected one, or the root when none is.</summary>
   SearchConditionNodeViewModel Selected => SelectedNode ?? Root;

   /// <summary>An editor filled in with the selected line's condition, or null when the line is a group, which has no editor.</summary>
   internal ConditionEditorViewModel? EditorForTheSelectedCondition() =>
      Selected.IsGroup ? null : ConditionEditorViewModel.ForChanging(Selected.Condition, _collection);

   /// <summary>Opens <paramref name="editor"/>, for a new condition, through <paramref name="editInADialog"/>, and adds the condition it returns, if any.</summary>
   internal void AddAConditionThroughAnEditor(ConditionEditorViewModel editor, Func<ConditionEditorViewModel, Task<NoteCondition?>> editInADialog) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await editInADialog(editor) is {} condition)
            AddCondition(condition);
      });

   /// <summary>
   /// Opens an editor filled in with the selected line's condition through <paramref name="editInADialog"/>, and puts the condition it returns, if any, in its place.<br/>
   /// Does nothing when the selected line is a group, which has no editor.
   /// </summary>
   internal void ChangeTheSelectedConditionThroughAnEditor(Func<ConditionEditorViewModel, Task<NoteCondition?>> editInADialog)
   {
      if(EditorForTheSelectedCondition() is not {} editor) return;

      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await editInADialog(editor) is {} changed)
            ChangeTheSelectedCondition(changed);
      });
   }

   /// <summary>Adds <paramref name="condition"/> to the selected group, or to the group the selected condition is in, and selects it.</summary>
   internal void AddCondition(NoteCondition condition) => ChangeTheSearch(_search.AddCondition(Selected.GroupToAddTo, condition));

   /// <summary>Replaces the selected line's condition with <paramref name="replacement"/>, keeping any negation around it.</summary>
   internal void ChangeTheSelectedCondition(NoteCondition replacement)
   {
      var selected = Selected;
      _search.ReplaceCondition(selected.ConditionPosition, replacement);
      ChangeTheSearch(selected.Position);
   }

   /// <summary>Removes the selected line, and a group with everything in it. The root can't be removed, so removing it does nothing.</summary>
   internal void RemoveTheSelectedNode()
   {
      if(Selected.IsRoot) return;

      var removed = Selected.Position;
      _search.RemoveCondition(removed);
      ChangeTheSearch(PositionToSelectAfterRemoving(removed));
   }

   /// <summary>Negates the selected line's condition, or takes its negation away.</summary>
   internal void ToggleNegationOfTheSelectedNode()
   {
      var selected = Selected;
      _search.ToggleNegation(selected.Position);
      ChangeTheSearch(selected.Position);
   }

   /// <summary>Switches the selected group between "all of" and "any of". Does nothing when the selected line isn't a group.</summary>
   internal void SwitchTheSelectedGroup()
   {
      var selected = Selected;
      if(!selected.IsGroup) return;

      _search.SwitchGroup(selected.ConditionPosition);
      ChangeTheSearch(selected.Position);
   }

   /// <summary>Undoes the last change to the search, if there is one.</summary>
   internal void Undo()
   {
      if(!_search.CanUndo) return;

      var selected = Selected;
      _search.Undo();
      ChangeTheSearch(selected.Position);
   }

   /// <summary>
   /// The add menu: the properties a condition can be on, and the groups, each picked by a single key, such as t for tags.<br/>
   /// A text property opens a submenu of the ways to compare its text, so s then c starts a "searchable text contains" condition in its editor.<br/>
   /// A property with only a few values opens a submenu of them that adds the condition at once, with no editor, so n then k adds "Note type is KanjiNote".
   /// </summary>
   /// <param name="addAConditionThrough">Opens the editor for the picked condition, and adds the condition it returns.</param>
   internal List<SpecMenuItem> AddConditionMenu(Action<ConditionEditorViewModel> addAConditionThrough)
   {
      return
      [
         TextPropertySubmenu('s', NoteProperties.SearchableText),
         TextPropertySubmenu('q', NoteProperties.Question),
         TextPropertySubmenu('a', NoteProperties.Answer),
         Command('t', NoteProperties.Tags.Name, () => addAConditionThrough(ConditionEditorViewModel.ForANewTagCondition(NoteProperties.Tags, _collection))),
         NoteTypeSubmenu('n', NoteProperties.NoteType),
         TrueOrFalseSubmenu('r', NoteProperties.StudyingReading),
         TrueOrFalseSubmenu('l', NoteProperties.StudyingListening),
         Submenu('g', "Group",
         [
            Command('a', "All of", () => AddCondition(ConditionGroup.AllOf())),
            Command('o', "Any of", () => AddCondition(ConditionGroup.AnyOf()))
         ])
      ];

      SpecMenuItem TextPropertySubmenu(char key, TextProperty property) =>
         Submenu(key, property.Name, [..TextMatchChoice.All.Select(choice => Command(choice.MenuKey, choice.Name, () => addAConditionThrough(ConditionEditorViewModel.ForANewTextCondition(property, choice))))]);

      SpecMenuItem NoteTypeSubmenu(char key, NoteTypeProperty property) =>
         Submenu(key, property.Name, [..NoteTypeChoice.AllFor(property).Select(choice => Command(choice.MenuKey, choice.Name, () => AddCondition(choice.Condition)))]);

      SpecMenuItem TrueOrFalseSubmenu(char key, TrueOrFalseProperty property) =>
         Submenu(key, property.Name, [Command('t', "is true", () => AddCondition(property.Is(true))), Command('f', "is false", () => AddCondition(property.Is(false)))]);

      static SpecMenuItem Command(char key, string text, Action action) => SpecMenuItem.Command(ShortcutFinger.Mnemonic(key, text), action, acceleratorKey: key);

      static SpecMenuItem Submenu(char key, string text, IReadOnlyList<SpecMenuItem> children) => SpecMenuItem.Submenu(ShortcutFinger.Mnemonic(key, text), children, acceleratorKey: key);
   }

   /// <summary>
   /// The item of <paramref name="menu"/>, such as the <see cref="AddConditionMenu"/>, that <paramref name="key"/> picks, ignoring case,<br/>
   /// or null when no item has that key. Typing a key on the condition tree does what typing it in the add menu would,<br/>
   /// so t opens the tag dialog, and q opens the add menu with the question's submenu open.
   /// </summary>
   internal static SpecMenuItem? ItemPickedBy(char key, IEnumerable<SpecMenuItem> menu) =>
      menu.FirstOrDefault(it => it.AcceleratorKey is {} itemKey && char.ToLowerInvariant(itemKey) == char.ToLowerInvariant(key));

   /// <summary>The selected line's right-click menu: everything the editor can do to it, each picked by a single key.</summary>
   /// <param name="openTheAddMenu">Opens the <see cref="AddConditionMenu"/>.</param>
   /// <param name="changeTheSelectedCondition">Opens the editor filled in with the selected line's condition.</param>
   internal List<SpecMenuItem> SelectedNodeMenu(Action openTheAddMenu, Action changeTheSelectedCondition) =>
   [
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('a', "Add a condition (+)"), openTheAddMenu),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('c', "Change (Enter, double-click)"), changeTheSelectedCondition, enabled: !Selected.IsGroup),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('n', Selected.IsNegated ? "Remove the negation (-)" : "Negate (-)"), ToggleNegationOfTheSelectedNode),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('s', Selected.Condition is AnyOfGroup ? "Switch to all of" : "Switch to any of"), SwitchTheSelectedGroup, enabled: Selected.IsGroup),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('r', "Remove (Del)"), RemoveTheSelectedNode, enabled: !Selected.IsRoot),
      SpecMenuItem.Separator(),
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('u', "Undo (Ctrl+Z)"), Undo, enabled: _search.CanUndo)
   ];

   /// <summary>
   /// Runs the same search again, F5 or the Refresh button, so the results show the notes it finds now.<br/>
   /// The rows of the notes still found stay, where they are and selected if they were, the rows of the notes no longer found go, and the newly found notes get rows.
   /// </summary>
   [RelayCommand]
   void Refresh() => _fatalErrorHandler.StartAndHandleExceptionsFrom(() => RunTheSearchAsync(Results.ShowWhatTheRefreshedSearchFound));

   void ChangeTheSearch(ConditionPosition positionToSelect)
   {
      ShowTheSearch(positionToSelect);
      _fatalErrorHandler.StartAndHandleExceptionsFrom(() => RunTheSearchAsync(Results.Show));
   }

   void ShowTheSearch(ConditionPosition positionToSelect)
   {
      Nodes = [new SearchConditionNodeViewModel(_search.Current, ConditionPosition.Root, parent: null)];
      SelectedNode = Root.ThisAndAllBelow().FirstOrDefault(it => it.Position == positionToSelect || it.ConditionPosition == positionToSelect) ?? Root;
   }

   /// <summary>The removed line's next sibling, which now has its position, or else its previous sibling, or else its group.</summary>
   ConditionPosition PositionToSelectAfterRemoving(ConditionPosition removed)
   {
      var group = _search.Current.ConditionAt(removed.Parent);
      return removed.IndexInParent < group.Children.Count ? removed
             : removed.IndexInParent > 0                  ? removed.Parent.Child(removed.IndexInParent - 1)
                                                          : removed.Parent;
   }

   /// <summary>Runs the search in the background, and shows the notes it found with <paramref name="showTheNotesFound"/>, unless a later run has started meanwhile.</summary>
   async Task RunTheSearchAsync(Action<IReadOnlyList<JPNote>> showTheNotesFound)
   {
      var search = _search.Current;
      var thisSearch = ++_searchesStarted;
      if(search.IsEmpty)
      {
         showTheNotesFound([]);
         StatusText = HowToSearch;
         IsSearching = false;
         return;
      }

      IsSearching = true;
      StatusText = "Searching...";
      var found = await TaskCE.Run(() => NotesFoundBy(search));
      if(ALaterSearchHasStarted(thisSearch)) return;

      showTheNotesFound(found);
      StatusText = found.Count switch
      {
         0 => "No notes found",
         1 => "1 note found",
         _ => $"{found.Count} notes found"
      };
      IsSearching = false;
   }

   /// <summary>Whether a change or a refresh made after the run numbered <paramref name="searchNumber"/> started started another, whose results replace its results.</summary>
   bool ALaterSearchHasStarted(int searchNumber) => searchNumber != _searchesStarted;

   /// <summary>The notes <paramref name="search"/> finds, in no particular order: the results grid sorts them.</summary>
   List<JPNote> NotesFoundBy(NoteCondition search) => [.._collection.NotesMatching(search)];
}
