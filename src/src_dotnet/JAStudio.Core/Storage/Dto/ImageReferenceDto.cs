using System.Text.Json.Serialization;
using JAStudio.Core.Note.Media;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>
/// An <see cref="ImageReference"/> as the corpus stores it, in a note's list of images: the file's name and the facts known about the file.<br/>
/// A fact that isn't known, or is false, is left out of the file, so that a reference with no facts is only its file's name.
/// </summary>
[MemoryPackable]
public partial class ImageReferenceDto
{
   public string FileName { get; init; } = string.Empty;

   [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
   public bool IsOurs { get; init; }

   [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
   public string? AltText { get; init; }

   public static ImageReferenceDto From(ImageReference image) => new() { FileName = image.FileName, IsOurs = image.IsOurs, AltText = image.AltText };

   public ImageReference ToReference() => new(FileName) { IsOurs = IsOurs, AltText = AltText };
}
