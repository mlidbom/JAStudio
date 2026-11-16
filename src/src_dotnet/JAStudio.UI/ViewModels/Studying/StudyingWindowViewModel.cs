using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.Studying;

namespace JAStudio.UI.ViewModels.Studying;

/// <summary>
/// The Studying window: a <see cref="SyncButton"/>, as the workspace has, which starts JAStudio's sync with Anki (<see cref="SRSSync"/>), all<br/>
/// that the last sync did, and the study-notes in Anki that belong to no note, as the last sync found them.
/// </summary>
/// <remarks>
/// It shows the last sync whoever started it, such as the workspace's Sync button or JAStudio's MCP server, and when the window opens: the sync<br/>
/// keeps what it last did (<see cref="SRSSync.LastReport"/>), and notifies each sync (<see cref="SRSSync.Completed"/>).
/// </remarks>
partial class StudyingWindowViewModel : ObservableObject, IDisposable
{
   readonly ISRSWindows _ankisWindows;
   readonly IDisposable _followingTheSyncs;

#pragma warning disable CS8618 // For the XAML designer, which needs no services.
   [Obsolete("For the XAML designer/previewer only")]
   public StudyingWindowViewModel() {}
#pragma warning restore CS8618

   /// <param name="services">The services the window resolves the sync from.</param>
   public StudyingWindowViewModel(TemporaryServiceCollection services)
   {
      var sync = services.Resolve<SRSSync>();
      _ankisWindows = services.SRSWindows;
      SyncButton = new SyncButtonViewModel(services);

      _followingTheSyncs = sync.Completed.Subscribe(report => Dispatcher.UIThread.Post(() => Show(report)));
      if(sync.LastReport is {} lastReport) Show(lastReport);
   }

   /// <summary>The window's Sync button: one click runs a sync, as the workspace's does.</summary>
   public SyncButtonViewModel SyncButton { get; }

   /// <summary>What the last sync did, a line for each thing, or that none has been made.</summary>
   [ObservableProperty] public partial IReadOnlyList<string> WhatTheLastSyncDid { get; set; } = ["No sync has been made since JAStudio started."];

   /// <summary>The study-notes in Anki that belong to no note, as the last sync found them: those that hold no corpus note's id first.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(ShowThemInAnkisBrowserCommand))]
   public partial IReadOnlyList<StudyNoteOfNoNoteRow> StudyNotesOfNoNote { get; set; } = [];

   bool ThereAreStudyNotesOfNoNote() => StudyNotesOfNoNote.Count > 0;

   /// <summary>Opens Anki's browser on the study-notes that belong to no note, to look at them there, and to delete or mend them.</summary>
   [RelayCommand(CanExecute = nameof(ThereAreStudyNotesOfNoNote))]
   void ShowThemInAnkisBrowser() => _ankisWindows.OpenTheBrowserOn([.. StudyNotesOfNoNote.Select(it => it.AnkiNoteId)]);

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

   public void Dispose()
   {
      _followingTheSyncs.Dispose();
      SyncButton.Dispose();
   }
}
