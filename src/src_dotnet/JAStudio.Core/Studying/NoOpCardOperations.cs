using JAStudio.Core.Note;

namespace JAStudio.Core.Studying;

/// <summary>The card operations of the specifications, which have no spaced repetition system: each does nothing.</summary>
class NoOpCardOperations : ISpacedRepetitionSystemCardOperations
{
   public void SuspendAllCardsForNote(NoteId noteId) {}
   public void UnsuspendAllCardsForNote(NoteId noteId) {}
   public void DeleteTheStudyNoteOf(NoteId noteId) {}
}
