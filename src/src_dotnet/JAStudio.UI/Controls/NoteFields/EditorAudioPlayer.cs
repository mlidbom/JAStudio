using JAStudio.Audio;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// The player that plays the notes' audio files in the workspace's editors (<see cref="AudioRowViewModel"/>): one for the whole process, so that playing<br/>
/// a file stops the one playing, as in the TTS dialog, whichever note's editor it's in.
/// </summary>
/// <remarks>
/// It's never disposed: it lives as long as the process. It opens no playback device until it first plays a file (<see cref="SoundFlowAudioPlayer"/>).<br/>
/// It isn't a service of the container since Core, which wires the container, doesn't know <c>JAStudio.Audio</c>.
/// </remarks>
static class EditorAudioPlayer
{
   public static IAudioPlayer Instance { get; } = new SoundFlowAudioPlayer();
}
