using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// The import profiles window: the saved <see cref="ImportProfile"/>s, the one being edited, and checking and running it.<br/>
/// Checking prepares an <see cref="ImportRun"/> and reports on every raw note it selected. Running executes the run that was checked, so it does<br/>
/// what the report said, and shows the sentences it imported in the workspace's List tab.
/// </summary>
partial class ImportProfilesWindowViewModel : ObservableObject
{
   readonly Importer _importer;
   readonly SavedImportProfiles _savedImportProfiles;
   readonly BackgroundTaskManager _backgroundTaskManager;
   readonly IReadOnlyList<AnkiNoteType> _noteTypesForRawNotes;
   readonly Action<NoteList> _showInTheWorkspace;

#pragma warning disable CS8618 // For the XAML designer, which needs no services.
   [Obsolete("For the XAML designer/previewer only")]
   public ImportProfilesWindowViewModel() {}
#pragma warning restore CS8618

   /// <param name="services">The services the window resolves the importer and the saved profiles from.</param>
   /// <param name="showInTheWorkspace">Shows a list of notes in the workspace's List tab, as the note menus' "Open" items do.</param>
   public ImportProfilesWindowViewModel(TemporaryServiceCollection services, Action<NoteList> showInTheWorkspace)
   {
      _importer = services.Resolve<Importer>();
      _savedImportProfiles = services.Resolve<SavedImportProfiles>();
      _backgroundTaskManager = services.BackgroundTaskManager;
      _showInTheWorkspace = showInTheWorkspace;
      _noteTypesForRawNotes = _importer.NoteTypesForRawNotes();

      Profiles = [.. _savedImportProfiles.Load().Select(EditorFor)];
      SelectedProfile = Profiles.FirstOrDefault();
   }

   public ObservableCollection<ImportProfileEditorViewModel> Profiles { get; }

   /// <summary>The profile being edited, checked and run. Choosing another forgets the <see cref="CheckedRun"/>, which was of this one.</summary>
   public ImportProfileEditorViewModel? SelectedProfile
   {
      get;
      set
      {
         if(SetProperty(ref field, value)) ForgetTheCheckedRun();
      }
   }

   /// <summary>The summary of the last check or run: the problems with the profile, and what running does.</summary>
   [ObservableProperty] string _summary = "";

   IReadOnlyList<CheckedRawNoteRow> _everyRow = [];

   /// <summary>A row for each raw note the last check selected, of the outcomes the <see cref="OutcomeFilters"/> show.</summary>
   [ObservableProperty] IReadOnlyList<CheckedRawNoteRow> _rows = [];

   /// <summary>A check box per outcome, with its count, that shows or hides its rows. None before a check.</summary>
   [ObservableProperty] IReadOnlyList<OutcomeFilterViewModel> _outcomeFilters = [];

   /// <summary>The field mapping of the profile the report is of, which decides the columns it shows: those of the mapped fields. Null without a report.</summary>
   [ObservableProperty] SentenceFieldMapping? _reportedFieldMapping;

   [ObservableProperty] bool _isBusy;

   /// <summary>The run the last check prepared, which <see cref="Run"/> executes: cleared by any change, so a run never does more than its report said.</summary>
   [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RunCommand))] ImportRun? _checkedRun;

   /// <summary>The last run executed, whose imported sentences <see cref="ShowTheImportedSentences"/> shows again.</summary>
   [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ShowTheImportedSentencesCommand))] ImportRun? _executedRun;

   [RelayCommand]
   void AddProfile()
   {
      var profile = EditorFor(new ImportProfile { Name = "New import profile" });
      Profiles.Add(profile);
      SelectedProfile = profile;
      SaveEveryProfile();
   }

   [RelayCommand]
   void DeleteProfile()
   {
      if(SelectedProfile == null) return;
      Profiles.Remove(SelectedProfile);
      SelectedProfile = Profiles.FirstOrDefault();
      SaveEveryProfile();
   }

   [RelayCommand]
   void Check()
   {
      if(SelectedProfile == null) return;
      var profile = SelectedProfile.Profile;
      IsBusy = true;
      Summary = "Checking…";
      ShowNoReport();
      _backgroundTaskManager.Run(() =>
      {
         try
         {
            var run = _importer.Prepare(profile);
            Dispatcher.UIThread.Invoke(() =>
            {
               CheckedRun = run;
               Summary = SummaryOf(run);
               ShowTheReportOn(run);
            });
         }
         finally
         {
            Dispatcher.UIThread.Invoke(() => IsBusy = false);
         }
      });
   }

   bool CanRun() => CheckedRun is { HasNotesToImportAndNoProblems: true };

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

   ImportProfileEditorViewModel EditorFor(ImportProfile profile) => new(profile, _noteTypesForRawNotes, SaveEveryProfile);

   void SaveEveryProfile()
   {
      ForgetTheCheckedRun();
      _savedImportProfiles.Save([.. Profiles.Select(it => it.Profile)]);
   }

   void ForgetTheCheckedRun()
   {
      CheckedRun = null;
      Summary = "";
      ShowNoReport();
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
