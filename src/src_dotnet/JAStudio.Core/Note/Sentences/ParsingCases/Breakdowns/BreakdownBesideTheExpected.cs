using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.UI.Web.Sentence;

namespace JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;

/// <summary>
/// The breakdown a sentence displays beside the one its parsing case expects under the same options, row by row, in the order of the sentence, to<br/>
/// show the two side by side: a word both display is one row, and a word only one of them displays stands beside the other's word that starts at<br/>
/// the same place, if it has one that the first doesn't display, or else beside nothing.
/// </summary>
/// <remarks>
/// The rows keep as many words as possible beside the same word, in order (the longest common subsequence of the two breakdowns), so that one<br/>
/// word more or less in one of them doesn't shift every word after it out of its row.
/// </remarks>
public sealed partial record BreakdownBesideTheExpected(IReadOnlyList<BreakdownBesideTheExpected.Row> Rows)
{
   /// <summary>Whether the sentence displays exactly the words expected, in the same order.</summary>
   public bool TheyAreTheSame => Rows.All(it => it.IsTheSameWord);

   /// <summary>
   /// <paramref name="displayed"/>, the words a sentence's breakdown displays outside edit mode (<see cref="TextAnalysisViewModel.MatchesDisplayedOutsideEditMode"/>),<br/>
   /// beside <paramref name="expected"/>, the breakdown its parsing case expects under the same options.
   /// </summary>
   public static BreakdownBesideTheExpected Of(IReadOnlyList<MatchViewModel> displayed, BreakdownUnderOptions expected)
   {
      var displayedWords = displayed.Select(BreakdownWord.Of).ToList();
      var expectedWords = expected.DisplayedWords;
      var rows = new List<Row>();
      var nextDisplayed = 0;
      var nextExpected = 0;
      foreach(var (sameDisplayed, sameExpected) in TheWordsBothDisplayInOrder())
      {
         AddTheRowsOfTheWordsOnlyOneDisplaysBefore(sameDisplayed, sameExpected);
         rows.Add(new Row(displayed[sameDisplayed], expectedWords[sameExpected]));
         nextDisplayed = sameDisplayed + 1;
         nextExpected = sameExpected + 1;
      }

      AddTheRowsOfTheWordsOnlyOneDisplaysBefore(displayedWords.Count, expectedWords.Count);
      return new BreakdownBesideTheExpected(rows);

      void AddTheRowsOfTheWordsOnlyOneDisplaysBefore(int displayedEnd, int expectedEnd)
      {
         while(nextDisplayed < displayedEnd || nextExpected < expectedEnd)
         {
            var hasDisplayed = nextDisplayed < displayedEnd;
            var hasExpected = nextExpected < expectedEnd;
            if(hasDisplayed && hasExpected && displayedWords[nextDisplayed].StartIndex == expectedWords[nextExpected].StartIndex)
               rows.Add(new Row(displayed[nextDisplayed++], expectedWords[nextExpected++]));
            else if(!hasExpected || (hasDisplayed && displayedWords[nextDisplayed].StartIndex < expectedWords[nextExpected].StartIndex))
               rows.Add(new Row(displayed[nextDisplayed++], null));
            else
               rows.Add(new Row(null, expectedWords[nextExpected++]));
         }
      }

      // The longest run of words, in order, that both breakdowns display: for each, where it is in the displayed words and in the expected ones.
      List<(int Displayed, int Expected)> TheWordsBothDisplayInOrder()
      {
         var lengthFrom = new int[displayedWords.Count + 1, expectedWords.Count + 1];
         for(var displayedIndex = displayedWords.Count - 1; displayedIndex >= 0; displayedIndex--)
         for(var expectedIndex = expectedWords.Count - 1; expectedIndex >= 0; expectedIndex--)
            lengthFrom[displayedIndex, expectedIndex] = IsTheSameWordAt(displayedIndex, expectedIndex)
                                                           ? lengthFrom[displayedIndex + 1, expectedIndex + 1] + 1
                                                           : Math.Max(lengthFrom[displayedIndex + 1, expectedIndex], lengthFrom[displayedIndex, expectedIndex + 1]);

         var same = new List<(int, int)>();
         for(int displayedIndex = 0, expectedIndex = 0; displayedIndex < displayedWords.Count && expectedIndex < expectedWords.Count;)
         {
            if(IsTheSameWordAt(displayedIndex, expectedIndex))
               same.Add((displayedIndex++, expectedIndex++));
            else if(lengthFrom[displayedIndex + 1, expectedIndex] >= lengthFrom[displayedIndex, expectedIndex + 1])
               displayedIndex++;
            else
               expectedIndex++;
         }

         return same;
      }

      bool IsTheSameWordAt(int displayedIndex, int expectedIndex) => displayedWords[displayedIndex].IsTheSameWordAs(expectedWords[expectedIndex]);
   }
}
