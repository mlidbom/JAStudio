using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JAStudio.Core.Note;

/// <summary>
/// Thread-safe bidirectional mapping between domain NoteIds and external long note IDs.
/// This is a pure data structure with no external dependencies — it lives in Core
/// so that caches and collections can use it without depending on any specific integration layer.
/// </summary>
public class ExternalNoteIdMap
{
   readonly ConcurrentDictionary<NoteId, long> _noteIdToExternal = new();
   readonly ConcurrentDictionary<long, NoteId> _externalToNoteId = new();

   public void Register(long externalId, NoteId noteId)
   {
      _externalToNoteId[externalId] = noteId;
      _noteIdToExternal[noteId] = externalId;
   }

   public NoteId? FromExternalId(long externalId) => _externalToNoteId.GetValueOrDefault(externalId);
   public long? ToExternalId(NoteId noteId) => _noteIdToExternal.TryGetValue(noteId, out var externalId) ? externalId : null;

   public long RequireExternalId(NoteId noteId) => _noteIdToExternal.TryGetValue(noteId, out var externalId)
                                                      ? externalId
                                                      : throw new KeyNotFoundException($"No external ID mapping found for NoteId {noteId}");


   public void Unregister(long externalId)
   {
      if(_externalToNoteId.TryRemove(externalId, out var noteId))
      {
         _noteIdToExternal.TryRemove(noteId, out _);
      }
   }

   public void Clear()
   {
      _noteIdToExternal.Clear();
      _externalToNoteId.Clear();
   }
}
