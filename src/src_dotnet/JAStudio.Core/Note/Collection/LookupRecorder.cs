using System.Collections.Generic;
using Compze.Contracts;
using Compze.Threading;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// The part of a note cache that records what its lookups return while a <see cref="LookupRecording{TNote}"/> runs: the cache passes each<br/>
/// lookup's result through <see cref="LookedUp(List{TNote})"/>. One recording runs at a time.
/// </summary>
/// <remarks>
/// It shares the cache's lock, so a recording starts and stops between two lookups, never during one.
/// </remarks>
/// <param name="cacheMonitor">The lock of the cache whose lookups this records, which the cache holds while it looks up.</param>
sealed class LookupRecorder<TNote>(IMonitor cacheMonitor) where TNote : JPNote
{
   readonly IMonitor _cacheMonitor = cacheMonitor;
   LookupRecording<TNote>? _runningRecording;

   /// <summary>Starts recording what the cache's lookups return, until the recording is disposed.</summary>
   public LookupRecording<TNote> StartRecording() => _cacheMonitor.Locked(() =>
   {
      Contract.State.Assert(_runningRecording == null);
      _runningRecording = new LookupRecording<TNote>(() => _cacheMonitor.Locked(() => _runningRecording = null));
      return _runningRecording;
   });

   /// <summary>Returns <paramref name="notes"/>, a lookup's result, recording them first if a recording runs. Called while the cache is locked.</summary>
   public List<TNote> LookedUp(List<TNote> notes)
   {
      _runningRecording?.Record(notes);
      return notes;
   }

   /// <summary>Returns <paramref name="note"/>, a lookup's result, recording it first if a recording runs. Called while the cache is locked.</summary>
   public TNote? LookedUp(TNote? note)
   {
      if(note != null) _runningRecording?.Record([note]);
      return note;
   }
}
