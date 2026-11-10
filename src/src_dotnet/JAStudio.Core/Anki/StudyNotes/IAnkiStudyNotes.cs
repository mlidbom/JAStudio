using System.Collections.Generic;
using JAStudio.Core.Note;

namespace JAStudio.Core.Anki.StudyNotes;

/// <summary>
/// The study-notes in the Anki profile JAStudio works with: the Anki notes of JAStudio's note types, which hold a note's cards and the few fields<br/>
/// Anki itself needs. JAStudio writes a note's study-note from the corpus (<see cref="JPNote.GetData"/>) whenever it updates the note.
/// </summary>
/// <remarks>
/// A study-note can differ from what JAStudio would write now: a reload passes on only the notes whose files changed while the collection held them,<br/>
/// and none at startup, so a change to the corpus made while Anki was closed, or a change to what JAStudio writes, leaves study-notes behind.<br/>
/// <see cref="StudyNoteComparison"/> finds them, and <see cref="Write"/> brings one up to date.
/// </remarks>
public interface IAnkiStudyNotes
{
   /// <summary>Every study-note in Anki now, read straight from Anki's database.</summary>
   IReadOnlyList<StudyNoteInAnki> ReadAll();

   /// <summary>Writes <paramref name="note"/>'s study-note in Anki, as JAStudio writes it whenever it updates the note.</summary>
   void Write(JPNote note);
}
