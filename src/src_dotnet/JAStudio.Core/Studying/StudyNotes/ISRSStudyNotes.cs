using System.Collections.Generic;
using JAStudio.Core.Note;

namespace JAStudio.Core.Studying.StudyNotes;

/// <summary>
/// The study-notes in the SRS, the spaced repetition system, that JAStudio works with. A study-note is what a note is studied through there: it holds the note's<br/>
/// cards, and the few fields the spaced repetition system itself needs. In Anki it's an Anki note of one of JAStudio's note types, in the Anki<br/>
/// profile JAStudio works with. JAStudio writes those fields from the corpus (<see cref="JPNote.StudyNoteFields"/>) whenever it updates the note.<br/>
/// A study-note's tags are the student's, never written by JAStudio.
/// </summary>
/// <remarks>
/// A study-note can differ from what JAStudio would write now: a reload passes on only the notes whose files changed while the collection held them,<br/>
/// and none at startup, so a change to the corpus made while Anki was closed, or a change to what JAStudio writes, leaves study-notes behind.<br/>
/// <see cref="StudyNoteComparison"/> finds them, and <see cref="Write"/> brings one up to date.
/// </remarks>
/// <remarks>review-naming:studying: ISRSStudyNotes was IAnkiStudyNotes (Claude, 2026-11-15).</remarks>
public interface ISRSStudyNotes
{
   /// <summary>Every study-note in the spaced repetition system now, read straight from it: for Anki, from its database.</summary>
   IReadOnlyList<StudyNoteInTheSRS> ReadAll();

   /// <summary>Writes <paramref name="note"/>'s study-note in the spaced repetition system, as JAStudio writes it whenever it updates the note.</summary>
   void Write(JPNote note);
}
