using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.Studying;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Views.Studying;

namespace JAStudio.UI.ViewModels.Studying;

/// <summary>
/// The Sync button: one click runs JAStudio's sync with Anki (<see cref="SRSSync"/>) in the background. It shows that a sync is under way,<br/>
/// whoever started it, such as another Sync button or JAStudio's MCP server, and is marked as synced once the last sync has finished.<br/>
/// A right-click opens the <see cref="StudyingWindow"/>, which shows what the last sync did. When a sync can't run because Anki has another<br/>
/// collection open than the one JAStudio works with, such as another profile's, a second button offers to work with that one instead.
/// </summary>
/// <remarks>
/// Magnus, 2026-11-15: "I don't want sync several clicks and a dialog away. Just a sync button. Click it and it spins and done, green marked sync<br/>
/// button. Maybe right click or something if you want the complex UI, or a separate UI item..."
/// </remarks>
/// <remarks>
/// The mark says that the last sync finished, not that nothing has changed since: JAStudio isn't told of what changes in Anki, and its own edits<br/>
/// reach the study-notes at the next sync.
/// </remarks>
partial class SyncButtonViewModel : ObservableObject, IDisposable
{
   readonly TemporaryServiceCollection _services;
   readonly SRSSync _sync;
   readonly BackgroundTaskManager _backgroundTaskManager;
   readonly IDisposable _followingTheSyncsThatFinish;
   readonly IDisposable _followingWhetherASyncIsUnderWay;

#pragma warning disable CS8618 // For the XAML designer, which needs no services.
   [Obsolete("For the XAML designer/previewer only")]
   public SyncButtonViewModel() {}
#pragma warning restore CS8618

   /// <param name="services">The services the button resolves the sync from, and opens the Studying window with.</param>
   public SyncButtonViewModel(TemporaryServiceCollection services)
   {
      _services = services;
      _sync = services.Resolve<SRSSync>();
      _backgroundTaskManager = services.BackgroundTaskManager;
      IsOffered = services.SRSAvailability.IsAvailable;

      _followingTheSyncsThatFinish = _sync.Completed.Subscribe(report => Dispatcher.UIThread.Post(() => ShowTheSyncThatFinished(report)));
      _followingWhetherASyncIsUnderWay = _sync.IsRunningChanged.Subscribe(_ => Dispatcher.UIThread.Post(ShowWhetherASyncIsUnderWay));
      IsSyncing = _sync.IsRunning;
      if(_sync.LastReport is {} lastReport) ShowTheSyncThatFinished(lastReport);
   }

   /// <summary>Whether there is a spaced repetition system to sync with: the button is offered only while there is.</summary>
   public bool IsOffered { get; }

   /// <summary>Whether a sync is under way, whoever started it: the button's icon spins, and the button starts no other sync.</summary>
   [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SyncCommand))] public partial bool IsSyncing { get; set; }

   /// <summary>Whether the last sync finished, and none has started since: the button is marked green.</summary>
   [ObservableProperty] public partial bool IsMarkedAsSynced { get; set; }

   /// <summary>Why the sync that was last asked for here didn't run, or empty when it ran.</summary>
   [ObservableProperty] public partial string WhyTheSyncDidNotRun { get; set; } = "";

   /// <summary>
   /// Whether the sync that was last asked for here didn't run because Anki has another collection open than the one JAStudio works with: a<br/>
   /// second button then offers to work with that one (<see cref="WorkWithTheCollectionAnkiHasOpen"/>).
   /// </summary>
   [ObservableProperty] public partial bool OffersTheCollectionAnkiHasOpen { get; set; }

   /// <summary>What the button says of itself when pointed at: what a sync does, what the last one did, and what a right-click does.</summary>
   [ObservableProperty] public partial string ToolTip { get; set; } = ToolTipSaying("No sync has been made since JAStudio started.");

   bool IsNotSyncing() => !IsSyncing;

   /// <summary>Runs a sync in the background, unless one can't run now: then it says why.</summary>
   [RelayCommand(CanExecute = nameof(IsNotSyncing))]
   void Sync()
   {
      WhyTheSyncDidNotRun = _sync.WhyItCannotRunNow is {} why ? $"The sync can't run now: {why}" : "";
      OffersTheCollectionAnkiHasOpen = WhyTheSyncDidNotRun != "" && _sync.TheSRSHasAnotherCollectionOpenThanJAStudioWorksWith;
      if(WhyTheSyncDidNotRun != "") return;

      IsSyncing = true; // at once, so that a second click can't start another sync before this one has notified that it's under way
      _backgroundTaskManager.Run(() =>
      {
         try
         {
            _sync.Run();
         }
         finally
         {
            Dispatcher.UIThread.Post(ShowWhetherASyncIsUnderWay); // a sync that was refused after all never notified that it was under way
         }
      });
   }

   /// <summary>
   /// Has JAStudio work with the collection Anki has open, in place of the one it worked with, and read that collection's study-notes, in the<br/>
   /// background (<see cref="SRSSync.WorkWithTheCollectionTheSRSHasOpen"/>). It writes nothing: the next sync does.
   /// </summary>
   [RelayCommand]
   void WorkWithTheCollectionAnkiHasOpen()
   {
      OffersTheCollectionAnkiHasOpen = false;
      WhyTheSyncDidNotRun = "";
      _backgroundTaskManager.Run(_sync.WorkWithTheCollectionTheSRSHasOpen);
   }

   /// <summary>Opens the Studying window, which shows what the last sync did, or brings the one already open to the front.</summary>
   [RelayCommand]
   void OpenTheStudyingWindow() => StudyingWindow.Open(_services);

   partial void OnIsSyncingChanged(bool value)
   {
      if(value) IsMarkedAsSynced = false;
   }

   void ShowWhetherASyncIsUnderWay() => IsSyncing = _sync.IsRunning;

   void ShowTheSyncThatFinished(SRSSyncReport report)
   {
      IsMarkedAsSynced = true;
      ToolTip = ToolTipSaying($"The last sync finished at {report.FinishedAt:HH:mm:ss}: it read {report.StudyNotesRead:N0} study-notes from Anki, and wrote {report.NotesWhoseStudyNoteWasWritten.Count:N0}.");
   }

   static string ToolTipSaying(string whatTheLastSyncDid) =>
      $"""
       Syncs with Anki: reads the study-notes from Anki, so that the notes show what changed there, writes the study-notes that differ from what JAStudio writes, and runs the import profiles that run automatically.

       {whatTheLastSyncDid}

       Right-click: the Studying window, with all that the last sync did.
       """;

   public void Dispose()
   {
      _followingTheSyncsThatFinish.Dispose();
      _followingWhetherASyncIsUnderWay.Dispose();
   }
}
