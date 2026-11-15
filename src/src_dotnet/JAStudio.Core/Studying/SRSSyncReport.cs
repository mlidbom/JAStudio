using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Core.Studying;

/// <summary>
/// What a sync with the SRS, the spaced repetition system, did (<see cref="SRSSync.Run"/>): what it read, what had changed there since JAStudio<br/>
/// last read it, what it wrote, and the study-notes there that belong to no note. What the import profiles that run automatically import isn't<br/>
/// in it: they run in the background, after the sync, and report by themselves.
/// </summary>
public sealed class SRSSyncReport
{
   internal SRSSyncReport(StudyNotesReadFromTheSRS read, StudyNoteComparison comparison, IReadOnlyList<JPNote> notesWhoseStudyNoteWasWritten)
   {
      StudyNotesRead = read.StudyNotes.Count;
      NotesWhoseStudyNoteHadChanged = read.NotesWhoseStudyNoteChanged;
      NotesWhoseStudyNoteIsGone = read.NotesWhoseStudyNoteIsGone;
      NotesWhoseStudyNoteWasWritten = notesWhoseStudyNoteWasWritten;
      StudyNotesThatHoldNoNotesId = comparison.StudyNotesThatHoldNoNotesId;
      StudyNotesWhoseNoteTheCollectionHasNot = comparison.StudyNotesWhoseNoteTheCollectionHasNot;
   }

   /// <summary>How many study-notes the sync read from the spaced repetition system: every one there.</summary>
   public int StudyNotesRead { get; }

   /// <summary>
   /// The notes whose study-note had changed in the spaced repetition system since JAStudio last read it: in the studying status of a card, such<br/>
   /// as one suspended there, in its tags, or in being new to the note. The notes show the change since the sync.
   /// </summary>
   public IReadOnlyList<JPNote> NotesWhoseStudyNoteHadChanged { get; }

   /// <summary>The notes whose study-note was gone from the spaced repetition system, such as one deleted there: they're no longer studied in it.</summary>
   public IReadOnlyList<JPNote> NotesWhoseStudyNoteIsGone { get; }

   /// <summary>The notes whose study-note differed from what JAStudio would write for the note, and which the sync so wrote.</summary>
   public IReadOnlyList<JPNote> NotesWhoseStudyNoteWasWritten { get; }

   /// <summary>
   /// The study-notes that hold no note's id, as no study-note JAStudio wrote does (<see cref="StudyNoteComparison.StudyNotesThatHoldNoNotesId"/>):<br/>
   /// listed so that the corruption shows. A sync does nothing with them.
   /// </summary>
   public IReadOnlyList<StudyNoteInTheSRS> StudyNotesThatHoldNoNotesId { get; }

   /// <summary>
   /// The study-notes that hold the id of a note the collection hasn't (<see cref="StudyNoteComparison.StudyNotesWhoseNoteTheCollectionHasNot"/>),<br/>
   /// such as one whose note was deleted with its study-note kept. A sync does nothing with them.
   /// </summary>
   public IReadOnlyList<StudyNoteInTheSRS> StudyNotesWhoseNoteTheCollectionHasNot { get; }
}
