using MemoryPack;

namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>
/// A word of a reservoir sentence's analysis: a valid match, as a sentence's stored parsing result holds them, or a match the sentence breakdown<br/>
/// displays, which a missing word's always is.
/// </summary>
/// <param name="StartIndex">The index in the sentence of the word's first character.</param>
/// <param name="Form">The form the match puts in a parsing result.</param>
/// <param name="VocabNumber">The vocab's number in the reservoir (<see cref="ReservoirAnalysisNumbers"/>), for a <see cref="ReservoirWordKind.Vocab"/> word; 0 otherwise.</param>
/// <param name="IsDisplayed">Whether the sentence breakdown displays it.</param>
[MemoryPackable]
public sealed partial record ReservoirWord(short StartIndex, string Form, ReservoirWordKind Kind, int VocabNumber, bool IsDisplayed);
