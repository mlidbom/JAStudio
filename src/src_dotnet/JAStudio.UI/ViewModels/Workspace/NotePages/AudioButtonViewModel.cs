using System.IO;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Audio;
using JAStudio.Core.Storage.Media;

namespace JAStudio.UI.ViewModels.Workspace.NotePages;

/// <summary>A ▶ button on a note's page that plays a note's audio file, as the cards' ▶ buttons do: the page's note's own, or that of a note it lists.</summary>
sealed partial class AudioButtonViewModel
{
   readonly AudioAttachment _audio;

   AudioButtonViewModel(AudioAttachment audio) => _audio = audio;

   /// <summary>The button for <paramref name="audio"/>; none when there's no audio, or it's of a kind JAStudio doesn't play (<see cref="DecodedAudio.CanDecode"/>).</summary>
   public static AudioButtonViewModel? For(AudioAttachment? audio) =>
      audio is { OriginalFileName: {} fileName } && DecodedAudio.CanDecode(fileName) ? new AudioButtonViewModel(audio) : null;

   /// <summary>Plays the audio, stopping any audio the workspace plays (<see cref="WorkspaceAudioPlayer"/>).</summary>
   [RelayCommand]
   public void Play() => WorkspaceAudioPlayer.Instance.Play(File.ReadAllBytes(_audio.FilePath));
}
