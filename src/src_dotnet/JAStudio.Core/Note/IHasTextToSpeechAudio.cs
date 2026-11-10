using JAStudio.Core.Note.Media;

namespace JAStudio.Core.Note;

/// <summary>
/// A note whose audio text-to-speech can generate: a vocab or a sentence. It says what text TTS speaks for it, and has the list its TTS audio goes<br/>
/// into: the corpus's own audio. See <c>dev_docs/text-to-speech.md</c>.
/// </summary>
public interface IHasTextToSpeechAudio
{
   /// <summary>The text TTS speaks for the note: its question, cleaned with <see cref="SpeakableTextOf"/>. The TTS dialog lets it be edited before speaking.</summary>
   string TextToSpeak { get; }

   /// <summary>
   /// The corpus's own audio for the note, added since its source, which JAStudio's TTS audio goes into: using a speech replaces the TTS audio there<br/>
   /// (<see cref="AudioReferenceList.ReplaceTheTextToSpeechAudioWith"/>).
   /// </summary>
   AudioReferenceList Audio { get; }

   /// <summary>All the note's audio, its source's and the corpus's own, in the order it's preferred in.</summary>
   AllAudioOfANote AllAudio { get; }

   /// <summary><paramref name="question"/> as TTS should speak it, cleaned as HyperTTS cleaned it: without markup, and without <c>〜</c> and <c>|</c>.</summary>
   static string SpeakableTextOf(string question) => StringExtensions.StripHtmlMarkup(question).Replace("〜", "").Replace("|", "").Trim();
}
