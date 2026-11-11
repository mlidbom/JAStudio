namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>A kept sentence's reference to a media file: the name its note refers to, and the hash the <see cref="MediaStore"/> keeps it under.</summary>
/// <param name="FileName">The file's name in the deck.</param>
/// <param name="Hash">The hash of its content in the media store, or null when the deck refers to a file its package doesn't hold.</param>
public sealed record KeptMediaReference(string FileName, string? Hash);
