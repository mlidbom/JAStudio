using JAStudio.Core.LanguageServices.Parsing.WordExtraction;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>How many of the candidate words the analysis of the corpus built span a number of tokens, as <see cref="AnalysisTools.CountTheCandidateWords"/> reports them.</summary>
/// <param name="Tokens">How many tokens each of these candidate words spans: 1 for a word of a single token, more for a compound.</param>
/// <param name="Built">How many the analysis built.</param>
/// <param name="Known">How many have a form a vocab or a dictionary entry has (<see cref="CandidateWord.IsWord"/>). Only these can get a match to either.</param>
/// <param name="Valid">How many got a valid match (<see cref="CandidateWord.HasValidWords"/>).</param>
public record CandidateWordsSpanningTokens(int Tokens, int Built, int Known, int Valid)
{
   /// <summary>No candidate words spanning <paramref name="tokens"/> tokens yet.</summary>
   public static CandidateWordsSpanningTokens None(int tokens) => new(tokens, 0, 0, 0);

   /// <summary>These counts with <paramref name="word"/> counted too.</summary>
   public CandidateWordsSpanningTokens With(CandidateWord word) =>
      new(Tokens, Built + 1, Known + (word.IsWord ? 1 : 0), Valid + (word.HasValidWords() ? 1 : 0));

   /// <summary>These counts and <paramref name="other"/>'s, of candidate words spanning as many tokens, added up.</summary>
   public CandidateWordsSpanningTokens Plus(CandidateWordsSpanningTokens other) =>
      new(Tokens, Built + other.Built, Known + other.Known, Valid + other.Valid);
}
