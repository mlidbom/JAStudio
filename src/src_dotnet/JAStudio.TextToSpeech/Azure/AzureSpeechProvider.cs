using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Compze.Internals.SystemCE;

namespace JAStudio.TextToSpeech.Azure;

/// <summary>
/// Azure Speech as a tts-provider, called over its REST API with a Speech resource's key and region (<see cref="AzureSpeechSettings"/>).<br/>
/// Its voices include the standard neural voices, such as <c>ja-JP-DaichiNeural</c>, and HD voices, such as <c>ja-JP-Masaru:DragonHDLatestNeural</c>.
/// </summary>
/// <remarks>
/// Speaking sends SSML, which names the voice, and asks for <see cref="Mp3Format"/>. The text goes into the SSML as XML text, so characters<br/>
/// such as <c>&amp;</c> and <c>&lt;</c> are spoken rather than read as markup.
/// See <see href="https://learn.microsoft.com/azure/ai-services/speech-service/rest-text-to-speech">the REST API</see>.
/// </remarks>
public sealed class AzureSpeechProvider : ITextToSpeechProvider
{
   const string Japanese = "ja-JP";
   const string KeyHeader = "Ocp-Apim-Subscription-Key";

   /// <summary>
   /// MP3 at 96 kbit/s and 24 kHz, mono: the format of nearly all the TTS audio HyperTTS made, and by ear all but indistinguishable<br/>
   /// from Azure's best, 192 kbit/s at 48 kHz, at half the size (Magnus, 2026-11-07). The media repositories' size matters.
   /// </summary>
   const string Mp3Format = "audio-24khz-96kbitrate-mono-mp3";

   /// <summary>What a standard neural voice's name ends with, as in <c>ja-JP-DaichiNeural</c>.</summary>
   const string StandardNeuralSuffix = "Neural";

   /// <summary>What separates an HD voice's persona from its HD model, as in <c>ja-JP-Nanami:DragonHDLatestNeural</c>.</summary>
   const char HdModelSeparator = ':';

   /// <summary>The latest Dragon HD Omni model, which has an HD version of nearly every voice.</summary>
   const string DragonHdOmniModel = "DragonHDOmniLatestNeural";

   static readonly XNamespace Ssml = "http://www.w3.org/2001/10/synthesis";

   readonly AzureSpeechSettings _settings;
   readonly HttpClient _http;

   public AzureSpeechProvider(AzureSpeechSettings settings, HttpClient http)
   {
      _settings = settings;
      _http = http;
   }

   public string Name => "Azure";

   string Host => $"https://{_settings.Region}.tts.speech.microsoft.com";

   /// <remarks>
   /// Azure lists every voice, of every language, so only those whose <c>Locale</c> is <c>ja-JP</c> are kept.<br/>
   /// It doesn't list its Dragon HD Omni voices, an HD version of each standard neural voice, so one is added for each,<br/>
   /// named as Microsoft documents: <c>ja-JP-Daichi:DragonHDOmniLatestNeural</c> for <c>ja-JP-DaichiNeural</c>.<br/>
   /// Each Japanese one worked when tried (2026-11-07). Azure refuses a voice name it doesn't know, so one that went away would fail when it speaks.
   /// </remarks>
   public async Task<IReadOnlyList<TextToSpeechVoice>> JapaneseVoicesAsync(CancellationToken cancellation)
   {
      using var request = new HttpRequestMessage(HttpMethod.Get, $"{Host}/cognitiveservices/voices/list");
      request.Headers.Add(KeyHeader, _settings.Key);
      using var response = await _http.SendAsync(request, cancellation);
      await TextToSpeechRequestFailedException.ThrowUnlessItSucceeded(response, Name, cancellation);

      var listedVoices = await response.Content.ReadFromJsonAsync<JsonElement>(cancellation);
      List<TextToSpeechVoice> listedJapaneseVoices = [..listedVoices.EnumerateArray().Where(it => it.GetProperty("Locale").GetString() == Japanese).Select(VoiceListedAs)];
      return [..listedJapaneseVoices, ..listedJapaneseVoices.Where(IsAStandardNeuralVoice).Select(TheDragonHdOmniVersionOf)];
   }

   public async Task<SynthesizedSpeech> SpeakAsync(string text, TextToSpeechVoice voice, CancellationToken cancellation)
   {
      using var request = new HttpRequestMessage(HttpMethod.Post, $"{Host}/cognitiveservices/v1");
      request.Content = new StringContent(SsmlFor(text, voice), Encoding.UTF8, "application/ssml+xml");
      request.Headers.Add(KeyHeader, _settings.Key);
      request.Headers.Add("X-Microsoft-OutputFormat", Mp3Format);
      request.Headers.UserAgent.ParseAdd("JAStudio");
      using var response = await _http.SendAsync(request, cancellation);
      await TextToSpeechRequestFailedException.ThrowUnlessItSucceeded(response, Name, cancellation);

      return new SynthesizedSpeech(text, voice, await response.Content.ReadAsByteArrayAsync(cancellation));
   }

   /// <summary>The voice that <paramref name="listed"/>, an entry of Azure's voice list, describes: its <c>ShortName</c>, which the SSML names, and its <c>Gender</c>.</summary>
   TextToSpeechVoice VoiceListedAs(JsonElement listed) =>
      new(this, listed.GetProperty("ShortName").GetString() ?? throw new JsonException("Azure listed a voice with no name."), GenderNamed(listed.GetProperty("Gender").GetString()));

   /// <summary>
   /// Whether <paramref name="voice"/> is a standard neural voice, such as <c>ja-JP-DaichiNeural</c>,<br/>
   /// rather than an HD voice, whose name holds the HD model after a colon, such as <c>ja-JP-Nanami:DragonHDLatestNeural</c>.
   /// </summary>
   static bool IsAStandardNeuralVoice(TextToSpeechVoice voice) => !voice.Name.Contains(HdModelSeparator) && voice.Name.EndsWithOrdinal(StandardNeuralSuffix);

   /// <summary>The Dragon HD Omni version of <paramref name="standardVoice"/>: <c>ja-JP-Daichi:DragonHDOmniLatestNeural</c> for <c>ja-JP-DaichiNeural</c>, with its gender.</summary>
   static TextToSpeechVoice TheDragonHdOmniVersionOf(TextToSpeechVoice standardVoice) =>
      standardVoice with { Name = $"{standardVoice.Name[..^StandardNeuralSuffix.Length]}{HdModelSeparator}{DragonHdOmniModel}" };

   static string SsmlFor(string text, TextToSpeechVoice voice) =>
      new XElement(Ssml + "speak",
                   new XAttribute("version", "1.0"),
                   new XAttribute(XNamespace.Xml + "lang", Japanese),
                   new XElement(Ssml + "voice", new XAttribute("name", voice.Name), text))
        .ToString(SaveOptions.DisableFormatting);

   static TextToSpeechVoiceGender GenderNamed(string? gender) => gender switch
   {
      "Female" => TextToSpeechVoiceGender.Female,
      "Male" => TextToSpeechVoiceGender.Male,
      "Neutral" => TextToSpeechVoiceGender.Neutral,
      _ => TextToSpeechVoiceGender.Unspecified
   };
}
