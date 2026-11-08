using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.UI.ViewModels.Importing;

/// <summary>
/// The import profiles window: the <see cref="SavedImportProfiles"/>, and the one being edited. Check opens the profile's import run window,<br/>
/// which checks it, reports on it and runs it: see <see cref="ImportRunWindowViewModel"/>.
/// </summary>
partial class ImportProfilesWindowViewModel : ObservableObject
{
   readonly SavedImportProfiles _savedImportProfiles;
   readonly IReadOnlyList<AnkiNoteType> _noteTypesForRawNotes;
   readonly Action<ImportProfile> _checkInItsImportRunWindow;

#pragma warning disable CS8618 // For the XAML designer, which needs no services.
   [Obsolete("For the XAML designer/previewer only")]
   public ImportProfilesWindowViewModel() {}
#pragma warning restore CS8618

   /// <param name="services">The services the window resolves the saved profiles and the note types for raw notes from.</param>
   /// <param name="checkInItsImportRunWindow">Checks a profile in its import run window, which it opens unless it's open.</param>
   public ImportProfilesWindowViewModel(TemporaryServiceCollection services, Action<ImportProfile> checkInItsImportRunWindow)
   {
      _savedImportProfiles = services.Resolve<SavedImportProfiles>();
      _noteTypesForRawNotes = services.Resolve<Importer>().NoteTypesForRawNotes();
      _checkInItsImportRunWindow = checkInItsImportRunWindow;

      Profiles = [.. _savedImportProfiles.All.Select(EditorFor)];
      SelectedProfile = Profiles.FirstOrDefault();
   }

   public ObservableCollection<ImportProfileEditorViewModel> Profiles { get; }

   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(DeleteProfileCommand))]
   [NotifyCanExecuteChangedFor(nameof(CheckCommand))]
   ImportProfileEditorViewModel? _selectedProfile;

   ImportProfileEditorViewModel TheSelectedProfile => SelectedProfile ?? throw new InvalidOperationException("No import profile is selected.");

   bool AProfileIsSelected() => SelectedProfile != null;

   [RelayCommand]
   void AddProfile()
   {
      var profile = EditorFor(_savedImportProfiles.AddNew());
      Profiles.Add(profile);
      SelectedProfile = profile;
   }

   [RelayCommand(CanExecute = nameof(AProfileIsSelected))]
   void DeleteProfile()
   {
      var profile = TheSelectedProfile;
      Profiles.Remove(profile);
      SelectedProfile = Profiles.FirstOrDefault();
      _savedImportProfiles.Delete(profile.Profile);
   }

   [RelayCommand(CanExecute = nameof(AProfileIsSelected))]
   void Check() => _checkInItsImportRunWindow(TheSelectedProfile.Profile);

   ImportProfileEditorViewModel EditorFor(ImportProfile profile) => new(profile, _noteTypesForRawNotes, _savedImportProfiles);
}
