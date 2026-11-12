using JAStudio.Mcp.Tools.Memory;

namespace JAStudio.Mcp.Tools.Analysis.Timing;

/// <summary>
/// What <see cref="AnalysisTools.TimeParsingEverySentence"/> reports: how long parsing every sentence of the corpus took, on how many threads.<br/>
/// Tokenizing is the tokenizer's share of the work; analyzing is all of it, the tokenizing included.
/// </summary>
/// <param name="Sentences">How many sentences were parsed.</param>
/// <param name="Threads">How many threads they were parsed on.</param>
/// <param name="GarbageCollectorMode">The garbage collector's mode in this process, which decides how its pauses scale with threads.</param>
/// <param name="Tokenizing">The pass that tokenized every sentence's question.</param>
/// <param name="Analyzing">The pass that analyzed every sentence, as its stored parsing result is made.</param>
public record TimeToParseEverySentence(int Sentences, int Threads, GarbageCollectorMode GarbageCollectorMode, TimedPass Tokenizing, TimedPass Analyzing);
