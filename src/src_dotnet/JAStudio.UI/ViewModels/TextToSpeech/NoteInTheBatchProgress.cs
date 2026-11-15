namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>How far the batch TTS dialog has come with one of its notes (<see cref="NoteInTheBatchViewModel"/>).</summary>
enum NoteInTheBatchProgress
{
   /// <summary>The dialog hasn't given the note TTS audio yet: the batch hasn't run for it, or was stopped before it, so running it again does it.</summary>
   NotGivenAudio,

   /// <summary>The running batch will do the note, once it has done the notes before it.</summary>
   Waiting,

   /// <summary>The batch's voice is speaking the note's text right now.</summary>
   Generating,

   /// <summary>
   /// The dialog has used a speech as the note's TTS audio: the batch's, or an attempt used in the note's own text-to-speech. Or the batch found<br/>
   /// the note already had TTS audio of its voice.
   /// </summary>
   GivenAudio,

   /// <summary>The note's TTS audio was discarded in its own text-to-speech, so it has none. The batch leaves it so.</summary>
   AudioDiscarded,

   /// <summary>The tts-provider refused the batch's request for the note, or it didn't reach the tts-provider. Running the batch again tries it again.</summary>
   Failed
}
