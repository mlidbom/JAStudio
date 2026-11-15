using Avalonia.Threading;
using JAStudio.Core.Note;
using JAStudio.Core.UI.Web;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI;

/// <summary>
/// What Anki's shortcuts, and the JA Studio menu in Anki's menu bar, open of JAStudio: the workspace, which everything else in JAStudio is<br/>
/// reached from.
/// </summary>
/// <remarks>
/// JAStudio inside Anki is called here by Python, through pythonnet. A JAStudio outside Anki is asked through its card server, which hands the two<br/>
/// toggles to this as <see cref="IWorkspaceToggles"/>.
/// </remarks>
/// <remarks>
/// wip: once Python no longer calls JAStudio through pythonnet, this is only the <see cref="IWorkspaceToggles"/>: it's then to be named for that,<br/>
/// and <see cref="ShowTheNoteInTheWorkspace"/>, which only Python calls, goes.
/// </remarks>
// ReSharper disable MemberCanBeInternal used from python
// ReSharper disable UnusedMember.Global used from python
public class AnkiDialogs : IWorkspaceToggles
{
   /// <summary>Ctrl+S in Anki, as in the workspace: opens the workspace on its search, or closes the search.</summary>
   public void ToggleNoteSearch() => WorkspaceWindow.ToggleNoteSearch();

   /// <summary>Ctrl+Shift+O in Anki: shows the workspace, or hides it.</summary>
   public void ToggleWorkspaceWindow() => Dispatcher.UIThread.Invoke(WorkspaceWindow.ToggleVisibility);

   /// <summary>
   /// Ctrl+O in Anki's reviewer, previewer and browser: opens <paramref name="note"/>, the note shown there, in the workspace, and brings the<br/>
   /// workspace to the front. What JAStudio does with a note is done there, where Anki's own windows had a menu of it until 2026-11-15.
   /// </summary>
   /// <remarks>
   /// Ctrl+O was the workspace's search until the same day, which is Ctrl+S since (Magnus, 2026-11-15). Ctrl+Alt+O, Claude's first choice, typed<br/>
   /// a character instead on a keyboard where AltGr, which Windows takes Ctrl+Alt for, makes one of O.
   /// </remarks>
   /// <remarks>review-naming:menus: ShowTheNoteInTheWorkspace is Claude's (2026-11-15).</remarks>
   public void ShowTheNoteInTheWorkspace(JPNote note) => WorkspaceWindow.OpenNote(note);
}
