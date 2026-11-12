using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Xml.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.TextToSpeech.Azure;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.TextToSpeech.Specifications.AICreatedSpecs;

/// <summary>Azure Speech as a tts-provider, with a key for the <c>swedencentral</c> region, against a fake of its web service.</summary>
public class When_using_azure_speech : IAIGeneratedSpec
{
   const string Key = "the-key";
   static readonly XNamespace Ssml = "http://www.w3.org/2001/10/synthesis";

   readonly FakeTextToSpeechService _service = new();
   readonly AzureSpeechProvider _azure;

   public When_using_azure_speech() => _azure = new AzureSpeechProvider(new AzureSpeechSettings { Key = Key, Region = "swedencentral" }, _service.Client);

   public class listing_the_japanese_voices : When_using_azure_speech
   {
      readonly IReadOnlyList<TextToSpeechVoice> _voices;

      public listing_the_japanese_voices()
      {
         _service.AnswersWith(HttpStatusCode.OK,
                              """
                              [
                                { "ShortName": "ja-JP-DaichiNeural", "DisplayName": "Daichi", "Locale": "ja-JP", "Gender": "Male" },
                                { "ShortName": "ja-JP-Nanami:DragonHDLatestNeural", "Locale": "ja-JP", "Gender": "Female" },
                                { "ShortName": "en-US-JennyNeural", "Locale": "en-US", "Gender": "Female" }
                              ]
                              """);
         _voices = _azure.JapaneseVoicesAsync(CancellationToken.None).GetAwaiter().GetResult();
      }

      [XF] public void it_asks_for_the_voice_list_of_the_region() =>
         (_service.Request.Method, _service.Request.Uri).Must().Be((HttpMethod.Get, "https://swedencentral.tts.speech.microsoft.com/cognitiveservices/voices/list"));
      [XF] public void it_sends_the_key() => _service.Request.Headers["Ocp-Apim-Subscription-Key"].Must().Be(Key);
      [XF] public void the_japanese_voices_listed_come_first_and_the_others_are_left_out() =>
         _voices.Take(2).Select(it => it.Name).Must().SequenceEqual(["ja-JP-DaichiNeural", "ja-JP-Nanami:DragonHDLatestNeural"]);
      [XF] public void the_dragon_hd_omni_version_of_each_standard_voice_is_added_since_azure_lists_none() =>
         _voices.Skip(2).Select(it => it.Name).Must().SequenceEqual(["ja-JP-Daichi:DragonHDOmniLatestNeural"]);
      [XF] public void a_voice_shows_its_name_without_the_language() =>
         _voices.Select(it => it.ShortName).Must().SequenceEqual(["DaichiNeural", "Nanami:DragonHDLatestNeural", "Daichi:DragonHDOmniLatestNeural"]);
      [XF] public void a_voice_has_the_gender_azure_gives_it_or_its_standard_version() =>
         _voices.Select(it => it.Gender).Must().SequenceEqual([TextToSpeechVoiceGender.Male, TextToSpeechVoiceGender.Female, TextToSpeechVoiceGender.Male]);
      [XF] public void a_voice_belongs_to_azure() => _voices.Select(it => new { it.Name, BelongsToAzure = ReferenceEquals(it.Provider, _azure) }).Must().Satisfy(voices => voices.All(it => it.BelongsToAzure));
   }

   public class speaking_a_text_holding_characters_that_are_markup_in_ssml : When_using_azure_speech
   {
      const string Text = "A&B<C>";
      static readonly byte[] TheAudio = [1, 2, 3];
      readonly SynthesizedSpeech _speech;

      public speaking_a_text_holding_characters_that_are_markup_in_ssml()
      {
         _service.AnswersWithAudio(TheAudio);
         _speech = _azure.SpeakAsync(Text, new TextToSpeechVoice(_azure, "ja-JP-DaichiNeural", TextToSpeechVoiceGender.Male), CancellationToken.None).GetAwaiter().GetResult();
      }

      XElement TheSsmlSent() => XElement.Parse(_service.Request.Body);

      XElement TheVoiceInTheSsmlSent() => TheSsmlSent().Element(Ssml + "voice") ?? throw new InvalidOperationException("The SSML sent names no voice.");

      [XF] public void it_posts_to_the_synthesis_endpoint_of_the_region() =>
         (_service.Request.Method, _service.Request.Uri).Must().Be((HttpMethod.Post, "https://swedencentral.tts.speech.microsoft.com/cognitiveservices/v1"));
      [XF] public void it_sends_the_key() => _service.Request.Headers["Ocp-Apim-Subscription-Key"].Must().Be(Key);
      [XF] public void it_asks_for_mp3_at_96_kbit_and_24_khz() => _service.Request.Headers["X-Microsoft-OutputFormat"].Must().Be("audio-24khz-96kbitrate-mono-mp3");
      [XF] public void it_sends_ssml() => _service.Request.ContentType.Must().Be("application/ssml+xml");
      [XF] public void the_ssml_is_in_japanese() => (TheSsmlSent().Name, TheSsmlSent().Attribute(XNamespace.Xml + "lang")?.Value).Must().Be((Ssml + "speak", "ja-JP"));
      [XF] public void the_ssml_names_the_voice() =>
         (TheVoiceInTheSsmlSent().Attribute("name") ?? throw new InvalidOperationException("The SSML's voice has no name.")).Value.Must().Be("ja-JP-DaichiNeural");
      [XF] public void the_ssml_holds_the_text_as_text_rather_than_as_markup() => TheVoiceInTheSsmlSent().Value.Must().Be(Text);
      [XF] public void the_speech_is_the_audio_the_service_answered_with() => _speech.Mp3.Must().SequenceEqual(TheAudio);
      [XF] public void the_speech_knows_its_text_and_voice() => (_speech.Text, _speech.Voice.Name).Must().Be((Text, "ja-JP-DaichiNeural"));
   }

   public class when_the_service_refuses_the_request : When_using_azure_speech
   {
      public when_the_service_refuses_the_request() => _service.AnswersWith(HttpStatusCode.Unauthorized, "");

      [XF] public void speaking_throws_naming_azure_and_the_status() =>
         Invoking(() => _azure.SpeakAsync("家", new TextToSpeechVoice(_azure, "ja-JP-DaichiNeural", TextToSpeechVoiceGender.Male), CancellationToken.None).GetAwaiter().GetResult())
           .Must().Throw<TextToSpeechRequestFailedException>().Which.Message.Must().Be("Azure refused the request: 401 Unauthorized.");
   }
}
