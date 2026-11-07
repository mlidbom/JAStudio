using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JAStudio.TextToSpeech;
using JAStudio.TextToSpeech.Specifications;

namespace JAStudio.UI.Specifications;

/// <summary>
/// A tts-provider, named "Fake", standing in for the real services, which the specifications can't call. It answers at once, with tones generated as WAV,<br/>
/// so a dialog's work on the answer is done when the call returns. It records what it was asked to speak.
/// </summary>
/// <remarks>
/// <see cref="Steady"/> speaks a text the same way every time, as a standard voice does. <see cref="Varying"/> speaks it differently every time,<br/>
/// a quarter of a second longer each time, as an HD voice may. <see cref="Refused"/> is refused, as a service refuses a voice it doesn't know.
/// </remarks>
sealed class FakeTextToSpeechProvider : ITextToSpeechProvider
{
   const double SteadysSeconds = 1;
   int _timesVaryingSpoke;

   public string Name => "Fake";

   public TextToSpeechVoice Steady => new(this, "ja-JP-Steady", VoiceGender.Female);
   public TextToSpeechVoice Varying => new(this, "ja-JP-Varying", VoiceGender.Male);
   public TextToSpeechVoice Refused => new(this, "ja-JP-Refused", VoiceGender.Female);

   /// <summary>What it was asked to speak, as "voice: text", in order.</summary>
   public List<string> Spoken { get; } = [];

   public Task<IReadOnlyList<TextToSpeechVoice>> JapaneseVoicesAsync(CancellationToken cancellation) => Task.FromResult<IReadOnlyList<TextToSpeechVoice>>([Steady, Varying, Refused]);

   public Task<SynthesizedSpeech> SpeakAsync(string text, TextToSpeechVoice voice, CancellationToken cancellation)
   {
      Spoken.Add($"{voice.ShortName}: {text}");
      return voice.Name switch
      {
         "ja-JP-Steady" => Task.FromResult(new SynthesizedSpeech(text, voice, GeneratedWav.Tone(440 + 10 * text.Length, SteadysSeconds))),
         "ja-JP-Varying" => Task.FromResult(new SynthesizedSpeech(text, voice, GeneratedWav.Tone(330, SteadysSeconds + 0.25 * ++_timesVaryingSpoke))),
         _ => Task.FromException<SynthesizedSpeech>(new TextToSpeechRequestFailedException("Fake refused the request: 400 Bad Request."))
      };
   }
}
