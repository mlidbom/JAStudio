using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;

/// <summary>A sentence where a word's parsed words changed, with the word as it changed and what the sentence displayed before and after.</summary>
/// <param name="Id">The sentence note's id.</param>
/// <param name="Sentence">The sentence's text, its question.</param>
/// <param name="Word">The parsed word that changed.</param>
/// <param name="DisplayedBefore">The forms of the words the sentence displayed in the version compared from, in order.</param>
/// <param name="DisplayedAfter">The forms of the words the sentence displays in the version compared to, in order.</param>
public record SentenceWhereTheWordChanged(string Id, string Sentence, ParsedWordReported Word, IReadOnlyList<string> DisplayedBefore, IReadOnlyList<string> DisplayedAfter)
{
   /// <summary>The sentence as it was <paramref name="before"/> and is <paramref name="after"/>, where <paramref name="word"/> changed.</summary>
   internal static SentenceWhereTheWordChanged Of(SentenceNote before, SentenceNote after, ParsedWordReported word) =>
      new(after.GetId().ToString(), after.GetQuestion(), word, DisplayedWordsOf(before), DisplayedWordsOf(after));

   static IReadOnlyList<string> DisplayedWordsOf(SentenceNote sentence) =>
      [.. sentence.GetParsingResult().ParsedWords.Where(it => it.IsDisplayed).OrderBy(it => it.StartIndex).Select(it => it.ParsedForm)];
}
