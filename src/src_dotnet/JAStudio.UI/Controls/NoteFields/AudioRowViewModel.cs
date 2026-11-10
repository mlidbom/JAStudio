using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Audio;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Storage.Media;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One audio file of a note's audio list, in an <see cref="AudioListEditorViewModel"/>: a button playing it, its name, what's known about it, and a<br/>
/// button removing it from the note.
/// </summary>
partial class AudioRowViewModel
{
   readonly AudioReference _audio;
   readonly AudioAttachment? _file;
   readonly IAudioPlayer _player;
   readonly Action<AudioReference> _remove;

   /// <param name="audio">The note's reference to the file.</param>
   /// <param name="file">The file in the media layer, or null when the media layer hasn't it.</param>
   /// <param name="player">Plays the file.</param>
   /// <param name="remove">Removes <paramref name="audio"/> from the note's list.</param>
   public AudioRowViewModel(AudioReference audio, AudioAttachment? file, IAudioPlayer player, Action<AudioReference> remove)
   {
      _audio = audio;
      _file = file;
      _player = player;
      _remove = remove;
   }

   /// <summary>The file's name, such as <c>文句_b.mp3</c>.</summary>
   public string FileName => _audio.FileName;

   /// <summary>What's known about the file, such as "TTS, Azure ja-JP-NanamiNeural, female, ours", or "recording" for audio of which nothing more is known.</summary>
   public string Facts => string.Join(", ", FactsAboutTheAudio().OfType<string>());

   /// <summary>Whether the file can be played: the media layer has it, and it's of a kind JAStudio plays (<see cref="DecodedAudio.CanDecode"/>).</summary>
   public bool CanPlay => _file is not null && DecodedAudio.CanDecode(FileName);

   /// <summary>The play button's tooltip: what it does, or why the file can't be played.</summary>
   public string PlayTip => _file is null ? "The file isn't in JAStudio's media"
                            : !DecodedAudio.CanDecode(FileName) ? "The file is of a kind JAStudio doesn't play"
                            : "Play";

   [RelayCommand(CanExecute = nameof(CanPlay))]
   void Play() => _player.Play(File.ReadAllBytes((_file ?? throw new InvalidOperationException($"{FileName} isn't in JAStudio's media, so it can't be played.")).FilePath));

   [RelayCommand] void Remove() => _remove(_audio);

   IEnumerable<string?> FactsAboutTheAudio() =>
      [_audio.IsTextToSpeech ? "TTS" : "recording", _audio.TextToSpeechVoice, _audio.VoiceGender?.ToString().ToLowerInvariant(), _audio.IsOurs ? "ours" : null];
}
