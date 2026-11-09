using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// What <see cref="VocabEditingTools.SetAMatchingFlagOfAVocab"/> reports: the names of the vocab's matching flags that are set, before and after,<br/>
/// in the order <see cref="Core.Note.Vocabulary.Matching.VocabMatchingFlags.All"/> has them.
/// </summary>
/// <param name="Vocab">The vocab's question, to recognize it by.</param>
public record MatchingFlagsChange(string Vocab, IReadOnlyList<string> Before, IReadOnlyList<string> After);
