using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;

/// <summary>
/// How a sentence's parsed words changed between two versions of it, compared as words: a word is the same word when it's the same form, at the<br/>
/// same place in the sentence, matched in the same way to the same vocab, or to none. A sentence can have the same word more than once, such as<br/>
/// えば, from 触れれば, displayed once and hidden once, so the words are compared as many times as each version has them.
/// </summary>
/// <param name="Added">The words parsed now that weren't before, such as a word now matched to another vocab.</param>
/// <param name="Removed">The words parsed before that aren't now, such as a word no longer matched to the vocab it was matched to.</param>
/// <param name="NowDisplayed">The words parsed in both, that are displayed now but weren't before.</param>
/// <param name="NoLongerDisplayed">The words parsed in both, that were displayed before but aren't now.</param>
public record ParsedWordsChange(
   IReadOnlyList<ParsedWordReported> Added,
   IReadOnlyList<ParsedWordReported> Removed,
   IReadOnlyList<ParsedWordReported> NowDisplayed,
   IReadOnlyList<ParsedWordReported> NoLongerDisplayed)
{
   public bool IsNoChange() => Added.Count == 0 && Removed.Count == 0 && NowDisplayed.Count == 0 && NoLongerDisplayed.Count == 0;

   /// <summary>Compares the parsed words of <paramref name="before"/> with those of <paramref name="after"/>, naming each vocab by its question in <paramref name="collection"/>.</summary>
   public static ParsedWordsChange Between(ParsingResult before, ParsingResult after, JPCollection collection)
   {
      var wordsOnlyAfter = WordsWithoutAnEqualWordIn(after.ParsedWords, before.ParsedWords);
      var wordsOnlyBefore = WordsWithoutAnEqualWordIn(before.ParsedWords, after.ParsedWords);

      var displayChanges = new List<ParsedMatch>();
      foreach(var word in wordsOnlyAfter.ToList())
      {
         var sameWordDisplayedOtherwise = wordsOnlyBefore.FirstOrDefault(it => SameWord(it) == SameWord(word));
         if(sameWordDisplayedOtherwise == null) continue;

         wordsOnlyBefore.Remove(sameWordDisplayedOtherwise);
         wordsOnlyAfter.Remove(word);
         displayChanges.Add(word);
      }

      return new ParsedWordsChange(Reported(wordsOnlyAfter),
                                   Reported(wordsOnlyBefore),
                                   Reported(displayChanges.Where(it => it.IsDisplayed)),
                                   Reported(displayChanges.Where(it => !it.IsDisplayed)));

      List<ParsedWordReported> Reported(IEnumerable<ParsedMatch> words) => [.. words.OrderBy(it => it.StartIndex).Select(it => ParsedWordReported.Of(it, collection))];
   }

   /// <summary>The words of <paramref name="words"/> that no word of <paramref name="otherWords"/> equals, displayed or not alike, each word of <paramref name="otherWords"/> equaling at most one.</summary>
   static List<ParsedMatch> WordsWithoutAnEqualWordIn(IEnumerable<ParsedMatch> words, IEnumerable<ParsedMatch> otherWords)
   {
      var otherWordsNotYetEqualed = otherWords.ToList();
      var wordsWithoutAnEqual = new List<ParsedMatch>();
      foreach(var word in words)
      {
         var indexOfAnEqualWord = otherWordsNotYetEqualed.FindIndex(it => SameWord(it) == SameWord(word) && it.IsDisplayed == word.IsDisplayed);
         if(indexOfAnEqualWord < 0) wordsWithoutAnEqual.Add(word);
         else otherWordsNotYetEqualed.RemoveAt(indexOfAnEqualWord);
      }

      return wordsWithoutAnEqual;
   }

   static (string Form, int StartIndex, string Variant, Guid? VocabId) SameWord(ParsedMatch word) => (word.ParsedForm, word.StartIndex, word.Variant, word.VocabId?.Value);
}
