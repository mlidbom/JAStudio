using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.TextToSpeech.Google;

/// <summary>
/// Google Cloud Text-to-Speech as a tts-provider, called over its REST API with an API key (<see cref="GoogleTextToSpeechSettings"/>).<br/>
/// Its voices include Neural2, WaveNet and Chirp 3 HD voices, such as <c>ja-JP-Neural2-B</c>.
/// </summary>
/// <remarks>
/// The text is sent as plain text, not SSML, which Chirp 3 HD voices may not support. The audio comes back as base64 in the response's JSON.<br/>
/// Google's MP3 is 32 kbit/s for Chirp 3 HD voices and 64 kbit/s for Neural2, at 24 kHz, below Azure's 96 kbit/s, but good enough. See <c>dev_docs/text-to-speech.md</c>.
/// </remarks>
public sealed class GoogleTextToSpeechProvider : ITextToSpeechProvider
{
   const string Japanese = "ja-JP";
   const string Host = "https://texttospeech.googleapis.com";
   const string KeyHeader = "X-Goog-Api-Key";

   readonly GoogleTextToSpeechSettings _settings;
   readonly HttpClient _http;

   public GoogleTextToSpeechProvider(GoogleTextToSpeechSettings settings, HttpClient http)
   {
      _settings = settings;
      _http = http;
   }

   public string Name => "Google";

   /// <remarks>Google lists only the voices for the language asked for, and leaves the list out of its JSON when there are none.</remarks>
   public async Task<IReadOnlyList<TextToSpeechVoice>> JapaneseVoicesAsync(CancellationToken cancellation)
   {
      using var request = new HttpRequestMessage(HttpMethod.Get, $"{Host}/v1/voices?languageCode={Japanese}");
      request.Headers.Add(KeyHeader, _settings.ApiKey);
      using var response = await _http.SendAsync(request, cancellation);
      await TextToSpeechRequestFailedException.ThrowUnlessItSucceeded(response, Name, cancellation);

      var answer = await response.Content.ReadFromJsonAsync<JsonElement>(cancellation);
      return answer.TryGetProperty("voices", out var listedVoices) ? [..listedVoices.EnumerateArray().Select(VoiceListedAs)] : [];
   }

   public async Task<SynthesizedSpeech> SpeakAsync(string text, TextToSpeechVoice voice, CancellationToken cancellation)
   {
      using var request = new HttpRequestMessage(HttpMethod.Post, $"{Host}/v1/text:synthesize");
      request.Content = JsonContent.Create(new
                                           {
                                              input = new { text },
                                              voice = new { languageCode = Japanese, name = voice.Name },
                                              audioConfig = new { audioEncoding = "MP3" }
                                           });
      request.Headers.Add(KeyHeader, _settings.ApiKey);
      using var response = await _http.SendAsync(request, cancellation);
      await TextToSpeechRequestFailedException.ThrowUnlessItSucceeded(response, Name, cancellation);

      var answer = await response.Content.ReadFromJsonAsync<JsonElement>(cancellation);
      var audioAsBase64 = answer.GetProperty("audioContent").GetString() ?? throw new JsonException("Google answered with no audio.");
      return new SynthesizedSpeech(text, voice, Convert.FromBase64String(audioAsBase64));
   }

   /// <summary>The voice that <paramref name="listed"/>, an entry of Google's voice list, describes: its <c>name</c> and its <c>ssmlGender</c>.</summary>
   TextToSpeechVoice VoiceListedAs(JsonElement listed) =>
      new(this, listed.GetProperty("name").GetString() ?? throw new JsonException("Google listed a voice with no name."), GenderNamed(listed.GetProperty("ssmlGender").GetString()));

   static TextToSpeechVoiceGender GenderNamed(string? ssmlGender) => ssmlGender switch
   {
      "FEMALE" => TextToSpeechVoiceGender.Female,
      "MALE" => TextToSpeechVoiceGender.Male,
      "NEUTRAL" => TextToSpeechVoiceGender.Neutral,
      _ => TextToSpeechVoiceGender.Unspecified
   };
}
