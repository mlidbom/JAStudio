using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Internals.SystemCE;
using MeCab;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

public sealed class JNTokenizer
{
   static readonly LazyCE<JNTokenizer> Instance = new(() => new JNTokenizer());

   public static JNTokenizer GetInstance() => Instance.Value;

   static readonly HashSet<string> CharactersThatMayConfuseTokenizerSoWeReplaceThemWithOrdinaryFullWidthSpaces =
      ["!", "！", "|", "（", "）", " "];

   readonly MeCabTagger _tagger;
   readonly object _lock = new();

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
      // The tokenizer does not fully understand that invisible spaces are word separators,
      // so we replace them with a sentinel token since they are not anything that should need to be parsed
      var sanitizedText = text.Replace(StringExtensions.InvisibleSpace.ToString(), JNToken.SplitterTokenText);

      foreach(var character in CharactersThatMayConfuseTokenizerSoWeReplaceThemWithOrdinaryFullWidthSpaces)
      {
         sanitizedText = sanitizedText.Replace(character, "　");
      }

      var jnTokens = TokenizeToTokenList(sanitizedText);

      // Link tokens with previous/next pointers
      for(var i = 0; i < jnTokens.Count; i++)
      {
         if(i > 0)
         {
            jnTokens[i].Previous = jnTokens[i - 1];
         }

         if(i < jnTokens.Count - 1)
         {
            jnTokens[i].Next = jnTokens[i + 1];
         }
      }

      return new JNTokenizedText(text, jnTokens);
   }

   List<JNToken> TokenizeToTokenList(string text)
   {
      lock(_lock)
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
      }
   }

   static string GetFeatureOrEmpty(string[] features, int index) =>
      index < features.Length ? features[index] : "";

   static string Sanitize(string? value) =>
      string.IsNullOrEmpty(value) ? "" : value.Replace("\n", " ").Replace("\r", " ");
}
