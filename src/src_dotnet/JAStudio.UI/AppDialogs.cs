using Avalonia.Threading;
using JAStudio.Core.Note;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI;

/// <summary>
/// What Anki's shortcuts, and the JA Studio menu in Anki's menu bar, open of JAStudio: the workspace, which everything else in JAStudio is<br/>
/// reached from.
/// </summary>
// ReSharper disable MemberCanBeInternal used from python
// ReSharper disable UnusedMember.Global used from python
public class AnkiDialogs
{
   /// <summary>Ctrl+O in Anki: opens the workspace on its search, or closes the search.</summary>
   public void ToggleNoteSearch() => WorkspaceWindow.ToggleNoteSearch();

   /// <summary>Ctrl+Shift+O in Anki: shows the workspace, or hides it.</summary>
   public void ToggleWorkspaceWindow() => Dispatcher.UIThread.Invoke(WorkspaceWindow.ToggleVisibility);

   /// <summary>
   /// Ctrl+Alt+O in Anki's reviewer and previewer: opens <paramref name="note"/>, the note shown there, in the workspace, and brings the<br/>
   /// workspace to the front. What JAStudio does with a note is done there, where Anki's own windows had a menu of it until 2026-11-15.
   /// </summary>
   /// <remarks>review-naming:menus: ShowTheNoteInTheWorkspace, and its key, Ctrl+Alt+O, are Claude's (2026-11-15).</remarks>
   public void ShowTheNoteInTheWorkspace(JPNote note) => WorkspaceWindow.OpenNote(note);
}
