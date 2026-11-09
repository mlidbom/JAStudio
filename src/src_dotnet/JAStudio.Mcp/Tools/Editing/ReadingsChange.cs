using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>What <see cref="VocabEditingTools.SetTheReadingsOfAVocab"/> reports: the vocab's readings before and after, each in the vocab's order.</summary>
/// <param name="Vocab">The vocab's question, to recognize it by.</param>
public record ReadingsChange(string Vocab, IReadOnlyList<string> Before, IReadOnlyList<string> After);
