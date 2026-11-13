using JAStudio.Audio;

namespace JAStudio.UI.ViewModels.Workspace;

static partial class WorkspaceAudioPlayer
{
   /// <summary>A player that plays nothing it's given (<see cref="PlayNothingFromNowOn"/>).</summary>
   sealed class PlayingNothing : IAudioPlayer
   {
      public void Play(byte[] audioFile) {}
   }
}
