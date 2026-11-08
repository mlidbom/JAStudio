using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.UI.ViewModels.Importing;

/// <summary>
/// Edits one <see cref="ImportProfile"/> in the import profiles window. Every change goes straight into the profile and is saved,<br/>
/// so there's nothing to save or lose when the window closes. An open import run window of the profile learns of it through<br/>
/// <see cref="SavedImportProfiles.ProfileChanged"/>.
/// </summary>
class ImportProfileEditorViewModel : ObservableObject
{
   /// <summary>The choice in a field mapping's dropdown that maps no Anki field, leaving the sentence's field empty.</summary>
   public const string NoField = "(none)";

   readonly IReadOnlyList<AnkiNoteType> _noteTypesForRawNotes;
   readonly SavedImportProfiles _savedImportProfiles;

   public ImportProfileEditorViewModel(ImportProfile profile, IReadOnlyList<AnkiNoteType> noteTypesForRawNotes, SavedImportProfiles savedImportProfiles)
   {
      Profile = profile;
      _noteTypesForRawNotes = noteTypesForRawNotes;
      _savedImportProfiles = savedImportProfiles;
   }

   public ImportProfile Profile { get; }

   public string DisplayName => Profile.Name == "" ? "(unnamed)" : Profile.Name;

   public string Name
   {
      get => Profile.Name;
      set => Change(() => Profile.Name = value, nameof(DisplayName));
   }

   public IReadOnlyList<string> AnkiNoteTypeChoices => [.. _noteTypesForRawNotes.Select(it => it.Name)];

   /// <summary>The chosen note type. Its dropdown sets null when it loses its selection, which chooses none.</summary>
   [AllowNull]
   public string AnkiNoteType
   {
      get => Profile.AnkiNoteType;
      set => Change(() => Profile.AnkiNoteType = value ?? "", nameof(AnkiFieldChoices));
   }

   /// <summary><see cref="NoField"/>, then the fields of the chosen note type, for the field mapping's dropdowns.</summary>
   public IReadOnlyList<string> AnkiFieldChoices =>
      [NoField, .._noteTypesForRawNotes.SingleOrDefault(it => it.Name == Profile.AnkiNoteType)?.FieldNames ?? []];

   public string AnkiSearch
   {
      get => Profile.AnkiSearch;
      set => Change(() => Profile.AnkiSearch = value.Trim());
   }

   public string SourceQuestion { get => Shown(Profile.FieldMapping.SourceQuestion); set => Change(() => Profile.FieldMapping.SourceQuestion = Mapped(value)); }
   public string SourceAnswer { get => Shown(Profile.FieldMapping.SourceAnswer); set => Change(() => Profile.FieldMapping.SourceAnswer = Mapped(value)); }
   public string Reading { get => Shown(Profile.FieldMapping.Reading); set => Change(() => Profile.FieldMapping.Reading = Mapped(value)); }
   public string ExternalId { get => Shown(Profile.FieldMapping.ExternalId); set => Change(() => Profile.FieldMapping.ExternalId = Mapped(value)); }
   public string SourceComments { get => Shown(Profile.FieldMapping.SourceComments); set => Change(() => Profile.FieldMapping.SourceComments = Mapped(value)); }
   public string Audio { get => Shown(Profile.FieldMapping.Audio); set => Change(() => Profile.FieldMapping.Audio = Mapped(value)); }
   public string Screenshot { get => Shown(Profile.FieldMapping.Screenshot); set => Change(() => Profile.FieldMapping.Screenshot = Mapped(value)); }

   public string AudioFolder
   {
      get => Profile.AudioFolder;
      set => Change(() => Profile.AudioFolder = value.Trim());
   }

   public string ScreenshotFolder
   {
      get => Profile.ScreenshotFolder;
      set => Change(() => Profile.ScreenshotFolder = value.Trim());
   }

   /// <summary><see cref="ImportProfile.TagsToAdd"/>, separated by spaces, as Anki shows tags.</summary>
   public string TagsToAdd
   {
      get => string.Join(" ", Profile.TagsToAdd);
      set => Change(() => Profile.TagsToAdd = [.. value.Split(' ', StringSplitOptions.RemoveEmptyEntries)]);
   }

   public bool DeletesTheRawNotesAfterwards
   {
      get => Profile.DeletesTheRawNotesAfterwards;
      set => Change(() => Profile.DeletesTheRawNotesAfterwards = value);
   }

   public bool CreatesAnkiNotesToStudy
   {
      get => Profile.CreatesAnkiNotesToStudy;
      set => Change(() => Profile.CreatesAnkiNotesToStudy = value);
   }

   public bool RunsAutomatically
   {
      get => Profile.RunsAutomatically;
      set => Change(() => Profile.RunsAutomatically = value);
   }

   void Change(Action change, string? alsoChangedProperty = null, [CallerMemberName] string changedProperty = "")
   {
      change();
      OnPropertyChanged(changedProperty);
      if(alsoChangedProperty != null) OnPropertyChanged(alsoChangedProperty);
      _savedImportProfiles.SaveTheChangeOf(Profile);
   }

   static string Shown(string ankiFieldName) => ankiFieldName == "" ? NoField : ankiFieldName;
   static string Mapped(string? choice) => choice is null or NoField ? "" : choice;
}
