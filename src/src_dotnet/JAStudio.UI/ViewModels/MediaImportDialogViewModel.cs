using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.ViewModels;

partial class MediaImportDialogViewModel : ObservableObject
{
   public static List<string> VocabFieldNames { get; } = Enum.GetNames<VocabMediaField>().ToList();
   public static List<string> SentenceFieldNames { get; } = Enum.GetNames<SentenceMediaField>().ToList();

   readonly TemporaryServiceCollection _services;
   readonly VocabCollection _vocabCollection;
   readonly SentenceCollection _sentenceCollection;
   readonly MediaFileIndex _index;
   readonly MediaStorageService _storageService;
   readonly TaskRunner _taskRunner;
   readonly IEnvironmentPaths _paths;

#pragma warning disable CS8618
   [Obsolete("Parameterless constructor is only for XAML designer support and should not be used directly.")]
   public MediaImportDialogViewModel() {}
#pragma warning restore CS8618

   public MediaImportDialogViewModel(TemporaryServiceCollection services)
   {
      _services = services;
      _vocabCollection = services.ServiceLocator.Resolve<VocabCollection>();
      _sentenceCollection = services.ServiceLocator.Resolve<SentenceCollection>();
      _index = services.ServiceLocator.Resolve<MediaFileIndex>();
      _storageService = services.ServiceLocator.Resolve<MediaStorageService>();
      _taskRunner = services.TaskRunner;
      _paths = services.ServiceLocator.Resolve<IEnvironmentPaths>();

      VocabTab = CreateVocabTab();
      SentenceTab = CreateSentenceTab();

      LoadPersistedRules();
   }

   public NoteTypeImportTabViewModel VocabTab { get; private set; } = null!;
   public NoteTypeImportTabViewModel SentenceTab { get; private set; } = null!;

   [ObservableProperty] string _statusText = "Analyzing...";
   [ObservableProperty] bool _hasPlan;

   MediaImportPlan? _currentPlan;
   MediaImportPlan _vocabPlan = new();
   MediaImportPlan _sentencePlan = new();
   [ObservableProperty] int _filesToImportCount;
   [ObservableProperty] int _alreadyStoredCount;
   [ObservableProperty] int _missingCount;

   public IRelayCommand? CloseCommand { get; set; }

   [RelayCommand] void AddVocabRule() => VocabTab.AddRuleCommand.Execute(null);
   [RelayCommand] void AddSentenceRule() => SentenceTab.AddRuleCommand.Execute(null);

   [RelayCommand] void RemoveVocabRule(EditableImportRule rule) => VocabTab.RemoveRuleCommand.Execute(rule);
   [RelayCommand] void RemoveSentenceRule(EditableImportRule rule) => SentenceTab.RemoveRuleCommand.Execute(rule);

   [RelayCommand]
   void Analyze()
   {
      StatusText = "Analyzing...";
      HasPlan = false;

      var vocabRules = BuildVocabRules();
      var sentenceRules = BuildSentenceRules();

      _services.BackgroundTaskManager.Run(() =>
      {
         var vocabFiles = ScanNoteFields(_vocabCollection.All(), note =>
         [
            (nameof(VocabMediaField.AudioFirst),  note.Audio.First.GetMediaReferences()),
            (nameof(VocabMediaField.AudioSecond), note.Audio.Second.GetMediaReferences()),
            (nameof(VocabMediaField.AudioTts),    note.Audio.Tts.GetMediaReferences()),
            (nameof(VocabMediaField.Image),       note.Image.GetMediaReferences()),
            (nameof(VocabMediaField.UserImage),   note.UserImage.GetMediaReferences()),
         ]);
         var sentenceFiles = ScanNoteFields(_sentenceCollection.All(), note =>
         [
            (nameof(SentenceMediaField.Audio),      note.Audio.GetMediaReferences()),
            (nameof(SentenceMediaField.Screenshot), note.Screenshot.GetMediaReferences()),
         ]);

         var ankiMediaDir = _paths.AnkiMediaDir;
         var analyzer = new MediaImportAnalyzer(ankiMediaDir, _index);

         var vocabPlan = vocabRules.Count > 0 ? analyzer.AnalyzeVocab(_vocabCollection.All(), vocabRules) : new MediaImportPlan();
         var sentencePlan = sentenceRules.Count > 0 ? analyzer.AnalyzeSentences(_sentenceCollection.All(), sentenceRules) : new MediaImportPlan();

         var merged = MergePlans(vocabPlan, sentencePlan);

         Dispatcher.UIThread.Invoke(() =>
         {
            VocabTab.SetScannedFiles(vocabFiles);
            SentenceTab.SetScannedFiles(sentenceFiles);

            _vocabPlan = vocabPlan;
            _sentencePlan = sentencePlan;
            _currentPlan = merged;
            FilesToImportCount = merged.FilesToImport.Count;
            AlreadyStoredCount = merged.AlreadyStored.Count;
            MissingCount = merged.Missing.Count;
            HasPlan = true;
            StatusText = $"Plan: {merged.FilesToImport.Count} to import, {merged.AlreadyStored.Count} already stored, {merged.Missing.Count} missing from Anki.";
         });
      });
   }

