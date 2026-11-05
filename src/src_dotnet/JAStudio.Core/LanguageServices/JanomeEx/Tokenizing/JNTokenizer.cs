using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Internals.SystemCE;
using Compze.Threading;
using MeCab;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

public sealed class JNTokenizer
{
   static readonly LazyCE<JNTokenizer> Instance = new(() => new JNTokenizer());

   public static JNTokenizer GetInstance() => Instance.Value;

   static readonly HashSet<string> CharactersThatMayConfuseTokenizerSoWeReplaceThemWithOrdinaryFullWidthSpaces =
      ["!", "！", "|", "（", "）", " "];

   readonly MeCabTagger _tagger;
   readonly IMonitor _monitor = IMonitor.New();

   JNTokenizer()
   {
      // MeCab defaults to looking for dic/ relative to CWD which doesn't work when hosted inside Anki.
      // Resolve relative to the assembly location instead.
      var assemblyDir = Path.GetDirectoryName(typeof(JNTokenizer).Assembly.Location)
                        ?? throw new InvalidOperationException("Cannot determine assembly directory");
      var dicDir = Path.Combine(assemblyDir, "dic");

      var param = new MeCabParam { DicDir = dicDir };
      _tagger = MeCabTagger.Create(param);
   }

   public JNTokenizedText Tokenize(string text)
   {
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
      var tokens = TokenizeToTokenList(tokenizerInput);
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
      _monitor.Locked(() =>
      {
         var result = new List<JNToken>();

         foreach(var node in _tagger.ParseToNodes(text))
         {
            if(node.CharType == 0) continue; // Skip BOS/EOS nodes

            var features = node.Feature.Split(',');

            // IPAdic feature CSV format: POS,sub1,sub2,sub3,inflType,inflForm,baseForm,reading,phonetic
            var partOfSpeech = string.Join(",", features.Take(4));
            var inflType = GetFeatureOrEmpty(features, 4);
            var inflForm = GetFeatureOrEmpty(features, 5);
            var baseForm = GetFeatureOrEmpty(features, 6);
            var reading = GetFeatureOrEmpty(features, 7);
            var phonetic = GetFeatureOrEmpty(features, 8);

            var partsOfSpeech = JNPartsOfSpeech.Fetch(partOfSpeech);

            result.Add(new JNToken(
                          partsOfSpeech,
                          baseForm: Sanitize(baseForm),
                          surface: Sanitize(node.Surface),
                          inflectionType: Sanitize(inflType),
                          inflectedForm: Sanitize(inflForm),
                          reading: Sanitize(reading),
                          phonetic: Sanitize(phonetic)
                       ));
         }

         return result;
      });

   static string GetFeatureOrEmpty(string[] features, int index) =>
      index < features.Length ? features[index] : "";

   static string Sanitize(string? value) =>
      string.IsNullOrEmpty(value) ? "" : value.Replace("\n", " ").Replace("\r", " ");
}
