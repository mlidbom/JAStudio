using System;
using JAStudio.Core.Note.Sentences.ParsingCases;

namespace JAStudio.UI.ViewModels.Workspace.ParsingCases;

/// <summary>
/// The words the workspace shows a parsing case's <see cref="ParsingCaseOutcome"/> and <see cref="Verdict"/> in, the same in the navigation pane's<br/>
/// list of the parsing cases and on a sentence's page.
/// </summary>
static class ParsingCaseTexts
{
   /// <summary><paramref name="outcome"/> as the workspace shows it, such as "to review".</summary>
   public static string InWords(this ParsingCaseOutcome outcome) => outcome switch
   {
      ParsingCaseOutcome.Fails            => "fails",
      ParsingCaseOutcome.IsToReview       => "to review",
      ParsingCaseOutcome.ParsesAsExpected => "as expected",
      _                                   => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "A parsing case has no such outcome.")
   };

   /// <summary><paramref name="verdict"/> as the workspace shows it, such as "verified".</summary>
   public static string InWords(this Verdict verdict) => verdict switch
   {
      Verdict.Unverified => "unverified",
      Verdict.Verified   => "verified",
      Verdict.Broken     => "broken",
      _                  => throw new ArgumentOutOfRangeException(nameof(verdict), verdict, "There is no such verdict.")
   };
}
