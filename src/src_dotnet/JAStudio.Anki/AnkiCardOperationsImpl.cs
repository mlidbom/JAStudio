using JAStudio.Core.Note;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>
/// Implementation of ICardOperations that uses AnkiFacade to call Python/Anki APIs.
/// Maps domain NoteId to Anki long ID at the boundary.
/// </summary>
public class AnkiCardOperationsImpl : ICardOperations
{
   readonly StudyNoteIds _studyNoteIds;

   public AnkiCardOperationsImpl(StudyNoteIds studyNoteIds) => _studyNoteIds = studyNoteIds;

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
   public void DeleteNote(NoteId noteId)
   {
      var studyNoteId = _studyNoteIds.StudyNoteIdOf(noteId);
      if(!studyNoteId.HasValue) return;

      AnkiFacade.NoteEx.RemoveNote(studyNoteId.Value);
      AnkiFacade.Browser.ExecuteLookup("");
   }
}
