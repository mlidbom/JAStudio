using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.TextToSpeech.Google;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.TextToSpeech.Specifications.AICreatedSpecs;

/// <summary>Google Cloud Text-to-Speech as a tts-provider, with an API key, against a fake of its web service.</summary>
public class When_using_google_text_to_speech : IAIGeneratedSpec
{
   const string ApiKey = "the-api-key";

   readonly FakeTextToSpeechService _service = new();
   readonly GoogleTextToSpeechProvider _google;

   public When_using_google_text_to_speech() => _google = new GoogleTextToSpeechProvider(new GoogleTextToSpeechSettings { ApiKey = ApiKey }, _service.Client);

   IReadOnlyList<TextToSpeechVoice> TheJapaneseVoices() => _google.JapaneseVoicesAsync(CancellationToken.None).GetAwaiter().GetResult();

   public class listing_the_japanese_voices : When_using_google_text_to_speech
   {
      readonly IReadOnlyList<TextToSpeechVoice> _voices;

      public listing_the_japanese_voices()
      {
         _service.AnswersWith(HttpStatusCode.OK,
                              """
                              {
                                "voices": [
                                  { "languageCodes": [ "ja-JP" ], "name": "ja-JP-Neural2-B", "ssmlGender": "FEMALE", "naturalSampleRateHertz": 24000 },
                                  { "languageCodes": [ "ja-JP" ], "name": "ja-JP-Chirp3-HD-Charon", "ssmlGender": "MALE", "naturalSampleRateHertz": 24000 }
                                ]
                              }
                              """);
         _voices = TheJapaneseVoices();
      }

      [XF] public void it_asks_for_the_voices_for_japanese() =>
         (_service.Request.Method, _service.Request.Uri).Must().Be((HttpMethod.Get, "https://texttospeech.googleapis.com/v1/voices?languageCode=ja-JP"));
      [XF] public void it_sends_the_api_key() => _service.Request.Headers["X-Goog-Api-Key"].Must().Be(ApiKey);
      [XF] public void the_voices_are_those_listed() => _voices.Select(it => it.Name).Must().SequenceEqual(["ja-JP-Neural2-B", "ja-JP-Chirp3-HD-Charon"]);
      [XF] public void a_voice_shows_its_name_without_the_language() => _voices.Select(it => it.ShortName).Must().SequenceEqual(["Neural2-B", "Chirp3-HD-Charon"]);
      [XF] public void a_voice_has_the_gender_google_gives_it() => _voices.Select(it => it.Gender).Must().SequenceEqual([TextToSpeechVoiceGender.Female, TextToSpeechVoiceGender.Male]);
   }

   public class listing_the_japanese_voices_when_there_are_none : When_using_google_text_to_speech
   {
      public listing_the_japanese_voices_when_there_are_none() => _service.AnswersWith(HttpStatusCode.OK, "{}");

      [XF] public void there_are_none_since_googles_json_leaves_out_an_empty_list() => TheJapaneseVoices().Must().BeEmpty();
   }

   public class speaking_a_text : When_using_google_text_to_speech
   {
      const string Text = "家に帰る";
      readonly SynthesizedSpeech _speech;

      public speaking_a_text()
      {
         _service.AnswersWith(HttpStatusCode.OK, """{ "audioContent": "AQID" }""");
         _speech = _google.SpeakAsync(Text, new TextToSpeechVoice(_google, "ja-JP-Neural2-B", TextToSpeechVoiceGender.Female), CancellationToken.None).GetAwaiter().GetResult();
      }

      JsonElement TheRequestSent() => JsonDocument.Parse(_service.Request.Body).RootElement;

      string ValueSentAt(params string[] path) =>
         path.Aggregate(TheRequestSent(), (element, name) => element.GetProperty(name)).GetString() ?? throw new KeyNotFoundException(string.Join(".", path));

      [XF] public void it_posts_to_the_synthesis_endpoint() => (_service.Request.Method, _service.Request.Uri).Must().Be((HttpMethod.Post, "https://texttospeech.googleapis.com/v1/text:synthesize"));
      [XF] public void it_sends_the_api_key() => _service.Request.Headers["X-Goog-Api-Key"].Must().Be(ApiKey);
      [XF] public void it_sends_the_text_as_plain_text() => ValueSentAt("input", "text").Must().Be(Text);
      [XF] public void it_names_the_voice_and_the_language() => (ValueSentAt("voice", "name"), ValueSentAt("voice", "languageCode")).Must().Be(("ja-JP-Neural2-B", "ja-JP"));
      [XF] public void it_asks_for_mp3() => ValueSentAt("audioConfig", "audioEncoding").Must().Be("MP3");
      [XF] public void the_speech_is_the_audio_decoded_from_the_answers_base64() => _speech.Mp3.Must().SequenceEqual((byte[])[1, 2, 3]);
   }

   public class when_the_service_refuses_the_request : When_using_google_text_to_speech
   {
      public when_the_service_refuses_the_request() => _service.AnswersWith(HttpStatusCode.Forbidden, """{"error":{"message":"API key not valid."}}""");

      [XF] public void listing_the_voices_throws_naming_google_the_status_and_what_google_said() =>
         Invoking(TheJapaneseVoices).Must().Throw<TextToSpeechRequestFailedException>()
                                            .Which.Message.Must().Be("""Google refused the request: 403 Forbidden. {"error":{"message":"API key not valid."}}""");
   }
}
