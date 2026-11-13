using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;

/// <summary>The sentences where a word's parsed words changed in one way, such as were removed: how many, and the first of them.</summary>
/// <param name="Count">How many times the word changed so, over every sentence compared.</param>
/// <param name="Sentences">The first of them, at most as many as asked for.</param>
public record SentencesOfOneKindOfChange(int Count, IReadOnlyList<SentenceWhereTheWordChanged> Sentences);
