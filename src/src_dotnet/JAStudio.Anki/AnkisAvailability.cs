using System;
using Compze.Threading;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>
/// Anki as the spaced repetition system JAStudio runs inside (<see cref="SRSAvailability"/>): there is one, and JAStudio may read and change its<br/>
/// collection while an Anki profile is open and Anki's own sync, with AnkiWeb, isn't under way. Anki tells JAStudio of both<br/>
/// (<see cref="AnkiLifecycleEvent"/>).
/// </summary>
/// <remarks>
/// Anki's sync holds its collection, and a full sync replaces it, so a read or a change made then would block until the sync ends, or fail. With<br/>
/// no profile open, Anki has no collection open.
/// </remarks>
public sealed class AnkisAvailability : SRSAvailability
{
   readonly IMonitor _monitor = IMonitor.New();
   bool _aProfileIsOpen;
   bool _ankisOwnSyncIsUnderWay;

   public override bool IsAvailable => true;

   public override string? WhyItCannotBeUsedNow => _monitor.Locked(() => !_aProfileIsOpen       ? "No Anki profile is open."
                                                                         : _ankisOwnSyncIsUnderWay ? "Anki is syncing."
                                                                                                     : null);

   /// <summary>Takes in that a profile opened or is closing, or that Anki's own sync is starting or finished, as Anki tells JAStudio.</summary>
   public void TakeIn(AnkiLifecycleEvent lifecycleEvent) => _monitor.Locked(() =>
   {
      switch(lifecycleEvent)
      {
         case AnkiLifecycleEvent.ProfileOpened:  _aProfileIsOpen = true; break;
         case AnkiLifecycleEvent.ProfileClosing: _aProfileIsOpen = false; break;
         case AnkiLifecycleEvent.SyncStarting:   _ankisOwnSyncIsUnderWay = true; break;
         case AnkiLifecycleEvent.SyncCompleted:  _ankisOwnSyncIsUnderWay = false; break;
         default:                                throw new ArgumentOutOfRangeException(nameof(lifecycleEvent), lifecycleEvent, null);
      }
   });
}
