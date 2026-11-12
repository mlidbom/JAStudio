using JAStudio.Mcp.Tools.Analysis;

namespace JAStudio.Mcp.Tools.Timing;

/// <summary>
/// What <see cref="AnalysisTools.TimeReparsingTheSentencesOfAVocab"/> reports: how long the reparse the vocab editor runs after each change to a vocab<br/>
/// took for one vocab, and how many sentences it reparsed.
/// </summary>
/// <param name="Vocab">The vocab's name, its question with its disambiguation.</param>
/// <param name="Sentences">How many sentences the reparse covered: those that may contain the vocab, and those it's matched in now.</param>
/// <param name="SecondsFindingTheSentences">How long finding those sentences took, timed on its own before the reparse.</param>
/// <param name="SecondsTheReparseTook">How long the editor's reparse took, from start to end, finding the sentences again included.</param>
/// <param name="ProcessorTime">The processor time the reparse used, and the machine's other processes meanwhile.</param>
public record TimeToReparseTheSentencesOfAVocab(string Vocab, int Sentences, double SecondsFindingTheSentences, double SecondsTheReparseTook, ProcessorTimeUsed ProcessorTime);
