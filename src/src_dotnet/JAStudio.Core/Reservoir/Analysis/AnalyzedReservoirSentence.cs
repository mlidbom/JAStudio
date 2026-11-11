namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>A reservoir sentence with its analysis, as the measures over the reservoir read them.</summary>
/// <param name="Id">The sentence's id in the reservoir.</param>
/// <param name="SourceId">The id in the reservoir of the source it's kept from.</param>
/// <param name="Occurrences">How often it was found, in every source.</param>
/// <param name="HasAudio">Whether the source it's kept from has its audio, as a deck does.</param>
public sealed record AnalyzedReservoirSentence(long Id, string Text, long SourceId, int Occurrences, bool HasAudio, ReservoirSentenceAnalysis Analysis);
