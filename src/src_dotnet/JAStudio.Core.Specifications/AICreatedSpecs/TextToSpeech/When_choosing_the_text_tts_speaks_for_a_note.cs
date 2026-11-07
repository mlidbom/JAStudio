using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.TextToSpeech;

/// <summary>The text TTS speaks for a vocab or a sentence: its question, cleaned as HyperTTS cleaned it, and which audio field it goes into.</summary>
public class When_choosing_the_text_tts_speaks_for_a_note : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;
   readonly SentenceNote _sentence;

   public When_choosing_the_text_tts_speaks_for_a_note()
   {
      _vocab = CreateVocab("〜について", "about", "について");
      _sentence = CreateTestSentence("家に帰る", "Go home");
   }

   [XF] public void markup_is_removed() => IHasTextToSpeechAudio.SpeakableTextOf("<b>家</b>に<i>帰る</i>").Must().Be("家に帰る");
   [XF] public void a_wave_dash_is_removed() => IHasTextToSpeechAudio.SpeakableTextOf("〜について").Must().Be("について");
   [XF] public void a_vertical_bar_is_removed() => IHasTextToSpeechAudio.SpeakableTextOf("食べ|る").Must().Be("食べる");
   [XF] public void the_ends_are_trimmed() => IHasTextToSpeechAudio.SpeakableTextOf(" 行く ").Must().Be("行く");

   [XF] public void a_vocab_speaks_its_question_cleaned() => _vocab.TextToSpeak.Must().Be("について");
   [XF] public void a_vocabs_tts_audio_goes_into_its_tts_audio_field() => ReferenceEquals(_vocab.TextToSpeechAudio, _vocab.Audio.Tts).Must().BeTrue();
   [XF] public void a_sentence_speaks_its_question() => _sentence.TextToSpeak.Must().Be("家に帰る");
   [XF] public void a_sentences_tts_audio_goes_into_its_audio_field() => ReferenceEquals(_sentence.TextToSpeechAudio, _sentence.Audio).Must().BeTrue();
}
