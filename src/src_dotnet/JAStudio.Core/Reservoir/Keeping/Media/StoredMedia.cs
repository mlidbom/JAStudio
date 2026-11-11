namespace JAStudio.Core.Reservoir.Keeping.Media;

/// <summary>A media file as the <see cref="MediaStore"/> keeps it: under the name its normalized content gives it, and what was done to it.</summary>
/// <param name="OriginalName">The file's name in its source, such as <c>show_01_0.01.mp3</c>, as the source's notes refer to it.</param>
/// <param name="NameInTheStore">The name the content kept is stored under: its SHA-256, and the extension of <paramref name="OriginalName"/>.</param>
/// <param name="OriginalLength">The original content's length, in bytes.</param>
/// <param name="GainStepsApplied">
/// For an MP3, the mp3gain steps of 1.5 dB its gain was changed by to bring it to the ReplayGain reference loudness; changing it back by as many<br/>
/// gives the original's audio. 0 for any other file, kept as it was.
/// </param>
/// <param name="WasNewToTheStore">Whether the store didn't have the content yet: false when an earlier file, from this source or another, had the same.</param>
public sealed record StoredMedia(string OriginalName, MediaStoreFileName NameInTheStore, int OriginalLength, int GainStepsApplied, bool WasNewToTheStore);
