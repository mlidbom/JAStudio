namespace JAStudio.Core.Reservoir.Archiving.Format;

/// <summary>The manifest of a reservoir archive's text folder, <c>manifest.json</c>: the format its files are in, and what the folder holds.</summary>
/// <param name="FormatVersion">The version of the format of every file in the folder (<see cref="ReservoirArchive.FormatVersion"/>).</param>
/// <param name="Description">What the folder holds and how it's laid out, for someone who comes across it.</param>
public sealed record ArchiveManifest(int FormatVersion, string Description);
