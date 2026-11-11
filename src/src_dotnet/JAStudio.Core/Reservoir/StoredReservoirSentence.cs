namespace JAStudio.Core.Reservoir;

/// <summary>A sentence as the reservoir keeps it (<see cref="ReservoirFile"/>), with where it was found and what came with it there.</summary>
/// <param name="SourceId">The id in the reservoir of the source it's kept from: the first it was found in, or the first with its audio.</param>
/// <param name="Position">Where in that source it is.</param>
/// <param name="Occurrences">How often it was found, in every source.</param>
public sealed record StoredReservoirSentence(long Id, string Text, long SourceId, string Position, string? Translation, string? AudioFile, string? ImageFile, int Occurrences);
