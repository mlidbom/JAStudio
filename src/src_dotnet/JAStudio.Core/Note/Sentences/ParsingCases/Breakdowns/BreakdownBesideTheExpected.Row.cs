using System;
using System.Collections.Generic;
using JAStudio.Core.UI.Web.Sentence;

namespace JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;

public sealed partial record BreakdownBesideTheExpected
{
   /// <summary>One row of a <see cref="BreakdownBesideTheExpected"/>: a word the sentence displays, a word its parsing case expects it to display, or both.</summary>
   /// <param name="Displayed">The word the sentence's breakdown displays. Null in the row of a word that is expected and not displayed.</param>
   /// <param name="Expected">The word the parsing case expects the breakdown to display. Null in the row of a word that is displayed and not expected.</param>
   public sealed record Row(MatchViewModel? Displayed, BreakdownWord? Expected)
   {
      /// <summary>Whether the row is of a word that is displayed just as it's expected: both sides have the same word.</summary>
      public bool IsTheSameWord => Displayed != null && Expected != null && BreakdownWord.Of(Displayed).IsTheSameWordAs(Expected);

      /// <summary>
      /// How what the sentence displays in this row differs from what its parsing case expects there, a line per difference, said of the word<br/>
      /// displayed: such as <c>displayed, but expected not displayed</c>, or <c>expected, but not displayed: ています</c>. Empty when the row<br/>
      /// <see cref="IsTheSameWord"/>.
      /// </summary>
      public IReadOnlyList<string> Differences => (Displayed, Expected) switch
      {
         ({} displayed, {} expected) => BreakdownWord.Of(displayed).HowItDiffersFrom(expected),
         ({}, null)                  => [BreakdownWord.DisplayedButNotExpected],
         (null, {} expected)         => [expected.ExpectedButNotDisplayed],
         (null, null)                => throw new InvalidOperationException("A row has a word on at least one of its sides.")
      };
   }
}
