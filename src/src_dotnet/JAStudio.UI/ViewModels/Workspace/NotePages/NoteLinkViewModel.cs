using JAStudio.Core.Note;

namespace JAStudio.UI.ViewModels.Workspace.NotePages;

/// <summary>
/// A link on a note's page to another note, such as a word of a sentence's breakdown to its vocab: its <paramref name="Text"/>, which clicking shows<br/>
/// the <paramref name="LinkedNote"/> in the same tab, and middle-clicking opens it in a new tab in the background (<see cref="NotePageViewModel.Follow"/>).
/// </summary>
sealed record NoteLinkViewModel(string Text, NoteId LinkedNote);
