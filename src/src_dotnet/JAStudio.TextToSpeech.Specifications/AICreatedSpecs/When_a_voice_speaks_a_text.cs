using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.TextToSpeech.Azure;

// ReSharper disable InconsistentNaming

namespace JAStudio.TextToSpeech.Specifications.AICreatedSpecs;

/// <summary>
/// Azure's voice <c>ja-JP-NanamiNeural</c> speaking a text, against a fake of Azure's web service that answers with speech Azure once generated:<br/>
/// the <c>azure-nanami-24khz</c> fixture (<see cref="Mp3Fixtures"/>).
/// </summary>
public class When_a_voice_speaks_a_text : IAIGeneratedSpec
{
   const string TheFixture = "azure-nanami-24khz";
   readonly SynthesizedSpeech _speech;

   public When_a_voice_speaks_a_text()
   {
      var service = new FakeTextToSpeechService();
      service.AnswersWithAudio(Mp3Fixtures.Original(TheFixture));
      var azure = new AzureSpeechProvider(new AzureSpeechSettings { Key = "the-key", Region = "swedencentral" }, service.Client);
      _speech = new TextToSpeechVoice(azure, "ja-JP-NanamiNeural", VoiceGender.Female).SpeakAsync("明日は学校に行きます。", CancellationToken.None).GetAwaiter().GetResult();
   }

   [XF] public void the_speech_is_brought_to_the_replaygain_reference_as_mp3gain_brings_it() => _speech.Mp3.Must().SequenceEqual(Mp3Fixtures.NormalizedByMp3Gain(TheFixture));
}
