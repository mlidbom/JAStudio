using JAStudio.Core.Note;

namespace JAStudio.Core.Studying;

/// <summary>
/// The changes JAStudio makes in the SRS, the spaced repetition system, to the cards of a note's study-note, and to the study-note itself:<br/>
/// suspending and unsuspending all its cards, and deleting it. The integration with a spaced repetition system implements it: for Anki,<br/>
/// <c>JAStudio.Anki</c>.
/// </summary>
/// <remarks>
/// review-naming:studying: ISRSCardOperations was ICardOperations, and DeleteTheStudyNoteOf was DeleteNote, which deletes no note (Claude, 2026-11-15).<br/>
/// "Cards" and "suspend" are Anki's words, left as they are: studying's own words for them are still to be designed (dev_docs/studying.md,<br/>
/// "To design").
/// </remarks>
public interface ISRSCardOperations
{
   /// <summary>Suspends all the cards of the study-note of the note with the id <paramref name="noteId"/>, which has one.</summary>
   void SuspendAllCardsForNote(NoteId noteId);

   /// <summary>Unsuspends all the cards of the study-note of the note with the id <paramref name="noteId"/>, which has one.</summary>
   void UnsuspendAllCardsForNote(NoteId noteId);

   /// <summary>
   /// Deletes the study-note of the note with the id <paramref name="noteId"/> from the spaced repetition system, if the note has one, with its<br/>
   /// cards and their review history. The note itself stays.
   /// </summary>
   void DeleteTheStudyNoteOf(NoteId noteId);
}
