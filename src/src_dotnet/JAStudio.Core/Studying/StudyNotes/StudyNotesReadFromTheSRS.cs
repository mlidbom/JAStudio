using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Studying.StudyNotes;

/// <summary>
/// The study-notes as the collection read them from the SRS, the spaced repetition system (<see cref="ISRSStudyNotes.ReadAll"/>), with what giving<br/>
/// each note its study-note from them found: at a load, and each time the collection reads them again<br/>
/// (<see cref="JPCollection.ReadTheStudyNotesFromTheSRSAgain"/>).
/// </summary>
/// <param name="StudyNotes">Every study-note read.</param>
/// <param name="NotesWhoseStudyNoteChanged">
/// The notes that have a study-note which differs from before: in the studying status of a card, in its tags, or in being new to the note. At a<br/>
/// load, every note with one.
/// </param>
/// <param name="NotesWhoseStudyNoteIsGone">The notes that had a study-note before the read, and have none now: it's gone from the spaced repetition system.</param>
/// <remarks>review-naming:studying: StudyNotesReadFromTheSRS is new (Claude, 2026-11-15).</remarks>
public sealed record StudyNotesReadFromTheSRS(IReadOnlyList<StudyNoteInTheSRS> StudyNotes, IReadOnlyList<JPNote> NotesWhoseStudyNoteChanged, IReadOnlyList<JPNote> NotesWhoseStudyNoteIsGone);
