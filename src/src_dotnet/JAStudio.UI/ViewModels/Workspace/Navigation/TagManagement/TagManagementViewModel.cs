using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace.Navigation.TagManagement;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;

/// <summary>
/// A workspace's tag management, the "Tags" tab of its <see cref="NavigationPaneViewModel"/> (<see cref="TagManagementView"/>): the <see cref="TagTree"/>,<br/>
/// with how many notes are in each branch, a <see cref="Filter"/> that narrows it to the tag paths containing the typed parts, and searching for a tag.<br/>
/// See <c>dev_docs/tags.md</c>.
/// </summary>
/// <remarks>
/// The tree is computed from the notes' tags each time the tab is shown, and on <see cref="RefreshCommand"/> (F5), so it shows the tags as they are then.<br/>
/// It isn't kept up to date while it's shown. The expanded lines and the selected line stay through that.
/// </remarks>
partial class TagManagementViewModel : ObservableObject
{
   readonly NavigationPaneViewModel _navigationPane;
   readonly JPCollection _collection;

   /// <summary>The paths of the lines expanded while there's no filter. They stay expanded when the tree is computed again, and when a filter is cleared.</summary>
   readonly HashSet<string> _pathsExpandedWithoutAFilter = new(StringComparer.OrdinalIgnoreCase);

   /// <summary>The tree as computed last. It's computed when the tab is first shown, which is before anything can use it.</summary>
   TagTree? _tree;

   public TagManagementViewModel(NavigationPaneViewModel navigationPane, TemporaryServiceCollection services)
   {
      _navigationPane = navigationPane;
      _collection = services.CoreApp.Collection;
   }

   /// <summary>Parts of a tag path typed to narrow the tree, separated by spaces, as in the tag picker: <c>nat s3</c> leaves the paths that contain both.</summary>
   [ObservableProperty] string _filter = "";

   /// <summary>The tree's top-level lines, of the tree as the <see cref="Filter"/> narrows it. While there's a filter, every line shown is expanded.</summary>
   [ObservableProperty] IReadOnlyList<TagTreeNodeViewModel> _topLevelLines = [];

   /// <summary>The selected line, which searching acts on. The tree sets it when a line is clicked.</summary>
   [ObservableProperty] TagTreeNodeViewModel? _selectedLine;

   partial void OnFilterChanged(string value) => ShowTheTreeFilteredBy(new TypedTagPathParts(value));

   /// <summary>Computes the tree from the notes' tags, and shows it. Called each time the tab is shown.</summary>
   internal void ShowTheTagsAsTheyAreNow()
   {
      _tree = _collection.TagTree();
      ShowTheTreeFilteredBy(new TypedTagPathParts(Filter));
   }

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

   /// <summary>The selected line's right-click menu.</summary>
   internal List<SpecMenuItem> SelectedLineMenu() =>
   [
      SpecMenuItem.Command(ShortcutFinger.Mnemonic('s', "Search for this tag (Enter, double-click)"), SearchForTheSelectedTag, enabled: SelectedLine is not null)
   ];

   /// <summary>
   /// Shows the tree as <paramref name="typedParts"/>, typed in the <see cref="Filter"/>, narrow it, keeping the selected path selected if it's still shown.<br/>
   /// With a filter, the lines are expanded, so the matching paths show, and expanding or collapsing them is forgotten when the filter changes.
   /// </summary>
   void ShowTheTreeFilteredBy(TypedTagPathParts typedParts)
   {
      var tree = _tree ?? throw new InvalidOperationException("The tag tree is shown before it's computed.");
      var selectedPath = SelectedLine?.TagPath;

      TopLevelLines = typedParts.AreNone
                         ? LinesFor(tree, _pathsExpandedWithoutAFilter)
                         : LinesWithEveryPathExpandedFor(tree.WithOnlyThePathsMatching(typedParts));
      SelectedLine = AllLines().FirstOrDefault(it => string.Equals(it.TagPath, selectedPath, StringComparison.OrdinalIgnoreCase));
      return;

      static IReadOnlyList<TagTreeNodeViewModel> LinesFor(TagTree shownTree, ISet<string> expandedPaths) => [..shownTree.TopLevel.Select(it => new TagTreeNodeViewModel(it, expandedPaths))];

      static IReadOnlyList<TagTreeNodeViewModel> LinesWithEveryPathExpandedFor(TagTree shownTree) =>
         LinesFor(shownTree, new HashSet<string>(shownTree.TopLevel.SelectMany(it => it.PathsInTheBranch()), StringComparer.OrdinalIgnoreCase));
   }

   IEnumerable<TagTreeNodeViewModel> AllLines() => TopLevelLines.SelectMany(it => it.ThisAndAllBelow());
}
