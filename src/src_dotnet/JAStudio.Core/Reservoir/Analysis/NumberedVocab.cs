namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>A vocab as a reservoir's analyses refer to it: by its number (<see cref="ReservoirAnalysisNumbers"/>), with its note id and its question when numbered.</summary>
public sealed record NumberedVocab(int Number, string NoteId, string Question);
