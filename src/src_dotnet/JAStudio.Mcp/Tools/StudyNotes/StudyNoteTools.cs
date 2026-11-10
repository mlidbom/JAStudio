using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>
/// The tools that compare the study-notes in Anki, the Anki notes that hold the notes' cards, with what JAStudio would write for them now, and write<br/>
/// those that differ (<see cref="IAnkiStudyNotes"/>). A change to the corpus made while Anki was closed, such as a fast-forward of the corpus checkout<br/>
/// that changes how notes are stored, reaches the study-notes only through them.
/// </summary>
[McpServerToolType]
public class StudyNoteTools
{
   /// <summary>How many notes of each type a tool lists unless asked for another number.</summary>
   const int NotesListedPerTypeByDefault = 10;

   readonly JPCollection _collection;
   readonly IAnkiStudyNotes _studyNotes;

   public StudyNoteTools(JPCollection collection, IAnkiStudyNotes studyNotes)
   {
      _collection = collection;
      _studyNotes = studyNotes;
   }

   [McpServerTool(Name = "list_the_study_notes_that_differ", ReadOnly = true, Idempotent = true)]
   [Description("""
                Compares every study-note in Anki, the Anki note holding a note's cards, with the study-note JAStudio would write for the note now:
                the fields JAStudio writes (the question, the audio and the note's id) and the tags. JAStudio writes a note's study-note whenever it
                updates the note, but a reload passes on only the notes whose files changed while it held them, and none at startup, so a change to
                the corpus made while Anki was closed, or a change to what JAStudio writes, leaves study-notes out of date until
                write_the_study_notes_that_differ writes them. Text is compared as Anki stores it, in Unicode normalization form C, and tags
                ignoring case and order, as Anki keeps them. In the headless JAStudio, Anki's latest changes may still be in its database's
                write-ahead log, which it doesn't read. Reports how many study-notes were compared, in how many each field and the tags differ,
                the notes whose study-note differs, by type, each with what differs, and how many study-notes in Anki belong to no note of the
                collection.
                """)]
   public StudyNotesThatDiffer ListTheStudyNotesThatDiffer(
      [Description("The most notes of each type to list. Every note whose study-note differs is counted either way.")] int maxNotesListedPerType = NotesListedPerTypeByDefault) =>
      StudyNotesThatDiffer.In(Comparison(), maxNotesListedPerType);

   [McpServerTool(Name = "write_the_study_notes_that_differ", Destructive = false, Idempotent = true)]
   [Description("""
                Writes the study-note in Anki of every note whose study-note differs from what JAStudio would write now, as
                list_the_study_notes_that_differ finds them, or of the notes given, whether theirs differ or not: as JAStudio writes a note's
                study-note whenever it updates the note. Only in the live JAStudio, inside Anki: the headless one has no Anki and refuses. Reports
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
