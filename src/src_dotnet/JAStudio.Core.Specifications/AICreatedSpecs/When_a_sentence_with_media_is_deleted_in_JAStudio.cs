using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Note.Tevents;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Storage.Media;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A sentence with an audio file of its own and a screenshot another sentence uses too, both in the media layer, deleted in JAStudio.<br/>
/// The media is part of the note, so the file only it uses goes with it, and the shared one stays.
/// </summary>
public class When_a_sentence_with_media_is_deleted_in_JAStudio : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly SentenceNote _deleted;
   readonly string _ownAudioPath;
   readonly string _sharedScreenshotPath;

   public When_a_sentence_with_media_is_deleted_in_JAStudio()
   {
      var mediaStorage = GetService<MediaStorageService>();
      _ownAudioPath = mediaStorage.StoreNewAudioFile([.. "ID3"u8], "free/sentences", "own-audio.mp3").FilePath;
      var screenshotToStore = Path.Combine(GetService<IEnvironmentPaths>().AddonRootDir, "shared-screenshot.jpg");
      File.WriteAllBytes(screenshotToStore, [0xFF, 0xD8, 0xFF]);
      mediaStorage.StoreFile(screenshotToStore, "free/sentences", "shared-screenshot.jpg", MediaType.Image);
      _sharedScreenshotPath = Path.Combine(GetService<IEnvironmentPaths>().MediaDir, "free/sentences", "shared-screenshot.jpg");

      _deleted = CreateTestSentence("部屋が広い。", "The room is large.");
      _deleted.Edit(it =>
      {
         it.SourceData.Audio.Add(new AudioReference("own-audio.mp3"));
         it.Screenshots.Add(new ImageReference("shared-screenshot.jpg"));
      });
      CreateTestSentence("部屋が狭い。", "The room is small.").Edit(it => it.SourceData.Screenshots.Add(new ImageReference("shared-screenshot.jpg")));

      GetService<JPCollection>().Delete(_deleted, ItsStudyNote.IsDeleted);
   }

   [XF] public void its_audio_file_that_no_other_note_uses_is_deleted_from_the_media_layer() => _ownAudioPath.Must().Satisfy(it => !File.Exists(it));
   [XF] public void the_media_file_index_no_longer_has_its_audio_file() => GetService<MediaFileIndex>().ContainsByOriginalFileName("own-audio.mp3").Must().BeFalse();
   [XF] public void its_screenshot_that_another_sentence_uses_stays() => _sharedScreenshotPath.Must().Satisfy(File.Exists);
   [XF] public void the_deletion_is_observed() => TeventsObservedSoFar().OfType<INoteTevent.Deleted>().Select(it => it.NoteId).Must().SequenceEqual([_deleted.GetId()]);
}
