using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs;

/// <summary>
/// How the notes whose files differ between two versions of the corpus differ as notes, by note type, and how the sentences' parsed words changed,<br/>
/// counted by word: what <see cref="CorpusTools.DiffTheChangedNotes"/> reports.
/// </summary>
/// <param name="From">The version compared from: a commit, by its hash.</param>
/// <param name="To">The version compared to: a commit, by its hash, or the working tree, the checkout's files as they are now.</param>
/// <param name="WordsWhoseParsedWordsChanged">How many words <paramref name="ParsedWordsChangedByWord"/> would list if it listed all of them.</param>
/// <param name="ParsedWordsChangedByWord">How the parsed words of each word changed over all the sentences, those that changed most first, at most as many as asked for.</param>
public record NotesDiff(
   string From,
   string To,
   NotesDiffOfOneType Vocab,
   NotesDiffOfOneType Kanji,
   NotesDiffOfOneType Sentences,
   int WordsWhoseParsedWordsChanged,
   IReadOnlyList<ParsedWordChangeCounts> ParsedWordsChangedByWord)
{
   /// <summary>The diff of <paramref name="notes"/>, naming the vocab sentences' parsed words are matched to by their questions in <paramref name="collection"/>.</summary>
   public static NotesDiff Of(string from, string to, IReadOnlyList<NoteVersions> notes, JPCollection collection, int maxNotesListedPerType, int maxWordsListed)
   {
      var diffs = notes.Select(it => (it.Latest, Diff: it.Diff(collection))).ToList();
      var parsedWordsChangedByWord = ParsedWordChangeCounts.Of([.. diffs.Select(it => it.Diff.ParsedWords).OfType<ParsedWordsChange>()]);

      return new NotesDiff(from,
                           to,
                           DiffOfThe<VocabNote>(),
                           DiffOfThe<KanjiNote>(),
                           DiffOfThe<SentenceNote>(),
                           parsedWordsChangedByWord.Count,
                           [.. parsedWordsChangedByWord.Take(maxWordsListed)]);

      NotesDiffOfOneType DiffOfThe<TNote>() where TNote : JPNote =>
         NotesDiffOfOneType.Of([.. diffs.Where(it => it.Latest is TNote).Select(it => it.Diff)], maxNotesListedPerType);
   }
}
