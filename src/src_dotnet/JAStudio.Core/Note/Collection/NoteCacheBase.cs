using System;
using System.Collections.Generic;
using Compze.Internals.SystemCE.LinqCE;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Threading;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// The notes of one type that JAStudio holds in memory, by id, with the indexes of <see cref="NoteCache{TNote,TSnapshot}"/> to look them up by.<br/>
/// Any number of threads look notes up at once, without locking; the cache changes one change at a time, under its lock.
/// </summary>
/// <remarks>
/// A change is made to copies of what lookups read, and published when it's done (<see cref="Change"/>): a lookup sees the cache as it was<br/>
/// before a change or after it, never in the middle of one. See <see cref="NoteIndex{TKey,TNote,TSnapshot}"/> for why.
/// </remarks>
abstract class NoteCacheBase<TNote> : IStudyNoteUpdateHandler
   where TNote : JPNote
{
   readonly SimpleObservable<TNote> _noteUpdated = new();
   readonly NoteServices _noteServices;
   readonly IMonitor _monitor = IMonitor.New();

   protected NoteCacheBase(NoteServices noteServices)
   {
      _noteServices = noteServices;
      LookupRecorder = new LookupRecorder<TNote>(_monitor);
   }

   protected NotesById<TNote> ById { get; } = new();

   /// <summary>The recorder every lookup in this cache passes its result through.</summary>
   protected LookupRecorder<TNote> LookupRecorder { get; }

   /// <summary>
   /// Notifies every note in this cache that JAStudio updates (<see cref="JpNoteUpdated"/>), after the cache holds the updated note.<br/>
   /// It's notified on the thread that made the update. A note saved in Anki isn't taken in: everything in it is edited in JAStudio.
   /// </summary>
   public IObservable<TNote> NoteUpdated => _noteUpdated;

   IObservable<JPNote> IStudyNoteUpdateHandler.NoteUpdated => _noteUpdated;

   /// <summary>Starts recording the notes this cache's lookups return, until the recording is disposed. One recording at a time.</summary>
   public LookupRecording<TNote> StartRecordingTheNotesLookedUp() => LookupRecorder.StartRecording();

   public List<TNote> All() => LookupRecorder.LookedUp(ById.All());

   public TNote? WithIdOrNone(NoteId noteId) => LookupRecorder.LookedUp(ById.WithIdOrNone(noteId));

   /// <summary>The note in this cache that the study-note with the id <paramref name="studyNoteId"/> belongs to, or null when this cache has none such.</summary>
   public TNote? WithTheStudyNoteOrNone(long studyNoteId) =>
      _noteServices.StudyNoteIds.NoteIdOfTheStudyNote(studyNoteId) is {} noteId ? WithIdOrNone(noteId) : null;

   /// <summary>The id of the study-note of the note with the id <paramref name="noteId"/>, or 0 when it has none.</summary>
   public long GetStudyNoteId(NoteId noteId) =>
      _noteServices.StudyNoteIds.StudyNoteIdOf(noteId) ?? 0;

   /// <summary>
   /// Called when a note's study-note is deleted in Anki: the study-note is forgotten, and with it its cards, so the note is no longer studied in this<br/>
   /// Anki profile.<br/>
   /// The note itself stays in the corpus and in JAStudio. Deleting it from the corpus is <see cref="JPCollection.Delete"/>.
   /// </summary>
   /// <remarks>
   /// Every cache is told about every note deleted in Anki, so a note this cache doesn't hold is left to the cache that does.
   /// </remarks>
   public void StudyNoteRemoved(long studyNoteId)
   {
      if(WithTheStudyNoteOrNone(studyNoteId) is not {} note) return;

      _noteServices.StudyNoteIds.Unregister(studyNoteId);
      note.ForgetItsDeletedStudyNote();
   }

   /// <summary>Refiles <paramref name="note"/>, which JAStudio changed, by what it holds now, then notifies <see cref="NoteUpdated"/>.</summary>
   public void JpNoteUpdated(TNote note)
   {
      Change(() => RefreshInCacheCore(note));
      _noteUpdated.OnNext(note);
   }

   /// <summary>Removes all notes and ID mappings from the cache.</summary>
   public void Clear() => Change(() =>
   {
      ById.Clear();
      ClearIndexes();
   });

   public void AddToCache(TNote note) => Change(() => AddToCacheCore(note));
   public void AddAllToCache(IEnumerable<TNote> notes) => Change(() => notes.ForEach(AddToCacheCore));
   public void RemoveFromCache(TNote note) => Change(() => RemoveFromCacheCore(note));

   /// <summary>Makes <paramref name="change"/> under the cache's lock, then publishes it to lookups, all at once.</summary>
   void Change(Action change) => _monitor.Locked(() =>
   {
      change();
      ById.PublishChanges();
      PublishChangesToTheIndexes();
   });

   protected abstract void AddToCacheCore(TNote note);
   protected abstract void RemoveFromCacheCore(TNote note);
   protected abstract void RefreshInCacheCore(TNote note);
   protected abstract void ClearIndexes();
   protected abstract void PublishChangesToTheIndexes();
}
