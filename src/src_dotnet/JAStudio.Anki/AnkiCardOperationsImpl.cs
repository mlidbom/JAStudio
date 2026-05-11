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

   public void DeleteNote(NoteId noteId)
   {
      var externalId = _idMap.ToExternalId(noteId);
      if(externalId.HasValue)
         AnkiFacade.NoteEx.RemoveNote(externalId.Value);
   }
}
