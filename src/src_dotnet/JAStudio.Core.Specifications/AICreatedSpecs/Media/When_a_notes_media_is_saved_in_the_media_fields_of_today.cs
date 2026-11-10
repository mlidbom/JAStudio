using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Media;

/// <summary>
/// A note's media lists written into the media fields the corpus stores them in until slice 9's step 3, Anki's markup in fixed fields, and read back, as<br/>
/// JAStudio saves and loads the corpus. Temporary, as those fields are: step 3 stores the lists themselves, and replaces this.
/// </summary>
public class When_a_notes_media_is_saved_in_the_media_fields_of_today : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   NoteSerializer Serializer => GetService<NoteSerializer>();

   /// <summary>
   /// The vocab 文句 with WaniKani's two recordings and a Core 2000 image from its source, and the corpus's own Forvo recording, JAStudio's own TTS audio and<br/>
   /// an image from the web, with an alt text.
   /// </summary>
   public class a_vocab_with_audio_and_images_from_its_source_and_of_the_corpuss_own : When_a_notes_media_is_saved_in_the_media_fields_of_today
   {
      static readonly ImageReference SourceImage = new("core2000_文句.jpg");
      static readonly ImageReference WebImage = new("web-文句.jpg") { AltText = "Complaints & \"grumbling\"" };
      readonly VocabNote _readBack;

      public a_vocab_with_audio_and_images_from_its_source_and_of_the_corpuss_own()
      {
         var vocab = CreateVocab("文句", "complaint", "もんく");
         vocab.SourceData.Audio.Add(new AudioReference("文句_b.mp3"));
         vocab.SourceData.Audio.Add(new AudioReference("文句_g.mp3"));
         vocab.Audio.Add(new AudioReference("jastudio-tts-azure-nanami-1.mp3") { IsOurs = true, IsTextToSpeech = true });
         vocab.Audio.Add(new AudioReference("forvo-文句.mp3"));
         vocab.SourceData.Images.Add(SourceImage);
         vocab.Images.Add(WebImage);
         _readBack = Serializer.DeserializeVocab(Serializer.Serialize(vocab));
      }

      [XF] public void its_audio_is_read_back_in_the_order_preferred_with_jastudios_own_tts_audio_as_ours() =>
         _readBack.AllAudio.InTheOrderPreferred.Must().SequenceEqual([
            new AudioReference("文句_b.mp3"),
            new AudioReference("文句_g.mp3"),
            new AudioReference("forvo-文句.mp3"),
            new AudioReference("jastudio-tts-azure-nanami-1.mp3") { IsOurs = true, IsTextToSpeech = true }]);

      [XF] public void all_its_audio_is_read_back_as_the_corpuss_own_since_the_fields_of_today_do_not_say_which_is_the_sources() =>
         (_readBack.SourceData.Audio.All.Count, _readBack.Audio.All.Count).Must().Be((0, 4));

      [XF] public void its_sources_images_are_read_back_as_its_sources() => _readBack.SourceData.Images.All.Must().SequenceEqual([SourceImage]);
      [XF] public void the_corpuss_own_images_are_read_back_as_its_own_with_their_alt_texts() => _readBack.Images.All.Must().SequenceEqual([WebImage]);
   }

   /// <summary>
   /// The sentence 家に帰る with an anime's recording and a screenshot from its source, and the corpus's own HyperTTS audio and pasted screenshot.
   /// </summary>
   public class a_sentence_with_audio_and_screenshots_from_its_source_and_of_the_corpuss_own : When_a_notes_media_is_saved_in_the_media_fields_of_today
   {
      readonly SentenceNote _readBack;

      public a_sentence_with_audio_and_screenshots_from_its_source_and_of_the_corpuss_own()
      {
         var sentence = CreateTestSentence("家に帰る", "Go home");
         sentence.SourceData.Audio.Add(new AudioReference("natsume_ep01.mp3"));
         sentence.Audio.Add(new AudioReference("hypertts-家に帰る.mp3") { IsTextToSpeech = true });
         sentence.SourceData.Screenshots.Add(new ImageReference("natsume_ep01.jpg"));
         sentence.Screenshots.Add(new ImageReference("paste-1.jpg"));
         _readBack = Serializer.DeserializeSentence(Serializer.Serialize(sentence));
      }

      [XF] public void its_recording_is_read_back_as_its_sources() => _readBack.SourceData.Audio.All.Must().SequenceEqual([new AudioReference("natsume_ep01.mp3")]);

      [XF] public void audio_whose_name_is_a_tts_files_is_read_back_as_the_corpuss_own_tts_audio() =>
         _readBack.Audio.All.Must().SequenceEqual([new AudioReference("hypertts-家に帰る.mp3") { IsTextToSpeech = true }]);

      [XF] public void all_its_screenshots_are_read_back_as_its_sources_in_the_order_its_card_shows_them() =>
         _readBack.AllScreenshots.Select(it => it.FileName).Must().SequenceEqual(["natsume_ep01.jpg", "paste-1.jpg"]);
   }

   /// <summary>The sentence 家に帰る tagged <c>_tts_audio</c>, whose audio's name, sentence-audio.mp3, isn't a TTS file's.</summary>
   public class a_sentence_tagged_as_having_tts_audio : When_a_notes_media_is_saved_in_the_media_fields_of_today
   {
      readonly SentenceNote _readBack;

      public a_sentence_tagged_as_having_tts_audio()
      {
         var sentence = CreateTestSentence("家に帰る", "Go home");
         sentence.Tags.Set(Tags.TTSAudio);
         sentence.SourceData.Audio.Add(new AudioReference("sentence-audio.mp3"));
         _readBack = Serializer.DeserializeSentence(Serializer.Serialize(sentence));
      }

      [XF] public void its_audio_is_read_back_as_the_corpuss_own_tts_audio() => _readBack.Audio.All.Must().SequenceEqual([new AudioReference("sentence-audio.mp3") { IsTextToSpeech = true }]);
   }
}
