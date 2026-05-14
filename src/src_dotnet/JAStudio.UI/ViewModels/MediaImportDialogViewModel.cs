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
   public static List<string> KanjiFieldNames { get; } = Enum.GetNames<KanjiMediaField>().ToList();

   readonly TemporaryServiceCollection _services;
   readonly VocabCollection _vocabCollection;
   readonly SentenceCollection _sentenceCollection;
   readonly KanjiCollection _kanjiCollection;
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
      _kanjiCollection = services.ServiceLocator.Resolve<KanjiCollection>();
      _index = services.ServiceLocator.Resolve<MediaFileIndex>();
      _storageService = services.ServiceLocator.Resolve<MediaStorageService>();
      _taskRunner = services.TaskRunner;
      _paths = services.ServiceLocator.Resolve<IEnvironmentPaths>();

      VocabTab = CreateVocabTab();
      SentenceTab = CreateSentenceTab();
      KanjiTab = CreateKanjiTab();

      LoadPersistedRules();
   }

   public NoteTypeImportTabViewModel<VocabImportRule> VocabTab { get; private set; } = null!;
   public NoteTypeImportTabViewModel<SentenceImportRule> SentenceTab { get; private set; } = null!;
   public NoteTypeImportTabViewModel<KanjiImportRule> KanjiTab { get; private set; } = null!;

   [ObservableProperty] string _statusText = "Click Scan to discover un-imported media.";
   [ObservableProperty] bool _isScanning;
   [ObservableProperty] bool _hasPlan;

   MediaImportPlan? _currentPlan;
   MediaImportPlan _vocabPlan = new();
   MediaImportPlan _sentencePlan = new();
   MediaImportPlan _kanjiPlan = new();
   [ObservableProperty] int _filesToImportCount;
   [ObservableProperty] int _alreadyStoredCount;
   [ObservableProperty] int _missingCount;

   public IRelayCommand? CloseCommand { get; set; }

   [RelayCommand] void AddVocabRule() => VocabTab.AddRuleCommand.Execute(null);
   [RelayCommand] void AddSentenceRule() => SentenceTab.AddRuleCommand.Execute(null);
   [RelayCommand] void AddKanjiRule() => KanjiTab.AddRuleCommand.Execute(null);

   [RelayCommand] void RemoveVocabRule(EditableImportRule rule) => VocabTab.RemoveRuleCommand.Execute(rule);
   [RelayCommand] void RemoveSentenceRule(EditableImportRule rule) => SentenceTab.RemoveRuleCommand.Execute(rule);
   [RelayCommand] void RemoveKanjiRule(EditableImportRule rule) => KanjiTab.RemoveRuleCommand.Execute(rule);

   [RelayCommand]
   void Scan()
   {
      IsScanning = true;
      StatusText = "Scanning notes for un-imported media...";

      _services.BackgroundTaskManager.Run(() =>
      {
         var vocabFiles = ScanNotes(_vocabCollection.All());
         var sentenceFiles = ScanNotes(_sentenceCollection.All());
         var kanjiFiles = ScanNotes(_kanjiCollection.All());

         Dispatcher.UIThread.Invoke(() =>
         {
            VocabTab.SetScannedFiles(vocabFiles);
            SentenceTab.SetScannedFiles(sentenceFiles);
            KanjiTab.SetScannedFiles(kanjiFiles);

            var total = vocabFiles.Count + sentenceFiles.Count + kanjiFiles.Count;
            StatusText = $"Scanned: {total} un-imported media files ({vocabFiles.Count} vocab, {sentenceFiles.Count} sentence, {kanjiFiles.Count} kanji).";
            IsScanning = false;
         });
      });
   }

   [RelayCommand]
   void Analyze()
   {
      StatusText = "Analyzing import plan...";
      HasPlan = false;

      _services.BackgroundTaskManager.Run(() =>
      {
         var ankiMediaDir = _paths.AnkiMediaDir;
         var analyzer = new MediaImportAnalyzer(ankiMediaDir, _index);

         var vocabRules = BuildVocabRules();
         var sentenceRules = BuildSentenceRules();
         var kanjiRules = BuildKanjiRules();

         var vocabPlan = vocabRules.Count > 0 ? analyzer.AnalyzeVocab(_vocabCollection.All(), vocabRules) : new MediaImportPlan();
         var sentencePlan = sentenceRules.Count > 0 ? analyzer.AnalyzeSentences(_sentenceCollection.All(), sentenceRules) : new MediaImportPlan();
         var kanjiPlan = kanjiRules.Count > 0 ? analyzer.AnalyzeKanji(_kanjiCollection.All(), kanjiRules) : new MediaImportPlan();

         var merged = MergePlans(vocabPlan, sentencePlan, kanjiPlan);

         Dispatcher.UIThread.Invoke(() =>
         {
            _vocabPlan = vocabPlan;
            _sentencePlan = sentencePlan;
            _kanjiPlan = kanjiPlan;
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
                         SentenceRules = BuildSentenceRules(),
                         KanjiRules = BuildKanjiRules()
                      };
      MediaImportRulePersistence.Save(persisted, _paths);
      StatusText = "Rules saved.";
   }

   [RelayCommand]
   void ShowMissingFiles()
   {
      var vocabRows = BuildMissingFileRows(_vocabPlan.Missing, noteId => _vocabCollection.WithIdOrNone(noteId)?.GetQuestion() ?? "?");
      var sentenceRows = BuildMissingFileRows(_sentencePlan.Missing, noteId => _sentenceCollection.WithIdOrNone(noteId)?.GetQuestion() ?? "?");
      var kanjiRows = BuildMissingFileRows(_kanjiPlan.Missing, noteId => _kanjiCollection.WithIdOrNone(noteId)?.GetQuestion() ?? "?");

      Dispatcher.UIThread.Invoke(() =>
      {
         var dialog = new Views.MissingFilesDialog(vocabRows, sentenceRows, kanjiRows, OpenNoteInAnki);
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

   static NoteTypeImportTabViewModel<VocabImportRule> CreateVocabTab()
   {
      var ruleSet = (MediaImportRuleSet?)null;
      return new NoteTypeImportTabViewModel<VocabImportRule>(
         "Vocab",
         VocabFieldNames,
         editableRules =>
         {
            var rules = BuildRulesFromEditableList<VocabImportRule, VocabMediaField>(editableRules);
            ruleSet = new MediaImportRuleSet(rules, [], []);
            return rules;
         },
         (sourceTag, fieldName) =>
         {
            if(ruleSet == null || !Enum.TryParse<VocabMediaField>(fieldName, out var field)) return null;
            return ruleSet.TryResolveVocab(sourceTag, field);
         });
   }

   static NoteTypeImportTabViewModel<SentenceImportRule> CreateSentenceTab()
   {
      var ruleSet = (MediaImportRuleSet?)null;
      return new NoteTypeImportTabViewModel<SentenceImportRule>(
         "Sentences",
         SentenceFieldNames,
         editableRules =>
         {
            var rules = BuildRulesFromEditableList<SentenceImportRule, SentenceMediaField>(editableRules);
            ruleSet = new MediaImportRuleSet([], rules, []);
            return rules;
         },
         (sourceTag, fieldName) =>
         {
            if(ruleSet == null || !Enum.TryParse<SentenceMediaField>(fieldName, out var field)) return null;
            return ruleSet.TryResolveSentence(sourceTag, field);
         });
   }

   static NoteTypeImportTabViewModel<KanjiImportRule> CreateKanjiTab()
   {
      var ruleSet = (MediaImportRuleSet?)null;
      return new NoteTypeImportTabViewModel<KanjiImportRule>(
         "Kanji",
         KanjiFieldNames,
         editableRules =>
         {
            var rules = BuildRulesFromEditableList<KanjiImportRule, KanjiMediaField>(editableRules);
            ruleSet = new MediaImportRuleSet([], [], rules);
            return rules;
         },
         (sourceTag, fieldName) =>
         {
            if(ruleSet == null || !Enum.TryParse<KanjiMediaField>(fieldName, out var field)) return null;
            return ruleSet.TryResolveKanji(sourceTag, field);
         });
   }

   static List<TRule> BuildRulesFromEditableList<TRule, TField>(List<EditableImportRule> editableRules) where TField : struct, Enum
   {
      var result = new List<TRule>();
      foreach(var r in editableRules)
      {
         if(!r.IsValid || !Enum.TryParse<TField>(r.SelectedField, out var field) || !Enum.TryParse<CopyrightStatus>(r.SelectedCopyright, out var copyright))
            continue;

         var sourceTag = SourceTag.Parse(r.SourceTagPrefix);
         object rule = typeof(TRule).Name switch
         {
            nameof(VocabImportRule)    => new VocabImportRule(sourceTag, (VocabMediaField)(object)field, r.TargetDirectory, copyright),
            nameof(SentenceImportRule) => new SentenceImportRule(sourceTag, (SentenceMediaField)(object)field, r.TargetDirectory, copyright),
            nameof(KanjiImportRule)    => new KanjiImportRule(sourceTag, (KanjiMediaField)(object)field, r.TargetDirectory, copyright),
            _                          => throw new InvalidOperationException($"Unknown rule type: {typeof(TRule).Name}")
         };
         result.Add((TRule)rule);
      }

      return result;
   }

   void LoadPersistedRules()
   {
      var persisted = MediaImportRulePersistence.Load(_paths);
      VocabTab.LoadRules(persisted.VocabRules.Select(r => new EditableImportRule
                                                          { SourceTagPrefix = r.Prefix.ToString(), SelectedField = r.Field.ToString(), TargetDirectory = r.TargetDirectory, SelectedCopyright = r.Copyright.ToString() }));
      SentenceTab.LoadRules(persisted.SentenceRules.Select(r => new EditableImportRule
                                                                { SourceTagPrefix = r.Prefix.ToString(), SelectedField = r.Field.ToString(), TargetDirectory = r.TargetDirectory, SelectedCopyright = r.Copyright.ToString() }));
      KanjiTab.LoadRules(persisted.KanjiRules.Select(r => new EditableImportRule
                                                          { SourceTagPrefix = r.Prefix.ToString(), SelectedField = r.Field.ToString(), TargetDirectory = r.TargetDirectory, SelectedCopyright = r.Copyright.ToString() }));
   }

   List<VocabImportRule> BuildVocabRules() => BuildRulesFromEditableList<VocabImportRule, VocabMediaField>(VocabTab.Rules.ToList());
   List<SentenceImportRule> BuildSentenceRules() => BuildRulesFromEditableList<SentenceImportRule, SentenceMediaField>(SentenceTab.Rules.ToList());
   List<KanjiImportRule> BuildKanjiRules() => BuildRulesFromEditableList<KanjiImportRule, KanjiMediaField>(KanjiTab.Rules.ToList());

   List<ScannedMediaFile> ScanNotes<TNote>(List<TNote> notes) where TNote : JPNote
   {
      var results = new List<ScannedMediaFile>();

      foreach(var note in notes)
      {
         var rawSourceTag = note.GetSourceTag();
         var sourceTag = string.IsNullOrEmpty(rawSourceTag) ? "anki::unknown" : $"source::{rawSourceTag}";

         foreach(var mediaRef in GetMediaReferences(note))
         {
            if(_index.ContainsByOriginalFileName(mediaRef.FileName)) continue;
            results.Add(new ScannedMediaFile(sourceTag, GetFieldName(note, mediaRef), mediaRef.FileName));
         }
      }

      return results;
   }

   static string GetFieldName(JPNote note, MediaReference mediaRef) =>
      note switch
      {
         VocabNote v    => IdentifyVocabField(v, mediaRef),
         SentenceNote s => IdentifySentenceField(s, mediaRef),
         KanjiNote k    => IdentifyKanjiField(k, mediaRef),
         _              => "Unknown"
      };

   static string IdentifyVocabField(VocabNote v, MediaReference mediaRef)
   {
      if(v.Audio.First.GetMediaReferences().Any(r => r.FileName == mediaRef.FileName)) return nameof(VocabMediaField.AudioFirst);
      if(v.Audio.Second.GetMediaReferences().Any(r => r.FileName == mediaRef.FileName)) return nameof(VocabMediaField.AudioSecond);
      if(v.Audio.Tts.GetMediaReferences().Any(r => r.FileName == mediaRef.FileName)) return nameof(VocabMediaField.AudioTts);
      if(v.Image.GetMediaReferences().Any(r => r.FileName == mediaRef.FileName)) return nameof(VocabMediaField.Image);
      if(v.UserImage.GetMediaReferences().Any(r => r.FileName == mediaRef.FileName)) return nameof(VocabMediaField.UserImage);
      return "Unknown";
   }

   static string IdentifySentenceField(SentenceNote s, MediaReference mediaRef)
   {
      if(s.Audio.GetMediaReferences().Any(r => r.FileName == mediaRef.FileName)) return nameof(SentenceMediaField.Audio);
      if(s.Screenshot.GetMediaReferences().Any(r => r.FileName == mediaRef.FileName)) return nameof(SentenceMediaField.Screenshot);
      return "Unknown";
   }

   static string IdentifyKanjiField(KanjiNote k, MediaReference mediaRef)
   {
      if(k.Audio.GetMediaReferences().Any(r => r.FileName == mediaRef.FileName)) return nameof(KanjiMediaField.Audio);
      if(k.Image.GetMediaReferences().Any(r => r.FileName == mediaRef.FileName)) return nameof(KanjiMediaField.Image);
      return "Unknown";
   }

   static List<MediaReference> GetMediaReferences(JPNote note) =>
      note switch
      {
         VocabNote v =>
         [
            ..v.Audio.First.GetMediaReferences(),
            ..v.Audio.Second.GetMediaReferences(),
            ..v.Audio.Tts.GetMediaReferences(),
            ..v.Image.GetMediaReferences(),
            ..v.UserImage.GetMediaReferences()
         ],
         SentenceNote s =>
         [
            ..s.Audio.GetMediaReferences(),
            ..s.Screenshot.GetMediaReferences()
         ],
         KanjiNote k =>
         [
            ..k.Audio.GetMediaReferences(),
            ..k.Image.GetMediaReferences()
         ],
         _ => []
      };

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
   [ObservableProperty] string _selectedCopyright = nameof(CopyrightStatus.Commercial);
   [ObservableProperty] int _matchCount;

   public IRelayCommand? RemoveSelfCommand { get; set; }

   public bool IsValid =>
      !string.IsNullOrWhiteSpace(SourceTagPrefix) &&
      !string.IsNullOrWhiteSpace(SelectedField) &&
      !string.IsNullOrWhiteSpace(TargetDirectory);

   public static List<string> CopyrightOptions { get; } = [nameof(CopyrightStatus.Unknown), nameof(CopyrightStatus.Free), nameof(CopyrightStatus.Commercial)];
}

record MissingFileRow(string Question, string NoteIdDisplay, string FieldName, string FileName, NoteId NoteId);
