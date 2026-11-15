using System;
using System.Threading.Tasks;
using Compze.Threading;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.SysUtils.Threading;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Anki;

/// <summary>
/// What JAStudio does as Anki tells it that a profile opened or is closing, and that Anki's own sync is starting or has finished<br/>
/// (<see cref="AnkiLifecycleEvent"/>): it keeps track of whether it may use Anki's collection (<see cref="AnkisAvailability"/>), and it reads the<br/>
/// profile that opened. The first profile to open has it load its collection, and each profile after that has it read the study-notes again<br/>
/// (<see cref="JPCollection.ReadTheStudyNotesFromTheSRSAgain"/>), since they're that profile's.
/// </summary>
/// <remarks>
/// It's all that Anki tells JAStudio of: what changes in Anki's collection after that, JAStudio reads at a sync a person starts<br/>
/// (<see cref="JAStudio.Core.Studying.SRSSync"/>). Until 2026-11-15 Anki also told it of each operation that changed notes, and of each note<br/>
/// added or deleted and each card suspended or unsuspended: see dev_docs/moving-jastudio-out-of-anki.md.
/// </remarks>
/// <remarks>
/// Anki syncs as a profile opens, when set to, and the sync starts just after the profile opened. JAStudio may not use Anki's collection during<br/>
/// the sync, so the profile is then read once the sync has finished.
/// </remarks>
/// <remarks>review-naming:studying: FollowingAnkisLifecycle was CatchingUpWithAnki, which also read the study-notes after each Anki operation (Claude, 2026-11-15).</remarks>
public sealed class FollowingAnkisLifecycle
{
   /// <summary>How long a profile that opened waits before it's read: the sync Anki makes as a profile opens starts just after.</summary>
   static readonly TimeSpan DelayAfterAProfileOpened = TimeSpan.FromMilliseconds(500);

   readonly JPCollection _collection;
   readonly AnkisAvailability _ankisAvailability;
   readonly BackgroundTaskManager _backgroundTasks;
   readonly BackgroundRefresh _readingTheProfileThatOpened;
   readonly IMonitor _monitor = IMonitor.New();
   bool _theProfileThatOpenedIsYetToBeRead;
   bool _theCollectionWasLoaded;

   /// <param name="collection">The collection that is loaded, and whose study-notes are read again.</param>
   /// <param name="ankisAvailability">Whether JAStudio may use Anki's collection, which is told of each lifecycle event here.</param>
   /// <param name="backgroundTasks">Runs the reading in the background, and surfaces what it throws.</param>
   public FollowingAnkisLifecycle(JPCollection collection, AnkisAvailability ankisAvailability, BackgroundTaskManager backgroundTasks)
   {
      _collection = collection;
      _ankisAvailability = ankisAvailability;
      _backgroundTasks = backgroundTasks;
      _readingTheProfileThatOpened = new BackgroundRefresh(backgroundTasks, ReadTheProfileThatOpenedIfItIsYetToBeRead);
   }

   /// <summary>Takes in that a profile opened or is closing, or that Anki's own sync is starting or has finished, as Anki tells JAStudio.</summary>
   public void Handle(AnkiLifecycleEvent lifecycleEvent)
   {
      _ankisAvailability.TakeIn(lifecycleEvent);
      switch(lifecycleEvent)
      {
         case AnkiLifecycleEvent.ProfileOpened:
            _monitor.Locked(() => _theProfileThatOpenedIsYetToBeRead = true);
            ReadTheProfileThatOpenedOnceTheDelayIsOver();
            break;

         case AnkiLifecycleEvent.SyncCompleted:
            _readingTheProfileThatOpened.Run();
            break;

         case AnkiLifecycleEvent.ProfileClosing:
         case AnkiLifecycleEvent.SyncStarting:
            break;

         default:
            throw new ArgumentOutOfRangeException(nameof(lifecycleEvent), lifecycleEvent, null);
      }
   }

   void ReadTheProfileThatOpenedOnceTheDelayIsOver() => _backgroundTasks.RunAsync(async () =>
   {
      await Task.Delay(DelayAfterAProfileOpened);
      _readingTheProfileThatOpened.Run();
   });

   /// <summary>
   /// Loads the collection, the first time, or reads the study-notes again, unless the profile that opened is read already, or JAStudio may not<br/>
   /// use Anki's collection now: then the end of Anki's sync, or the next profile to open, has it read.
   /// </summary>
   void ReadTheProfileThatOpenedIfItIsYetToBeRead()
   {
      if(!_ankisAvailability.CanBeUsedNow) return;
      if(!_monitor.Locked(TakeOnTheReadIfItIsDue)) return;

      if(_monitor.Locked(() => _theCollectionWasLoaded))
      {
         _collection.ReadTheStudyNotesFromTheSRSAgain();
      } else
      {
         _collection.ReloadFromBackend();
         _monitor.Locked(() => _theCollectionWasLoaded = true);
      }

      return;

      bool TakeOnTheReadIfItIsDue()
      {
         var itIsDue = _theProfileThatOpenedIsYetToBeRead;
         _theProfileThatOpenedIsYetToBeRead = false;
         return itIsDue;
      }
   }
}
