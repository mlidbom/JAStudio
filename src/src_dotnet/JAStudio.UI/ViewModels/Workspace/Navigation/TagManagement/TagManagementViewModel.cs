using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace.Navigation.TagManagement;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;

/// <summary>
/// A workspace's tag management, the "Tags" tab of its <see cref="NavigationPaneViewModel"/> (<see cref="TagManagementView"/>): the <see cref="TagTree"/>,<br/>
/// with how many notes are in each branch, a <see cref="Filter"/> that narrows it to the tag paths containing the typed parts, searching for a tag,<br/>
/// and renaming and deleting a branch on every note with a tag in it. See <c>dev_docs/tags.md</c>.
/// </summary>
/// <remarks>
/// The tree is computed from the notes' tags each time the tab is shown, and on <see cref="RefreshCommand"/> (F5), so it shows the tags as they are then.<br/>
/// It isn't kept up to date while it's shown, except after its own renames and deletions. The expanded lines and the selected line stay through that.
/// </remarks>
/// <remarks>
/// Renaming and deleting change each note in the background, with a progress dialog (<see cref="BulkNoteChanges"/>). A branch below <c>anki::</c>,<br/>
/// of the notes' Anki tags, is renamed or deleted in Anki instead, on every Anki note with a tag in it, as in Anki, and Anki's Edit > Undo undoes it.<br/>
/// The source branch itself, and a branch holding a tag whose name JAStudio uses, can't be renamed by hand (<see cref="TagBranch.WhyItCannotBeRenamedByHand"/>),<br/>
/// nor can they, or a branch holding a tag JAStudio maintains, be deleted (<see cref="TagBranch.WhyItCannotBeDeletedByHand"/>); nor can <c>anki</c> itself.<br/>
/// Renaming or deleting a branch in the source branch warns that the notes' sources change.
/// </remarks>
partial class TagManagementViewModel : ObservableObject
{
   readonly NavigationPaneViewModel _navigationPane;
   readonly JPCollection _collection;
   readonly BulkNoteChanges _bulkNoteChanges;
   readonly AnkiTagChanges _ankiTagChanges;
   readonly IFatalErrorHandler _fatalErrorHandler;

   /// <summary>The paths of the lines expanded while there's no filter. They stay expanded when the tree is computed again, and when a filter is cleared.</summary>
   readonly HashSet<string> _pathsExpandedWithoutAFilter = new(StringComparer.OrdinalIgnoreCase);

   /// <summary>The tree as computed last. It's computed when the tab is first shown, which is before anything can use it.</summary>
   TagTree? _tree;

   public TagManagementViewModel(NavigationPaneViewModel navigationPane, TemporaryServiceCollection services)
   {
      _navigationPane = navigationPane;
      _collection = services.CoreApp.Collection;
      _bulkNoteChanges = new BulkNoteChanges(services);
      _ankiTagChanges = services.Resolve<AnkiTagChanges>();
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
   }

   /// <summary>Parts of a tag path typed to narrow the tree, separated by spaces, as in the tag picker: <c>nat s3</c> leaves the paths that contain both.</summary>
   [ObservableProperty] string _filter = "";

   /// <summary>
   /// The tree's top-level lines, of the tree as the <see cref="Filter"/> narrows it.<br/>
   /// While there's a filter, the lines above the matching paths are expanded, so the matches show, collapsed.
   /// </summary>
   [ObservableProperty] IReadOnlyList<TagTreeNodeViewModel> _topLevelLines = [];

   /// <summary>The selected line, which searching, renaming and deleting act on. The tree sets it when a line is clicked.</summary>
   [ObservableProperty] TagTreeNodeViewModel? _selectedLine;

   partial void OnFilterChanged(string value) => ShowTheTreeFilteredBy(new TypedTagPathParts(value), SelectedLine?.TagPath);

   /// <summary>Computes the tree from the notes' tags, and shows it. Called each time the tab is shown.</summary>
   internal void ShowTheTagsAsTheyAreNow() => ShowTheTagsAsTheyAreNowWithSelected(SelectedLine?.TagPath);

   /// <summary>F5, or the Refresh button: computes the tree again from the notes' tags, which change while it's shown.</summary>
   [RelayCommand]
   void Refresh() => ShowTheTagsAsTheyAreNow();

   /// <summary>
   /// Searches for the notes in the selected line's branch: replaces the workspace's search with "Tags include" its path, and shows the search tab.<br/>
   /// Does nothing when no line is selected.
   /// </summary>
   internal void SearchForTheSelectedTag()
   {
      if(SelectedLine is not {} selected) return;

      _navigationPane.SearchFor(NoteProperties.Tags.Include(selected.TagPath));
   }

