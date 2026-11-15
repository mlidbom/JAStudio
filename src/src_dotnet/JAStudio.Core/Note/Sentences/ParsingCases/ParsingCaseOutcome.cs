namespace JAStudio.Core.Note.Sentences.ParsingCases;

/// <summary>
/// How what a parsing case's sentence parses to now compares with what the case expects (<see cref="ParsingCaseComparison.Outcome"/>), the one<br/>
/// most in need of a person first, so that sorting by it puts those on top.
/// </summary>
public enum ParsingCaseOutcome
{
   /// <summary>The breakdown displays other vocab than a person verified as right: an error (<see cref="ParsingCaseComparison.Fails"/>).</summary>
   Fails,

   /// <summary>
   /// The sentence parses otherwise than expected in what nobody has verified: a change for a person to review<br/>
   /// (<see cref="ParsingCaseComparison.IsToReview"/>).
   /// </summary>
   IsToReview,

   /// <summary>The sentence parses exactly to what the case expects (<see cref="ParsingCaseComparison.ParsesAsExpected"/>).</summary>
   ParsesAsExpected
}
