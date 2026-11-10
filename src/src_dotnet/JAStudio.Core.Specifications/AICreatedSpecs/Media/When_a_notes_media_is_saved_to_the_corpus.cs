using System.Linq;
using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Media;

/// <summary>
/// A note's media lists, its source's and the corpus's own, with the facts known about each file, written as the corpus's JSON and read back, as JAStudio<br/>
/// saves and loads the corpus.
/// </summary>
public class When_a_notes_media_is_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected NoteSerializer Serializer => GetService<NoteSerializer>();

   /// <summary>
   /// The vocab 文句 with WaniKani's recordings in a male and a female voice and a Core 2000 image from its source, and the corpus's own Forvo recording, of<br/>
   /// which nothing is known, TTS audio of ours with its voice, an image from the web with an alt text, and an AI image of ours.
   /// </summary>
   public class a_vocab_with_audio_and_images_from_its_source_and_of_the_corpuss_own : When_a_notes_media_is_saved_to_the_corpus
   {
      static readonly AudioReference MaleRecording = new("文句_b.mp3") { VoiceGender = VoiceGender.Male };
      static readonly AudioReference FemaleRecording = new("文句_g.mp3") { VoiceGender = VoiceGender.Female };
      static readonly AudioReference ForvoRecording = new("forvo-文句.mp3");
      static readonly AudioReference TextToSpeech = new("jastudio-tts-1.mp3") { IsOurs = true, IsTextToSpeech = true, TextToSpeechVoice = "Azure ja-JP-NanamiNeural", VoiceGender = VoiceGender.Female };
      static readonly ImageReference SourceImage = new("4e0a8f3c.jpg");
      static readonly ImageReference WebImage = new("web-文句.jpg") { AltText = "Complaints & \"grumbling\"" };
      static readonly ImageReference AIImage = new("OIG3.jpg") { IsOurs = true };
      readonly string _json;
      readonly VocabNote _readBack;

      public a_vocab_with_audio_and_images_from_its_source_and_of_the_corpuss_own()
      {
         var vocab = CreateVocab("文句", "complaint", "もんく");
         vocab.SourceData.Audio.Add(MaleRecording);
         vocab.SourceData.Audio.Add(FemaleRecording);
         vocab.SourceData.Images.Add(SourceImage);
         vocab.Audio.Add(ForvoRecording);
         vocab.Audio.Add(TextToSpeech);
         vocab.Images.Add(WebImage);
         vocab.Images.Add(AIImage);
         _json = Serializer.Serialize(vocab);
         _readBack = Serializer.DeserializeVocab(_json);
      }

      JsonElement TheWrittenVocab => JsonDocument.Parse(_json).RootElement;

      /// <summary>The properties written for a reference, each with its value as JSON.</summary>
      static (string Name, string Value)[] PropertiesOf(JsonElement reference) => [..reference.EnumerateObject().Select(it => (it.Name, it.Value.GetRawText()))];

      [XF] public void its_sources_audio_is_read_back_as_its_sources_with_the_voices_genders() => _readBack.SourceData.Audio.All.Must().SequenceEqual([MaleRecording, FemaleRecording]);
      [XF] public void its_sources_images_are_read_back_as_its_sources() => _readBack.SourceData.Images.All.Must().SequenceEqual([SourceImage]);
      [XF] public void the_corpuss_own_audio_is_read_back_as_its_own_with_every_fact_known() => _readBack.Audio.All.Must().SequenceEqual([ForvoRecording, TextToSpeech]);
      [XF] public void the_corpuss_own_images_are_read_back_as_its_own_with_their_alt_texts_and_whether_they_are_ours() => _readBack.Images.All.Must().SequenceEqual([WebImage, AIImage]);

      [XF] public void a_file_of_which_nothing_is_known_is_written_as_its_name_alone() =>
         PropertiesOf(TheWrittenVocab.GetProperty("audio")[0]).Select(it => it.Name).Must().SequenceEqual(["fileName"]);

      [XF] public void the_facts_known_are_written_with_the_voices_gender_in_lower_case() =>
         PropertiesOf(TheWrittenVocab.GetProperty("audio")[1]).Must().SequenceEqual([
            ("fileName", "\"jastudio-tts-1.mp3\""), ("isOurs", "true"), ("isTextToSpeech", "true"), ("textToSpeechVoice", "\"Azure ja-JP-NanamiNeural\""), ("voiceGender", "\"female\"")]);

      [XF] public void its_sources_media_is_written_in_its_source_data() =>
         PropertiesOf(TheWrittenVocab.GetProperty("sourceData").GetProperty("audio")[0]).Must().SequenceEqual([("fileName", "\"文句_b.mp3\""), ("voiceGender", "\"male\"")]);

      [XF] public void the_vocab_is_written_the_same_again_after_being_read_back() => Serializer.Serialize(_readBack).Must().Be(_json);

      public class read_from_a_file_with_a_property_the_vocab_does_not_have : a_vocab_with_audio_and_images_from_its_source_and_of_the_corpuss_own
      {
         [XF] public void such_as_an_older_formats_audio_field_reading_it_fails_rather_than_reading_a_vocab_without_what_it_held() =>
            Invoking(() => Serializer.DeserializeVocab(_json.Replace("\"audio\": [", "\"audioB\": \"[sound:文句_b.mp3]\",\n  \"audio\": ["))).Must().Throw<JsonException>();
      }
   }

   /// <summary>The sentence 家に帰る with an anime's recording and a frame of the scene from its source, and the corpus's own TTS audio and pasted screenshot.</summary>
   public class a_sentence_with_audio_and_screenshots_from_its_source_and_of_the_corpuss_own : When_a_notes_media_is_saved_to_the_corpus
   {
      static readonly AudioReference Recording = new("natsume_ep01.mp3");
      static readonly AudioReference TextToSpeech = new("hypertts-家に帰る.mp3") { IsOurs = true, IsTextToSpeech = true, TextToSpeechVoice = "Azure ja-JP-DaichiNeural", VoiceGender = VoiceGender.Male };
      static readonly ImageReference Frame = new("natsume_ep01.jpg");
      static readonly ImageReference PastedScreenshot = new("paste-1.jpg");
      readonly SentenceNote _readBack;

      public a_sentence_with_audio_and_screenshots_from_its_source_and_of_the_corpuss_own()
      {
         var sentence = CreateTestSentence("家に帰る", "Go home");
         sentence.SourceData.Audio.Add(Recording);
         sentence.SourceData.Screenshots.Add(Frame);
         sentence.Audio.Add(TextToSpeech);
         sentence.Screenshots.Add(PastedScreenshot);
         _readBack = Serializer.DeserializeSentence(Serializer.Serialize(sentence));
      }

      [XF] public void its_sources_recording_is_read_back_as_its_sources() => _readBack.SourceData.Audio.All.Must().SequenceEqual([Recording]);
      [XF] public void its_sources_frame_is_read_back_as_its_sources() => _readBack.SourceData.Screenshots.All.Must().SequenceEqual([Frame]);
      [XF] public void the_corpuss_own_tts_audio_is_read_back_as_its_own() => _readBack.Audio.All.Must().SequenceEqual([TextToSpeech]);
      [XF] public void the_corpuss_own_screenshot_is_read_back_as_its_own() => _readBack.Screenshots.All.Must().SequenceEqual([PastedScreenshot]);

      [XF] public void its_card_shows_its_sources_screenshot_then_its_own() => _readBack.AllScreenshots.Must().SequenceEqual([Frame, PastedScreenshot]);
   }
}
