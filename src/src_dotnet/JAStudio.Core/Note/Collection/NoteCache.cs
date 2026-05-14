using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Utilities.SystemCE.LinqCE;
using Compze.Utilities.SystemCE.ThreadingCE.ResourceAccess;
using JAStudio.Core.SysUtils.Collections.Generic;

namespace JAStudio.Core.Note.Collection;

class CachedNote
{
   public NoteId Id { get; }
   public string Question { get; }

   public CachedNote(JPNote note)
   {
      Id = note.GetId();
      Question = note.GetQuestion();
   }
}

abstract class NoteCacheBase<TNote>(Func<NoteServices, NoteData, TNote> noteConstructor, NoteServices noteServices) : IExternalNoteUpdateHandler
   where TNote : JPNote
{
   readonly Func<NoteServices, NoteData, TNote> _noteConstructor = noteConstructor;
   protected readonly Dictionary<NoteId, TNote> _byId = new();
   readonly List<Action<TNote>> _updateListeners = [];
   readonly NoteServices _noteServices = noteServices;
   protected readonly IMonitorCE _monitor = IMonitorCE.WithDefaultTimeout();

   public void OnNoteUpdated(Action<JPNote> listener) => _updateListeners.Add(note => listener(note));

   public void OnNoteUpdated(Action<TNote> listener) => _updateListeners.Add(listener);

   public TNote? WithIdOrNone(NoteId noteId) => _monitor.Read(() => WithIdOrNoneCore(noteId));
   protected TNote? WithIdOrNoneCore(NoteId noteId) => _byId.TryGetValue(noteId, out var note) ? note : null;

   /// <summary>Look up a note by its external long ID (uses the shared ExternalNoteIdMap).</summary>
   public TNote? WithExternalIdOrNone(long externalNoteId) =>
      _noteServices.ExternalNoteIdMap.FromExternalId(externalNoteId) is {} noteId ? WithIdOrNone(noteId) : null;

   TNote? WithExternalIdOrNoneCore(long externalNoteId) =>
      _noteServices.ExternalNoteIdMap.FromExternalId(externalNoteId) is {} noteId ? WithIdOrNoneCore(noteId) : null;

   /// <summary>Converts an external long ID to the corresponding domain NoteId.</summary>
   public NoteId? ExternalIdToNoteId(long externalNoteId) =>
      _noteServices.ExternalNoteIdMap.FromExternalId(externalNoteId);

   /// <summary>Returns the external long note ID for the given domain NoteId.</summary>
   public long GetExternalNoteId(NoteId noteId) =>
      _noteServices.ExternalNoteIdMap.ToExternalId(noteId) ?? 0;

   public void ExternalNoteAdded(long externalNoteId, NoteData data)
   {
      // If we already registered this external ID (we created the note ourselves),
      // don't create a duplicate — the note is already in the cache.
      if(_noteServices.ExternalNoteIdMap.FromExternalId(externalNoteId) != null)
         return;

      data.Id = CreateTypedId(Guid.NewGuid());
      _noteServices.ExternalNoteIdMap.Register(externalNoteId, data.Id);
      var note = _noteConstructor(_noteServices, data);
      note.UpdateGeneratedData();
      AddToCache(note);
   }

   // ReSharper disable once FieldCanBeMadeReadOnly.Local
   bool _ankiStillOwnsNoFields = true;

   public void ExternalNoteWillFlush(long externalNoteId, NoteData data)
   {
      // TODO: Currently disabled: Anki owns no note fields (all authoritative data is in the file system repository). Consider whether to remove this and related code entirely
      if(_ankiStillOwnsNoFields) //Keeping the code below around and not detected as unreachable through this little hack
         return;

      var noteId = _noteServices.ExternalNoteIdMap.FromExternalId(externalNoteId);
      if(noteId == null) return;
      var existing = WithIdOrNone(noteId);
      if(existing == null) return;
      if(existing.IsFlushing) return; // Our code initiated this flush, nothing to do

      data.Id = noteId;
      var note = CreateNoteByMergingAnkiData(_noteServices, existing, data);
      note.CopyStudyingStatusFrom(existing);
      note.UpdateGeneratedData();
      _monitor.Read(() => RefreshInCacheCore(note));
      NotifyUpdateListeners(note);
   }

   public void ExternalNoteRemoved(long externalNoteId)
   {
      var noteId = _noteServices.ExternalNoteIdMap.FromExternalId(externalNoteId);
      if(noteId == null) return;
      _monitor.Read(() =>
      {
         var existing = WithIdOrNoneCore(noteId);
         if(existing != null)
         {
            RemoveFromCacheCore(existing);
         }
      });

      _noteServices.ExternalNoteIdMap.Unregister(externalNoteId);
   }

   /// <summary>Creates the correctly typed NoteId for this cache (VocabId, KanjiId, etc.)</summary>
   protected abstract NoteId CreateTypedId(Guid value);

   /// <summary>Creates a new note by merging Anki-owned fields into the existing note's data, preserving all fields Anki does not store.</summary>
   protected abstract TNote CreateNoteByMergingAnkiData(NoteServices services, TNote existing, NoteData ankiData);

   public void JpNoteUpdated(TNote note)
   {
      _monitor.Read(() =>
      {
         var existing = WithIdOrNoneCore(note.GetId());
         if(existing == null)
            throw new InvalidOperationException($"JpNoteUpdated called for {typeof(TNote).Name} with id {note.GetId()} but it is not in the cache. Only persisted (cached) notes should flush.");

         RefreshInCacheCore(note);
      });

      NotifyUpdateListeners(note);
   }

   void NotifyUpdateListeners(TNote note)
   {
      foreach(var listener in _updateListeners)
      {
         listener(note);
      }
   }

   void RefreshInCacheCore(TNote note)
   {
      RemoveFromCacheCore(note);
      AddToCacheCore(note);
   }

   /// <summary>Removes all notes and ID mappings from the cache. Subclasses must override ClearInheritorIndexes to clear their custom indexes.</summary>
   public void Clear() => _monitor.Read(() =>
   {
      _byId.Clear();
      ClearInheritorIndexes();
   });

   protected abstract void ClearInheritorIndexes();

   public void AddToCache(TNote note) => _monitor.Read(() => AddToCacheCore(note));
   public void AddAllToCache(IEnumerable<TNote> notes) => _monitor.Read(() => notes.ForEach(AddToCacheCore));
   public void RemoveFromCache(TNote note) => _monitor.Read(() => RemoveFromCacheCore(note));

   protected abstract void AddToCacheCore(TNote note);
   protected abstract void RemoveFromCacheCore(TNote note);

   public void SetStudyingStatuses(Dictionary<long, List<CardStudyingStatus>> statusesByExternalId)
   {
      _monitor.Read(() =>
      {
         foreach(var (externalId, statuses) in statusesByExternalId)
         {
            var note = WithExternalIdOrNoneCore(externalId);
            if(note != null)
            {
               foreach(var status in statuses)
                  note.SetStudyingStatus(status);
            }
         }
      });
   }
}

