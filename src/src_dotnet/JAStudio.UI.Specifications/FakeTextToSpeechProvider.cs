using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JAStudio.TextToSpeech;

namespace JAStudio.UI.Specifications;

/// <summary>
/// A tts-provider, named "Fake", standing in for the real services, which the specifications can't call. It answers at once, so a dialog's work<br/>
/// on the answer is done when the call returns. It records what it was asked to speak.
/// </summary>
/// <remarks>
/// Its speech is tones, as MP3s encoded as Azure's speech is, which JAStudio normalizes as it does all speech: the <c>FakeSpeech</c> folder,<br/>
/// made by <c>dev_scripts/generate-mp3-fixtures.cs</c>. <see cref="Steady"/> speaks every text the same way, a second of 440 Hz, as a standard voice<br/>
/// speaks a text the same way every time. <see cref="Varying"/> speaks differently every time, a quarter of a second longer each time, from 1.25 seconds,<br/>
/// as an HD voice may: up to three times. <see cref="Refused"/> is refused, as a service refuses a voice it doesn't know.
/// </remarks>
sealed class FakeTextToSpeechProvider : ITextToSpeechProvider
{
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
         "ja-JP-Steady" => Task.FromResult(new SynthesizedSpeech(text, voice, FakeSpeech("steady"))),
         "ja-JP-Varying" => Task.FromResult(new SynthesizedSpeech(text, voice, FakeSpeech($"varying-{++_timesVaryingSpoke}"))),
         _ => Task.FromException<SynthesizedSpeech>(new TextToSpeechRequestFailedException("Fake refused the request: 400 Bad Request."))
      };
   }

   static byte[] FakeSpeech(string name)
   {
      var path = Path.Combine(AppContext.BaseDirectory, "FakeSpeech", name + ".mp3");
      return File.Exists(path) ? File.ReadAllBytes(path) : throw new InvalidOperationException($"The fake tts-provider has no speech named {name}: add it with dev_scripts/generate-mp3-fixtures.cs.");
   }
}
