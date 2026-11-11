using JAStudio.Core.Reservoir.Keeping.Media;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>A kept sentence's reference to a media file: the name its note refers to, and the hash the <see cref="MediaStore"/> keeps it under.</summary>
/// <param name="FileName">The file's name in the deck.</param>
/// <param name="Hash">The hash of its content in the media store, or null when the deck refers to a file its package doesn't hold.</param>
public sealed record KeptMediaReference(string FileName, string? Hash)
{
   /// <summary>
   /// The reference to the file the deck refers to as <paramref name="fileName"/>, with the hash it's kept under in <paramref name="media"/>: none<br/>
   /// when the package lacks the file, as a deck's package can, and the reference is kept all the same.
   /// </summary>
   public static KeptMediaReference To(string fileName, StoredDeckMedia media) => new(fileName, media.FileNamed(fileName)?.NameInTheStore.Hash);
}
