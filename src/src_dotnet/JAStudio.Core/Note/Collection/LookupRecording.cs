using System;
using System.Collections.Generic;
using Compze.Threading;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// A recording of the notes a collection's lookups return, such as every vocab parsing a sentence looks at: the vocab of each form it tries,<br/>
/// whether it ends up matched or not. Started by <see cref="VocabCollection.StartRecordingTheVocabLookedUp"/>, and stopped by disposing it.
/// </summary>
/// <remarks>
/// It's what makes a small corpus that parses some sentences exactly as the whole corpus does: one holding every vocab the parsing of those<br/>
/// sentences looked up. Every lookup then returns the same vocab from either, so each sentence parses the same against both.
/// </remarks>
/// <remarks>
/// It records every lookup made while it runs, from any thread, under a lock of its own, and nothing once it's stopped. Read<br/>
/// <see cref="NotesLookedUp"/> once it's stopped.
/// </remarks>
public sealed class LookupRecording<TNote> : IDisposable where TNote : JPNote
{
   readonly HashSet<TNote> _notesLookedUp = [];
   readonly IMonitor _monitor = IMonitor.New();
   readonly Action _stop;
   bool _stopped;

   internal LookupRecording(Action stop) => _stop = stop;

   /// <summary>Every note a lookup returned while this recording ran, each once.</summary>
   public IReadOnlyCollection<TNote> NotesLookedUp => _notesLookedUp;

   internal void Record(IEnumerable<TNote> notesLookedUp) => _monitor.Locked(() =>
   {
      if(!_stopped) _notesLookedUp.UnionWith(notesLookedUp);
   });

   /// <summary>Stops recording.</summary>
   public void Dispose()
   {
      _monitor.Locked(() => _stopped = true);
      _stop();
   }
}
