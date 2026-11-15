using System;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.TextToSpeech;

/// <summary>One of a tts-provider's voices, such as Azure's <c>ja-JP-DaichiNeural</c>, which can speak a text.</summary>
/// <param name="Provider">The tts-provider the voice belongs to.</param>
/// <param name="Name">The name the tts-provider knows the voice by, such as <c>ja-JP-DaichiNeural</c> or <c>ja-JP-Neural2-B</c>.</param>
/// <param name="Gender">The gender the tts-provider gives the voice.</param>
public sealed record TextToSpeechVoice(ITextToSpeechProvider Provider, string Name, TextToSpeechVoiceGender Gender)
{
   const string JapanesePrefix = "ja-JP-";

   /// <summary>The <see cref="Name"/> without the <c>ja-JP-</c>, in any case, that Japanese voices' names start with, such as <c>DaichiNeural</c>, for showing in a list.</summary>
   public string ShortName => Name.StartsWith(JapanesePrefix, StringComparison.OrdinalIgnoreCase) ? Name[JapanesePrefix.Length..] : Name;

   /// <summary>How long a voice waits before asking again, the first time its tts-provider refuses it for the rate of requests. Each wait after it is twice as long.</summary>
   static readonly TimeSpan FirstWaitAfterARefusalForTheRate = TimeSpan.FromSeconds(1);

   /// <summary>The longest a voice waits before asking again. After it, a refusal for the rate is reported as any refusal is: about half a minute of waits in all.</summary>
   static readonly TimeSpan LongestWaitAfterARefusalForTheRate = TimeSpan.FromSeconds(16);

   /// <summary>
   /// Has this voice speak <paramref name="text"/>, brought to ReplayGain's reference loudness (<see cref="SynthesizedSpeech.NormalizedToTheReplayGainReference"/>),<br/>
   /// so that every voice sounds as loud as the others, and as the TTS audio HyperTTS made.
   /// </summary>
   /// <remarks>
   /// A request refused for the rate of requests (<see cref="TextToSpeechRequestFailedException.IsARefusalForTheRateOfRequests"/>) is made again after a wait,<br/>
   /// a second, then two, and so on up to sixteen, as when TTS audio is generated for many notes at once. Azure documents that it refuses so when a resource<br/>
   /// makes more requests than its tier allows, and while it scales up to more of them, and that every client should wait and ask again<br/>
   /// (<see href="https://learn.microsoft.com/azure/ai-services/speech-service/speech-services-quotas-and-limits">quotas and limits</see>). Google limits the requests a minute too.
   /// </remarks>
   public async Task<SynthesizedSpeech> SpeakAsync(string text, CancellationToken cancellation)
   {
      for(var wait = FirstWaitAfterARefusalForTheRate;; wait *= 2)
      {
         try
         {
            return (await Provider.SpeakAsync(text, this, cancellation)).NormalizedToTheReplayGainReference();
         }
         catch(TextToSpeechRequestFailedException refusal) when(refusal.IsARefusalForTheRateOfRequests && wait <= LongestWaitAfterARefusalForTheRate)
         {
            await Task.Delay(wait, cancellation);
         }
      }
   }

   public override string ToString() => $"{Provider.Name} {ShortName}";
}
