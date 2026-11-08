using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.Checking;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.ViewModels.Workspace.Navigation;

namespace JAStudio.UI.ViewModels.Importing;

/// <summary>
/// The import run window of one <see cref="ImportProfile"/>: checking it, the report on every raw note the check selected, and running it.<br/>
/// Checking prepares an <see cref="ImportRun"/>. Running executes the run that was checked, so it does what the report said, and shows the<br/>
/// sentences it imported in the workspace's List tab.
/// </summary>
/// <remarks>
/// It's a window of its own, apart from the import profiles window, so that each fits a laptop's screen: the report grows with the raw notes.<br/>
/// The profile is edited in the import profiles window while the report stays in view, to fix what it found. A change to the profile leaves<br/>
/// the report as it is, but running waits for checking again, so a run never does what its report didn't show.
/// </remarks>
partial class ImportRunWindowViewModel : ObservableObject, IDisposable
{
   readonly ImportProfile _profile;
   readonly Importer _importer;
   readonly SavedImportProfiles _savedImportProfiles;
   readonly BackgroundTaskManager _backgroundTaskManager;
   readonly Action<NoteList> _showInTheWorkspace;
   readonly Action _closeTheWindow;
   IReadOnlyList<CheckedRawNoteRow> _everyRow = [];
   bool _theProfileChangedSinceTheCheckStarted;

#pragma warning disable CS8618 // For the XAML designer, which needs no services.
   [Obsolete("For the XAML designer/previewer only")]
   public ImportRunWindowViewModel() {}
#pragma warning restore CS8618

   /// <summary>Starts checking <paramref name="profile"/>.</summary>
   /// <param name="profile">The saved profile the window checks and runs, as it is at each check.</param>
   /// <param name="services">The services the window resolves the importer and the saved profiles from.</param>
   /// <param name="showInTheWorkspace">Shows a list of notes in the workspace's List tab, as the note menus' "Open" items do.</param>
   /// <param name="closeTheWindow">Closes the window, as it does when <paramref name="profile"/> is deleted.</param>
   public ImportRunWindowViewModel(ImportProfile profile, TemporaryServiceCollection services, Action<NoteList> showInTheWorkspace, Action closeTheWindow)
   {
      _profile = profile;
      _importer = services.Resolve<Importer>();
      _savedImportProfiles = services.Resolve<SavedImportProfiles>();
      _backgroundTaskManager = services.BackgroundTaskManager;
      _showInTheWorkspace = showInTheWorkspace;
      _closeTheWindow = closeTheWindow;

      _savedImportProfiles.ProfileChanged += OnAProfileChanged;
      Check();
   }

   public string Title => $"Import run: {_profile.Name}";

   /// <summary>The summary of the last check or run: the problems with the profile, and what running does.</summary>
   [ObservableProperty] string _summary = "";

   /// <summary>A row for each raw note the last check selected, of the outcomes the <see cref="OutcomeFilters"/> show.</summary>
   [ObservableProperty] IReadOnlyList<CheckedRawNoteRow> _rows = [];

   /// <summary>A check box per outcome, with its count, that shows or hides its rows. None before a check.</summary>
   [ObservableProperty] IReadOnlyList<OutcomeFilterViewModel> _outcomeFilters = [];

   /// <summary>The field mapping the report is of, which decides the columns it shows: those of the mapped fields. Null without a report.</summary>
   [ObservableProperty] SentenceFieldMapping? _reportedFieldMapping;

   /// <summary>Whether a check or a run is under way.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(CheckCommand))]
   [NotifyCanExecuteChangedFor(nameof(RunCommand))]
   bool _isBusy;

