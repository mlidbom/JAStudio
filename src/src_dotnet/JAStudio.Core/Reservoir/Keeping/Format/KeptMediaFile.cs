using JAStudio.Core.Reservoir.Keeping.Media;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>A media file of a kept deck, by the name its notes refer to it by, and where the <see cref="MediaStore"/> keeps it.</summary>
/// <param name="Name">The file's name in the deck, such as <c>show_01_0.01.mp3</c>.</param>
/// <param name="Hash">The SHA-256 of its normalized content, its name in the media store (<see cref="MediaStoreFileName.Hash"/>).</param>
/// <param name="GainStepsApplied">For an MP3, the mp3gain steps its gain was changed by when normalized (<see cref="StoredMedia.GainStepsApplied"/>); 0 otherwise.</param>
public sealed record KeptMediaFile(string Name, string Hash, int GainStepsApplied)
{
   /// <summary>The kept deck's record of <paramref name="file"/>, one of its media files.</summary>
   public static KeptMediaFile Of(StoredMedia file) => new(file.OriginalName, file.NameInTheStore.Hash, file.GainStepsApplied);
}
