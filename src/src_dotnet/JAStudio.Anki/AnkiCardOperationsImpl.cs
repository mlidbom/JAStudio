using JAStudio.Core.Note;

namespace JAStudio.Anki;

/// <summary>
/// Implementation of ICardOperations that uses AnkiFacade to call Python/Anki APIs.
/// Maps domain NoteId to Anki long ID at the boundary.
/// </summary>
public class AnkiCardOperationsImpl : ICardOperations
{
   readonly ExternalNoteIdMap _idMap;

   public AnkiCardOperationsImpl(ExternalNoteIdMap idMap) => _idMap = idMap;

   public void SuspendAllCardsForNote(NoteId noteId)
   {
      AnkiFacade.NoteEx.SuspendAllCardsForNote(_idMap.RequireExternalId(noteId));
   }

   public void UnsuspendAllCardsForNote(NoteId noteId)
   {
      AnkiFacade.NoteEx.UnsuspendAllCardsForNote(_idMap.RequireExternalId(noteId));
   }

   /// <summary>
   /// Removes the note's Anki note, if it has one, then resets the search of Anki's browser, which would otherwise fail when it refreshes the note<br/>
   /// that no longer exists.
   /// </summary>
   public void DeleteNote(NoteId noteId)
   {
      var externalId = _idMap.ToExternalId(noteId);
      if(!externalId.HasValue) return;

      AnkiFacade.NoteEx.RemoveNote(externalId.Value);
      AnkiFacade.Browser.ExecuteLookup("");
   }
}
