using System.Collections.Generic;
using System.Linq;

namespace JAStudio.TextToSpeech;

/// <summary>
/// A voice chosen to be offered when generating TTS audio for a note. The chosen voices are in priority order, and the first is the default,<br/>
/// which speaks as soon as the TTS dialog opens. Kept in the <see cref="TextToSpeechSettings"/>, so it holds names:<br/>
/// a <see cref="TextToSpeechVoice"/> belongs to a tts-provider object, which only exists while JAStudio runs.
/// </summary>
/// <param name="Provider">The tts-provider's <see cref="ITextToSpeechProvider.Name"/>, such as "Azure".</param>
/// <param name="Name">The voice's <see cref="TextToSpeechVoice.Name"/>, such as <c>ja-JP-Daichi:DragonHDOmniLatestNeural</c>.</param>
/// <param name="Gender">The voice's <see cref="TextToSpeechVoice.Gender"/>, kept so it shows without asking the tts-provider for its voice list.</param>
public sealed record ChosenVoice(string Provider, string Name, TextToSpeechVoiceGender Gender)
{
   public static ChosenVoice Of(TextToSpeechVoice voice) => new(voice.Provider.Name, voice.Name, voice.Gender);

   /// <summary>Whether this is <paramref name="voice"/>.</summary>
   public bool Is(TextToSpeechVoice voice) => voice.Provider.Name == Provider && voice.Name == Name;

   /// <summary>The voice this is, of its tts-provider among <paramref name="providers"/>, or null when there's no key for that tts-provider.</summary>
   public TextToSpeechVoice? VoiceOf(IEnumerable<ITextToSpeechProvider> providers) =>
      providers.FirstOrDefault(it => it.Name == Provider) is {} provider ? new TextToSpeechVoice(provider, Name, Gender) : null;
}
