using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Anki;
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
         var vocabNotes = LoadAnkiVocabNotes();
         var sentenceNotes = LoadAnkiSentenceNotes();
         var kanjiNotes = LoadAnkiKanjiNotes();

         var vocabFiles = ScanAnkiVocab(vocabNotes);
         var sentenceFiles = ScanAnkiSentences(sentenceNotes);
         var kanjiFiles = ScanAnkiKanji(kanjiNotes);

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

         var vocabNotes = LoadAnkiVocabNotes();
         var sentenceNotes = LoadAnkiSentenceNotes();
         var kanjiNotes = LoadAnkiKanjiNotes();

         var vocabPlan = vocabRules.Count > 0 ? analyzer.AnalyzeAnkiVocab(vocabNotes, vocabRules) : new MediaImportPlan();
         var sentencePlan = sentenceRules.Count > 0 ? analyzer.AnalyzeAnkiSentences(sentenceNotes, sentenceRules) : new MediaImportPlan();
         var kanjiPlan = kanjiRules.Count > 0 ? analyzer.AnalyzeAnkiKanji(kanjiNotes, kanjiRules) : new MediaImportPlan();

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

   List<AnkiVocabNote> LoadAnkiVocabNotes() => LoadAnkiNotesOfType(NoteTypes.Vocab, g => new VocabId(g), id => _vocabCollection.WithIdOrNone(id) != null, n => new AnkiVocabNote(n));
   List<AnkiSentenceNote> LoadAnkiSentenceNotes() => LoadAnkiNotesOfType(NoteTypes.Sentence, g => new SentenceId(g), id => _sentenceCollection.WithIdOrNone(id) != null, n => new AnkiSentenceNote(n));
   List<AnkiKanjiNote> LoadAnkiKanjiNotes() => LoadAnkiNotesOfType(NoteTypes.Kanji, g => new KanjiId(g), id => _kanjiCollection.WithIdOrNone(id) != null, n => new AnkiKanjiNote(n));

   static List<TWrapper> LoadAnkiNotesOfType<TWrapper>(string noteTypeName, Func<Guid, NoteId> idFactory, Func<NoteId, bool> isKnownInCorpus, Func<NoteData, TWrapper> wrap)
   {
      var dbPath = AnkiFacade.Col.DbFilePath();
      if(dbPath == null) return [];

      var loaded = NoteBulkLoader.LoadAllNotesOfType(dbPath, noteTypeName, idFactory);
      return loaded.Notes
                   .Where(n => n.Id != null && isKnownInCorpus(n.Id))
                   .Select(wrap)
                   .ToList();
   }

   List<ScannedMediaFile> ScanAnkiVocab(List<AnkiVocabNote> notes)
   {
      var results = new List<ScannedMediaFile>();
      foreach(var note in notes)
      {
         var sourceTag = ResolveSourceTagString(note.Tags);
         AddScannedReferences(results, sourceTag, nameof(VocabMediaField.AudioFirst), MediaFieldParsing.ParseAudioReferences(note.AudioB));
         AddScannedReferences(results, sourceTag, nameof(VocabMediaField.AudioSecond), MediaFieldParsing.ParseAudioReferences(note.AudioG));
         AddScannedReferences(results, sourceTag, nameof(VocabMediaField.AudioTts), MediaFieldParsing.ParseAudioReferences(note.AudioTTS));
         AddScannedReferences(results, sourceTag, nameof(VocabMediaField.Image), MediaFieldParsing.ParseImageReferences(note.Image));
         AddScannedReferences(results, sourceTag, nameof(VocabMediaField.UserImage), MediaFieldParsing.ParseImageReferences(note.UserImage));
      }
      return results;
   }

   List<ScannedMediaFile> ScanAnkiSentences(List<AnkiSentenceNote> notes)
   {
      var results = new List<ScannedMediaFile>();
      foreach(var note in notes)
      {
         var sourceTag = ResolveSourceTagString(note.Tags);
         AddScannedReferences(results, sourceTag, nameof(SentenceMediaField.Audio), MediaFieldParsing.ParseAudioReferences(note.Audio));
         AddScannedReferences(results, sourceTag, nameof(SentenceMediaField.Screenshot), MediaFieldParsing.ParseImageReferences(note.Screenshot));
      }
      return results;
   }

   List<ScannedMediaFile> ScanAnkiKanji(List<AnkiKanjiNote> notes)
   {
      var results = new List<ScannedMediaFile>();
      foreach(var note in notes)
      {
         var sourceTag = ResolveSourceTagString(note.Tags);
         AddScannedReferences(results, sourceTag, nameof(KanjiMediaField.Audio), MediaFieldParsing.ParseAudioReferences(note.Audio));
         AddScannedReferences(results, sourceTag, nameof(KanjiMediaField.Image), MediaFieldParsing.ParseImageReferences(note.Image));
      }
      return results;
   }

   void AddScannedReferences(List<ScannedMediaFile> results, string sourceTag, string fieldName, List<MediaReference> references)
   {
      foreach(var mediaRef in references)
      {
         if(_index.ContainsByOriginalFileName(mediaRef.FileName)) continue;
         results.Add(new ScannedMediaFile(sourceTag, fieldName, mediaRef.FileName));
      }
   }

   static string ResolveSourceTagString(IReadOnlyList<string> tags)
   {
      var sourceTag = tags.Where(t => t.StartsWith(Tags.Source.Folder))
                          .OrderBy(t => t.Length)
                          .FirstOrDefault();
      return sourceTag ?? "anki::unknown";
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
