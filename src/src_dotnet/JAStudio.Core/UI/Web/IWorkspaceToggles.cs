namespace JAStudio.Core.UI.Web;

/// <summary>
/// The two toggles of JAStudio's workspace window that the spaced repetition system's keys and menu work: showing the workspace or hiding it, and<br/>
/// opening its search or closing it. In Anki they're Ctrl+Shift+O and Ctrl+S, and the JA Studio menu.
/// </summary>
/// <remarks>
/// The Anki add-on asks for them through the card server (<c>JAStudio.Web.CardServer</c>), which hands them to this. Showing a note in the workspace,<br/>
/// as Ctrl+O does, is <see cref="INoteLinkOpener.OpenInWorkspace"/>, as for a click on a note link.
/// </remarks>
/// <remarks>review-naming:anki-http: IWorkspaceToggles is Claude's (2026-11-15).</remarks>
public interface IWorkspaceToggles
{
   /// <summary>Shows the workspace window and brings it to the front, or hides it.</summary>
   void ToggleWorkspaceWindow();

   /// <summary>Opens the workspace on its search, or closes the search.</summary>
   void ToggleNoteSearch();
}
