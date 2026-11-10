namespace JAStudio.Core.Note.Media;

/// <summary>A note's reference to an image file, such as a vocab's image from Core 2000 or from the web, or a screenshot of the scene a sentence is from.</summary>
public sealed record ImageReference(string FileName) : MediaReference(FileName)
{
   /// <summary>The image's alt text, when it has one: for an image from the web, mostly the title of the page it was found on.</summary>
   public string? AltText { get; init; }
}
