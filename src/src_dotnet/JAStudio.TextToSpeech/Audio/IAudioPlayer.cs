namespace JAStudio.TextToSpeech.Audio;

/// <summary>
/// Plays audio files, such as a synthesized speech or a note's current audio, one at a time: playing one stops the one playing.<br/>
/// <see cref="SoundFlowAudioPlayer"/> plays them on the computer's default output. The specifications use one that only records what it was given.
/// </summary>
public interface IAudioPlayer
{
   /// <summary>Starts playing <paramref name="audioFile"/>, the bytes of an MP3, WAV or FLAC file, and returns at once.</summary>
   void Play(byte[] audioFile);
}
