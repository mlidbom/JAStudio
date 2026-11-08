using Avalonia.Threading;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI;

/// <summary>What Anki's shortcuts open of JAStudio: the workspace, which everything else in JAStudio is reached from.</summary>
// ReSharper disable MemberCanBeInternal used from python
// ReSharper disable UnusedMember.Global used from python
public class AnkiDialogs
{
   /// <summary>Ctrl+O in Anki: opens the workspace on its search, or closes the search.</summary>
   public void ToggleNoteSearch() => WorkspaceWindow.ToggleNoteSearch();

   /// <summary>Ctrl+Shift+O in Anki: shows the workspace, or hides it.</summary>
   public void ToggleWorkspaceWindow() => Dispatcher.UIThread.Invoke(WorkspaceWindow.ToggleVisibility);
}
