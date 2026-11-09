using System.Collections.Generic;
using System.Linq;
using JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;
using JAStudio.Mcp.Tools.Corpus.NoteDiffs.Properties;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs;

/// <summary>How one note differs between two versions of the corpus, as <see cref="CorpusTools.DiffTheChangedNotes"/> reports it.</summary>
/// <param name="Question">The note's question, as the latest of the two versions that has the note has it.</param>
/// <param name="ValuesChanged">The properties whose values changed. Empty for a note added or deleted.</param>
/// <param name="ListsChanged">The lists that items were added to or removed from. Empty for a note added or deleted.</param>
/// <param name="ParsedWords">How a sentence's parsed words changed. Null for a sentence whose parsed words didn't, and for vocab and kanji.</param>
public record NoteDiff(
   string Id,
   string Question,
   NoteChangeKind Change,
   IReadOnlyList<ValueChange> ValuesChanged,
   IReadOnlyList<ListChange> ListsChanged,
   ParsedWordsChange? ParsedWords)
{
   /// <summary>The paths of the properties that changed, as the corpus stores the note, such as <c>forms</c>.</summary>
   public IEnumerable<string> PropertiesChanged() =>
      ValuesChanged.Select(it => it.Property)
                   .Concat(ListsChanged.Select(it => it.Property))
                   .Concat(ParsedWords == null ? [] : [NoteVersions.ParsedWordsProperty]);
}
