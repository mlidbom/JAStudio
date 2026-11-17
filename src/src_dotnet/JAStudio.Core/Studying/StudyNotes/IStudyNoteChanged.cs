using Compze.Tessaging.TessageTypes;
using JAStudio.Core.Note;

namespace JAStudio.Core.Studying.StudyNotes;

/// <summary>
/// What the collection holds of a note's study-note in the spaced repetition system changed: the studying status of its cards, when JAStudio<br/>
/// suspended or unsuspended them, or whatever reading the study-notes again found changed, as a sync does, such as its tags in Anki, or the<br/>
/// study-note itself, created or deleted. None of it is stored with the note, so no unit of work changes the note for it.
/// </summary>
public interface IStudyNoteChanged : ITevent
{
   NoteId NoteId { get; }
}
