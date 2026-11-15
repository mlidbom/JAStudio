using JAStudio.Core.Storage.Media;

namespace JAStudio.Core.Studying;

/// <summary>
/// The media of the SRS, the spaced repetition system: its own copies of the files its study-notes reference, which it plays as a card is studied,<br/>
/// and which its own sync takes to the student's other devices. In Anki, the files in its media folder. JAStudio keeps its media in the media<br/>
/// layer, and gives the spaced repetition system a copy of a file it makes. JAStudio running without a spaced repetition system has none<br/>
/// (<see cref="NoSRSMedia"/>).
/// </summary>
/// <remarks>
/// review-naming:studying: ISRSMedia is Claude's (2026-11-15), named as <see cref="ISRSWindows"/> is. Until then the UI copied the file into Anki's<br/>
/// media folder itself, through the Anki integration.
/// </remarks>
public interface ISRSMedia
{
   /// <summary>
   /// Gives the spaced repetition system a copy of <paramref name="audio"/>, a file in the media layer, under the file's own name, by which a<br/>
   /// study-note references it. It fails when the spaced repetition system already has a different file by that name.
   /// </summary>
   void Add(AudioAttachment audio);
}
