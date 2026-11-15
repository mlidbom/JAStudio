using JAStudio.Core.Note;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>
/// The changes JAStudio makes to a note's cards and study-note (<see cref="ISRSCardOperations"/>), made in the collection<br/>
/// Anki has open, through Python (<see cref="AnkiFacade"/>). A note's study-note is its Anki note, found by its id (<see cref="StudyNoteIds"/>).
/// </summary>
public class AnkiCardOperations : ISRSCardOperations
{
   readonly StudyNoteIds _studyNoteIds;

   public AnkiCardOperations(StudyNoteIds studyNoteIds) => _studyNoteIds = studyNoteIds;

   public void SuspendAllCardsForNote(NoteId noteId)
   {
      AnkiFacade.NoteEx.SuspendAllCardsForNote(_studyNoteIds.RequiredStudyNoteIdOf(noteId));
   }

   public void UnsuspendAllCardsForNote(NoteId noteId)
   {
      AnkiFacade.NoteEx.UnsuspendAllCardsForNote(_studyNoteIds.RequiredStudyNoteIdOf(noteId));
   }

   /// <summary>
   /// Removes the note's Anki note, if it has one, then resets the search of Anki's browser, which would otherwise fail when it refreshes the note<br/>
   /// that no longer exists.
   /// </summary>
   public void DeleteTheStudyNoteOf(NoteId noteId)
   {
      var studyNoteId = _studyNoteIds.StudyNoteIdOf(noteId);
      if(!studyNoteId.HasValue) return;

      AnkiFacade.NoteEx.RemoveNote(studyNoteId.Value);
      AnkiFacade.Browser.ExecuteLookup("");
   }
}
