using System;
using System.Collections.Generic;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// A <see cref="NoteCacheBase{TNote}"/> with indexes: each a <see cref="NoteIndex{TKey,TNote,TSnapshot}"/> of the notes by keys taken from each<br/>
/// note's snapshot, which the cache refiles a note in whenever the note changes.
/// </summary>
abstract class NoteCache<TNote, TSnapshot> : NoteCacheBase<TNote>
   where TNote : JPNote
   where TSnapshot : CachedNote
{
   readonly List<INoteIndex<TNote, TSnapshot>> _indexes = [];

   /// <summary>Each cached note's snapshot as it was filed: only changes read it, under the cache's lock.</summary>
   readonly Dictionary<NoteId, TSnapshot> _snapshotById = new();

   readonly NoteIndex<string, TNote, TSnapshot> _byQuestion;

   protected NoteCache(NoteServices noteServices) : base(noteServices) => _byQuestion = NewIndex<string>(snapshot => [snapshot.Question]);

   public List<TNote> WithQuestion(string question) => _byQuestion.NotesWith(question);

   /// <summary>
   /// Creates an index of this cache's notes by the keys <paramref name="keysOf"/> takes from each note's snapshot, which the cache keeps up to<br/>
   /// date as its notes change.
   /// </summary>
   protected NoteIndex<TKey, TNote, TSnapshot> NewIndex<TKey>(Func<TSnapshot, IEnumerable<TKey>> keysOf) where TKey : notnull
   {
      var index = new NoteIndex<TKey, TNote, TSnapshot>(keysOf, LookupRecorder);
      _indexes.Add(index);
      return index;
   }

   protected abstract TSnapshot CreateSnapshot(TNote note);

   protected override void AddToCacheCore(TNote note)
   {
      var id = note.GetId();

      if(_snapshotById.ContainsKey(id))
         throw new InvalidOperationException($"Cannot add {typeof(TNote).Name} with id {id} to cache: already present. Use RefreshInCache for updates.");

      var snapshot = CreateSnapshot(note);
      _snapshotById[id] = snapshot;
      ById.Set(note);
      _indexes.ForEach(it => it.Add(note, snapshot));

      note.MarkAsPersisted();
   }

   protected override void RemoveFromCacheCore(TNote note)
   {
      var id = note.GetId();

      if(!_snapshotById.Remove(id, out var snapshot))
         throw new InvalidOperationException($"Cannot remove {typeof(TNote).Name} with id {id} from cache: not found in snapshot index");

      ById.Remove(id);
      _indexes.ForEach(it => it.Remove(note, snapshot));
   }

   protected override void RefreshInCacheCore(TNote note)
   {
      var id = note.GetId();

      if(!_snapshotById.TryGetValue(id, out var snapshotBefore))
         throw new InvalidOperationException($"JpNoteUpdated called for {typeof(TNote).Name} with id {id} but it is not in the cache. Only persisted (cached) notes should flush.");

      var snapshotAfter = CreateSnapshot(note);
      _snapshotById[id] = snapshotAfter;
      ById.Set(note);
      _indexes.ForEach(it => it.Update(note, snapshotBefore, snapshotAfter));

      note.MarkAsPersisted();
   }

   protected override void ClearIndexes()
   {
      _snapshotById.Clear();
      _indexes.ForEach(it => it.Clear());
   }

   protected override void PublishChangesToTheIndexes() => _indexes.ForEach(it => it.PublishChanges());
}
