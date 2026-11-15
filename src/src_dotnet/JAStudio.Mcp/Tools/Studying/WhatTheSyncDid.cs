using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Studying;
using JAStudio.Mcp.Tools.StudyNotes;

namespace JAStudio.Mcp.Tools.Studying;

/// <summary>What <see cref="SyncTools.SyncWithAnki"/> reports of a sync with Anki (<see cref="SRSSyncReport"/>), and how long the sync took.</summary>
/// <param name="StudyNotesRead">How many study-notes the sync read from Anki: every one there.</param>
/// <param name="NotesWhoseStudyNoteHadChanged">The notes whose study-note had changed in Anki since JAStudio last read it: a card's studying status, its tags, or being new to the note.</param>
/// <param name="NotesWhoseStudyNoteIsGone">The notes whose study-note was gone from Anki.</param>
/// <param name="NotesWhoseStudyNoteWasWritten">The notes whose study-note differed from what JAStudio would write, and which the sync so wrote.</param>
/// <param name="StudyNotesThatHoldNoNotesId">The study-notes in Anki that hold no corpus note's id, every one: corruption, which a sync does nothing with.</param>
/// <param name="StudyNotesWhoseNoteTheCollectionHasNot">The study-notes in Anki that hold the id of a note the collection hasn't, every one.</param>
public record WhatTheSyncDid(
   double SecondsTaken,
   int StudyNotesRead,
   NotesByType NotesWhoseStudyNoteHadChanged,
   NotesByType NotesWhoseStudyNoteIsGone,
   NotesByType NotesWhoseStudyNoteWasWritten,
   IReadOnlyList<StudyNoteOfNoNote> StudyNotesThatHoldNoNotesId,
   IReadOnlyList<StudyNoteOfNoNote> StudyNotesWhoseNoteTheCollectionHasNot)
{
   /// <summary>The report of <paramref name="report"/>, listing at most <paramref name="maxNotesListedPerType"/> notes of each type.</summary>
   public static WhatTheSyncDid Of(double secondsTaken, SRSSyncReport report, int maxNotesListedPerType) =>
      new(secondsTaken,
          report.StudyNotesRead,
          NotesByType.Of(report.NotesWhoseStudyNoteHadChanged, maxNotesListedPerType),
          NotesByType.Of(report.NotesWhoseStudyNoteIsGone, maxNotesListedPerType),
          NotesByType.Of(report.NotesWhoseStudyNoteWasWritten, maxNotesListedPerType),
          [.. report.StudyNotesThatHoldNoNotesId.Select(StudyNoteOfNoNote.Of)],
          [.. report.StudyNotesWhoseNoteTheCollectionHasNot.Select(StudyNoteOfNoNote.Of)]);
}
