namespace JAStudio.Core.Note;

class NoOpCardOperations : ICardOperations
{
   public void SuspendAllCardsForNote(NoteId noteId) {}
   public void UnsuspendAllCardsForNote(NoteId noteId) {}
   public void DeleteNote(NoteId noteId) {}
}