   /// <summary>The run the last check prepared, which <see cref="Run"/> executes. Cleared when the profile changes, so a run never does more than its report said.</summary>
   [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RunCommand))] ImportRun? _checkedRun;

   /// <summary>The last run executed, whose imported sentences <see cref="ShowTheImportedSentences"/> shows again.</summary>
   [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ShowTheImportedSentencesCommand))] ImportRun? _executedRun;

   bool IsIdle() => !IsBusy;

   /// <summary>Checks the profile as it is now, and reports on it.</summary>
   [RelayCommand(CanExecute = nameof(IsIdle))]
   void Check()
   {
      IsBusy = true;
      _theProfileChangedSinceTheCheckStarted = false;
      CheckedRun = null;
      Summary = "Checking…";
      ShowNoReport();
      _backgroundTaskManager.Run(() =>
      {
         try
         {
            var run = _importer.Prepare(_profile);
            Dispatcher.UIThread.Invoke(() => ShowTheCheckOf(run));
         }
         finally
         {
            Dispatcher.UIThread.Invoke(() => IsBusy = false);
         }
      });
   }

   /// <summary>Checks the profile again, as the import profiles window's Check does with this window open, unless a check or a run is under way.</summary>
   internal void CheckAgainUnlessBusy()
   {
      if(CheckCommand.CanExecute(null)) Check();
   }

   bool CanRun() => !IsBusy && CheckedRun is { HasNotesToImportAndNoProblems: true };

   [RelayCommand(CanExecute = nameof(CanRun))]
   void Run()
   {
      var run = CheckedRun!;
      CheckedRun = null;
      IsBusy = true;
      Summary = "Importing…";
      ShowNoReport();
      _backgroundTaskManager.Run(() =>
      {
         try
         {
            run.Execute();
            Dispatcher.UIThread.Invoke(() =>
            {
               ExecutedRun = run;
               Summary = SummaryOfTheExecuted(run);
               ShowTheImportedSentences();
            });
         }
         finally
         {
            Dispatcher.UIThread.Invoke(() => IsBusy = false);
         }
      });
   }

   bool HasImportedSentencesToShow() => ExecutedRun is { ImportedSentences.Count: > 0 };

   /// <summary>Shows the sentences the last run imported in the workspace's List tab, titled with the profile and the time of the run.</summary>
   [RelayCommand(CanExecute = nameof(HasImportedSentencesToShow))]
   void ShowTheImportedSentences()
   {
      var run = ExecutedRun!;
      _showInTheWorkspace(new NoteList($"Imported by {run.Profile.Name}, {DateTime.Now:yyyy-MM-dd HH:mm}", [.. run.ImportedSentences]));
   }

   /// <summary>Stops following the changes to the saved profiles, when the window closes.</summary>
   public void Dispose() => _savedImportProfiles.ProfileChanged -= OnAProfileChanged;

   void OnAProfileChanged(ImportProfile changedProfile)
   {
      if(changedProfile != _profile) return;

      if(!_savedImportProfiles.Contains(_profile))
      {
         _closeTheWindow();
         return;
      }

      OnPropertyChanged(nameof(Title));
      _theProfileChangedSinceTheCheckStarted = true;
      if(CheckedRun is {} checkedRun) ShowThatTheProfileChangedAfterTheCheckOf(checkedRun);
   }

   void ShowTheCheckOf(ImportRun run)
   {
      ShowTheReportOn(run);
      if(_theProfileChangedSinceTheCheckStarted)
         ShowThatTheProfileChangedAfterTheCheckOf(run);
      else
      {
         CheckedRun = run;
         Summary = SummaryOf(run);
      }
   }

   /// <summary>Keeps the report in view, to fix what it found, but makes running wait for checking again.</summary>
   void ShowThatTheProfileChangedAfterTheCheckOf(ImportRun run)
   {
      CheckedRun = null;
      Summary = $"The profile was changed after this check, so what follows may be out of date: check again before running.{Environment.NewLine}{Environment.NewLine}{SummaryOf(run)}";
   }

   void ShowTheReportOn(ImportRun run)
   {
      _everyRow = [.. run.CheckedRawNotes.Select(it => new CheckedRawNoteRow(it))];
      ReportedFieldMapping = run.Profile.FieldMapping;
      OutcomeFilters = [.. Enum.GetValues<RawNoteOutcome>().Select(it => new OutcomeFilterViewModel(it, run.With(it).Count, ShowTheRowsOfTheShownOutcomes))];
      ShowTheRowsOfTheShownOutcomes();
   }

   void ShowTheRowsOfTheShownOutcomes()
   {
      var shownOutcomes = OutcomeFilters.Where(it => it.IsShown).Select(it => it.Outcome).ToHashSet();
      Rows = [.. _everyRow.Where(it => shownOutcomes.Contains(it.Outcome))];
   }

   void ShowNoReport()
   {
      _everyRow = [];
      Rows = [];
      OutcomeFilters = [];
      ReportedFieldMapping = null;
   }

   static string SummaryOf(ImportRun run)
   {
      var summary = new StringBuilder();
      if(run.ProblemsWithTheProfile.Count > 0)
      {
         summary.AppendLine("Problems with the profile. Nothing can be imported until they're fixed:");
         run.ProblemsWithTheProfile.ToList().ForEach(it => summary.AppendLine($"  • {it}"));
         return summary.ToString();
      }

      if(!run.CanExecute)
         summary.AppendLine("The raw notes with a red ✗ stop the run: fix them in Anki, and check again.");
      else if(run.HasNotesToImportAndNoProblems)
         summary.AppendLine($"Running imports {run.With(RawNoteOutcome.WillBeImported).Count} sentences{(run.Profile.CreatesAnkiNotesToStudy ? ", each with an Anki note to study" : " into the corpus only")}, "
                          + $"then their raw notes are {WhatBecomesOfTheImportedRawNotesOf(run.Profile)}. The other raw notes are left as they are.");
      else
         summary.AppendLine("There's nothing to import.");

      return summary.ToString();
   }

   static string SummaryOfTheExecuted(ImportRun run) =>
      $"Imported {run.ImportedSentences.Count} sentences, shown in the workspace's List tab. Their raw notes were {WhatBecomesOfTheImportedRawNotesOf(run.Profile)}.";

   static string WhatBecomesOfTheImportedRawNotesOf(ImportProfile profile) =>
      profile.DeletesTheRawNotesAfterwards ? "deleted from Anki" : $"tagged {RawNote.ImportedTag} in Anki";
}
