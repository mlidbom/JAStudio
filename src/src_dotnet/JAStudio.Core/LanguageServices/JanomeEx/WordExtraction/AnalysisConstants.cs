using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

static class AnalysisConstants
{
   static readonly HashSet<string> RealQuoteCharacters = ["「", "」", "\""];
   public static readonly HashSet<string> PseudoQuoteCharacters = ["と", "って"];
   public static readonly HashSet<string> AllQuoteCharacters;

   public static readonly HashSet<string> SpaceCharacters = [" ", "　", "\t", StringExtensions.InvisibleSpace];

   public static readonly HashSet<string> QuestionMarks = ["？", "?"];
   public static readonly HashSet<string> Periods = [".", "。", "｡"];
   public static readonly HashSet<string> Commas = [",", "、"];
   public static readonly HashSet<string> Tilde = ["～", "~"];
   public static readonly HashSet<string> Exclamations = ["!"]; // TODO: Full-width exclamation mark?

   public static readonly HashSet<string> AllPunctuationCharacters;
   public static readonly HashSet<string> SentenceStartCharacters;
   public static readonly HashSet<string> SentenceEndCharacters;
   public static readonly HashSet<string> NoiseCharacters;

   public static readonly HashSet<string> PassiveVerbEndings = ["あれる", "られる", "される"];
   public static readonly HashSet<string> CausativeVerbEndings = ["あせる", "させる", "あす", "さす"];

   static AnalysisConstants()
   {
      AllQuoteCharacters = new HashSet<string>(RealQuoteCharacters);
      AllQuoteCharacters.UnionWith(PseudoQuoteCharacters);

      AllPunctuationCharacters = new HashSet<string>(RealQuoteCharacters);
      AllPunctuationCharacters.UnionWith(QuestionMarks);
      AllPunctuationCharacters.UnionWith(Periods);
      AllPunctuationCharacters.UnionWith(Commas);
      AllPunctuationCharacters.UnionWith(Exclamations);
      AllPunctuationCharacters.UnionWith(Tilde);
      AllPunctuationCharacters.UnionWith([":", ";", "/", "|"]);

      SentenceStartCharacters = new HashSet<string>(RealQuoteCharacters);
      SentenceStartCharacters.UnionWith(SpaceCharacters);
      SentenceStartCharacters.UnionWith(QuestionMarks);
      SentenceStartCharacters.UnionWith(Periods);

      SentenceEndCharacters = new HashSet<string>(AllQuoteCharacters);
      SentenceEndCharacters.UnionWith(SpaceCharacters);
      SentenceEndCharacters.UnionWith(QuestionMarks);
      SentenceEndCharacters.UnionWith(Periods);

      NoiseCharacters = new HashSet<string>(AllPunctuationCharacters);
      NoiseCharacters.UnionWith(SpaceCharacters);
   }
}
