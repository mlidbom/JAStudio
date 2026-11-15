using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Sentences.ParsingCases;

namespace JAStudio.Mcp.Tools.ParsingCases;

/// <summary>A parsing case as <see cref="ParsingCaseTools.ListTheParsingCases"/> lists it.</summary>
/// <param name="SentenceId">The id of the sentence that is the parsing case, to get, edit or run a command on.</param>
/// <param name="Sentence">The sentence's question.</param>
/// <param name="Name">What the case is about.</param>
/// <param name="Why">Why the case exists, or what is wrong with how it parses.</param>
/// <param name="VerdictOnTheDisplayedVocab">What a person has found about the vocab the case expects the breakdown to display.</param>
/// <param name="Outcome">How what the sentence parses to now compares with what the case expects.</param>
/// <param name="DifferencesCounted">How many differences there are, listed or not.</param>
/// <param name="Differences">The first differences, a line each, at most as many as asked for.</param>
public record ParsingCaseListed(string SentenceId,
                                string Sentence,
                                string Name,
                                string Why,
                                Verdict VerdictOnTheDisplayedVocab,
                                ParsingCaseOutcome Outcome,
                                int DifferencesCounted,
                                IReadOnlyList<string> Differences)
{
   /// <summary><paramref name="compared"/> as it's listed, with at most <paramref name="maxDifferencesListed"/> of its differences.</summary>
   public static ParsingCaseListed Of(ComparedParsingCase compared, int maxDifferencesListed) =>
      new(compared.Sentence.GetId().ToString(),
          compared.Sentence.GetQuestion(),
          compared.Case.Name,
          compared.Case.Why,
          compared.Case.VerdictOnTheDisplayedVocab,
          compared.Comparison.Outcome,
          compared.Comparison.Differences.Count,
          [.. compared.Comparison.Differences.Take(maxDifferencesListed)]);
}
