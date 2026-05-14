using System;

// ReSharper disable UnusedMember.Global used from python
namespace JAStudio.Core.Note.Collection;

public interface IExternalNoteUpdateHandler
{
   void ExternalNoteAdded(long externalNoteId, AnkiNoteData data);
   void ExternalNoteWillFlush(long externalNoteId, AnkiNoteData data);
   void ExternalNoteRemoved(long externalNoteId);
   void OnNoteUpdated(Action<JPNote> listener);
   long GetExternalNoteId(NoteId noteId);
}
