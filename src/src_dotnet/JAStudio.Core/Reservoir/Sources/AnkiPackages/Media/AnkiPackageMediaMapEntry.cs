namespace JAStudio.Core.Reservoir.Sources.AnkiPackages.Media;

/// <summary>An entry of an Anki package's media map (<see cref="AnkiPackageMediaMap"/>): which zip entry of the package holds which media file.</summary>
/// <param name="ZipEntryName">The name of the zip entry holding the file, a number, such as <c>0</c>.</param>
/// <param name="FileName">The file's own name, such as <c>show_01_0.01.mp3</c>, as the deck's notes refer to it.</param>
public sealed record AnkiPackageMediaMapEntry(string ZipEntryName, string FileName);
