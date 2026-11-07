using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.TextToSpeech;

/// <summary>
/// A tts-provider: a text-to-speech service JAStudio calls itself, such as Azure Speech or Google Cloud Text-to-Speech.<br/>
/// It lists its Japanese voices, and speaks a text with one of them, as MP3. See <c>dev_docs/text-to-speech.md</c>.
/// </summary>
/// <remarks>A request the service refuses throws a <see cref="TextToSpeechRequestFailedException"/>, naming the tts-provider and the HTTP status.</remarks>
public interface ITextToSpeechProvider
{
   /// <summary>The tts-provider's name as JAStudio shows it, such as "Azure".</summary>
   string Name { get; }

   /// <summary>The tts-provider's voices for Japanese as spoken in Japan (<c>ja-JP</c>), as the service lists them now.</summary>
   Task<IReadOnlyList<TextToSpeechVoice>> JapaneseVoicesAsync(CancellationToken cancellation);

   /// <summary>Has <paramref name="voice"/>, one of this tts-provider's voices, speak <paramref name="text"/>.</summary>
   Task<SynthesizedSpeech> SpeakAsync(string text, TextToSpeechVoice voice, CancellationToken cancellation);
}
