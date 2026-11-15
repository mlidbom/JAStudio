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
   }
}
