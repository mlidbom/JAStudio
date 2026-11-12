using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Media;

/// <summary>
/// A note's audio, what its source gave it and the corpus's own, added since: which of it is played for the note, and what the Anki-projection writes<br/>
/// into its study-note (<see cref="JAStudio.Core.Note.JPNote.StudyNoteFields"/>).
/// </summary>
public class When_a_note_has_audio : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   static AudioReference TextToSpeech(string fileName) => new(fileName) { IsTextToSpeech = true };

   /// <summary>
   /// The vocab 文句 with WaniKani's two recordings from its source, 文句_b.mp3 and 文句_g.mp3, and the corpus's own TTS audio, hypertts-文句.mp3, and<br/>
   /// a Forvo recording, forvo-文句.mp3, added in that order.
   /// </summary>
   public class a_vocab_with_recordings_from_its_source_and_tts_audio_and_a_recording_of_the_corpuss_own : When_a_note_has_audio
   {
      readonly VocabNote _vocab;

      public a_vocab_with_recordings_from_its_source_and_tts_audio_and_a_recording_of_the_corpuss_own()
      {
         _vocab = CreateVocab("文句", "complaint", "もんく");
         _vocab.SourceData.Audio.Add(new AudioReference("文句_b.mp3") { VoiceGender = VoiceGender.Male });
         _vocab.SourceData.Audio.Add(new AudioReference("文句_g.mp3") { VoiceGender = VoiceGender.Female });
         _vocab.Audio.Add(TextToSpeech("hypertts-文句.mp3"));
         _vocab.Audio.Add(new AudioReference("forvo-文句.mp3"));
      }

      [XF] public void its_recordings_come_before_its_tts_audio_and_its_sources_recordings_first() =>
         _vocab.AllAudio.InTheOrderPreferred.Select(it => it.FileName).Must().SequenceEqual(["文句_b.mp3", "文句_g.mp3", "forvo-文句.mp3", "hypertts-文句.mp3"]);

      [XF] public void the_audio_played_for_it_is_its_sources_first_recording() => _vocab.AllAudio.Preferred.Must().Be(new AudioReference("文句_b.mp3") { VoiceGender = VoiceGender.Male });
      [XF] public void its_study_note_gets_its_first_recording_in_Audio_b() => _vocab.StudyNoteFields()["Audio_b"].Must().Be("[sound:文句_b.mp3]");
      [XF] public void its_study_note_gets_its_other_recordings_in_Audio_g() => _vocab.StudyNoteFields()["Audio_g"].Must().Be("[sound:文句_g.mp3][sound:forvo-文句.mp3]");
      [XF] public void its_study_note_gets_its_tts_audio_in_Audio_TTS() => _vocab.StudyNoteFields()["Audio_TTS"].Must().Be("[sound:hypertts-文句.mp3]");
      [XF] public void it_is_not_marked_as_having_only_tts_audio() => _vocab.GetMetaTags().Must().Satisfy(it => !it.Contains("tts_audio"));
   }

   /// <summary>The sentence 家に帰る with only TTS audio of the corpus's own, hypertts-家に帰る.mp3.</summary>
   public class a_sentence_with_only_tts_audio : When_a_note_has_audio
   {
      readonly SentenceNote _sentence;

      public a_sentence_with_only_tts_audio()
      {
         _sentence = CreateTestSentence("家に帰る", "Go home");
         _sentence.Audio.Add(TextToSpeech("hypertts-家に帰る.mp3"));
      }

      [XF] public void its_study_note_gets_it_in_Audio_Sentence() => _sentence.StudyNoteFields()["Audio Sentence"].Must().Be("[sound:hypertts-家に帰る.mp3]");
      [XF] public void it_is_marked_as_having_only_tts_audio() => _sentence.GetMetaTags().Must().Contain("tts_audio");
   }

   /// <summary>The sentence 家に帰る with a recording from its source, natsume_ep01.mp3, and TTS audio of the corpus's own, hypertts-家に帰る.mp3.</summary>
   public class a_sentence_with_a_recording_from_its_source_and_tts_audio_of_the_corpuss_own : When_a_note_has_audio
   {
      readonly SentenceNote _sentence;

      public a_sentence_with_a_recording_from_its_source_and_tts_audio_of_the_corpuss_own()
      {
         _sentence = CreateTestSentence("家に帰る", "Go home");
         _sentence.Audio.Add(TextToSpeech("hypertts-家に帰る.mp3"));
         _sentence.SourceData.Audio.Add(new AudioReference("natsume_ep01.mp3"));
      }

      [XF] public void the_audio_played_for_it_is_the_recording() => _sentence.AllAudio.Preferred.Must().Be(new AudioReference("natsume_ep01.mp3"));
      [XF] public void its_study_note_gets_only_the_recording_in_Audio_Sentence() => _sentence.StudyNoteFields()["Audio Sentence"].Must().Be("[sound:natsume_ep01.mp3]");
      [XF] public void it_is_not_marked_as_having_only_tts_audio() => _sentence.GetMetaTags().Must().Satisfy(it => !it.Contains("tts_audio"));
   }

   /// <summary>The sentence 家に帰る with no audio.</summary>
   public class a_sentence_without_audio : When_a_note_has_audio
   {
      readonly SentenceNote _sentence;

      public a_sentence_without_audio() => _sentence = CreateTestSentence("家に帰る", "Go home");

      [XF] public void no_audio_is_played_for_it() => _sentence.AllAudio.Preferred.Must().BeNull();
      [XF] public void its_study_note_gets_an_empty_Audio_Sentence() => _sentence.StudyNoteFields()["Audio Sentence"].Must().Be("");
      [XF] public void it_is_not_marked_as_having_only_tts_audio() => _sentence.GetMetaTags().Must().Satisfy(it => !it.Contains("tts_audio"));
   }
}