   /// <summary>
   /// Rename (F2): renames the last level of the selected line's path, as typed in the dialog <paramref name="renameInADialog"/> shows,<br/>
   /// on every note with a tag in its branch. Afterwards the renamed line is selected. Does nothing when the selected branch can't be renamed by hand.
   /// </summary>
   /// <param name="renameInADialog">Shows the dialog, and returns the rename it was accepted with, or null when it was cancelled.</param>
   internal void RenameTheSelectedTag(Func<TagRenameDialogViewModel, Task<TagBranchRename?>> renameInADialog) =>
      RenameTheSelectedBranch(renameInADialog, TagRenameDialogViewModel.ForRenamingTheLastLevelOf);

   /// <summary>Rename with parents: as <see cref="RenameTheSelectedTag"/>, but the whole path is typed, which can move the branch below other tags.</summary>
   /// <param name="renameInADialog">Shows the dialog, and returns the rename it was accepted with, or null when it was cancelled.</param>
   internal void RenameTheSelectedTagWithItsParents(Func<TagRenameDialogViewModel, Task<TagBranchRename?>> renameInADialog) =>
      RenameTheSelectedBranch(renameInADialog, TagRenameDialogViewModel.ForRenamingWithItsParents);

   /// <summary>
   /// Delete (Del): removes the selected line's tag and every tag below it from every note with one, once <paramref name="confirmInADialog"/> confirms it.<br/>
   /// Afterwards the nearest line above it that's left is selected. Does nothing when the selected branch can't be deleted by hand.
   /// </summary>
   /// <param name="confirmInADialog">Asks the question it's given in a dialog, and returns whether the answer was yes.</param>
   internal void DeleteTheSelectedTag(Func<string, Task<bool>> confirmInADialog)
   {
      if(SelectedBranchThatCanBeDeletedByHand() is not {} branch) return;

      var notes = _collection.NotesWithATagIn(branch);
      var question = branch.IsInTheAnkiBranch
                        ? $"Delete {branch.Path} and every tag below it from {AnkiNotesCounted(_ankiTagChanges.NumberOfAnkiNotesWithATagIn(branch))} in Anki? Anki's Edit > Undo undoes it."
                        : $"Delete {branch.Path} and every tag below it from {NotesCounted(notes.Count)}?{(branch.IsInTheSourceBranch ? $" This changes the source of {NotesCounted(notes.Count)}." : "")} This can't be undone.";
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(!await confirmInADialog(question)) return;

         await _bulkNoteChanges.DeleteTheBranch(branch, notes);
         ShowTheTagsAsTheyAreNowWithSelected(TheNearestPathAboveThatIsStillInUse(branch));
      });
   }

   /// <summary>
   /// The deepest path above <paramref name="branch"/> that a note still has a tag in, or null when there is none.<br/>
   /// Deleting a branch also removes the paths above it that held nothing else.
   /// </summary>
   string? TheNearestPathAboveThatIsStillInUse(TagBranch branch)
   {
      var pathsInUse = _collection.TagPathsInUse().All;
      return branch.PathsAboveIt.LastOrDefault(it => pathsInUse.Contains(it, StringComparer.OrdinalIgnoreCase));
   }

   /// <summary>The selected line's right-click menu. The actions that need a dialog are given by the view, which shows it.</summary>
   internal List<SpecMenuItem> SelectedLineMenu(Action rename, Action renameWithParents, Action delete)
   {
      var canBeRenamed = SelectedBranchThatCanBeRenamedByHand() is not null;
      var canBeDeleted = SelectedBranchThatCanBeDeletedByHand() is not null;
      return
      [
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('s', "Search for this tag (Enter, double-click)"), SearchForTheSelectedTag, enabled: SelectedLine is not null),
         SpecMenuItem.Separator(),
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('r', "Rename... (F2)"), rename, enabled: canBeRenamed),
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('p', "Rename with parents..."), renameWithParents, enabled: canBeRenamed),
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('d', "Delete... (Del)"), delete, enabled: canBeDeleted),
         ..TheExplanationsOf([WhyTheSelectedBranchCannotBeRenamedByHand(), WhyTheSelectedBranchCannotBeDeletedByHand()])
      ];

      static IEnumerable<SpecMenuItem> TheExplanationsOf(IEnumerable<string?> whyItCannotBeChanged)
      {
         var explanations = whyItCannotBeChanged.OfType<string>().Distinct().ToList();
         return explanations.Count == 0 ? [] : [SpecMenuItem.Separator(), ..explanations.Select(it => SpecMenuItem.Command(it, () => {}, enabled: false))];
      }
   }

   void RenameTheSelectedBranch(Func<TagRenameDialogViewModel, Task<TagBranchRename?>> renameInADialog,
                                Func<TagBranch, int, IReadOnlyCollection<Tag>, TagRenameDialogViewModel> dialogFor)
   {
      if(SelectedBranchThatCanBeRenamedByHand() is not {} branch) return;

      var notes = _collection.NotesWithATagIn(branch);
      var dialog = dialogFor(branch, branch.IsInTheAnkiBranch ? _ankiTagChanges.NumberOfAnkiNotesWithATagIn(branch) : notes.Count, [.._collection.TagsInUse()]);
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await renameInADialog(dialog) is not {} rename) return;

         await _bulkNoteChanges.RenameTheBranch(rename, notes);
         ShowTheTagsAsTheyAreNowWithSelected(rename.Renamed.Path);
      });
   }

   /// <summary>The selected line's branch, or null when no line is selected, or its branch can't be renamed by hand.</summary>
   TagBranch? SelectedBranchThatCanBeRenamedByHand() => SelectedLine is {} selected && WhyTheSelectedBranchCannotBeRenamedByHand() is null ? new TagBranch(selected.TagPath) : null;

   /// <summary>The selected line's branch, or null when no line is selected, or its branch can't be deleted by hand.</summary>
   TagBranch? SelectedBranchThatCanBeDeletedByHand() => SelectedLine is {} selected && WhyTheSelectedBranchCannotBeDeletedByHand() is null ? new TagBranch(selected.TagPath) : null;

   /// <summary>Why the selected line's branch can't be renamed by hand, for its menu to show, or null when it can, or no line is selected.</summary>
   string? WhyTheSelectedBranchCannotBeRenamedByHand() => SelectedLine is {} selected ? new TagBranch(selected.TagPath).WhyItCannotBeRenamedByHand(_collection.TagsInUse()) : null;

   /// <summary>Why the selected line's branch can't be deleted by hand, for its menu to show, or null when it can, or no line is selected.</summary>
   string? WhyTheSelectedBranchCannotBeDeletedByHand() => SelectedLine is {} selected ? new TagBranch(selected.TagPath).WhyItCannotBeDeletedByHand(_collection.TagsInUse()) : null;

   /// <summary>
   /// Computes the tree from the notes' tags, and shows it with the line of <paramref name="pathToSelect"/> selected, if it's shown,<br/>
   /// expanding the lines above it, so that it shows when there's no filter.
   /// </summary>
   void ShowTheTagsAsTheyAreNowWithSelected(string? pathToSelect)
   {
      if(pathToSelect is not null)
         _pathsExpandedWithoutAFilter.UnionWith(new TagBranch(pathToSelect).PathsAboveIt);
      _tree = _collection.TagTree();
      ShowTheTreeFilteredBy(new TypedTagPathParts(Filter), pathToSelect);
   }

   /// <summary>
   /// Shows the tree as <paramref name="typedParts"/>, typed in the <see cref="Filter"/>, narrow it, with the line of <paramref name="pathToSelect"/> selected, if it's shown.<br/>
   /// With a filter, the lines above the matches are expanded, and expanding or collapsing lines is forgotten when the filter changes.
   /// </summary>
   void ShowTheTreeFilteredBy(TypedTagPathParts typedParts, string? pathToSelect)
   {
      var tree = _tree ?? throw new InvalidOperationException("The tag tree is shown before it's computed.");

      TopLevelLines = typedParts.AreNone
                         ? LinesFor(tree, _pathsExpandedWithoutAFilter)
                         : LinesExpandedDownToTheMatchesIn(tree.WithOnlyThePathsMatching(typedParts), typedParts);
      SelectedLine = AllLines().FirstOrDefault(it => string.Equals(it.TagPath, pathToSelect, StringComparison.OrdinalIgnoreCase));
   }

   /// <summary>The top-level lines of <paramref name="shownTree"/>, each expanded when its path is in <paramref name="expandedPaths"/>, which they keep up to date.</summary>
   static IReadOnlyList<TagTreeNodeViewModel> LinesFor(TagTree shownTree, ISet<string> expandedPaths) => [..shownTree.TopLevel.Select(it => new TagTreeNodeViewModel(it, expandedPaths))];

   /// <summary>
   /// The lines of <paramref name="filteredTree"/> with the lines above the matches expanded, so the matches show, and the matches collapsed.<br/>
   /// The paths below a match all match too, since they contain its path, so expanding the matches would show their whole branches.
   /// </summary>
   static IReadOnlyList<TagTreeNodeViewModel> LinesExpandedDownToTheMatchesIn(TagTree filteredTree, TypedTagPathParts typedParts) =>
      LinesFor(filteredTree, new HashSet<string>(PathsAboveTheMatchesIn(filteredTree, typedParts), StringComparer.OrdinalIgnoreCase));

   /// <summary>The paths of <paramref name="filteredTree"/> that don't match <paramref name="typedParts"/>, which are in it only because a path below them does.</summary>
   static IEnumerable<string> PathsAboveTheMatchesIn(TagTree filteredTree, TypedTagPathParts typedParts) =>
      filteredTree.TopLevel.SelectMany(it => it.PathsInTheBranch()).Where(path => !typedParts.AreAllIn(path));

   IEnumerable<TagTreeNodeViewModel> AllLines() => TopLevelLines.SelectMany(it => it.ThisAndAllBelow());

   static string NotesCounted(int numberOfNotes) => numberOfNotes == 1 ? "1 note" : $"{numberOfNotes} notes";

   static string AnkiNotesCounted(int numberOfAnkiNotes) => numberOfAnkiNotes == 1 ? "1 Anki note" : $"{numberOfAnkiNotes} Anki notes";
}
