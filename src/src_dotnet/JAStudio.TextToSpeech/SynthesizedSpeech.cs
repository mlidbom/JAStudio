namespace JAStudio.TextToSpeech;

/// <summary>The audio a voice generated for a text, as MP3, not yet kept. Keeping one makes it a note's audio. See <c>dev_docs/text-to-speech.md</c>.</summary>
/// <param name="Text">The text spoken.</param>
/// <param name="Voice">The voice that spoke it.</param>
/// <param name="Mp3">The audio, as the bytes of an MP3 file.</param>
public sealed record SynthesizedSpeech(string Text, TextToSpeechVoice Voice, byte[] Mp3)
{
   /// <summary>Not the audio's bytes, which would only fill a log.</summary>
   public override string ToString() => $"{Voice} speaking {Text} ({Mp3.Length} bytes of MP3)";
}
