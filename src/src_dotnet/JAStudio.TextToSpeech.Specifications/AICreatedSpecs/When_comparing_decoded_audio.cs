using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.TextToSpeech.Audio;

// ReSharper disable InconsistentNaming

namespace JAStudio.TextToSpeech.Specifications.AICreatedSpecs;

/// <summary>
/// Whether two attempts at speaking a text sound the same, decided on their decoded audio, since a tts-provider may send slightly different bytes<br/>
/// for the same speech. The audio is a 440 Hz tone of 2 seconds, generated as WAV.
/// </summary>
public class When_comparing_decoded_audio : IAIGeneratedSpec
{
   static readonly DecodedAudio TheTone = DecodedAudio.Of(GeneratedWav.Tone(440, 2));

   static bool SoundsTheSame(byte[] other) => TheTone.SoundsTheSameAs(DecodedAudio.Of(other));

   [XF] public void it_lasts_as_long_as_the_audio() => TheTone.Duration.TotalSeconds.Must().Be(2.0);
   [XF] public void identical_audio_sounds_the_same() => SoundsTheSame(GeneratedWav.Tone(440, 2)).Must().BeTrue();
   [XF] public void audio_with_a_little_noise_added_sounds_the_same_as_azure_sends_the_same_speech_with_slightly_different_bytes() =>
      SoundsTheSame(GeneratedWav.Tone(440, 2, noise: 0.02)).Must().BeTrue();
   [XF] public void audio_at_another_pitch_sounds_different() => SoundsTheSame(GeneratedWav.Tone(330, 2)).Must().BeFalse();
   [XF] public void audio_a_tenth_longer_sounds_different() => SoundsTheSame(GeneratedWav.Tone(440, 2.2)).Must().BeFalse();
   [XF] public void audio_with_much_noise_sounds_different() => SoundsTheSame(GeneratedWav.Tone(440, 2, noise: 1)).Must().BeFalse();
}
