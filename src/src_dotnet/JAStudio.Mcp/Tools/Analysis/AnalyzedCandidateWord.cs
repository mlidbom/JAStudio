using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>A candidate word a sentence's analysis found a match for, as <see cref="AnalysisTools.AnalyzeSentence"/> reports it.</summary>
/// <param name="Form">The candidate word's form: its tokens' surfaces, or for its base variant, the last token's base form.</param>
/// <param name="Variant"><c>surface</c> when <see cref="Form"/> is the tokens' surfaces, <c>base</c> when it ends in the last token's base form.</param>
/// <param name="StartIndex">The index in the sentence of the candidate word's first character.</param>
/// <param name="TokenCount">How many tokens the candidate word spans: more than one for a compound.</param>
/// <param name="IsDisplayWord">Whether the analysis chose this candidate word to display at its place in the sentence, over the others there.</param>
public record AnalyzedCandidateWord(string Form, string Variant, int StartIndex, int TokenCount, bool IsDisplayWord, IReadOnlyList<AnalyzedMatch> Matches)
{
   public static AnalyzedCandidateWord Of(CandidateWordVariant candidate) =>
      new(candidate.Form,
          candidate.IsSurface ? "surface" : "base",
          candidate.StartIndex,
          candidate.Word.LocationCount,
          candidate.Word.Analysis.DisplayWordVariants.Contains(candidate),
          [.. candidate.Matches.Select(AnalyzedMatch.Of)]);
}
