using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Studying.StudyNotes;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>
/// The tools that compare the fields of the study-notes in Anki, the Anki notes that hold the notes' cards, with what JAStudio would write now, and write<br/>
/// those that differ (<see cref="ISRSStudyNotes"/>), as JAStudio's sync with Anki does, which has a tool of its own (<see cref="Studying.SyncTools"/>). They show<br/>
/// what a sync would write before it runs, and write without the rest of a sync.
/// </summary>
[McpServerToolType]
public class StudyNoteTools
{
   /// <summary>How many notes of each type a tool lists unless asked for another number.</summary>
   const int NotesListedPerTypeByDefault = 10;

   readonly JPCollection _collection;
   readonly ISRSStudyNotes _studyNotes;

   public StudyNoteTools(JPCollection collection, ISRSStudyNotes studyNotes)
   {
      _collection = collection;
      _studyNotes = studyNotes;
   }

   [McpServerTool(Name = "list_the_study_notes_that_differ", ReadOnly = true, Idempotent = true)]
   [Description("""
                Compares every study-note in Anki, the Anki note holding a note's cards, with the study-note JAStudio would write for the note now:
                the fields JAStudio writes, which are the question, the audio and the note's id. A study-note's tags are the student's, which
                JAStudio never writes. JAStudio writes the study-notes that differ at each sync with Anki, which a person starts, so a study-note
                is out of date from when JAStudio changes its note until the next sync: sync_with_anki runs one, and
                write_the_study_notes_that_differ writes them without the rest of a sync. Text is compared as Anki
                stores it, in Unicode normalization form C. In the headless JAStudio, Anki's latest changes may still be in its database's
                write-ahead log, which it doesn't read. Reports how many study-notes were compared, in how many each field differs, the notes
                whose study-note differs, by type, each with the fields that differ, and every study-note in Anki that belongs to no note of the
                collection, with the id of its Anki note, its question and its jas_note_id, the id of the note it was written for. Of the notes whose study-note differs it also counts, and marks, those where only the markup of the audio differs:
                the fields that differ are audio fields, holding the same audio files in the same order in Anki as JAStudio would write, so only
                the markup around the files, such as a space or an &nbsp;, or which of the fields holds which file, differs.
                """)]
   public StudyNotesThatDiffer ListTheStudyNotesThatDiffer(
      [Description("The most notes of each type to list. Every note whose study-note differs is counted either way.")] int maxNotesListedPerType = NotesListedPerTypeByDefault,
      [Description("Whether to leave out of the lists the notes where only the markup of the audio differs, to list only those that differ in more. They're counted either way.")]
      bool leaveOutTheNotesWhereOnlyTheMarkupOfTheAudioDiffers = false) =>
      StudyNotesThatDiffer.In(Comparison(), maxNotesListedPerType, it => leaveOutTheNotesWhereOnlyTheMarkupOfTheAudioDiffers && it.OnlyTheMarkupOfItsAudioDiffers);

   [McpServerTool(Name = "write_the_study_notes_that_differ", Destructive = false, Idempotent = true)]
   [Description("""
                Writes the study-note in Anki of every note whose study-note differs from what JAStudio would write now, as
                list_the_study_notes_that_differ finds them, or of the notes given, whether theirs differ or not: as a sync with Anki writes
                them, without reading Anki's changes or running imports. Only in the live JAStudio, inside Anki: the headless one has no Anki and refuses. Reports
                how long it took, the notes whose study-notes it wrote, by type, and how many study-notes differ afterwards, compared again.
                """)]
   public StudyNotesWritten WriteTheStudyNotesThatDiffer(
      [Description("The ids of the notes whose study-notes to write, as search_notes lists them. Leave out to write every study-note that differs.")] string[]? noteIds = null,
      [Description("The most notes of each type to list. Every note whose study-note was written is counted either way.")] int maxNotesListedPerType = NotesListedPerTypeByDefault)
   {
      var stopwatch = Stopwatch.StartNew();
      var notesToWrite = noteIds == null ? [.. Comparison().NotesWhoseStudyNoteDiffers.Select(it => it.Note)] : noteIds.Select(NoteWithTheId).ToList();
      notesToWrite.ForEach(_studyNotes.Write);
      return StudyNotesWritten.Of(stopwatch.Elapsed.TotalSeconds, notesToWrite, maxNotesListedPerType, Comparison().NotesWhoseStudyNoteDiffers.Count);
   }

   StudyNoteComparison Comparison() => new(_collection, _studyNotes.ReadAll());

   JPNote NoteWithTheId(string noteId) => _collection.NoteFromNoteId(NoteId.Parse(noteId)) ?? throw new ArgumentException($"No note has the id {noteId}.", nameof(noteId));
}
