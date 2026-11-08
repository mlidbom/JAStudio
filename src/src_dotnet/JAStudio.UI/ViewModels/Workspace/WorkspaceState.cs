using System.Collections.Generic;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Search;
using JAStudio.UI.ViewModels.Workspace.Navigation;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// What the workspace showed when it was last left, as <see cref="SavedWorkspaceState"/> keeps it between sessions, so that it comes back<br/>
/// after a restart: the tabs and the one shown, the panes, the search, and the list the List tab showed. Notes are kept by their ids.
/// </summary>
/// <param name="TabNoteIds">The note each tab showed, in the tab strip's order. Tabs that showed no note aren't kept.</param>
/// <param name="ShownTabNoteId">The note the shown tab showed, or null if none did.</param>
/// <param name="IsNavigationPaneOpen">Whether the navigation pane was open.</param>
/// <param name="ShownNavigationPaneTab">Which of the navigation pane's tabs was shown.</param>
/// <param name="NavigationPaneWidth">The navigation pane's width, as it was last dragged.</param>
/// <param name="IsEditorSectionOpen">Whether the editor section was open.</param>
/// <param name="EditorSectionWidth">The editor section's width, as it was last dragged.</param>
/// <param name="Search">The search, as <see cref="NoteConditionJson"/> writes it.</param>
/// <param name="NoteListTitle">The title of the list the List tab showed, or null if it showed none.</param>
/// <param name="NoteListNoteIds">The notes of that list.</param>
record WorkspaceState(
   IReadOnlyList<string> TabNoteIds,
   string? ShownTabNoteId,
   bool IsNavigationPaneOpen,
   NavigationPaneTab ShownNavigationPaneTab,
   double NavigationPaneWidth,
   bool IsEditorSectionOpen,
   double EditorSectionWidth,
   JsonObject Search,
   string? NoteListTitle,
   IReadOnlyList<string> NoteListNoteIds);
