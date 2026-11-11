namespace JAStudio.Core.Reservoir.Storage.Format;

/// <summary>The manifest of the reservoir's text folder, <c>manifest.json</c>: the format its files are in, and what the folder holds.</summary>
/// <param name="FormatVersion">The version of the format of every file in the folder (<see cref="CurrentFormatVersion"/>).</param>
/// <param name="Description">What the folder holds and how it's laid out, for someone who comes across it.</param>
public sealed record Manifest(int FormatVersion, string Description)
{
   /// <summary>The version of the format the files are written in now, which <c>manifest.json</c> and each <c>deck.json</c> record.</summary>
   public const int CurrentFormatVersion = 1;

   /// <summary>The manifest of a text folder whose files are in the current format.</summary>
   public static Manifest OfTheCurrentFormat { get; } =
      new(CurrentFormatVersion,
          "Sources of JAStudio's sentence reservoir, stored as JAStudio's own. decks/<package name>/ holds a deck's collection database as its "
        + "package had it, deck.json (the package, how its note types map to sentences, and its media files by the SHA-256 of their normalized "
        + "content, which names them in the media store), and notes.jsonl, a line of JSON per note: its fields as they are, and the sentence it gives.");
}
