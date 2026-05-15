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

   public MediaImportPlan AnalyzeVocab(IReadOnlyList<VocabNote> notes, IReadOnlyList<ImportRule> rules)
   {
      var plan = new MediaImportPlan();
      foreach(var note in notes)
      {
         var sourceTag = note.SourceTag;
         var noteId = note.GetId();
         AnalyzeField(note.Audio.First.GetMediaReferences(),  rules.TryResolve(sourceTag, nameof(VocabMediaField.AudioFirst)),  nameof(VocabMediaField.AudioFirst),  sourceTag, noteId, plan);
         AnalyzeField(note.Audio.Second.GetMediaReferences(), rules.TryResolve(sourceTag, nameof(VocabMediaField.AudioSecond)), nameof(VocabMediaField.AudioSecond), sourceTag, noteId, plan);
         AnalyzeField(note.Audio.Tts.GetMediaReferences(),    rules.TryResolve(sourceTag, nameof(VocabMediaField.AudioTts)),    nameof(VocabMediaField.AudioTts),    sourceTag, noteId, plan);
         AnalyzeField(note.Image.GetMediaReferences(),        rules.TryResolve(sourceTag, nameof(VocabMediaField.Image)),       nameof(VocabMediaField.Image),       sourceTag, noteId, plan);
         AnalyzeField(note.UserImage.GetMediaReferences(),    rules.TryResolve(sourceTag, nameof(VocabMediaField.UserImage)),   nameof(VocabMediaField.UserImage),   sourceTag, noteId, plan);
      }
      return plan;
   }

   public MediaImportPlan AnalyzeSentences(IReadOnlyList<SentenceNote> notes, IReadOnlyList<ImportRule> rules)
   {
      var plan = new MediaImportPlan();
      foreach(var note in notes)
      {
         var sourceTag = note.SourceTag;
         var noteId = note.GetId();
         AnalyzeField(note.Audio.GetMediaReferences(),      rules.TryResolve(sourceTag, nameof(SentenceMediaField.Audio)),      nameof(SentenceMediaField.Audio),      sourceTag, noteId, plan);
         AnalyzeField(note.Screenshot.GetMediaReferences(), rules.TryResolve(sourceTag, nameof(SentenceMediaField.Screenshot)), nameof(SentenceMediaField.Screenshot), sourceTag, noteId, plan);
      }
      return plan;
   }

   public MediaImportPlan AnalyzeKanji(IReadOnlyList<KanjiNote> notes, IReadOnlyList<ImportRule> rules)
   {
      var plan = new MediaImportPlan();
      foreach(var note in notes)
      {
         var sourceTag = note.SourceTag;
         var noteId = note.GetId();
         AnalyzeField(note.Audio.GetMediaReferences(), rules.TryResolve(sourceTag, nameof(KanjiMediaField.Audio)), nameof(KanjiMediaField.Audio), sourceTag, noteId, plan);
         AnalyzeField(note.Image.GetMediaReferences(), rules.TryResolve(sourceTag, nameof(KanjiMediaField.Image)), nameof(KanjiMediaField.Image), sourceTag, noteId, plan);
      }
      return plan;
   }

   void AnalyzeField(List<MediaReference> references, ImportRule? rule, string fieldName, SourceTag sourceTag, NoteId noteId, MediaImportPlan plan)
   {
      if(rule == null) return;
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

         plan.FilesToImport.Add(new PlannedFileImport(sourcePath, rule.TargetDirectory, sourceTag, reference.FileName, noteId, reference.Type));
      }
   }
}

