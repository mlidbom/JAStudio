using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

static class AnalysisConstants
{
   static readonly HashSet<char> RealQuoteCharacters = ['「', '」', '"'];
   public static readonly HashSet<string> PseudoQuoteCharacters = ["と", "って"];
   public static readonly HashSet<string> AllQuoteCharacters;

   public static readonly HashSet<char> SpaceCharacters = [' ', '　', '\t', StringExtensions.InvisibleSpace];

   public static readonly HashSet<char> QuestionMarks = ['？', '?'];
   public static readonly HashSet<char> Periods = ['.', '。', '｡'];
   public static readonly HashSet<char> Commas = [',', '、'];
   public static readonly HashSet<char> Tilde = ['～', '~'];
   public static readonly HashSet<char> Exclamations = ['!']; // TODO: Full-width exclamation mark?

   public static readonly HashSet<char> AllPunctuationCharacters;
   public static readonly HashSet<char> SentenceStartCharacters;
   public static readonly HashSet<string> SentenceEndCharacters;
   public static readonly HashSet<char> NoiseCharacters;

   public static readonly HashSet<string> PassiveVerbEndings = ["あれる", "られる", "される"];
   public static readonly HashSet<string> CausativeVerbEndings = ["あせる", "させる", "あす", "さす"];

   static AnalysisConstants()
   {
      AllQuoteCharacters = new HashSet<string>(RealQuoteCharacters.Select(c => c.ToString()));
      AllQuoteCharacters.UnionWith(PseudoQuoteCharacters);

      AllPunctuationCharacters = new HashSet<char>(RealQuoteCharacters);
      AllPunctuationCharacters.UnionWith(QuestionMarks);
      AllPunctuationCharacters.UnionWith(Periods);
      AllPunctuationCharacters.UnionWith(Commas);
      AllPunctuationCharacters.UnionWith(Exclamations);
      AllPunctuationCharacters.UnionWith(Tilde);
      AllPunctuationCharacters.UnionWith([':', ';', '/', '|']);

      SentenceStartCharacters = new HashSet<char>(RealQuoteCharacters);
      SentenceStartCharacters.UnionWith(SpaceCharacters);
      SentenceStartCharacters.UnionWith(QuestionMarks);
      SentenceStartCharacters.UnionWith(Periods);

      SentenceEndCharacters = new HashSet<string>(AllQuoteCharacters);
      SentenceEndCharacters.UnionWith(SpaceCharacters.Select(c => c.ToString()));
      SentenceEndCharacters.UnionWith(QuestionMarks.Select(c => c.ToString()));
      SentenceEndCharacters.UnionWith(Periods.Select(c => c.ToString()));

      NoiseCharacters = new HashSet<char>(AllPunctuationCharacters);
      NoiseCharacters.UnionWith(SpaceCharacters);
   }
}
