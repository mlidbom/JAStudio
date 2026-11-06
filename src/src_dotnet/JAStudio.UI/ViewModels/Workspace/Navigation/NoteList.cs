using System.Collections.Generic;
using JAStudio.Core.Note;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// Notes to list in the navigation pane's "List" tab (<see cref="NoteListViewModel"/>), with a title saying what they are,<br/>
/// such as the sentences a vocab matches. The "Open" items of the note menus show one.
/// </summary>
/// <param name="Title">What the notes are, such as "Sentences with 走る".</param>
/// <param name="Notes">The notes, in the order to list them.</param>
record NoteList(string Title, IReadOnlyList<JPNote> Notes);
