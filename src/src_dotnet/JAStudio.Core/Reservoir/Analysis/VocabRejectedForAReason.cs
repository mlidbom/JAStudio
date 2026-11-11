using JAStudio.Core.Reservoir.Analysis.Numbering;

namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>A vocab whose match a failure reason rejected in a reservoir sentence, the vocab and the reason by their numbers (<see cref="ReservoirAnalysisNumbers"/>).</summary>
public readonly record struct VocabRejectedForAReason(int VocabNumber, int ReasonNumber);
