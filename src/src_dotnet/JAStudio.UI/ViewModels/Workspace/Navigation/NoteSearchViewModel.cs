using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Editing;
using JAStudio.Core.Note.Search.Grouping;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Navigation.Search;
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
partial class NoteSearchViewModel : ObservableObject
{
   const string HowToSearch = "Type to search. + adds a condition, - negates one, Del removes one, Ctrl+Z undoes. Right-click for more.";

   readonly JPCollection _collection;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly EditableSearch _search = new();
   int _searchesStarted;

   public NoteSearchViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services)
   {
      _collection = services.CoreApp.Collection;
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      Results = new ListedNotesViewModel(workspace);
      ShowTheSearch(ConditionPosition.Root);
   }

   /// <summary>The notes the search found.</summary>
   public ListedNotesViewModel Results { get; }

   /// <summary>The condition tree's lines: just the root, with every other line under it.</summary>
   [ObservableProperty] IReadOnlyList<SearchConditionNodeViewModel> _nodes = [];

   /// <summary>The selected line, which the keys and the right-click menu act on. The tree sets it when a line is clicked.</summary>
   [ObservableProperty] SearchConditionNodeViewModel? _selectedNode;

   [ObservableProperty] string _statusText = HowToSearch;

   [ObservableProperty] bool _isSearching;

   SearchConditionNodeViewModel Root => Nodes[0];

   /// <summary>The line the operations act on: the selected one, or the root when none is.</summary>
   SearchConditionNodeViewModel Selected => SelectedNode ?? Root;

   /// <summary>An editor for a new condition on <paramref name="property"/>, with <paramref name="typedText"/> already typed into it if it takes text.</summary>
   internal ConditionEditorViewModel EditorForANewConditionOn(NoteProperty property, string typedText = "") =>
      ConditionEditorViewModel.ForANewConditionOn(property, _collection, typedText);

   /// <summary>An editor filled in with the selected line's condition, or null when the line is a group, which has no editor.</summary>
   internal ConditionEditorViewModel? EditorForTheSelectedCondition() =>
      Selected.IsGroup ? null : ConditionEditorViewModel.ForChanging(Selected.Condition, _collection);

   /// <summary>
   /// Opens an editor for a new condition on <paramref name="property"/> through <paramref name="editInADialog"/>, and adds the condition it returns, if any.<br/>
   /// <paramref name="typedText"/> is already typed into the editor, if it takes text.
   /// </summary>
   internal void AddAConditionThroughAnEditor(NoteProperty property, Func<ConditionEditorViewModel, Task<NoteCondition?>> editInADialog, string typedText = "") =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await editInADialog(EditorForANewConditionOn(property, typedText)) is {} condition)
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

   /// <summary>The add menu: the properties a condition can be on, each picked by a single key, such as T for tags.</summary>
   /// <param name="addAConditionOn">Opens the editor for a new condition on the picked property.</param>
   internal static List<SpecMenuItem> AddConditionMenu(Action<NoteProperty> addAConditionOn) =>
   [
      SpecMenuItem.Command("_Searchable text", () => addAConditionOn(NoteProperties.SearchableText)),
      SpecMenuItem.Command("_Question", () => addAConditionOn(NoteProperties.Question)),
      SpecMenuItem.Command("_Answer", () => addAConditionOn(NoteProperties.Answer)),
      SpecMenuItem.Command("_Tags", () => addAConditionOn(NoteProperties.Tags)),
      SpecMenuItem.Command("_Note type", () => addAConditionOn(NoteProperties.NoteType)),
      SpecMenuItem.Command("Studying _reading", () => addAConditionOn(NoteProperties.StudyingReading)),
      SpecMenuItem.Command("Studying _listening", () => addAConditionOn(NoteProperties.StudyingListening))
   ];

   /// <summary>The selected line's right-click menu: everything the editor can do to it, each picked by a single key.</summary>
   /// <param name="openTheAddMenu">Opens the <see cref="AddConditionMenu"/>.</param>
   /// <param name="changeTheSelectedCondition">Opens the editor filled in with the selected line's condition.</param>
   internal List<SpecMenuItem> SelectedNodeMenu(Action openTheAddMenu, Action changeTheSelectedCondition) =>
   [
      SpecMenuItem.Command("_Add a condition (+)", openTheAddMenu),
      SpecMenuItem.Command("_Change (Enter)", changeTheSelectedCondition, enabled: !Selected.IsGroup),
      SpecMenuItem.Command(Selected.IsNegated ? "Remove the _negation (-)" : "_Negate (-)", ToggleNegationOfTheSelectedNode),
      SpecMenuItem.Command(Selected.Condition is AnyOfGroup ? "_Switch to all of" : "_Switch to any of", SwitchTheSelectedGroup, enabled: Selected.IsGroup),
      SpecMenuItem.Command("_Remove (Del)", RemoveTheSelectedNode, enabled: !Selected.IsRoot),
      SpecMenuItem.Separator(),
      SpecMenuItem.Command("_Undo (Ctrl+Z)", Undo, enabled: _search.CanUndo)
   ];

   void ChangeTheSearch(ConditionPosition positionToSelect)
   {
      ShowTheSearch(positionToSelect);
      _fatalErrorHandler.StartAndHandleExceptionsFrom(RunTheSearchAsync);
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

   async Task RunTheSearchAsync()
   {
      var search = _search.Current;
      var thisSearch = ++_searchesStarted;
      if(search.IsEmpty)
      {
         Results.Show([]);
         StatusText = HowToSearch;
         IsSearching = false;
         return;
      }

      IsSearching = true;
      StatusText = "Searching...";
      var found = await TaskCE.Run(() => NotesFoundBy(search));
      if(ALaterSearchHasStarted(thisSearch)) return;

      Results.Show(found);
      StatusText = found.Count switch
      {
         0 => "No notes found",
         1 => "1 note found",
         _ => $"{found.Count} notes found"
      };
      IsSearching = false;
   }

   /// <summary>Whether a change made after the search numbered <paramref name="searchNumber"/> started started another, whose results replace its results.</summary>
   bool ALaterSearchHasStarted(int searchNumber) => searchNumber != _searchesStarted;

   /// <summary>The notes <paramref name="search"/> finds, shortest question first, which tends to put the closest match on top.</summary>
   List<JPNote> NotesFoundBy(NoteCondition search) =>
      [.._collection.NotesMatching(search).OrderBy(it => it.GetQuestion().Length).ThenBy(it => it.GetQuestion(), StringComparer.Ordinal)];
}
