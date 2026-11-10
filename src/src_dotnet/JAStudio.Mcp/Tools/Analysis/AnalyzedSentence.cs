using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>What <see cref="AnalysisTools.AnalyzeSentence"/> reports of a sentence's <see cref="TextAnalysis"/>.</summary>
/// <param name="Text">The sentence analyzed.</param>
/// <param name="DisplayedWords">The words the sentence breakdown shows, in order: the forms of the displayed matches.</param>
/// <param name="Tokens">The tokens the analysis works on: the tokenizer's, after JAStudio's preprocessing, which splits some of them.</param>
/// <param name="CandidateWords">Every candidate word that got a match, in order of where it starts, the longest first.</param>
public record AnalyzedSentence(string Text, IReadOnlyList<string> DisplayedWords, IReadOnlyList<AnalyzedToken> Tokens, IReadOnlyList<AnalyzedCandidateWord> CandidateWords)
{
   public static AnalyzedSentence Of(TextAnalysis analysis) =>
      new(analysis.Text,
          [.. analysis.DisplayMatches.OrderBy(it => it.StartIndex).Select(it => it.ParsedForm)],
          [.. analysis.PreProcessedTokens.Select(AnalyzedToken.Of)],
          [.. CandidateWordsWithMatches(analysis).Select(AnalyzedCandidateWord.Of)]);

   static IEnumerable<CandidateWordVariant> CandidateWordsWithMatches(TextAnalysis analysis) =>
      analysis.Locations
              .SelectMany(it => it.CandidateWords)
              .SelectMany(it => it.Variants)
              .Where(it => it.Matches.Count > 0)
              .OrderBy(it => it.StartIndex)
              .ThenByDescending(it => it.Word.LocationCount);
}
