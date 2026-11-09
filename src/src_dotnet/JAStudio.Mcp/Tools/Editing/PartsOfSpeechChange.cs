using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>What <see cref="VocabEditingTools.SetThePartsOfSpeechOfAVocab"/> reports: the vocab's parts of speech before and after, each in alphabetical order.</summary>
/// <param name="Vocab">The vocab's question, to recognize it by.</param>
public record PartsOfSpeechChange(string Vocab, IReadOnlyList<string> Before, IReadOnlyList<string> After);
