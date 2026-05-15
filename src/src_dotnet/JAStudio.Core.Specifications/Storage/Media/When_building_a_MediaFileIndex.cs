using System;
using System.IO;
using Compze.Utilities.Testing.Must;
using Compze.Utilities.Testing.XUnit.BDD;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeProtected.Global

namespace JAStudio.Core.Specifications.Storage.Media;

public class When_building_a_MediaFileIndex : SpecificationStartingWithAnEmptyCollection
{
   readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"JAStudio_test_{Guid.NewGuid():N}");
   readonly MediaFileIndex _index;

   public When_building_a_MediaFileIndex()
   {
      Directory.CreateDirectory(_tempDir);
      _index = new MediaFileIndex(_tempDir, GetService<TaskRunner>(), GetService<BackgroundTaskManager>());
   }

   public new void Dispose()
   {
      base.Dispose();
      Directory.Delete(_tempDir, recursive: true);
   }

   static void CreateMediaFile(string dir, string originalFileName)
   {
      Directory.CreateDirectory(dir);
      File.WriteAllText(Path.Combine(dir, originalFileName), "fake audio");
   }

   public class over_a_directory_with_media_files : When_building_a_MediaFileIndex
   {
      public over_a_directory_with_media_files()
      {
         var fileDir = Path.Combine(_tempDir, "anime", "natsume", "a1");
         CreateMediaFile(fileDir, "natsume_ep01_03m22s.mp3");
         _index.Build();
      }

      [XF] public void it_indexes_the_file() => _index.Count.Must().Be(1);

      public class and_looking_up_by_original_filename : over_a_directory_with_media_files
      {
         readonly MediaAttachment? _attachment;

         public and_looking_up_by_original_filename() =>
            _attachment = _index.TryGetByOriginalFileName("natsume_ep01_03m22s.mp3");

         [XF] public void it_finds_the_file() => _attachment.Must().NotBeNull();
         [XF] public void it_resolves_the_file_path() => File.Exists(_attachment!.FilePath).Must().BeTrue();
         [XF] public void it_is_an_audio_attachment() => (_attachment is AudioAttachment).Must().BeTrue();
      }

      public class and_querying_note_media_by_filename : over_a_directory_with_media_files
      {
         readonly NoteMedia _noteMedia;

         public and_querying_note_media_by_filename() =>
            _noteMedia = _index.GetNoteMedia([new MediaReference("natsume_ep01_03m22s.mp3", MediaType.Audio)]);

         [XF] public void it_returns_one_audio() => _noteMedia.Audio.Count.Must().Be(1);
         [XF] public void it_returns_no_images() => _noteMedia.Images.Count.Must().Be(0);
      }
   }

   public class over_a_directory_with_non_media_files_only : When_building_a_MediaFileIndex
   {
      public over_a_directory_with_non_media_files_only()
      {
         var fileDir = Path.Combine(_tempDir, "some", "path");
         Directory.CreateDirectory(fileDir);
         File.WriteAllText(Path.Combine(fileDir, "readme.txt"), "some text");
         File.WriteAllText(Path.Combine(fileDir, "notes.json"), "{}");
         _index.Build();
      }

      [XF] public void it_indexes_nothing() => _index.Count.Must().Be(0);
   }

   public class over_a_nonexistent_directory : When_building_a_MediaFileIndex
   {
      readonly MediaFileIndex _nonexistentIndex;

      public over_a_nonexistent_directory()
      {
         _nonexistentIndex = new MediaFileIndex(Path.Combine(_tempDir, "does_not_exist"), GetService<TaskRunner>(), GetService<BackgroundTaskManager>());
         _nonexistentIndex.Build();
      }

      [XF] public void it_indexes_nothing() => _nonexistentIndex.Count.Must().Be(0);
   }

   public class without_explicit_Build_call : When_building_a_MediaFileIndex
   {
      public without_explicit_Build_call()
      {
         var fileDir = Path.Combine(_tempDir, "a1");
         CreateMediaFile(fileDir, "test.mp3");
      }

      [XF] public void it_lazy_initializes_on_first_access() => _index.ContainsByOriginalFileName("test.mp3").Must().BeTrue();
   }

   public class querying_note_media_with_no_matches : When_building_a_MediaFileIndex
   {
      public querying_note_media_with_no_matches() => _index.Build();

      [XF] public void it_returns_empty_note_media() =>
         _index.GetNoteMedia([new MediaReference("unknown.mp3", MediaType.Audio)]).Audio.Count.Must().Be(0);
   }
}
