using System.Collections.Generic;
using System.IO;
using JAStudio.Core.Note;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage.Media;

public class MediaImportAnalyzer
{
   readonly string _ankiMediaDir;
   readonly MediaFileIndex _index;

   public MediaImportAnalyzer(string ankiMediaDir, MediaFileIndex index)
   {
      _ankiMediaDir = ankiMediaDir;
      _index = index;
   }

   public bool IsAlreadyStored(string originalFileName) => _index.ContainsByOriginalFileName(originalFileName);

   public MediaImportPlan AnalyzeVocab(IReadOnlyList<VocabNote> notes, IReadOnlyList<VocabImportRule> rules)
   {
      var ruleSet = new MediaImportRuleSet([..rules], [], []);
      var plan = new MediaImportPlan();

      foreach(var note in notes)
      {
         var sourceTag = ResolveSourceTag(note);
         var noteId = note.GetId();

         AnalyzeField(note.Audio.First.GetMediaReferences(), ruleSet.TryResolveVocab(sourceTag, VocabMediaField.AudioFirst), nameof(VocabMediaField.AudioFirst), sourceTag, noteId, plan);
         AnalyzeField(note.Audio.Second.GetMediaReferences(), ruleSet.TryResolveVocab(sourceTag, VocabMediaField.AudioSecond), nameof(VocabMediaField.AudioSecond), sourceTag, noteId, plan);
         AnalyzeField(note.Audio.Tts.GetMediaReferences(), ruleSet.TryResolveVocab(sourceTag, VocabMediaField.AudioTts), nameof(VocabMediaField.AudioTts), sourceTag, noteId, plan);
         AnalyzeField(note.Image.GetMediaReferences(), ruleSet.TryResolveVocab(sourceTag, VocabMediaField.Image), nameof(VocabMediaField.Image), sourceTag, noteId, plan);
         AnalyzeField(note.UserImage.GetMediaReferences(), ruleSet.TryResolveVocab(sourceTag, VocabMediaField.UserImage), nameof(VocabMediaField.UserImage), sourceTag, noteId, plan);
      }

      return plan;
   }

   public MediaImportPlan AnalyzeSentences(IReadOnlyList<SentenceNote> notes, IReadOnlyList<SentenceImportRule> rules)
   {
      var ruleSet = new MediaImportRuleSet([], [..rules], []);
      var plan = new MediaImportPlan();

      foreach(var note in notes)
      {
         var sourceTag = ResolveSourceTag(note);
         var noteId = note.GetId();

         AnalyzeField(note.Audio.GetMediaReferences(), ruleSet.TryResolveSentence(sourceTag, SentenceMediaField.Audio), nameof(SentenceMediaField.Audio), sourceTag, noteId, plan);
         AnalyzeField(note.Screenshot.GetMediaReferences(), ruleSet.TryResolveSentence(sourceTag, SentenceMediaField.Screenshot), nameof(SentenceMediaField.Screenshot), sourceTag, noteId, plan);
      }

      return plan;
   }

   public MediaImportPlan AnalyzeKanji(IReadOnlyList<KanjiNote> notes, IReadOnlyList<KanjiImportRule> rules)
   {
      var ruleSet = new MediaImportRuleSet([], [], [..rules]);
      var plan = new MediaImportPlan();

      foreach(var note in notes)
      {
         var sourceTag = ResolveSourceTag(note);
         var noteId = note.GetId();

         AnalyzeField(note.Audio.GetMediaReferences(), ruleSet.TryResolveKanji(sourceTag, KanjiMediaField.Audio), nameof(KanjiMediaField.Audio), sourceTag, noteId, plan);
         AnalyzeField(note.Image.GetMediaReferences(), ruleSet.TryResolveKanji(sourceTag, KanjiMediaField.Image), nameof(KanjiMediaField.Image), sourceTag, noteId, plan);
      }

      return plan;
   }

   void AnalyzeField(List<MediaReference> references, VocabImportRule? rule, string fieldName, SourceTag sourceTag, NoteId noteId, MediaImportPlan plan) =>
      AnalyzeField(references, rule?.TargetDirectory, rule?.Copyright, fieldName, sourceTag, noteId, plan);

   void AnalyzeField(List<MediaReference> references, SentenceImportRule? rule, string fieldName, SourceTag sourceTag, NoteId noteId, MediaImportPlan plan) =>
      AnalyzeField(references, rule?.TargetDirectory, rule?.Copyright, fieldName, sourceTag, noteId, plan);

   void AnalyzeField(List<MediaReference> references, KanjiImportRule? rule, string fieldName, SourceTag sourceTag, NoteId noteId, MediaImportPlan plan) =>
      AnalyzeField(references, rule?.TargetDirectory, rule?.Copyright, fieldName, sourceTag, noteId, plan);

   void AnalyzeField(List<MediaReference> references, string? targetDirectory, CopyrightStatus? copyright, string fieldName, SourceTag sourceTag, NoteId noteId, MediaImportPlan plan)
   {
      if(targetDirectory == null || copyright == null) return;
      if(references.Count == 0) return;

      foreach(var reference in references)
      {
         var existing = _index.TryGetByOriginalFileName(reference.FileName);
         if(existing != null)
         {
            plan.AlreadyStored.Add(new AlreadyStoredFile(existing, noteId));
            continue;
         }

         var sourcePath = Path.Combine(_ankiMediaDir, reference.FileName);
         if(!File.Exists(sourcePath))
         {
            plan.Missing.Add(new MissingFile(reference.FileName, noteId, fieldName));
            continue;
         }

         plan.FilesToImport.Add(new PlannedFileImport(sourcePath, targetDirectory, copyright.Value, sourceTag, reference.FileName, noteId, reference.Type));
      }
   }

   static readonly SourceTag FallbackSourceTag = SourceTag.Parse("anki::unknown");

   static SourceTag ResolveSourceTag(JPNote note)
   {
      var rawSourceTag = note.GetSourceTag();
      return string.IsNullOrEmpty(rawSourceTag) ? FallbackSourceTag : SourceTag.Parse($"{Tags.Source.Folder}{rawSourceTag}");
   }
}
