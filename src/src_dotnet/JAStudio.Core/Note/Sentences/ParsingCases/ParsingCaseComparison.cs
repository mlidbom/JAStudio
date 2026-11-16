using System.Collections.Generic;

namespace JAStudio.Core.Note.Sentences.ParsingCases;

/// <summary>
/// What a parsing case's sentence parses to now, compared with what the case expects (<see cref="ParsingCase.Expected"/>). A difference is an<br/>
/// error only where a person has verified what's expected: the case <see cref="Fails"/>. Any other difference is a change nobody can call right or<br/>
/// wrong yet, for a person to review (<see cref="IsToReview"/>).
/// </summary>
/// <param name="Case">The parsing case.</param>
/// <param name="Parsed">What its sentence parses to now.</param>
/// <param name="Differences">How <paramref name="Parsed"/> differs from what the case expects, a line per difference. Empty when it doesn't.</param>
public sealed record ParsingCaseComparison(ParsingCase Case, ParsingCaseResult Parsed, IReadOnlyList<string> Differences)
{
   /// <summary>Whether the sentence parses exactly to what the case expects.</summary>
   public bool ParsesAsExpected => Differences.Count == 0;

   /// <summary>Whether the breakdown displays other vocab than the case expects (<see cref="ParsingCaseResult.DisplayedVocab"/>).</summary>
   public bool TheDisplayedVocabDiffer => !Case.Expected.DisplayedVocab.IsTheSameAs(Parsed.DisplayedVocab);

   /// <summary>Whether the breakdown displays other vocab than a person verified as right: the one difference that is an error.</summary>
   public bool Fails => TheDisplayedVocabDiffer && Case.VerdictOnTheDisplayedVocab == Verdict.Verified;

   /// <summary>
   /// Whether the sentence parses otherwise than expected in what nobody has verified: the displayed vocab of a case that isn't verified, or<br/>
   /// anything else of any case. Suspect, not an error: a person reviews the change, and accepts it or finds it wrong.
   /// </summary>
   public bool IsToReview => !ParsesAsExpected && !Fails;

   /// <summary>Which of the three the comparison comes out as: <see cref="Fails"/>, <see cref="IsToReview"/> or <see cref="ParsesAsExpected"/>.</summary>
   public ParsingCaseOutcome Outcome => Fails        ? ParsingCaseOutcome.Fails
                                        : IsToReview ? ParsingCaseOutcome.IsToReview
                                                       : ParsingCaseOutcome.ParsesAsExpected;
}
