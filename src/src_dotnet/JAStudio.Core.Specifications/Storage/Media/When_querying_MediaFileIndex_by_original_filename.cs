using System;
using System.IO;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Storage.Media;

public class When_querying_MediaFileIndex_by_original_filename : SpecificationStartingWithAnEmptyCollection
{
   readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"JAStudio_test_{Guid.NewGuid():N}");
   readonly MediaFileIndex _index;

   public When_querying_MediaFileIndex_by_original_filename()
   {
      Directory.CreateDirectory(_tempDir);
      _index = new MediaFileIndex(_tempDir, GetService<TaskRunner>());
   }

   public new void Dispose()
   {
      base.Dispose();
      Directory.Delete(_tempDir, recursive: true);
   }

   static void CreateMediaFile(string dir, string originalFileName)
   {
      Directory.CreateDirectory(dir);
      File.WriteAllText(Path.Combine(dir, originalFileName), "fake");
   }

   public class with_an_indexed_file : When_querying_MediaFileIndex_by_original_filename
   {
      public with_an_indexed_file()
      {
         var fileDir = Path.Combine(_tempDir, "a1");
         CreateMediaFile(fileDir, "test_audio.mp3");
         _index.Build();
      }

      [XF] public void it_finds_by_exact_name() =>
         _index.ContainsByOriginalFileName("test_audio.mp3").Must().BeTrue();

      [XF] public void it_finds_case_insensitively() =>
         _index.ContainsByOriginalFileName("TEST_AUDIO.MP3").Must().BeTrue();

      [XF] public void it_returns_false_for_unknown_name() =>
         _index.ContainsByOriginalFileName("nonexistent.mp3").Must().BeFalse();
   }

   public class with_a_registered_attachment : When_querying_MediaFileIndex_by_original_filename
   {
      public with_a_registered_attachment()
      {
         _index.Build();
         _index.Register(new AudioAttachment { OriginalFileName = "registered.mp3", FilePath = "/fake/path/registered.mp3" });
      }

      [XF] public void it_is_found_by_original_name() =>
         _index.ContainsByOriginalFileName("registered.mp3").Must().BeTrue();
   }
}
