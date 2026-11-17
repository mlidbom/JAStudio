using JAStudio.Core.Note;

namespace JAStudio.Core.Studying.StudyNotes;

public sealed class StudyNoteChanged(NoteId noteId) : IStudyNoteChanged
{
   public NoteId NoteId { get; } = noteId;
}