abstract class NoteCache<TNote, TSnapshot>(Func<NoteServices, NoteData, TNote> noteConstructor, NoteServices noteServices) : NoteCacheBase<TNote>(noteConstructor, noteServices)
   where TNote : JPNote
   where TSnapshot : CachedNote
{
   readonly Dictionary<string, HashSet<TNote>> _byQuestion = new();
   readonly Dictionary<NoteId, TSnapshot> _snapshotById = new();

   public List<TNote> All() => _monitor.Read(() => _byId.Values.ToList());

   public List<TNote> WithQuestion(string question) => _monitor.Read(() => _byQuestion.TryGetValue(question, out var notes) ? notes.ToList() : []);

   protected abstract TSnapshot CreateSnapshot(TNote note);
   protected abstract void InheritorRemoveFromCache(TNote note, TSnapshot snapshot);
   protected abstract void InheritorAddToCache(TNote note, TSnapshot snapshot);

   protected override void ClearInheritorIndexes()
   {
      _byQuestion.Clear();
      _snapshotById.Clear();
      ClearDerivedIndexes();
   }

   /// <summary>Override in leaf caches to clear type-specific secondary indexes.</summary>
   protected abstract void ClearDerivedIndexes();

   protected override void RemoveFromCacheCore(TNote note)
   {
      var id = note.GetId();

      if(!_snapshotById.Remove(id, out var snapshot))
         throw new InvalidOperationException($"Cannot remove {typeof(TNote).Name} with id {id} from cache: not found in snapshot index");

      _byId.Remove(id);

      _byQuestion.RemoveFromSet(snapshot.Question, note);

      InheritorRemoveFromCache(note, snapshot);
   }

   protected override void AddToCacheCore(TNote note)
   {
      var id = note.GetId();

      if(_snapshotById.ContainsKey(id))
         throw new InvalidOperationException($"Cannot add {typeof(TNote).Name} with id {id} to cache: already present. Use RefreshInCache for updates.");

      var snapshot = CreateSnapshot(note);
      _snapshotById[id] = snapshot;
      _byId[id] = note;

      _byQuestion.AddToSet(snapshot.Question, note);

      InheritorAddToCache(note, snapshot);

      note.MarkAsPersisted();
   }
}
