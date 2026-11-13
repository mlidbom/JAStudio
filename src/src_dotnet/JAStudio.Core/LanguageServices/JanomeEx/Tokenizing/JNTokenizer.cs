using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.TokenizerDictionaries;
using MeCab;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

public sealed class JNTokenizer
{
   static readonly JNTokenizer Instance = new();

   public static JNTokenizer GetInstance() => Instance;

   static readonly HashSet<string> CharactersThatMayConfuseTokenizerSoWeReplaceThemWithOrdinaryFullWidthSpaces =
      ["!", "！", "|", "（", "）", " "];

   JNTokenizer() {}

   public JNTokenizedText Tokenize(string text)
   {
      //refactor: the tokenizer is MeCab, with the IPADIC dictionary, but the namespace JanomeEx and the types JNTokenizer, JNToken,
      //   JNTokenizedText, JNPartsOfSpeech and JNPOS are named for Janome, the Python tokenizer JAStudio used before. So is the comment on
      //   JNTokenizedText.RawTokens. The names should say what they are.
      var tokenizerInput = ReplaceCharactersThatConfuseTheTokenizer(text);
      var tokens = TokenizeToLinkedTokenList(tokenizerInput);

      if(MisTokenizedQuotativeDatte.AnyIn(tokens))
      {
         tokens = TokenizeToLinkedTokenList(MisTokenizedQuotativeDatte.SplitDaFromTteInEach(tokenizerInput, tokens));
      }

      return new JNTokenizedText(text, tokens);
   }

   static string ReplaceCharactersThatConfuseTheTokenizer(string text)
   {
      // The tokenizer does not fully understand that invisible spaces are word separators,
      // so we replace them with a sentinel token since they are not anything that should need to be parsed
      var tokenizerInput = text.Replace(StringExtensions.InvisibleSpace.ToString(), JNToken.SplitterTokenText);

      foreach(var character in CharactersThatMayConfuseTokenizerSoWeReplaceThemWithOrdinaryFullWidthSpaces)
      {
         tokenizerInput = tokenizerInput.Replace(character, "　");
      }

      return tokenizerInput;
   }

   List<JNToken> TokenizeToLinkedTokenList(string tokenizerInput)
   {
      var tokens = MisTokenizedIchidanVerbUnknownToTheTokenizer.CorrectEachIn(TokenizeToTokenList(tokenizerInput));
      LinkNeighbouringTokens();
      return tokens;

      void LinkNeighbouringTokens()
      {
         for(var i = 0; i < tokens.Count; i++)
         {
            if(i > 0)
            {
               tokens[i].Previous = tokens[i - 1];
            }

            if(i < tokens.Count - 1)
            {
               tokens[i].Next = tokens[i + 1];
            }
         }
      }
   }

   List<JNToken> TokenizeToTokenList(string text) =>
      TokenizerDictionary.TheBundledIpadic.LendATaggerTo(tagger => tagger.ParseToNodes(text)
                                                                         .Where(it => !IsTheBeginningOrEndOfTheSentence(it))
                                                                         .Select(TokenOf)
                                                                         .ToList());

   static bool IsTheBeginningOrEndOfTheSentence(MeCabNode node) =>
      //bug: this is true of more than the BOS/EOS nodes. MeCab gives every node of a character in its DEFAULT character category CharType 0,
      //   so such a token is dropped from the sentence: 雨だ♪★😀ね tokenizes as 雨, だ, ♪★, ね, without 😀 (analyze_sentence, 2026-11-12).
      //   MeCabNode.Stat tells the BOS/EOS nodes apart. Fixing it changes what sentences parse as, so it needs a full rebuild and its analysis.
      node.CharType == 0;

   static JNToken TokenOf(MeCabNode node)
   {
      var features = node.Feature.Split(',');

      // IPAdic feature CSV format: POS,sub1,sub2,sub3,inflType,inflForm,baseForm,reading,phonetic
      var partOfSpeech = string.Join(",", features.Take(4));
      var inflType = GetFeatureOrEmpty(features, 4);
      var inflForm = GetFeatureOrEmpty(features, 5);
      var baseForm = GetFeatureOrEmpty(features, 6);
      var reading = GetFeatureOrEmpty(features, 7);
      var phonetic = GetFeatureOrEmpty(features, 8);

      return new JNToken(
         JNPartsOfSpeech.Fetch(partOfSpeech),
         baseForm: Sanitize(baseForm),
         surface: Sanitize(node.Surface),
         inflectionType: Sanitize(inflType),
         inflectedForm: Sanitize(inflForm),
         reading: Sanitize(reading),
         phonetic: Sanitize(phonetic));
   }

   static string GetFeatureOrEmpty(string[] features, int index) =>
      index < features.Length ? features[index] : "";

   static string Sanitize(string? value) =>
      string.IsNullOrEmpty(value) ? "" : value.Replace("\n", " ").Replace("\r", " ");
}
