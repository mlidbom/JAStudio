using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>
/// What <see cref="AnalysisTools.CountTheCandidateWords"/> reports: how many candidate words analyzing every sentence of the corpus built, how many<br/>
/// of them could get a match, and how many did, in all and by how many tokens each spans.
/// </summary>
/// <param name="Sentences">How many sentences were analyzed.</param>
/// <param name="ByTokensSpanned">The counts of the candidate words spanning each number of tokens, from one token up.</param>
public record CandidateWordCounts(int Sentences, IReadOnlyList<CandidateWordsSpanningTokens> ByTokensSpanned)
{
   /// <summary>How many candidate words the analysis built in all.</summary>
   public int Built => ByTokensSpanned.Sum(it => it.Built);

   /// <summary>How many of them have a form a vocab or a dictionary entry has: the only ones that can get a match.</summary>
   public int Known => ByTokensSpanned.Sum(it => it.Known);

   /// <summary>How many of them got a valid match.</summary>
   public int Valid => ByTokensSpanned.Sum(it => it.Valid);
}
