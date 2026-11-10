using System;
using System.Threading.Tasks;
using Compze.Threading;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.SysUtils.Threading;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Anki.PythonInterop;

/// <summary>
/// Keeps JAStudio caught up with the collection Anki has open. Whenever Anki may have changed it, JAStudio reads the study-notes from it again<br/>
/// (<see cref="JPCollection.ReadTheStudyNotesFromAnkiAgain"/>), or loads the collection the first time, then makes the <see cref="StudyNoteWrites"/><br/>
/// held meanwhile. It does so only while JAStudio may use Anki's collection: while an Anki profile is open, and no sync is under way.
/// </summary>
/// <remarks>
/// Anki may change its collection under JAStudio when an operation changes notes, when a sync finishes, and when another profile opens. Anki announces<br/>
/// each operation through its <c>operation_did_execute</c> hook, undo and redo included, and a sync too: once a sync finishes, Anki fires that hook<br/>
/// with every kind of change flagged (<c>mw.reset()</c>), as its own screens need it to. So a sync needs no catching up of its own. A profile opening<br/>
/// isn't announced so, and catches up itself.
/// </remarks>
public sealed class CatchingUpWithAnki
{
   /// <summary>How long a profile that opened waits before catching up: Anki syncs as a profile opens, when set to, and the sync starts just after.</summary>
   static readonly TimeSpan DelayAfterAProfileOpened = TimeSpan.FromMilliseconds(500);

   readonly IMonitor _monitor = IMonitor.New();
   readonly JPCollection _collection;
   readonly StudyNoteWrites _studyNoteWrites;
   readonly BackgroundTaskManager _backgroundTasks;
   readonly BackgroundRefresh _catchingUp;
   bool _aProfileIsOpen;
   bool _aSyncIsUnderWay;
   bool _theCollectionWasLoaded;

   /// <param name="collection">The collection kept caught up.</param>
   /// <param name="studyNoteWrites">The writes held while JAStudio may not use Anki's collection, and made once it has caught up.</param>
   /// <param name="backgroundTasks">Runs the catching up in the background, and surfaces what it throws.</param>
   public CatchingUpWithAnki(JPCollection collection, StudyNoteWrites studyNoteWrites, BackgroundTaskManager backgroundTasks)
   {
      _collection = collection;
      _studyNoteWrites = studyNoteWrites;
      _backgroundTasks = backgroundTasks;
      _catchingUp = new BackgroundRefresh(backgroundTasks, CatchUp);
   }

   /// <summary>Takes in that a profile opened or is closing, or that a sync is starting or finished, as Anki tells JAStudio.</summary>
   public void Handle(AnkiLifecycleEvent lifecycleEvent)
   {
      switch(lifecycleEvent)
      {
         case AnkiLifecycleEvent.ProfileOpened:
            _monitor.Locked(() => _aProfileIsOpen = true);
            CatchUpOnceTheDelayAfterAProfileOpenedIsOver();
            break;

         case AnkiLifecycleEvent.ProfileClosing:
            _monitor.Locked(() => _aProfileIsOpen = false);
            _studyNoteWrites.Hold();
            break;

         case AnkiLifecycleEvent.SyncStarting:
            _monitor.Locked(() => _aSyncIsUnderWay = true);
            _studyNoteWrites.Hold();
            break;

         case AnkiLifecycleEvent.SyncCompleted:
            _monitor.Locked(() => _aSyncIsUnderWay = false);
            break;

         default:
            throw new ArgumentOutOfRangeException(nameof(lifecycleEvent), lifecycleEvent, null);
      }
   }

   /// <summary>
   /// An Anki operation changed notes, such as marking one, editing tags in Anki's browser, a review that made a card a leech, which tags its note, an<br/>
   /// undo or redo, or a sync that finished: catches up, in the background, one catch-up at a time.
   /// </summary>
   public void AnkiChangedNotes() => _catchingUp.Run();

   void CatchUpOnceTheDelayAfterAProfileOpenedIsOver() => _backgroundTasks.RunAsync(async () =>
   {
      await Task.Delay(DelayAfterAProfileOpened);
      _catchingUp.Run();
   });

   /// <summary>
   /// Reads the study-notes from Anki again, or loads the collection the first time, then makes the held writes, unless JAStudio may not use Anki's<br/>
   /// collection, at the start or once it has read. Then whatever ends that, a profile opening or the operation Anki announces a sync's end with,<br/>
   /// catches up again.
   /// </summary>
   void CatchUp()
   {
      if(!JAStudioMayUseAnkisCollection) return;

      if(_theCollectionWasLoaded)
      {
         _collection.ReadTheStudyNotesFromAnkiAgain();
      } else
      {
         _collection.ReloadFromBackend();
         _theCollectionWasLoaded = true;
      }

      if(JAStudioMayUseAnkisCollection) _studyNoteWrites.MakeThem();
   }

   bool JAStudioMayUseAnkisCollection => _monitor.Locked(() => _aProfileIsOpen && !_aSyncIsUnderWay);
}
