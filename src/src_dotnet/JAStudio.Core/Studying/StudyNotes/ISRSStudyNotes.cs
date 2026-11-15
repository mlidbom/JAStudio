using System.Collections.Generic;
using JAStudio.Core.Note;

namespace JAStudio.Core.Studying.StudyNotes;

/// <summary>
/// The study-notes in the SRS, the spaced repetition system, that JAStudio works with: reading them, writing one from its note, suspending and<br/>
/// unsuspending its cards, and deleting it. A study-note is what a note is studied through there: it holds the note's cards, and the few fields<br/>
/// the spaced repetition system itself needs. In Anki it's an Anki note of one of JAStudio's note types, in the Anki profile JAStudio works with.<br/>
/// JAStudio writes those fields from the corpus (<see cref="JPNote.StudyNoteFields"/>) whenever it updates the note. A study-note's tags are the<br/>
/// student's, never written by JAStudio. The integration with a spaced repetition system implements it: for Anki, <c>JAStudio.Anki</c>.
/// </summary>
/// <remarks>
/// A study-note can differ from what JAStudio would write now: a reload passes on only the notes whose files changed while the collection held them,<br/>
/// and none at startup, so a change to the corpus made while Anki was closed, or a change to what JAStudio writes, leaves study-notes behind.<br/>
/// <see cref="StudyNoteComparison"/> finds them, and <see cref="Write"/> brings one up to date.
/// </remarks>
/// <remarks>
/// review-naming:studying: ISRSStudyNotes was IAnkiStudyNotes (Claude, 2026-11-15). Since the same day it's also what ISRSDataLoader and<br/>
/// ISRSCardOperations were, which read the same study-notes and changed them: one interface for the study-notes, with one read of them.<br/>
/// "Cards" and "suspend" are Anki's words, left as they are: studying's own words for them are still to be designed (dev_docs/studying.md,<br/>
/// "To design").
/// </remarks>
public interface ISRSStudyNotes
{
   /// <summary>
   /// Every study-note in the spaced repetition system now, read straight from it: for Anki, from its database. Each comes with all that the<br/>
   /// spaced repetition system holds of it that JAStudio uses (<see cref="StudyNoteInTheSRS"/>).
   /// </summary>
   IReadOnlyList<StudyNoteInTheSRS> ReadAll();

   /// <summary>Writes <paramref name="note"/>'s study-note in the spaced repetition system, as JAStudio writes it whenever it updates the note.</summary>
   void Write(JPNote note);

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
