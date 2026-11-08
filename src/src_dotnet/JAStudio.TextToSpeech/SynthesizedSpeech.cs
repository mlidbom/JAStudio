using System;
using Mp3LoudnessNormalization;

namespace JAStudio.TextToSpeech;

/// <summary>The audio a voice generated for a text, as MP3, not yet kept. Keeping one makes it a note's audio. See <c>dev_docs/text-to-speech.md</c>.</summary>
/// <param name="Text">The text spoken.</param>
/// <param name="Voice">The voice that spoke it.</param>
/// <param name="Mp3">The audio, as the bytes of an MP3 file.</param>
public sealed record SynthesizedSpeech(string Text, TextToSpeechVoice Voice, byte[] Mp3)
{
   /// <summary>
   /// A new file name for keeping this speech, unique in the media layer and in Anki's media folder, which recognize files by name alone,<br/>
   /// and naming the voice, such as <c>jastudio-tts-azure-daichi-dragonhdomnilatestneural-3f2a….mp3</c>.
   /// </summary>
   public string UniqueFileName() => $"jastudio-tts-{Voice.Provider.Name}-{Voice.ShortName.Replace(':', '-')}-{Guid.NewGuid():N}.mp3".ToLowerInvariant();

   /// <summary>
   /// This speech brought to ReplayGain's reference loudness without re-encoding it, as <c>mp3gain /r /c</c> would bring it (<see cref="Mp3File.NormalizedToTheReplayGainReference"/>):<br/>
   /// the TTS audio HyperTTS made was normalized so, by Magnus's mp3gain script, so the two sound as loud as each other.
   /// </summary>
   public SynthesizedSpeech NormalizedToTheReplayGainReference() => this with { Mp3 = new Mp3File(Mp3).NormalizedToTheReplayGainReference().Bytes() };

   /// <summary>Not the audio's bytes, which would only fill a log.</summary>
   public override string ToString() => $"{Voice} speaking {Text} ({Mp3.Length} bytes of MP3)";
}
