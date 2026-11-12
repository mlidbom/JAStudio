using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE.LinqCE;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// One of a note cache's indexes: its notes by keys taken from each note's snapshot, such as vocab by form. Any number of threads look notes<br/>
/// up in it at once, without locking.
/// </summary>
/// <remarks>
/// Parsing looks vocab up all the time, from many threads, while the cache changes seldom. When every lookup took the cache's lock, parsing<br/>
/// on 16 threads spent about a third of its time waiting for it.
/// </remarks>
/// <remarks>
/// The cache changes the index one change at a time, under its lock, and a change never touches a set of notes a lookup may be reading: the<br/>
/// first time a change touches a key, it copies the key's set, and changes the copy. <see cref="PublishChanges"/> then puts the copies in place.<br/>
/// So a lookup sees a key's notes as they were before a change or after it, never in the middle of it. Copying a set keeps its order, so the<br/>
/// notes come in the order they did when the sets were changed in place.
/// </remarks>
sealed class NoteIndex<TKey, TNote, TSnapshot> : INoteIndex<TNote, TSnapshot>
   where TKey : notnull
   where TNote : JPNote
{
   /// <summary>What lookups read: each key's notes, a set no change touches once it's here.</summary>
   readonly ConcurrentDictionary<TKey, HashSet<TNote>> _published = new();

   /// <summary>The copies of the published sets that the change under way has touched, which <see cref="PublishChanges"/> puts in place.</summary>
   readonly Dictionary<TKey, HashSet<TNote>> _changedCopies = new();

   readonly Func<TSnapshot, IEnumerable<TKey>> _keysOf;
   readonly LookupRecorder<TNote> _lookupRecorder;

   /// <param name="keysOf">The keys a note is filed under, taken from its snapshot.</param>
   /// <param name="lookupRecorder">The recorder of the cache's lookups, which every lookup here passes through.</param>
   public NoteIndex(Func<TSnapshot, IEnumerable<TKey>> keysOf, LookupRecorder<TNote> lookupRecorder)
   {
      _keysOf = keysOf;
      _lookupRecorder = lookupRecorder;
   }

   /// <summary>The notes filed under <paramref name="key"/>: a lookup, recorded if a <see cref="LookupRecording{TNote}"/> runs.</summary>
   public List<TNote> NotesWith(TKey key) => _lookupRecorder.LookedUp(_published.TryGetValue(key, out var notes) ? [.. notes] : []);

   public void Add(TNote note, TSnapshot snapshot) => _keysOf(snapshot).ForEach(key => ChangedCopyOfTheNotesWith(key).Add(note));

   public void Remove(TNote note, TSnapshot snapshot) => _keysOf(snapshot).ForEach(key => ChangedCopyOfTheNotesWith(key).Remove(note));

   public void Update(TNote note, TSnapshot before, TSnapshot after)
   {
      var keysBefore = _keysOf(before).ToHashSet();
      var keysAfter = _keysOf(after).ToHashSet();
      keysBefore.Except(keysAfter).ForEach(key => ChangedCopyOfTheNotesWith(key).Remove(note));
      keysAfter.Except(keysBefore).ForEach(key => ChangedCopyOfTheNotesWith(key).Add(note));
   }

   public void Clear()
   {
      _changedCopies.Clear();
      _published.Clear();
   }

   public void PublishChanges()
   {
      foreach(var (key, notes) in _changedCopies) _published[key] = notes;
      _changedCopies.Clear();
   }

   HashSet<TNote> ChangedCopyOfTheNotesWith(TKey key)
   {
      if(_changedCopies.TryGetValue(key, out var changedCopy)) return changedCopy;

      changedCopy = _published.TryGetValue(key, out var published) ? new HashSet<TNote>(published) : []; // the copy constructor clones the set as it is, order included, where a collection expression would add the notes one by one
      _changedCopies[key] = changedCopy;
      return changedCopy;
   }
}
