using System.Collections.Generic;
using JAStudio.TextToSpeech.Audio;

namespace JAStudio.UI.Specifications;

/// <summary>An audio player for the specifications, which plays nothing and records what it was given to play, in order.</summary>
sealed class RecordingAudioPlayer : IAudioPlayer
{
   public List<byte[]> Played { get; } = [];

   public void Play(byte[] audioFile) => Played.Add(audioFile);
}