   [RelayCommand]
   void Import()
   {
      if(_currentPlan == null) return;
      var plan = _currentPlan;
      _currentPlan = null;
      HasPlan = false;
      StatusText = "Importing...";

      _services.BackgroundTaskManager.Run(() =>
      {
         var executor = new MediaImportExecutor(_storageService, _taskRunner);
         executor.Execute(plan);

         Dispatcher.UIThread.Invoke(() =>
         {
            StatusText = $"Import complete. {plan.FilesToImport.Count} files imported, {plan.AlreadyStored.Count} references updated.";
         });
      });
   }

   [RelayCommand]
   void SaveRules()
   {
      var persisted = new PersistedImportRules
                      {
                         VocabRules = BuildVocabRules(),
                         SentenceRules = BuildSentenceRules()
                      };
      MediaImportRulePersistence.Save(persisted, _paths);
      StatusText = "Rules saved.";
   }

   [RelayCommand]
   void ShowMissingFiles()
   {
      var vocabRows = BuildMissingFileRows(_vocabPlan.Missing, noteId => _vocabCollection.WithIdOrNone(noteId)?.GetQuestion() ?? "?");
      var sentenceRows = BuildMissingFileRows(_sentencePlan.Missing, noteId => _sentenceCollection.WithIdOrNone(noteId)?.GetQuestion() ?? "?");

      Dispatcher.UIThread.Invoke(() =>
      {
         var dialog = new Views.MissingFilesDialog(vocabRows, sentenceRows, OpenNoteInAnki);
         dialog.Show();
      });
   }

   void OpenNoteInAnki(NoteId noteId)
   {
      var query = _services.QueryBuilder().NotesByIds([noteId]);
      if(!string.IsNullOrEmpty(query))
         AnkiFacade.Browser.ExecuteLookup(query);
   }

   static List<MissingFileRow> BuildMissingFileRows(List<MissingFile> missing, Func<NoteId, string> getQuestion) =>
      missing.Select(m => new MissingFileRow(getQuestion(m.NoteId), m.NoteId.ToString(), m.FieldName, m.FileName, m.NoteId))
             .OrderBy(r => r.Question)
             .ThenBy(r => r.FieldName)
             .ToList();

   static NoteTypeImportTabViewModel CreateVocabTab() =>
      new("Vocab", VocabFieldNames, BuildRulesFromEditableList);

   static NoteTypeImportTabViewModel CreateSentenceTab() =>
      new("Sentences", SentenceFieldNames, BuildRulesFromEditableList);

   static List<ImportRule> BuildRulesFromEditableList(List<EditableImportRule> editableRules)
   {
      var result = new List<ImportRule>();
      foreach(var r in editableRules)
      {
      if(!r.IsValid) continue;
         result.Add(new ImportRule(SourceTag.Parse(r.SourceTagPrefix), r.SelectedField, r.TargetDirectory));
      }
      return result;
   }

   void LoadPersistedRules()
   {
      var persisted = MediaImportRulePersistence.Load(_paths);
      VocabTab.LoadRules(persisted.VocabRules.Select(r => new EditableImportRule
                                                          { SourceTagPrefix = r.Prefix.ToString(), SelectedField = r.FieldName, TargetDirectory = r.TargetDirectory }));
      SentenceTab.LoadRules(persisted.SentenceRules.Select(r => new EditableImportRule
                                                                { SourceTagPrefix = r.Prefix.ToString(), SelectedField = r.FieldName, TargetDirectory = r.TargetDirectory }));
   }

   List<ImportRule> BuildVocabRules() => BuildRulesFromEditableList(VocabTab.Rules.ToList());
   List<ImportRule> BuildSentenceRules() => BuildRulesFromEditableList(SentenceTab.Rules.ToList());

   List<ScannedMediaFile> ScanNoteFields<TNote>(IReadOnlyList<TNote> notes, Func<TNote, (string FieldName, List<MediaReference> Refs)[]> getFields) where TNote : JPNote
   {
      var results = new List<ScannedMediaFile>();
      foreach(var note in notes)
      {
         var sourceTag = note.SourceTag.ToString();
         foreach(var (fieldName, refs) in getFields(note))
            foreach(var r in refs)
               if(!_index.ContainsByOriginalFileName(r.FileName))
                  results.Add(new ScannedMediaFile(sourceTag, fieldName, r.FileName));
      }
      return results;
   }

   static MediaImportPlan MergePlans(params MediaImportPlan[] plans)
   {
      var merged = new MediaImportPlan();
      foreach(var plan in plans)
      {
         merged.FilesToImport.AddRange(plan.FilesToImport);
         merged.AlreadyStored.AddRange(plan.AlreadyStored);
         merged.Missing.AddRange(plan.Missing);
      }

      return merged;
   }
}

partial class EditableImportRule : ObservableObject
{
   [ObservableProperty] string _sourceTagPrefix = "";
   [ObservableProperty] string _selectedField = "";
   [ObservableProperty] string _targetDirectory = "";
   [ObservableProperty] int _matchCount;

   public IRelayCommand? RemoveSelfCommand { get; set; }

   public bool IsValid =>
      !string.IsNullOrWhiteSpace(SourceTagPrefix) &&
      !string.IsNullOrWhiteSpace(SelectedField) &&
      !string.IsNullOrWhiteSpace(TargetDirectory);
}

record MissingFileRow(string Question, string NoteIdDisplay, string FieldName, string FileName, NoteId NoteId);
