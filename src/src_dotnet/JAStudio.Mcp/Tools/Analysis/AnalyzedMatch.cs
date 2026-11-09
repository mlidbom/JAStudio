using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>A match of a candidate word, as <see cref="AnalysisTools.AnalyzeSentence"/> reports it: what it matched, and whether and why it's used.</summary>
/// <param name="MatchedTo"><c>vocab</c>, <c>dictionary</c> for a dictionary entry when no vocab decides, or <c>missing</c> for a token nothing has.</param>
/// <param name="ParsedForm">The form the match puts in the sentence's parsing result.</param>
/// <param name="VocabId">The id of the vocab matched, for a vocab match.</param>
/// <param name="VocabQuestion">The question of the vocab matched, for a vocab match.</param>
/// <param name="IsValid">Whether the match is valid: a valid match is in the sentence's stored parsing result.</param>
/// <param name="IsDisplayed">Whether the sentence breakdown displays the match, when its candidate word is the one displayed at its place.</param>
/// <param name="FailureReasons">Why the match isn't valid.</param>
/// <param name="HidingReasons">Why the match isn't displayed.</param>
public record AnalyzedMatch(
   string MatchedTo,
   string ParsedForm,
   string? VocabId,
   string? VocabQuestion,
   string Answer,
   bool IsValid,
   bool IsDisplayed,
   IReadOnlyList<string> FailureReasons,
   IReadOnlyList<string> HidingReasons)
{
   public static AnalyzedMatch Of(Match match) =>
      new(MatchedToOf(match),
          match.ParsedForm,
          (match as VocabMatch)?.Vocab.GetId().ToString(),
          (match as VocabMatch)?.Vocab.GetQuestion(),
          match.Answer,
          match.IsValid,
          match.IsDisplayed,
          match.FailureReasons,
          match.HidingReasons);

   static string MatchedToOf(Match match) => match switch
   {
      VocabMatch => "vocab",
      DictionaryMatch => "dictionary",
      MissingMatch => "missing",
      _ => throw new ArgumentOutOfRangeException(nameof(match), match.GetType(), "A kind of match the analysis doesn't make.")
   };
}
