namespace JAStudio.Core.Note;

/// <summary>
/// Interface for card operations (suspend/unsuspend) that must be implemented by the integration layer.
/// This allows JAStudio.Core to request card operations without depending on any specific backend.
/// </summary>
public interface ICardOperations
{
   /// <summary>Suspend all cards for the given note.</summary>
   void SuspendAllCardsForNote(NoteId noteId);

   /// <summary>Unsuspend all cards for the given note.</summary>
   void UnsuspendAllCardsForNote(NoteId noteId);

   /// <summary>Delete the Anki note for the given domain note ID, if it exists in Anki.</summary>
   void DeleteNote(NoteId noteId);
}
