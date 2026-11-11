namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>The manifest of a folder of kept sources, <c>manifest.json</c>: the format its files are in, and what the folder holds.</summary>
/// <param name="FormatVersion">The version of the format of every file in the folder (<see cref="ReservoirKeeping.FormatVersion"/>).</param>
/// <param name="Description">What the folder holds and how it's laid out, for someone who comes across it.</param>
public sealed record KeptManifest(int FormatVersion, string Description);
