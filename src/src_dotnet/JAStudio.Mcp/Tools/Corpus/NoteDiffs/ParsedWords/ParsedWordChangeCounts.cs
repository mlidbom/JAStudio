using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;

/// <summary>
/// How the parsed words of one word changed over all the sentences a diff compares: how many of them were added, removed, now displayed and no<br/>
/// longer displayed, from each sentence's <see cref="ParsedWordsChange"/>. What a full rebuild changed in parsing, word by word.
/// </summary>
/// <param name="Word">The question of the vocab the words are matched to; for words matched to no vocab the collection has, their form.</param>
/// <param name="VocabId">The id of the vocab the words are matched to. Null for words matched to no vocab, but only to the dictionary.</param>
public record ParsedWordChangeCounts(string Word, string? VocabId, int Added, int Removed, int NowDisplayed, int NoLongerDisplayed)
{
   /// <summary>The counts of each word whose parsed words changed in <paramref name="changes"/>, those that changed most first.</summary>
   public static IReadOnlyList<ParsedWordChangeCounts> Of(IReadOnlyList<ParsedWordsChange> changes)
   {
      var added = CountsByWord(changes.SelectMany(it => it.Added));
      var removed = CountsByWord(changes.SelectMany(it => it.Removed));
      var nowDisplayed = CountsByWord(changes.SelectMany(it => it.NowDisplayed));
      var noLongerDisplayed = CountsByWord(changes.SelectMany(it => it.NoLongerDisplayed));

      return [.. added.Keys.Union(removed.Keys).Union(nowDisplayed.Keys).Union(noLongerDisplayed.Keys)
                       .Select(word => new ParsedWordChangeCounts(word.Word,
                                                                  word.VocabId,
                                                                  added.GetValueOrDefault(word),
                                                                  removed.GetValueOrDefault(word),
                                                                  nowDisplayed.GetValueOrDefault(word),
                                                                  noLongerDisplayed.GetValueOrDefault(word)))
                       .OrderByDescending(it => it.Added + it.Removed + it.NowDisplayed + it.NoLongerDisplayed)
                       .ThenBy(it => it.Word, StringComparer.Ordinal)];

      static Dictionary<(string Word, string? VocabId), int> CountsByWord(IEnumerable<ParsedWordReported> words) =>
         words.CountBy(it => (Word: it.Vocab ?? it.Form, it.VocabId)).ToDictionary();
   }
}
