using System;
using System.Collections.Generic;
using System.IO;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeProtected.Global

namespace JAStudio.Core.Specifications.Storage.Media.AnkiSync;

public class When_importing_media_from_anki : SpecificationStartingWithAnEmptyCollection, IDisposable
{
   readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"JAStudio_test_{Guid.NewGuid():N}");
   readonly string _ankiMediaDir;
   readonly MediaFileIndex _index;
   readonly NoteMediaFieldScanner _scanner;
   readonly MediaImportExecutor _executor;

   public When_importing_media_from_anki()
   {
      _ankiMediaDir = Path.Combine(_tempDir, "anki_media");
      var mediaRoot = Path.Combine(_tempDir, "corpus_files");
      Directory.CreateDirectory(_ankiMediaDir);
      Directory.CreateDirectory(mediaRoot);

      _index = new MediaFileIndex(mediaRoot, GetService<TaskRunner>());
      var storageService = new MediaStorageService(mediaRoot, _index);
      _scanner = new NoteMediaFieldScanner(_ankiMediaDir, _index);
      _executor = new MediaImportExecutor(storageService, GetService<TaskRunner>());
   }

   MediaImportPlan ScanVocabAndBuildPlan(IReadOnlyList<VocabNote> notes, IReadOnlyList<ImportRule> rules)
   {
      var scans = _scanner.GetVocabMediaFieldsImportState(notes);
      foreach(var scan in scans) scan.MatchingRule = rules.TryResolve(scan.SourceTag, scan.FieldName);
      return MediaImportPlan.From(scans);
   }

   MediaImportPlan ScanSentencesAndBuildPlan(IReadOnlyList<SentenceNote> notes, IReadOnlyList<ImportRule> rules)
   {
      var scans = _scanner.GetSentenceMediaFieldsImportState(notes);
      foreach(var scan in scans) scan.MatchingRule = rules.TryResolve(scan.SourceTag, scan.FieldName);
      return MediaImportPlan.From(scans);
   }

   public new void Dispose()
   {
      base.Dispose();
      Directory.Delete(_tempDir, recursive: true);
   }

   void CreateAnkiMediaFile(string fileName, string content = "fake content")
   {
      File.WriteAllText(Path.Combine(_ankiMediaDir, fileName), content);
   }

   public class for_a_vocab_batch_with_audio_and_image : When_importing_media_from_anki
   {
      readonly MediaImportPlan _plan;

      public for_a_vocab_batch_with_audio_and_image()
      {
         CreateAnkiMediaFile("vocab_audio.mp3");
         CreateAnkiMediaFile("vocab_image.jpg");

         var note = CreateVocab("走る", "to run", "はしる");
         note.Audio.First.SetRawValue("[sound:vocab_audio.mp3]");
         note.Image.SetRawValue("<img src=\"vocab_image.jpg\">");

         _plan = ScanVocabAndBuildPlan([note],
                                       [
                                          new ImportRule(SourceTag.Parse("anki"), nameof(VocabMediaField.AudioFirst), "general"),
                                          new ImportRule(SourceTag.Parse("anki"), nameof(VocabMediaField.Image),      "general")
                                       ]);
         _executor.Execute(_plan);
      }

      [XF] public void it_plans_two_files() => _plan.FilesToImport.Count.Must().Be(2);
      [XF] public void it_stores_both_files() => _index.Count.Must().Be(2);

      [XF] public void the_audio_file_is_indexed_by_original_name() =>
         _index.ContainsByOriginalFileName("vocab_audio.mp3").Must().BeTrue();

      [XF] public void the_image_file_is_indexed_by_original_name() =>
         _index.ContainsByOriginalFileName("vocab_image.jpg").Must().BeTrue();
   }

   public class for_a_sentence_batch_with_audio_and_screenshot : When_importing_media_from_anki
   {
      public for_a_sentence_batch_with_audio_and_screenshot()
      {
         CreateAnkiMediaFile("sentence_audio.mp3");
         CreateAnkiMediaFile("screenshot.png");

         var note = CreateSentence("テスト文");
         note.Audio.SetRawValue("[sound:sentence_audio.mp3]");
         note.Screenshot.SetRawValue("<img src=\"screenshot.png\">");

         var plan = ScanSentencesAndBuildPlan([note],
                                              [
                                                 new ImportRule(SourceTag.Parse("anki"), nameof(SentenceMediaField.Audio),      "general"),
                                                 new ImportRule(SourceTag.Parse("anki"), nameof(SentenceMediaField.Screenshot), "general")
                                              ]);
         _executor.Execute(plan);
      }

      [XF] public void it_copies_both_files() => _index.Count.Must().Be(2);

      [XF] public void the_audio_is_indexed() =>
         _index.ContainsByOriginalFileName("sentence_audio.mp3").Must().BeTrue();

      [XF] public void the_screenshot_is_indexed() =>
         _index.ContainsByOriginalFileName("screenshot.png").Must().BeTrue();
   }

   public class when_a_file_is_already_stored : When_importing_media_from_anki
   {
      readonly MediaImportPlan _secondPlan;

      public when_a_file_is_already_stored()
      {
         CreateAnkiMediaFile("already_stored.mp3");

         var note = CreateVocab("食べる", "to eat", "たべる");
         note.Audio.First.SetRawValue("[sound:already_stored.mp3]");

         var rules = new[] { new ImportRule(SourceTag.Parse("anki"), nameof(VocabMediaField.AudioFirst), "general") };

         var firstPlan = ScanVocabAndBuildPlan([note], rules);
         _executor.Execute(firstPlan);
         _index.Count.Must().Be(1);

         _secondPlan = ScanVocabAndBuildPlan([note], rules);
         _executor.Execute(_secondPlan);
      }

      [XF] public void it_does_not_duplicate_the_file() => _index.Count.Must().Be(1);
      [XF] public void the_second_plan_has_one_already_stored() => _secondPlan.AlreadyStored.Count.Must().Be(1);
      [XF] public void the_second_plan_has_no_files_to_import() => _secondPlan.FilesToImport.Count.Must().Be(0);
   }

   public class when_the_source_file_is_missing : When_importing_media_from_anki
   {
      readonly MediaImportPlan _plan;

      public when_the_source_file_is_missing()
      {
         var note = CreateVocab("飲む", "to drink", "のむ");
         note.Audio.First.SetRawValue("[sound:missing_audio.mp3]");

         _plan = ScanVocabAndBuildPlan([note],
                                       [new ImportRule(SourceTag.Parse("anki"), nameof(VocabMediaField.AudioFirst), "general")]);
         _executor.Execute(_plan);
      }

      [XF] public void it_imports_nothing() => _index.Count.Must().Be(0);
      [XF] public void the_plan_reports_one_missing() => _plan.Missing.Count.Must().Be(1);
      [XF] public void the_missing_file_has_the_correct_name() => _plan.Missing[0].FileName.Must().Be("missing_audio.mp3");
   }

   public class when_a_field_has_no_matching_rule : When_importing_media_from_anki
   {
      readonly MediaImportPlan _plan;

      public when_a_field_has_no_matching_rule()
      {
         CreateAnkiMediaFile("audio.mp3");
         CreateAnkiMediaFile("image.jpg");

         var note = CreateVocab("見る", "to see", "みる");
         note.Audio.First.SetRawValue("[sound:audio.mp3]");
         note.Image.SetRawValue("<img src=\"image.jpg\">");

         // Only configure AudioFirst — Image has no rule
         _plan = ScanVocabAndBuildPlan([note],
                                       [new ImportRule(SourceTag.Parse("anki"), nameof(VocabMediaField.AudioFirst), "general")]);
         _executor.Execute(_plan);
      }

      [XF] public void it_imports_only_the_matched_field() => _index.Count.Must().Be(1);
      [XF] public void the_plan_has_one_file_to_import() => _plan.FilesToImport.Count.Must().Be(1);

      [XF] public void the_audio_is_imported() =>
         _index.ContainsByOriginalFileName("audio.mp3").Must().BeTrue();

      [XF] public void the_image_is_not_imported() =>
         _index.ContainsByOriginalFileName("image.jpg").Must().BeFalse();
   }

   public class for_a_vocab_batch_with_typed_attachments : When_importing_media_from_anki
   {
      readonly MediaAttachment? _audioAttachment;
      readonly MediaAttachment? _imageAttachment;

      public for_a_vocab_batch_with_typed_attachments()
      {
         CreateAnkiMediaFile("routed_audio.mp3");
         CreateAnkiMediaFile("routed_image.jpg");

         var note = CreateVocab("書く", "to write", "かく");
         note.Audio.First.SetRawValue("[sound:routed_audio.mp3]");
         note.Image.SetRawValue("<img src=\"routed_image.jpg\">");

         var plan = ScanVocabAndBuildPlan([note],
                                          [
                                             new ImportRule(SourceTag.Parse("anki"), nameof(VocabMediaField.AudioFirst), "general"),
                                             new ImportRule(SourceTag.Parse("anki"), nameof(VocabMediaField.Image),      "general")
                                          ]);
         _executor.Execute(plan);

         foreach(var attachment in _index.All)
         {
            if(attachment.OriginalFileName == "routed_audio.mp3") _audioAttachment = attachment;
            if(attachment.OriginalFileName == "routed_image.jpg") _imageAttachment = attachment;
         }
      }

      [XF] public void the_audio_is_stored() => _audioAttachment.Must().NotBeNull();
      [XF] public void the_image_is_stored() => _imageAttachment.Must().NotBeNull();
      [XF] public void the_audio_is_an_audio_attachment() => (_audioAttachment is AudioAttachment).Must().BeTrue();
      [XF] public void the_image_is_an_image_attachment() => (_imageAttachment is ImageAttachment).Must().BeTrue();
   }
}
