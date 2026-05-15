using System;
using System.IO;
using Compze.Utilities.Testing.Must;
using Compze.Utilities.Testing.XUnit.BDD;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Storage.Media;

public class When_storing_a_media_file : SpecificationStartingWithAnEmptyCollection
{
   readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"JAStudio_test_{Guid.NewGuid():N}");
   readonly string _mediaRoot;
   readonly MediaFileIndex _index;
   readonly MediaStorageService _service;

   public When_storing_a_media_file()
   {
      Directory.CreateDirectory(_tempDir);
      _mediaRoot = Path.Combine(_tempDir, "media");
      _index = new MediaFileIndex(_mediaRoot, GetService<TaskRunner>(), GetService<BackgroundTaskManager>());
      _service = new MediaStorageService(_mediaRoot, _index);
   }

   public new void Dispose()
   {
      base.Dispose();
      Directory.Delete(_tempDir, recursive: true);
   }

   protected string CreateSourceFile(string content = "fake audio content")
   {
      var sourceDir = Path.Combine(_tempDir, "source");
      Directory.CreateDirectory(sourceDir);
      var path = Path.Combine(sourceDir, "test.mp3");
      File.WriteAllText(path, content);
      return path;
   }

   public class with_a_target_directory : When_storing_a_media_file
   {
      readonly MediaAttachment? _attachment;

      public with_a_target_directory()
      {
         var sourceFile = CreateSourceFile();
         _service.StoreFile(sourceFile,
                            "commercial-001",
                            "natsume_ep01_03m22s.mp3",
                            MediaType.Audio);
         _attachment = _index.TryGetByOriginalFileName("natsume_ep01_03m22s.mp3");
      }

      [XF] public void the_file_is_found_by_original_filename() => _attachment.Must().NotBeNull();
      [XF] public void the_file_exists_on_disk() => File.Exists(_attachment!.FilePath).Must().BeTrue();
      [XF] public void the_path_contains_the_target_directory() => _attachment!.FilePath.Must().Contain("commercial-001");
   }

   public class after_storing_a_file : When_storing_a_media_file
   {
      public after_storing_a_file()
      {
         var sourceFile = CreateSourceFile();
         _service.StoreFile(sourceFile,
                            "general",
                            "test.mp3",
                            MediaType.Audio);
      }

      [XF] public void stored_file_exists() => _index.ContainsByOriginalFileName("test.mp3").Must().BeTrue();
      [XF] public void unknown_file_does_not_exist() => _index.ContainsByOriginalFileName("nonexistent.mp3").Must().BeFalse();
   }

   public class when_rebuilding_the_index_from_filesystem : When_storing_a_media_file
   {
      readonly MediaFileIndex _freshIndex;

      public when_rebuilding_the_index_from_filesystem()
      {
         var sourceFile = CreateSourceFile();
         _service.StoreFile(sourceFile,
                            "general",
                            "ep01.mp3",
                            MediaType.Audio);

         _freshIndex = new MediaFileIndex(_mediaRoot, GetService<TaskRunner>(), GetService<BackgroundTaskManager>());
         _freshIndex.Build();
      }

      [XF] public void the_fresh_index_finds_the_file() => _freshIndex.ContainsByOriginalFileName("ep01.mp3").Must().BeTrue();
      [XF] public void the_original_filename_is_recoverable() => _freshIndex.TryGetByOriginalFileName("ep01.mp3")!.OriginalFileName!.Must().Be("ep01.mp3");
      [XF] public void the_attachment_is_readable() => _freshIndex.TryGetByOriginalFileName("ep01.mp3").Must().NotBeNull();
   }
}
