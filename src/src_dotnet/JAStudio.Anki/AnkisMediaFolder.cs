using Compze.Contracts;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>
/// Anki's media folder, of the collection Anki has open, as the media of the spaced repetition system: where Anki plays a study-note's audio from,<br/>
/// and what its media sync takes to the student's other devices. JAStudio adds files to it through Python (<see cref="AnkiFacade"/>).
/// </summary>
public sealed class AnkisMediaFolder : ISRSMedia
{
   public void Add(AudioAttachment audio)
   {
      var nameInAnki = AnkiFacade.Media.AddFile(audio.FilePath);
      Contract.State.Assert(nameInAnki == audio.OriginalFileName,
                            () => $"Anki stored {audio.OriginalFileName} as {nameInAnki}, so the note's study-note would reference a file Anki doesn't have.");
   }
}
