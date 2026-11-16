using System.Collections.Generic;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Studying.StudyNotes;

/// <summary>
/// Every study-note in the spaced repetition system compared with the one JAStudio would write for its note now: which differ, and in which<br/>
/// fields (<see cref="StudyNoteInTheSRS.FieldsThatDifferFromTheStudyNoteOf"/>). A study-note's note is the one whose id it holds<br/>
/// (<see cref="StudyNoteInTheSRS.NoteId"/>).
/// </summary>
public sealed class StudyNoteComparison
{
   readonly List<NoteWhoseStudyNoteDiffers> _notesWhoseStudyNoteDiffers = [];
   readonly List<StudyNoteInTheSRS> _studyNotesThatHoldNoNotesId = [];
   readonly List<StudyNoteInTheSRS> _studyNotesWhoseNoteTheCollectionHasNot = [];

   /// <summary>
   /// Compares each of <paramref name="studyNotesInTheSRS"/> with the study-note JAStudio would write for its note in<br/>
   /// <paramref name="collection"/>.
   /// </summary>
   public StudyNoteComparison(JPCollection collection, IReadOnlyList<StudyNoteInTheSRS> studyNotesInTheSRS)
   {
      foreach(var studyNote in studyNotesInTheSRS)
      {
         if(studyNote.NoteId is not {} noteId)
         {
            _studyNotesThatHoldNoNotesId.Add(studyNote);
            continue;
         }

         if(collection.NoteFromNoteId(noteId) is not {} note)
         {
            _studyNotesWhoseNoteTheCollectionHasNot.Add(studyNote);
            continue;
         }

         NotesCompared++;
         var fieldsThatDiffer = studyNote.FieldsThatDifferFromTheStudyNoteOf(note);
         if(fieldsThatDiffer.Count > 0) _notesWhoseStudyNoteDiffers.Add(new NoteWhoseStudyNoteDiffers(note, fieldsThatDiffer));
      }
   }

   /// <summary>How many study-notes were compared: those whose note the collection has.</summary>
   public int NotesCompared { get; }

   public IReadOnlyList<NoteWhoseStudyNoteDiffers> NotesWhoseStudyNoteDiffers => _notesWhoseStudyNoteDiffers;

   /// <summary>
   /// The study-notes in the spaced repetition system that hold no note's id, as no study-note JAStudio wrote does: such as an Anki note of one of<br/>
   /// JAStudio's note types added in Anki itself. A note can't be made of one (Magnus, 2026-11-15: "This is some sort of data corruption").<br/>
   /// Nothing compares them.
   /// </summary>
   public IReadOnlyList<StudyNoteInTheSRS> StudyNotesThatHoldNoNotesId => _studyNotesThatHoldNoNotesId;

   /// <summary>
   /// The study-notes in the spaced repetition system that hold the id of a note the collection hasn't, such as one whose note was deleted with<br/>
   /// its study-note kept. Nothing compares them.
   /// </summary>
   public IReadOnlyList<StudyNoteInTheSRS> StudyNotesWhoseNoteTheCollectionHasNot => _studyNotesWhoseNoteTheCollectionHasNot;

   /// <summary>
   /// The study-notes in the spaced repetition system that belong to no note of the collection: <see cref="StudyNotesThatHoldNoNotesId"/>, then<br/>
   /// <see cref="StudyNotesWhoseNoteTheCollectionHasNot"/>.
   /// </summary>
   public IReadOnlyList<StudyNoteInTheSRS> StudyNotesOfNoNote => [.. _studyNotesThatHoldNoNotesId, .. _studyNotesWhoseNoteTheCollectionHasNot];
}
