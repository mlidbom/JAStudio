namespace JAStudio.Core.Note.Media;

/// <summary>A note's reference to an audio file: a recording, such as WaniKani's audio of a word or an anime's line, or audio made by text-to-speech.</summary>
public sealed record AudioReference(string FileName) : MediaReference(FileName)
{
   /// <summary>
   /// Whether the audio was made by text-to-speech: true only when that's proven, as by a file name that names a TTS voice. Audio that isn't TTS is a<br/>
   /// recording (<see cref="IsARecording"/>). It matters legally: TTS audio we generated is about the only audio anyone may surely play and store.<br/>
   /// See <c>corpus-data-model.md</c>, "TTS audio".
   /// </summary>
   public bool IsTextToSpeech { get; init; }

   /// <summary>Whether the audio is a recording, of a person speaking or singing: any audio not made by text-to-speech (<see cref="IsTextToSpeech"/>).</summary>
   public bool IsARecording => !IsTextToSpeech;

   /// <summary>The text-to-speech voice that made the audio, as its tts-provider and its name, such as <c>Azure ja-JP-NanamiNeural</c>, when that's known.</summary>
   public string? TextToSpeechVoice { get; init; }

   /// <summary>The gender of the voice speaking, when that's known: from WaniKani's file names, such as <c>文句_b.mp3</c>, and from TTS voices.</summary>
   public VoiceGender? VoiceGender { get; init; }
}
