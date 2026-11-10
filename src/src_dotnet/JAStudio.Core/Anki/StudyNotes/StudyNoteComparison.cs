using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Anki.StudyNotes;

/// <summary>
/// Every study-note in Anki compared with the one JAStudio would write for its note now: which differ, and how<br/>
/// (<see cref="StudyNoteInAnki.DifferencesFromTheStudyNoteOf"/>).
/// </summary>
public sealed class StudyNoteComparison
{
   readonly List<NoteWhoseStudyNoteDiffers> _notesWhoseStudyNoteDiffers = [];
   readonly List<StudyNoteInAnki> _studyNotesOfNoNote = [];

   /// <summary>Compares each of <paramref name="studyNotesInAnki"/> with the study-note JAStudio would write for its note in <paramref name="collection"/>.</summary>
   public StudyNoteComparison(JPCollection collection, IReadOnlyList<StudyNoteInAnki> studyNotesInAnki)
   {
      foreach(var studyNote in studyNotesInAnki)
      {
         if(collection.NoteFromExternalId(studyNote.AnkiNoteId) is not {} note)
         {
            _studyNotesOfNoNote.Add(studyNote);
            continue;
         }

         NotesCompared++;
         var differences = studyNote.DifferencesFromTheStudyNoteOf(note);
         if(differences.Any) _notesWhoseStudyNoteDiffers.Add(new NoteWhoseStudyNoteDiffers(note, differences));
      }
   }

   /// <summary>How many study-notes were compared: those whose note the collection has.</summary>
   public int NotesCompared { get; }

   public IReadOnlyList<NoteWhoseStudyNoteDiffers> NotesWhoseStudyNoteDiffers => _notesWhoseStudyNoteDiffers;

   /// <summary>
   /// The study-notes in Anki whose note the collection hasn't, such as one added in Anki rather than by JAStudio, or one whose note was deleted<br/>
   /// with its Anki note kept. Nothing compares them.
   /// </summary>
   public IReadOnlyList<StudyNoteInAnki> StudyNotesOfNoNote => _studyNotesOfNoNote;
}
