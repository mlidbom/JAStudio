using JAStudio.Audio;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.ViewModels.Workspace.NotePages;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// The player that plays the notes' audio files in the workspace: in the editors (<see cref="AudioRowViewModel"/>) and on the notes' pages<br/>
/// (<see cref="AudioButtonViewModel"/>). One for the whole process, so that playing a file stops the one playing, as in the TTS dialog, wherever it is.
/// </summary>
/// <remarks>
/// It's never disposed: it lives as long as the process. It opens no playback device until it first plays a file (<see cref="SoundFlowAudioPlayer"/>).<br/>
/// It isn't a service of the container since Core, which wires the container, doesn't know <c>JAStudio.Audio</c>.
/// </remarks>
static partial class WorkspaceAudioPlayer
{
   public static IAudioPlayer Instance { get; private set; } = new SoundFlowAudioPlayer();

   /// <summary>
   /// Has the workspace play no audio from now on, as in the headless JAStudio, which an agent drives on Magnus's computer: a sentence's page plays the<br/>
   /// sentence's audio as it's shown, which would sound out of nowhere.
   /// </summary>
   internal static void PlayNothingFromNowOn() => Instance = new PlayingNothing();
}
