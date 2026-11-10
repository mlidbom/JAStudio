namespace JAStudio.Core.Note.Media;

/// <summary>
/// The gender of the voice in an audio file (<see cref="AudioReference.VoiceGender"/>). Kept where it's known since a student may prefer one gender's<br/>
/// voices (Magnus, 2026-11-10).
/// </summary>
/// <remarks>
/// <c>JAStudio.TextToSpeech</c> has a <c>VoiceGender</c> of its own, the gender a tts-provider gives its voices, as Core doesn't reference that project:<br/>
/// using a TTS voice's speech as a note's audio translates the one into the other.
/// </remarks>
public enum VoiceGender
{
   Female,
   Male,
   Neutral
}
