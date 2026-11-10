using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Storage.Media;

namespace JAStudio.Core.Note.Media;

/// <summary>
/// All of a note's audio, what its source gave it and the corpus's own, added since, in the order it's preferred in: a recording before text-to-speech,<br/>
/// and among recordings, the source's first (<c>corpus-data-model.md</c>, Media, "The shape").
/// </summary>
/// <remarks>
/// The <see cref="Preferred"/> audio is what JAStudio plays for the note, and what the Anki-projection gives a sentence's study-note. A vocab's study-note<br/>
/// gets all of it, as its card templates play the recordings, else the TTS audio.
/// </remarks>
public class AllAudioOfANote
{
   readonly AudioReferenceList _fromItsSource;
   readonly AudioReferenceList _theCorpussOwn;

   public AllAudioOfANote(AudioReferenceList fromItsSource, AudioReferenceList theCorpussOwn)
   {
      _fromItsSource = fromItsSource;
      _theCorpussOwn = theCorpussOwn;
   }

   /// <summary>The recordings (<see cref="AudioReference.IsARecording"/>), the source's first.</summary>
   public IReadOnlyList<AudioReference> Recordings => [..All.Where(it => it.IsARecording)];

   /// <summary>The audio made by text-to-speech (<see cref="AudioReference.IsTextToSpeech"/>), the source's first.</summary>
   public IReadOnlyList<AudioReference> TextToSpeech => [..All.Where(it => it.IsTextToSpeech)];

   /// <summary>All of it, in the order it's preferred in: the <see cref="Recordings"/>, then the <see cref="TextToSpeech"/> audio.</summary>
   public IReadOnlyList<AudioReference> InTheOrderPreferred => [..Recordings, ..TextToSpeech];

   /// <summary>The audio played for the note: its first recording, else its first TTS audio, or null when it has no audio.</summary>
   public AudioReference? Preferred => InTheOrderPreferred.FirstOrDefault();

   /// <summary>The file of the <see cref="Preferred"/> audio in <paramref name="media"/>, or null when the note has no audio, or the media layer hasn't its file.</summary>
   public AudioAttachment? PreferredFileIn(MediaFileIndex media) => Preferred is {} preferred ? media.Resolve(preferred) : null;

   /// <summary>Whether the note has audio, and all of it is text-to-speech: no recording.</summary>
   public bool IsAllTextToSpeech => Preferred is { IsTextToSpeech: true };

   IEnumerable<AudioReference> All => _fromItsSource.All.Concat(_theCorpussOwn.All);
}
