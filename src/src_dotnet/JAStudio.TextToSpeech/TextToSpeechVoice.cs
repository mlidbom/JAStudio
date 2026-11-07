using System;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.TextToSpeech;

/// <summary>One of a tts-provider's voices, such as Azure's <c>ja-JP-DaichiNeural</c>, which can speak a text.</summary>
/// <param name="Provider">The tts-provider the voice belongs to.</param>
/// <param name="Name">The name the tts-provider knows the voice by, such as <c>ja-JP-DaichiNeural</c> or <c>ja-JP-Neural2-B</c>.</param>
/// <param name="Gender">The gender the tts-provider gives the voice.</param>
public sealed record TextToSpeechVoice(ITextToSpeechProvider Provider, string Name, VoiceGender Gender)
{
   const string JapanesePrefix = "ja-JP-";

   /// <summary>The <see cref="Name"/> without the <c>ja-JP-</c>, in any case, that Japanese voices' names start with, such as <c>DaichiNeural</c>, for showing in a list.</summary>
   public string ShortName => Name.StartsWith(JapanesePrefix, StringComparison.OrdinalIgnoreCase) ? Name[JapanesePrefix.Length..] : Name;

   /// <summary>
   /// Has this voice speak <paramref name="text"/>, brought to ReplayGain's reference loudness (<see cref="SynthesizedSpeech.NormalizedToTheReplayGainReference"/>),<br/>
   /// so that every voice sounds as loud as the others, and as the TTS audio HyperTTS made.
   /// </summary>
   public async Task<SynthesizedSpeech> SpeakAsync(string text, CancellationToken cancellation) =>
      (await Provider.SpeakAsync(text, this, cancellation)).NormalizedToTheReplayGainReference();

   public override string ToString() => $"{Provider.Name} {ShortName}";
}
