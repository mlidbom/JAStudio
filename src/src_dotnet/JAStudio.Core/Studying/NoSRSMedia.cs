using JAStudio.Core.Storage.Media;

namespace JAStudio.Core.Studying;

/// <summary>
/// The media of the SRS, the spaced repetition system, of a JAStudio running without one, such as the headless JAStudio and JAStudio started on<br/>
/// its own: there is none, so a file JAStudio makes stays in the media layer alone.
/// </summary>
/// <remarks>review-naming:studying: NoSRSMedia is Claude's (2026-11-15), named as <see cref="NoSRSWindows"/> is.</remarks>
public sealed class NoSRSMedia : ISRSMedia
{
   /// <summary>Does nothing: there is nothing to give the copy to.</summary>
   public void Add(AudioAttachment audio) {}
}
