using System;
using System.IO;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.TextToSpeech.Azure;
using Mp3LoudnessNormalization;

// ReSharper disable InconsistentNaming

namespace JAStudio.TextToSpeech.Specifications.AICreatedSpecs;

/// <summary>
/// Azure's voice <c>ja-JP-NanamiNeural</c> speaking a text, against a fake of Azure's web service that answers with speech Azure once sent:<br/>
/// <c>SpeechFromTheServices/azure-nanami-24khz.mp3</c>, which is 6 dB louder than ReplayGain's reference.
/// </summary>
public class When_a_voice_speaks_a_text : IAIGeneratedSpec
{
   static readonly byte[] WhatAzureSent = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "SpeechFromTheServices", "azure-nanami-24khz.mp3"));
   readonly SynthesizedSpeech _speech;

   public When_a_voice_speaks_a_text()
   {
      var service = new FakeTextToSpeechService();
      service.AnswersWithAudio(WhatAzureSent);
      var azure = new AzureSpeechProvider(new AzureSpeechSettings { Key = "the-key", Region = "swedencentral" }, service.Client);
      _speech = new TextToSpeechVoice(azure, "ja-JP-NanamiNeural", TextToSpeechVoiceGender.Female).SpeakAsync("明日は学校に行きます。", CancellationToken.None).GetAwaiter().GetResult();
   }

   [XF] public void the_speech_is_what_azure_sent_brought_to_the_replaygain_reference() =>
      _speech.Mp3.Must().SequenceEqual(new Mp3File(WhatAzureSent).NormalizedToTheReplayGainReference().Bytes());

   [XF] public void the_speech_differs_from_what_azure_sent_which_was_louder() => _speech.Mp3.Must().Satisfy(it => !it.AsSpan().SequenceEqual(WhatAzureSent));
}
