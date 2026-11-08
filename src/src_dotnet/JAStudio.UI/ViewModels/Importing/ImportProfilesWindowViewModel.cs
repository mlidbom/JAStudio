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
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.ViewModels.Importing;

/// <summary>
/// The import profiles window: the saved <see cref="ImportProfile"/>s, the one being edited, and checking and running it.<br/>
/// Checking prepares an <see cref="ImportRun"/> and shows what it found. Running executes the run that was checked, so it does what the report said.
/// </summary>
partial class ImportProfilesWindowViewModel : ObservableObject
{
   readonly Importer _importer;
   readonly SavedImportProfiles _savedImportProfiles;
   readonly BackgroundTaskManager _backgroundTaskManager;
   readonly IReadOnlyList<AnkiNoteType> _noteTypesForRawNotes;

#pragma warning disable CS8618 // For the XAML designer, which needs no services.
   [Obsolete("For the XAML designer/previewer only")]
   public ImportProfilesWindowViewModel() {}
#pragma warning restore CS8618

   public ImportProfilesWindowViewModel(TemporaryServiceCollection services)
   {
      _importer = services.Resolve<Importer>();
      _savedImportProfiles = services.Resolve<SavedImportProfiles>();
      _backgroundTaskManager = services.BackgroundTaskManager;
      _noteTypesForRawNotes = _importer.NoteTypesForRawNotes();

      Profiles = [.. _savedImportProfiles.Load().Select(EditorFor)];
      SelectedProfile = Profiles.FirstOrDefault();
   }

   public ObservableCollection<ImportProfileEditorViewModel> Profiles { get; }

   ImportProfileEditorViewModel? _selectedProfile;

   /// <summary>The profile being edited, checked and run. Choosing another forgets the <see cref="CheckedRun"/>, which was of this one.</summary>
   public ImportProfileEditorViewModel? SelectedProfile
   {
      get => _selectedProfile;
      set
      {
         if(SetProperty(ref _selectedProfile, value)) ForgetTheCheckedRun();
      }
   }

   [ObservableProperty] string _report = "";
   [ObservableProperty] bool _isBusy;

   /// <summary>The run the last check prepared, which <see cref="Run"/> executes: cleared by any change, so a run never does more than its report said.</summary>
   [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RunCommand))] ImportRun? _checkedRun;

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
      Report = "Checking…";
      _backgroundTaskManager.Run(() =>
      {
         try
         {
            var run = _importer.Prepare(profile);
            Dispatcher.UIThread.Invoke(() =>
            {
               CheckedRun = run;
               Report = ReportOn(run);
            });
         }
         finally
         {
            Dispatcher.UIThread.Invoke(() => IsBusy = false);
         }
      });
   }

   bool CanRun() => CheckedRun is { HasRawNotesAndNoProblems: true };

   [RelayCommand(CanExecute = nameof(CanRun))]
   void Run()
   {
      var run = CheckedRun!;
      CheckedRun = null;
      IsBusy = true;
      Report = "Importing…";
      _backgroundTaskManager.Run(() =>
      {
         try
         {
            run.Execute();
            Dispatcher.UIThread.Invoke(() => Report = ReportOnTheExecuted(run));
         }
         finally
         {
            Dispatcher.UIThread.Invoke(() => IsBusy = false);
         }
      });
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
      Report = "";
   }

   static string ReportOn(ImportRun run)
   {
      var report = new StringBuilder();
      if(run.Problems.Count > 0)
      {
         report.AppendLine("Problems. Nothing can be imported until they're fixed:");
         run.Problems.ToList().ForEach(it => report.AppendLine($"  • {it}"));
         report.AppendLine();
      }

      report.AppendLine($"Raw notes found: {run.RawNotes.Count}. To import: {run.NotesToImport.Count}. Already imported: {run.AlreadyImported.Count}.");

      if(run.AlreadyImported.Count > 0)
      {
         report.AppendLine();
         report.AppendLine($"WARNING: these look imported already, so they're skipped, and {WhatHappensToTheRawNotesOf(run.Profile)} like the rest:");
         run.AlreadyImported.ToList().ForEach(it => report.AppendLine($"  • {it}"));
      }

      if(!run.HasRawNotesAndNoProblems) return report.ToString();

      report.AppendLine();
      report.AppendLine($"Running imports {run.NotesToImport.Count} sentences{(run.Profile.CreatesAnkiNotesToStudy ? ", each with an Anki note to study" : " into the corpus only")}. "
                      + $"Then the {run.RawNotes.Count} raw notes are {WhatHappensToTheRawNotesOf(run.Profile)}.");
      return report.ToString();
   }

   static string ReportOnTheExecuted(ImportRun run) =>
      $"Imported {run.NotesToImport.Count} sentences. Skipped {run.AlreadyImported.Count} that looked imported already. "
    + $"The {run.RawNotes.Count} raw notes were {WhatHappensToTheRawNotesOf(run.Profile)}.";

   static string WhatHappensToTheRawNotesOf(ImportProfile profile) =>
      profile.DeletesTheRawNotesAfterwards ? "deleted from Anki" : $"tagged {RawNote.ImportedTag} in Anki";
}
