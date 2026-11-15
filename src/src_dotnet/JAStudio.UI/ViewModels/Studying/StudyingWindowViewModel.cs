using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Studying;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.ViewModels.Studying;

/// <summary>
/// The Studying window: JAStudio's sync with Anki (<see cref="SRSSync"/>), which its Sync button starts, what the last sync did, and the<br/>
/// study-notes in Anki that belong to no note, as the last sync found them.
/// </summary>
/// <remarks>
/// It shows the last sync whoever started it, such as JAStudio's MCP server, and when the window opens: the sync keeps what it last did<br/>
/// (<see cref="SRSSync.LastReport"/>), and notifies each sync (<see cref="SRSSync.Completed"/>).
/// </remarks>
partial class StudyingWindowViewModel : ObservableObject, IDisposable
{
   readonly SRSSync _sync;
   readonly ISRSWindows _ankisWindows;
   readonly BackgroundTaskManager _backgroundTaskManager;
   readonly IDisposable _followingTheSyncs;

#pragma warning disable CS8618 // For the XAML designer, which needs no services.
   [Obsolete("For the XAML designer/previewer only")]
   public StudyingWindowViewModel() {}
#pragma warning restore CS8618

   /// <param name="services">The services the window resolves the sync from.</param>
   public StudyingWindowViewModel(TemporaryServiceCollection services)
   {
      _sync = services.Resolve<SRSSync>();
      _ankisWindows = services.SRSWindows;
      _backgroundTaskManager = services.BackgroundTaskManager;

      _followingTheSyncs = _sync.Completed.Subscribe(report => Dispatcher.UIThread.Post(() => Show(report)));
      if(_sync.LastReport is {} lastReport) Show(lastReport);
   }

   /// <summary>What the last sync did, a line for each thing, or that none has been made.</summary>
   [ObservableProperty] public partial IReadOnlyList<string> WhatTheLastSyncDid { get; set; } = ["No sync has been made since JAStudio started."];

   /// <summary>The study-notes in Anki that belong to no note, as the last sync found them: those that hold no corpus note's id first.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(ShowThemInAnkisBrowserCommand))]
   public partial IReadOnlyList<StudyNoteOfNoNoteRow> StudyNotesOfNoNote { get; set; } = [];

   /// <summary>Why the sync that was last asked for didn't run, or empty when it ran.</summary>
   [ObservableProperty] public partial string WhyTheSyncDidNotRun { get; set; } = "";

   /// <summary>Whether a sync this window started is under way.</summary>
   [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SyncCommand))] public partial bool IsSyncing { get; set; }

   bool IsNotSyncing() => !IsSyncing;

   /// <summary>Runs a sync in the background, unless one can't run now: then it says why.</summary>
   [RelayCommand(CanExecute = nameof(IsNotSyncing))]
   void Sync()
   {
      WhyTheSyncDidNotRun = _sync.WhyItCannotRunNow is {} why ? $"The sync can't run now: {why}" : "";
      if(WhyTheSyncDidNotRun != "") return;

      IsSyncing = true;
      _backgroundTaskManager.Run(() =>
      {
         try
         {
            _sync.Run();
         }
         finally
         {
            Dispatcher.UIThread.Invoke(() => IsSyncing = false);
         }
      });
   }

   bool ThereAreStudyNotesOfNoNote() => StudyNotesOfNoNote.Count > 0;

   /// <summary>Opens Anki's browser on the study-notes that belong to no note, to look at them there, and to delete or mend them.</summary>
   [RelayCommand(CanExecute = nameof(ThereAreStudyNotesOfNoNote))]
   void ShowThemInAnkisBrowser() => _ankisWindows.OpenTheBrowserOn(QueryBuilder.StudyNotesWithTheIds([.. StudyNotesOfNoNote.Select(it => it.AnkiNoteId)]));

   void Show(SRSSyncReport report)
   {
      WhatTheLastSyncDid =
      [
         $"The last sync finished at {report.FinishedAt:HH:mm:ss} on {report.FinishedAt:yyyy-MM-dd}.",
         $"Study-notes read from Anki: {report.StudyNotesRead:N0}",
         $"Study-notes written, since they differed from what JAStudio writes: {report.NotesWhoseStudyNoteWasWritten.Count:N0}",
         $"Notes whose study-note had changed in Anki, in a card's studying status, in its tags, or in being new: {report.NotesWhoseStudyNoteHadChanged.Count:N0}",
         $"Notes whose study-note is gone from Anki: {report.NotesWhoseStudyNoteIsGone.Count:N0}",
         $"Study-notes in Anki that hold no corpus note's id: {report.StudyNotesThatHoldNoNotesId.Count:N0}",
         $"Study-notes in Anki that hold the id of a note JAStudio hasn't: {report.StudyNotesWhoseNoteTheCollectionHasNot.Count:N0}"
      ];
      StudyNotesOfNoNote =
      [
         .. report.StudyNotesThatHoldNoNotesId.Select(it => StudyNoteOfNoNoteRow.Of(it, "It holds no corpus note's id.")),
         .. report.StudyNotesWhoseNoteTheCollectionHasNot.Select(it => StudyNoteOfNoNoteRow.Of(it, "JAStudio has no note with the id it holds."))
      ];
   }

   public void Dispose() => _followingTheSyncs.Dispose();
}
