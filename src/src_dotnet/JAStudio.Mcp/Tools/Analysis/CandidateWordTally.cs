using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>
/// A running count of candidate words, by how many tokens each spans, for <see cref="AnalysisTools.CountTheCandidateWords"/>: each thread counts<br/>
/// the candidate words of the sentences it analyzes in a tally of its own, and the tallies are then added up.
/// </summary>
sealed class CandidateWordTally
{
   readonly Dictionary<int, CandidateWordsSpanningTokens> _byTokensSpanned = [];

   /// <summary>Counts every candidate word <paramref name="analysis"/> built, at every location of its sentence.</summary>
   public CandidateWordTally AddEveryCandidateWordOf(TextAnalysis analysis)
   {
      foreach(var word in analysis.Locations.SelectMany(it => it.CandidateWords))
      {
         _byTokensSpanned[word.LocationCount] = CountsOfWordsSpanning(word.LocationCount).With(word);
      }

      return this;
   }

   /// <summary>Adds the counts of <paramref name="other"/> to this tally's.</summary>
   public CandidateWordTally Add(CandidateWordTally other)
   {
      foreach(var counts in other._byTokensSpanned.Values)
      {
         _byTokensSpanned[counts.Tokens] = CountsOfWordsSpanning(counts.Tokens).Plus(counts);
      }

      return this;
   }

   /// <summary>The counts, from the candidate words of a single token to the longest compounds.</summary>
   public IReadOnlyList<CandidateWordsSpanningTokens> ByTokensSpanned() => [.. _byTokensSpanned.Values.OrderBy(it => it.Tokens)];

   CandidateWordsSpanningTokens CountsOfWordsSpanning(int tokens) => _byTokensSpanned.GetValueOrDefault(tokens) ?? CandidateWordsSpanningTokens.None(tokens);
}
