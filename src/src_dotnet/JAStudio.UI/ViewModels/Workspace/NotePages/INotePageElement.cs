namespace JAStudio.UI.ViewModels.Workspace.NotePages;

/// <summary>
/// An element of a note's page that a right-click is about, such as a sentence's breakdown entry: the page's right-click menu is the element's own<br/>
/// (<see cref="NotePageViewModel.MenuOf"/>), starting with the items that apply to it as it is, such as marking the match the entry shows incorrect.<br/>
/// A right-click on no element, such as on the sentence's question, gets the note's menu (<see cref="NotePageViewModel.NoteContextMenuSpec"/>).
/// </summary>
/// <remarks>
/// The view finds the element right-clicked as the nearest one shown by the control right-clicked or by a control around it, so that a right-click on an<br/>
/// entry's meta tag is about the entry, and one on a compound part under the entry about the compound part.
/// </remarks>
interface INotePageElement
{
}
