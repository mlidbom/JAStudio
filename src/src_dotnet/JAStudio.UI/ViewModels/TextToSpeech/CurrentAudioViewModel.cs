using System;
using System.IO;
using JAStudio.Core.Note;
using JAStudio.Core.Storage.Media;
using JAStudio.Audio;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// The note's TTS audio as it is when the TTS dialog opens, which using an attempt replaces:<br/>
/// played to listen to it again, and decoded once, to compare each attempt with.
/// </summary>
/// <remarks>
/// There may be nothing to play: the note may have no TTS audio, the media layer may not have its file yet,<br/>
/// or the file may be of a kind JAStudio doesn't play, such as the Ogg Vorbis audio of a few imported sentences (<see cref="DecodedAudio.CanDecode"/>).
/// </remarks>
sealed class CurrentAudioViewModel
{
   readonly byte[]? _audioFile;

   CurrentAudioViewModel(string description, byte[]? audioFile)
   {
      Description = description;
      _audioFile = audioFile;
      Decoded = audioFile is null ? null : DecodedAudio.Of(audioFile);
   }

   /// <summary>The TTS audio of <paramref name="note"/> as it is now, found in <paramref name="media"/>.</summary>
   public static CurrentAudioViewModel Of(IHasTextToSpeechAudio note, MediaFileIndex media)
   {
      if(note.Audio.TextToSpeech is not [var speech, ..]) return new CurrentAudioViewModel("none", null);
      if(media.Resolve(speech) is not {} file) return new CurrentAudioViewModel($"{speech.FileName}, not in JAStudio's media yet", null);

      return DecodedAudio.CanDecode(speech.FileName)
                ? new CurrentAudioViewModel(speech.FileName, File.ReadAllBytes(file.FilePath))
                : new CurrentAudioViewModel($"{speech.FileName}, of a kind JAStudio doesn't play", null);
   }

   /// <summary>The audio as the dialog shows it: its file name, such as <c>hypertts-….mp3</c>, "none", or why it can't be played.</summary>
   public string Description { get; }

   public bool CanPlay => _audioFile is not null;

   /// <summary>The audio decoded, to compare the attempts with. Null when there's nothing to play (<see cref="CanPlay"/>).</summary>
   public DecodedAudio? Decoded { get; }

   /// <summary>Plays the audio with <paramref name="player"/>. Only when it <see cref="CanPlay"/>.</summary>
   public void PlayWith(IAudioPlayer player) => player.Play(_audioFile ?? throw new InvalidOperationException($"The note's TTS audio can't be played: {Description}."));
}
