using MemoryPack;

namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>
/// A vocab whose form a reservoir sentence has at some place, but which the analysis didn't take there, with why: its match-condition, or another of<br/>
/// the match's requirements, failed. What decides these is what a vocab's conditions are for, so they show where the conditions are exercised.
/// </summary>
/// <param name="StartIndex">The index in the sentence of the candidate word's first character.</param>
/// <param name="VocabNumber">The vocab's number in the reservoir (<see cref="ReservoirAnalysisNumbers"/>).</param>
/// <param name="ReasonNumbers">The numbers of the match's failure reasons in the reservoir (<see cref="ReservoirAnalysisNumbers"/>).</param>
[MemoryPackable]
public sealed partial record RejectedVocabMatch(short StartIndex, int VocabNumber, int[] ReasonNumbers);
