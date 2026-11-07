using System;
using System.Globalization;
using JAStudio.TextToSpeech;
using JAStudio.TextToSpeech.Audio;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// One attempt in the TTS dialog: a voice's speech of the text, numbered in the order generated, kept in memory to replay without generating it again.<br/>
/// It says whether it sounds the same as the attempt before it with the same voice and text, since a voice may say a text differently each time,<br/>
/// and whether it sounds the same as the note's current audio, which keeping it would replace.
/// </summary>
sealed class SpeechAttemptViewModel
{
   /// <param name="number">The attempt's number in the dialog, from 1.</param>
   /// <param name="speech">The speech generated.</param>
   /// <param name="earlierWithTheSameVoiceAndText">The latest earlier attempt with the same voice and text, or null when this is the first.</param>
   /// <param name="currentAudio">The note's TTS audio as it was when the dialog opened.</param>
   public SpeechAttemptViewModel(int number, SynthesizedSpeech speech, SpeechAttemptViewModel? earlierWithTheSameVoiceAndText, CurrentAudioViewModel currentAudio)
   {
      Number = number;
      Speech = speech;
      Audio = DecodedAudio.Of(speech.Mp3);
      ComparisonWithTheEarlierAttempt = earlierWithTheSameVoiceAndText is {} earlier ? HowItComparesWith(earlier.Audio, $"#{earlier.Number}") : "";
      ComparisonWithTheCurrentAudio = currentAudio.Decoded is {} current ? HowItComparesWith(current, "current") : "";
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
   public string ComparisonWithTheEarlierAttempt { get; }

   /// <summary>
   /// How it compares with the note's current audio, whatever its voice: "same as current", or "differs from current: 0.40 s longer".<br/>
   /// Empty when there's no current audio to compare with (<see cref="CurrentAudioViewModel.CanPlay"/>).
   /// </summary>
   public string ComparisonWithTheCurrentAudio { get; }

   /// <summary>Whether it was generated with <paramref name="voice"/>, speaking <paramref name="text"/>.</summary>
   public bool IsOf(TextToSpeechVoice voice, string text) => Speech.Voice == voice && Speech.Text == text;

   /// <summary>How this attempt's audio compares with <paramref name="other"/>, which the comparison calls <paramref name="otherName"/>, such as "#1" or "current".</summary>
   string HowItComparesWith(DecodedAudio other, string otherName)
   {
      if(Audio.SoundsTheSameAs(other)) return $"same as {otherName}";

      var longer = Audio.Duration - other.Duration;
      return longer == TimeSpan.Zero ? $"differs from {otherName}, as long"
             : longer > TimeSpan.Zero ? $"differs from {otherName}: {Seconds(longer)} longer"
                                      : $"differs from {otherName}: {Seconds(longer.Negate())} shorter";
   }

   static string Seconds(TimeSpan time) => $"{time.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture)} s";
}
