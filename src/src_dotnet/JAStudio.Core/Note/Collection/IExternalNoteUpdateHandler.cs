using System;

// ReSharper disable UnusedMember.Global used from python
namespace JAStudio.Core.Note.Collection;

public interface IExternalNoteUpdateHandler
{
   void ExternalNoteAdded(long externalNoteId, AnkiNoteData data);
   void ExternalNoteWillFlush(long externalNoteId, AnkiNoteData data);
   void ExternalNoteRemoved(long externalNoteId);

   /// <summary>Notifies every updated note of this handler's note type, on the thread that updated it.</summary>
   IObservable<JPNote> NoteUpdated { get; }

   long GetExternalNoteId(NoteId noteId);
}
