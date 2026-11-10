using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;

namespace JAStudio.Core.Note.Media;

/// <summary>One list of a note's references to audio files (<see cref="MediaReferenceList{TReference}"/>), such as the audio a vocab's source gave it.</summary>
public class AudioReferenceList(IEnumerable<AudioReference> references, NoteGuard guard) : MediaReferenceList<AudioReference>(references, guard)
{
   /// <summary>The audio made by text-to-speech (<see cref="AudioReference.IsTextToSpeech"/>), in the order it's preferred in.</summary>
   public IReadOnlyList<AudioReference> TextToSpeech => [..All.Where(it => it.IsTextToSpeech)];

   /// <summary>
   /// Makes <paramref name="speech"/>, audio JAStudio's text-to-speech just generated, the list's only TTS audio: it replaces the TTS audio the list had,<br/>
   /// and leaves its recordings as they are. The file it replaces stays in the media layer, unreferenced (<c>media-storage-and-import.md</c>).
   /// </summary>
   public void ReplaceTheTextToSpeechAudioWith(AudioReference speech)
   {
      Contract.Argument.Assert(speech.IsTextToSpeech, () => $"{speech.FileName} isn't text-to-speech audio, so it can't replace the TTS audio.");
      ReplaceThoseThat(it => it.IsTextToSpeech, speech);
   }
}
