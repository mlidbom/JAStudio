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

   /// <summary>Every mapping held now, by external id: to give to <see cref="TakeIn"/> once the mappings are read from Anki again.</summary>
   public IReadOnlyDictionary<long, NoteId> Snapshot() => new Dictionary<long, NoteId>(_externalToNoteId);

   /// <summary>
   /// Takes in <paramref name="readFromAnki"/>, every mapping as read from Anki again, in place of <paramref name="heldWhenTheReadBegan"/>, the mappings<br/>
   /// held when that read began (<see cref="Snapshot"/>), one mapping at a time, so that the map is never empty meanwhile. A mapping registered since<br/>
   /// the read began, as when a study-note was created meanwhile, is kept, since the read can't have seen it.
   /// </summary>
   /// <returns>The notes whose study-note is gone from Anki: mapped when the read began, and no longer in <paramref name="readFromAnki"/>.</returns>
   public IReadOnlyList<NoteId> TakeIn(IReadOnlyDictionary<long, NoteId> readFromAnki, IReadOnlyDictionary<long, NoteId> heldWhenTheReadBegan)
   {
      List<NoteId> notesWhoseStudyNoteIsGone = [];
      foreach(var (externalId, noteId) in heldWhenTheReadBegan)
      {
         if(readFromAnki.ContainsKey(externalId)) continue;

         Unregister(externalId);
         notesWhoseStudyNoteIsGone.Add(noteId);
      }

      foreach(var (externalId, noteId) in readFromAnki)
         Register(externalId, noteId);

      return notesWhoseStudyNoteIsGone;
   }
}
