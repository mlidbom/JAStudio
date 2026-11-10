using System;
using System.Collections.Generic;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Threading;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.PythonInterop;

namespace JAStudio.Anki.PythonInterop;

/// <summary>
/// The writes of the notes' study-notes into Anki, one each time a note is updated, made by Python: held while Anki's collection can't take them, during<br/>
/// a sync and while no Anki profile is open, and made once it can again. Held, a note's write is made once, however often the note was updated meanwhile.
/// </summary>
/// <remarks>
/// A note can be edited in JAStudio during a sync, since the collection stays loaded, but a sync holds Anki's collection, and a full sync replaces it, so<br/>
/// a study-note written then would block until the sync ends, or fail.
/// </remarks>
public sealed class StudyNoteWrites
{
   readonly IMonitor _monitor = IMonitor.New();
   readonly Dictionary<NoteId, (JPNote Note, Action<JPNote> Write)> _held = [];
   bool _theyAreHeld;

   /// <summary>
   /// Makes <paramref name="writeTheStudyNote"/>, a Python callable, write the study-note of each note of <paramref name="handler"/>'s note type that is<br/>
   /// updated, for as long as Anki runs: at once, or, while the writes are held, once they're made again.
   /// </summary>
   // ReSharper disable once UnusedMember.Global used from python
   public void Register(IExternalNoteUpdateHandler handler, dynamic writeTheStudyNote)
   {
      Action<JPNote> write = PythonDotNetShim.Action.ToDotNet<JPNote>(writeTheStudyNote);
      handler.NoteUpdated.Subscribe(note => WriteOrHold(note, write));
   }

   /// <summary>Holds every write from now on, as when a sync starts or the Anki profile closes.</summary>
   public void Hold() => _monitor.Locked(() => _theyAreHeld = true);

   /// <summary>Makes the writes held, and every write from now on, as once a sync has finished, or an Anki profile has opened.</summary>
   public void MakeThem()
   {
      var held = _monitor.Locked(() =>
      {
         _theyAreHeld = false;
         List<(JPNote Note, Action<JPNote> Write)> writes = [.._held.Values];
         _held.Clear();
         return writes;
      });

      held.ForEach(it => it.Write(it.Note));
   }

   void WriteOrHold(JPNote note, Action<JPNote> write)
   {
      var itIsHeld = _monitor.Locked(() =>
      {
         if(_theyAreHeld) _held[note.GetId()] = (note, write);
         return _theyAreHeld;
      });

      if(!itIsHeld) write(note);
   }
}
