using System.Collections.Generic;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Studying.StudyNotes;

/// <summary>
/// Every study-note in the spaced repetition system compared with the one JAStudio would write for its note now: which differ, and in which<br/>
/// fields (<see cref="StudyNoteInTheSpacedRepetitionSystem.FieldsThatDifferFromTheStudyNoteOf"/>).
/// </summary>
public sealed class StudyNoteComparison
{
   readonly List<NoteWhoseStudyNoteDiffers> _notesWhoseStudyNoteDiffers = [];
   readonly List<StudyNoteInTheSpacedRepetitionSystem> _studyNotesOfNoNote = [];

   /// <summary>
   /// Compares each of <paramref name="studyNotesInTheSpacedRepetitionSystem"/> with the study-note JAStudio would write for its note in<br/>
   /// <paramref name="collection"/>.
   /// </summary>
   public StudyNoteComparison(JPCollection collection, IReadOnlyList<StudyNoteInTheSpacedRepetitionSystem> studyNotesInTheSpacedRepetitionSystem)
   {
      foreach(var studyNote in studyNotesInTheSpacedRepetitionSystem)
      {
         if(collection.NoteOfTheStudyNote(studyNote.StudyNoteId) is not {} note)
         {
            _studyNotesOfNoNote.Add(studyNote);
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
   /// The study-notes in the spaced repetition system whose note the collection hasn't, such as one added in Anki rather than by JAStudio, or one<br/>
   /// whose note was deleted with its study-note kept. Nothing compares them.
   /// </summary>
   public IReadOnlyList<StudyNoteInTheSpacedRepetitionSystem> StudyNotesOfNoNote => _studyNotesOfNoNote;
}
