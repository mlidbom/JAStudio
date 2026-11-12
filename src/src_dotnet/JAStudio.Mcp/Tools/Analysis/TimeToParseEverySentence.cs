namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>
/// What <see cref="AnalysisTools.TimeParsingEverySentence"/> reports: how long parsing every sentence of the corpus took, on how many threads.<br/>
/// Tokenizing is the tokenizer's share of the work; analyzing is all of it, the tokenizing included.
/// </summary>
/// <param name="Sentences">How many sentences were parsed.</param>
/// <param name="Threads">How many threads they were parsed on.</param>
/// <param name="SecondsToTokenize">How long tokenizing every sentence's question took.</param>
/// <param name="SecondsToAnalyze">How long analyzing every sentence took, as its stored parsing result is made.</param>
public record TimeToParseEverySentence(int Sentences, int Threads, double SecondsToTokenize, double SecondsToAnalyze);
