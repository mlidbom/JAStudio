using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note;

/// <summary>
/// A note whose audio text-to-speech can generate: a vocab or a sentence. It says what text TTS speaks for it, and which of its audio<br/>
/// fields TTS writes into. See <c>dev_docs/text-to-speech.md</c>.
/// </summary>
public interface IHasTextToSpeechAudio
{
   /// <summary>The text TTS speaks for the note: its question, cleaned with <see cref="SpeakableTextOf"/>. The TTS dialog lets it be edited before speaking.</summary>
   string TextToSpeak { get; }

   /// <summary>The audio field TTS writes into: a vocab's <c>Audio_TTS</c>, a sentence's <c>Audio Sentence</c>, as HyperTTS's TTS presets did.</summary>
   WritableAudioValue TextToSpeechAudio { get; }

   /// <summary><paramref name="question"/> as TTS should speak it, cleaned as HyperTTS cleaned it: without markup, and without <c>〜</c> and <c>|</c>.</summary>
   static string SpeakableTextOf(string question) => StringExtensions.StripHtmlMarkup(question).Replace("〜", "").Replace("|", "").Trim();
}
