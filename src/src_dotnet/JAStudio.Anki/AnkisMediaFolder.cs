using Compze.Contracts;
using JAStudio.Anki.AnkisFunctions;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>
/// Anki's media folder, of the collection Anki has open, as the media of the spaced repetition system (<see cref="ISRSMedia"/>): where Anki plays a<br/>
/// study-note's audio from, and what its media sync takes to the student's other devices. JAStudio adds files to it by calling Anki<br/>
/// (<see cref="IAnkisMedia"/>).
/// </summary>
public sealed class AnkisMediaFolder(IAnkisMedia ankisMedia) : ISRSMedia
{
   readonly IAnkisMedia _ankisMedia = ankisMedia;

   public void Add(AudioAttachment audio)
   {
      var nameInAnki = _ankisMedia.AddFile(audio.FilePath);
      Contract.State.Assert(nameInAnki == audio.OriginalFileName,
                            () => $"Anki stored {audio.OriginalFileName} as {nameInAnki}, so the note's study-note would reference a file Anki doesn't have.");
   }
}
