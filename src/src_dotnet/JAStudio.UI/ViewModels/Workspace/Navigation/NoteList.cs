using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// Notes to list in the navigation pane's "List" tab (<see cref="NoteListViewModel"/>), with a title saying what they are,<br/>
/// such as the sentences a vocab matches. The "Open" items of the note menus show one.
/// </summary>
/// <param name="Title">What the notes are, such as "Sentences with 走る".</param>
/// <param name="Notes">The notes. The "List" tab orders them by their question.</param>
record NoteList(string Title, IReadOnlyList<JPNote> Notes)
{
   /// <summary>The same list without <paramref name="deleted"/>.</summary>
   public NoteList Without(JPNote deleted) => this with { Notes = [.. Notes.Where(it => !it.GetId().Equals(deleted.GetId()))] };
}
