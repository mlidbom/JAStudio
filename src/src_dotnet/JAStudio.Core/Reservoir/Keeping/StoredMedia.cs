namespace JAStudio.Core.Reservoir.Keeping;

/// <summary>A media file as the <see cref="MediaStore"/> keeps it: under the hash of its normalized content, and what was done to it.</summary>
/// <param name="OriginalName">The file's name in its source, such as <c>show_01_0.01.mp3</c>, as the source's notes refer to it.</param>
/// <param name="Hash">The SHA-256 of the content kept, as lowercase hex: the file's name in the store, with <paramref name="Extension"/>.</param>
/// <param name="Extension">The original name's extension, lowercase, such as <c>.mp3</c>.</param>
/// <param name="OriginalLength">The original content's length, in bytes.</param>
/// <param name="GainStepsApplied">
/// For an MP3, the mp3gain steps of 1.5 dB its gain was changed by to bring it to the ReplayGain reference loudness; changing it back by as many<br/>
/// gives the original's audio. 0 for any other file, kept as it was.
/// </param>
/// <param name="WasNewToTheStore">Whether the store didn't have the content yet: false when an earlier file, from this source or another, had the same.</param>
public sealed record StoredMedia(string OriginalName, string Hash, string Extension, int OriginalLength, int GainStepsApplied, bool WasNewToTheStore);
