using System;
using System.Collections.Generic;
using Compze.Internals.SystemCE.LinqCE;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Threading;
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

abstract class NoteCacheBase<TNote> : IExternalNoteUpdateHandler
   where TNote : JPNote
{
   protected readonly Dictionary<NoteId, TNote> _byId = new();
   readonly SimpleObservable<TNote> _noteUpdated = new();
   readonly NoteServices _noteServices;
   protected readonly IMonitor _monitor = IMonitor.New();
   readonly LookupRecorder<TNote> _lookupRecorder;

   protected NoteCacheBase(NoteServices noteServices)
   {
      _noteServices = noteServices;
      _lookupRecorder = new LookupRecorder<TNote>(_monitor);
   }

   /// <summary>
   /// Notifies every note in this cache that JAStudio updates (<see cref="JpNoteUpdated"/>), after the cache holds the updated note.<br/>
   /// It's notified on the thread that made the update. A note saved in Anki isn't taken in: everything in it is edited in JAStudio.
   /// </summary>
   public IObservable<TNote> NoteUpdated => _noteUpdated;

   IObservable<JPNote> IExternalNoteUpdateHandler.NoteUpdated => _noteUpdated;

   /// <summary>Starts recording the notes this cache's lookups return, until the recording is disposed. One recording at a time.</summary>
   public LookupRecording<TNote> StartRecordingTheNotesLookedUp() => _lookupRecorder.StartRecording();

   public List<TNote> All() => _monitor.Locked(() => _lookupRecorder.LookedUp([.. _byId.Values]));

   public TNote? WithIdOrNone(NoteId noteId) => _monitor.Locked(() => _lookupRecorder.LookedUp(WithIdOrNoneCore(noteId)));
   protected TNote? WithIdOrNoneCore(NoteId noteId) => _byId.GetValueOrDefault(noteId);

   /// <summary>The notes <paramref name="index"/> holds under <paramref name="key"/>: a lookup, recorded if a <see cref="LookupRecording{TNote}"/> runs.</summary>
   protected List<TNote> LookUp(Dictionary<string, HashSet<TNote>> index, string key) =>
      _monitor.Locked(() => _lookupRecorder.LookedUp(index.TryGetValue(key, out var notes) ? [.. notes] : []));

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

   /// <summary>
   /// Called when a note is deleted in Anki: the note's Anki note is forgotten, and with it its cards, so the note is no longer studied in this Anki profile.<br/>
   /// The note itself stays in the corpus and in JAStudio. Deleting it from the corpus is <see cref="JPCollection.Delete"/>.
   /// </summary>
   /// <remarks>
   /// Every cache is told about every note deleted in Anki, so a note this cache doesn't hold is left to the cache that does.
   /// </remarks>
   public void ExternalNoteRemoved(long externalNoteId)
   {
      if(WithExternalIdOrNone(externalNoteId) is not {} note) return;

      _noteServices.ExternalNoteIdMap.Unregister(externalNoteId);
      note.ForgetTheCardsOfItsDeletedAnkiNote();
   }

   public void JpNoteUpdated(TNote note)
   {
      _monitor.Locked(() =>
      {
         var existing = WithIdOrNoneCore(note.GetId());
         if(existing == null)
            throw new InvalidOperationException($"JpNoteUpdated called for {typeof(TNote).Name} with id {note.GetId()} but it is not in the cache. Only persisted (cached) notes should flush.");

         RefreshInCacheCore(note);
      });

      _noteUpdated.OnNext(note);
   }

   void RefreshInCacheCore(TNote note)
   {
      RemoveFromCacheCore(note);
      AddToCacheCore(note);
   }

   /// <summary>Removes all notes and ID mappings from the cache. Subclasses must override ClearInheritorIndexes to clear their custom indexes.</summary>
   public void Clear() => _monitor.Locked(() =>
   {
      _byId.Clear();
      ClearInheritorIndexes();
   });

   protected abstract void ClearInheritorIndexes();

   public void AddToCache(TNote note) => _monitor.Locked(() => AddToCacheCore(note));
   public void AddAllToCache(IEnumerable<TNote> notes) => _monitor.Locked(() => notes.ForEach(AddToCacheCore));
   public void RemoveFromCache(TNote note) => _monitor.Locked(() => RemoveFromCacheCore(note));

   protected abstract void AddToCacheCore(TNote note);
   protected abstract void RemoveFromCacheCore(TNote note);

   public void SetStudyingStatuses(Dictionary<long, List<CardStudyingStatus>> statusesByExternalId)
   {
      _monitor.Locked(() =>
      {
         foreach(var (externalId, statuses) in statusesByExternalId)
         {
            var note = WithExternalIdOrNoneCore(externalId);
            if(note == null) continue;

            foreach(var status in statuses)
               note.SetStudyingStatus(status);
         }
      });
   }
}

abstract class NoteCache<TNote, TSnapshot>(NoteServices noteServices) : NoteCacheBase<TNote>(noteServices)
   where TNote : JPNote
   where TSnapshot : CachedNote
{
   readonly Dictionary<string, HashSet<TNote>> _byQuestion = new();
   readonly Dictionary<NoteId, TSnapshot> _snapshotById = new();

   public List<TNote> WithQuestion(string question) => LookUp(_byQuestion, question);

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
