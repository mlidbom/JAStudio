using System;
using JAStudio.Core.Note;

namespace JAStudio.StandaloneHost;

/// <summary>
/// The standalone JAStudio's changes to the notes' cards in Anki: none, since it has no Anki, and so no note has an Anki note or cards. Suspending and<br/>
/// unsuspending are offered only for a note with active or suspended cards.
/// </summary>
sealed class NoAnkiCardOperations : ICardOperations
{
   public void SuspendAllCardsForNote(NoteId noteId) => throw NoCards(noteId);
   public void UnsuspendAllCardsForNote(NoteId noteId) => throw NoCards(noteId);

   /// <summary>Does nothing: without Anki, the note has no Anki note to delete.</summary>
   public void DeleteNote(NoteId noteId) {}

   static InvalidOperationException NoCards(NoteId noteId) => new($"The standalone JAStudio has no Anki, so the note {noteId} has no cards to change.");
}
