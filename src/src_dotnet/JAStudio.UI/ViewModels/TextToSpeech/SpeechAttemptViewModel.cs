using System;
using System.Globalization;
using JAStudio.TextToSpeech;
using JAStudio.TextToSpeech.Audio;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// One attempt in the TTS dialog: a voice's speech of the text, numbered in the order generated, kept in memory to replay without generating it again.<br/>
/// It says whether it sounds the same as the attempt before it with the same voice and text, since a voice may say a text differently each time.
/// </summary>
sealed class SpeechAttemptViewModel
{
   /// <param name="number">The attempt's number in the dialog, from 1.</param>
   /// <param name="speech">The speech generated.</param>
   /// <param name="earlierWithTheSameVoiceAndText">The latest earlier attempt with the same voice and text, or null when this is the first.</param>
   public SpeechAttemptViewModel(int number, SynthesizedSpeech speech, SpeechAttemptViewModel? earlierWithTheSameVoiceAndText)
   {
      Number = number;
      Speech = speech;
      Audio = DecodedAudio.Of(speech.Mp3);
      Comparison = earlierWithTheSameVoiceAndText is {} earlier ? HowItComparesWith(earlier) : "";
   }

   public int Number { get; }

   public SynthesizedSpeech Speech { get; }

   public DecodedAudio Audio { get; }

   /// <summary>The attempt as the list shows it, such as "#3 Azure Daichi:DragonHDOmniLatestNeural".</summary>
   public string Description => $"#{Number} {Speech.Voice}";

   /// <summary>How long it lasts, such as "2.26 s".</summary>
   public string Length => Seconds(Audio.Duration);

   /// <summary>
   /// How it compares with the attempt before it with the same voice and text: "same as #1", or "differs from #1: 0.29 s shorter".<br/>
   /// Empty for the first attempt with its voice and text.
   /// </summary>
   public string Comparison { get; }

   /// <summary>Whether it was generated with <paramref name="voice"/>, speaking <paramref name="text"/>.</summary>
   public bool IsOf(TextToSpeechVoice voice, string text) => Speech.Voice == voice && Speech.Text == text;

   string HowItComparesWith(SpeechAttemptViewModel earlier)
   {
      if(Audio.SoundsTheSameAs(earlier.Audio)) return $"same as #{earlier.Number}";

      var longer = Audio.Duration - earlier.Audio.Duration;
      return longer == TimeSpan.Zero ? $"differs from #{earlier.Number}, as long"
             : longer > TimeSpan.Zero ? $"differs from #{earlier.Number}: {Seconds(longer)} longer"
                                      : $"differs from #{earlier.Number}: {Seconds(longer.Negate())} shorter";
   }

   static string Seconds(TimeSpan time) => $"{time.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture)} s";
}
