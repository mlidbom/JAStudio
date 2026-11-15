using System;
using System.ComponentModel;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences.ParsingCases;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.ParsingCases;

/// <summary>
/// The tools about the corpus's parsing cases (<see cref="ParsingCase"/>): the sentences marked as parsing cases, which a change to parsing is<br/>
/// measured against.
/// </summary>
[McpServerToolType]
public class ParsingCaseTools
{
   /// <summary>How many parsing cases <see cref="ListTheParsingCases"/> lists unless told otherwise.</summary>
   public const int CasesListedByDefault = 50;

   /// <summary>How many differences <see cref="ListTheParsingCases"/> lists for each case unless told otherwise: a case whose sentence parses quite otherwise has dozens.</summary>
   public const int DifferencesListedForEachCaseByDefault = 10;

   readonly JPCollection _collection;

   public ParsingCaseTools(JPCollection collection) => _collection = collection;

   [McpServerTool(Name = "list_the_parsing_cases", ReadOnly = true, Idempotent = true)]
   [Description("""
                Parses every parsing case of the corpus now, and lists those that parse otherwise than they expect. A parsing case is a sentence
                marked as one, with the exact result it's expected to parse to: every parsed word, and its breakdown under every combination of the
                options that change it. A person's verdict is on the vocab the breakdown displays with every option off: verified, broken or
                unverified. Each case's outcome is one of:
                - Fails: the breakdown displays other vocab than a person verified as right. The one difference that is an error.
                - IsToReview: it parses otherwise than expected in what nobody verified. Suspect, for a person to review, and then to accept
                  (run_a_command_on_a_note with acceptWhatItParsesToAsAParsingCase) or to find wrong.
                - ParsesAsExpected.
                Reports how many cases there are of each outcome, then the cases, those that fail first, each with its sentence's id, its name,
                its why, its verdict and its differences, a line each. A sentence's name, why and verdict are fields that edit_a_note writes:
                parsingCase.name, parsingCase.why and parsingCase.verdictOnTheDisplayedVocab.
                """)]
   public ParsingCasesListed ListTheParsingCases(
      [Description("Whether to list the cases that parse as expected too. They're counted either way.")] bool includingThoseThatParseAsExpected = false,
      [Description("The most cases to list.")] int maxCasesListed = CasesListedByDefault,
      [Description("The most differences to list for each case. Every difference is counted either way.")] int maxDifferencesListedForEachCase = DifferencesListedForEachCaseByDefault)
   {
      var cases = _collection.Sentences.ParsingCasesCompared();
      var casesToList = cases.Where(it => includingThoseThatParseAsExpected || !it.Comparison.ParsesAsExpected)
                             .OrderBy(it => it.Comparison.Outcome)
                             .ThenBy(it => it.Case.Name, StringComparer.Ordinal)
                             .ThenBy(it => it.Sentence.GetQuestion(), StringComparer.Ordinal);

      return new ParsingCasesListed(cases.Count,
                                    cases.Count(it => it.Comparison.Fails),
                                    cases.Count(it => it.Comparison.IsToReview),
                                    cases.Count(it => it.Comparison.ParsesAsExpected),
                                    [.. casesToList.Take(maxCasesListed).Select(it => ParsingCaseListed.Of(it, maxDifferencesListedForEachCase))]);
   }
}
