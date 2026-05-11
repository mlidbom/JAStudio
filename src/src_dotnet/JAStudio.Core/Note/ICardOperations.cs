using System;

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

/// <summary>
/// Service locator for card operations.
/// The integration layer (e.g. JAStudio.Anki) must call SetImplementation() during initialization.
/// </summary>
public class CardOperations
{
   internal CardOperations() {}

   static ICardOperations? _implementation;

   /// <summary>
   /// Set the implementation of card operations (called during initialization).
   /// </summary>
   public void SetImplementation(ICardOperations implementation)
   {
      _implementation = implementation;
   }

   /// <summary>Suspend all cards for the given note.</summary>
   public void SuspendAllCardsForNote(NoteId noteId)
   {
      if(_implementation == null)
      {
         throw new InvalidOperationException(
            "CardOperations.SetImplementation() must be called before using card operations");
      }

      _implementation.SuspendAllCardsForNote(noteId);
   }

   /// <summary>Unsuspend all cards for the given note.</summary>
   public void UnsuspendAllCardsForNote(NoteId noteId)
   {
      if(_implementation == null)
      {
         throw new InvalidOperationException(
            "CardOperations.SetImplementation() must be called before using card operations");
      }

      _implementation.UnsuspendAllCardsForNote(noteId);
   }

   /// <summary>Delete the Anki note for the given domain note ID, if it exists in Anki.</summary>
   public void DeleteNote(NoteId noteId)
   {
      if(_implementation == null)
      {
         throw new InvalidOperationException(
            "CardOperations.SetImplementation() must be called before using card operations");
      }

      _implementation.DeleteNote(noteId);
   }
}
