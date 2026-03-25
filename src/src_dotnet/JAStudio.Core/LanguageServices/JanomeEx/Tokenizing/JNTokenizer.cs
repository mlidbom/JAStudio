using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Utilities.SystemCE;
using MeCab;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

public sealed class JNTokenizer
{
   public const string Version = "mecab_1";

   static readonly LazyCE<JNTokenizer> Instance = new(() => new JNTokenizer());

   public static JNTokenizer GetInstance() => Instance.Value;

   static readonly HashSet<string> CharactersThatMayConfuseTokenizerSoWeReplaceThemWithOrdinaryFullWidthSpaces =
      ["!", "！", "|", "（", "）", " "];

   /// <summary>The invisible space character (U+200B) used as field separator in the serialized token string.</summary>
   const string FieldSeparator = StringExtensions.InvisibleSpace;

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

   public JNTokenizeResult Tokenize(string text, string? cachedSerializedTokens = null)
   {
      // The tokenizer does not fully understand that invisible spaces are word separators,
      // so we replace them with a sentinel token since they are not anything that should need to be parsed
      var sanitizedText = text.Replace(StringExtensions.InvisibleSpace, JNToken.SplitterTokenText);

      foreach(var character in CharactersThatMayConfuseTokenizerSoWeReplaceThemWithOrdinaryFullWidthSpaces)
      {
         sanitizedText = sanitizedText.Replace(character, "　");
      }

      // Use cached serialized tokens if available, otherwise tokenize with MeCab
      var serialized = cachedSerializedTokens ?? TokenizeToSerializedString(sanitizedText);

      var jnTokens = ParseSerializedTokens(serialized);

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

      return new JNTokenizeResult(new JNTokenizedText(text, jnTokens), serialized);
   }

   string TokenizeToSerializedString(string text)
   {
      lock(_lock)
      {
         var lines = new List<string>();

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

            var fields = string.Join(FieldSeparator, [
               partOfSpeech,
               Sanitize(baseForm),
               Sanitize(node.Surface),
               Sanitize(inflType),
               Sanitize(inflForm),
               Sanitize(reading),
               Sanitize(phonetic),
               "" // node_type — not actively used in the codebase
            ]);

            lines.Add(fields);
         }

         return string.Join("\n", lines);
      }
   }

   static string GetFeatureOrEmpty(string[] features, int index) =>
      index < features.Length ? features[index] : "";

   static string Sanitize(string? value) =>
      string.IsNullOrEmpty(value) ? "" : value.Replace("\n", " ").Replace("\r", " ");

   static List<JNToken> ParseSerializedTokens(string serialized)
   {
      if(string.IsNullOrEmpty(serialized))
         return [];

      var lines = serialized.Split('\n');
      var result = new List<JNToken>(lines.Length);

      foreach(var line in lines)
      {
         if(line.Length == 0) continue;

         var fields = line.Split(FieldSeparator);
         // Fields: 0=part_of_speech, 1=base_form, 2=surface, 3=infl_type, 4=infl_form, 5=reading, 6=phonetic, 7=node_type
         var partsOfSpeech = JNPartsOfSpeech.Fetch(fields[0]);

         result.Add(new JNToken(
                       partsOfSpeech,
                       baseForm: fields[1],
                       surface: fields[2],
                       inflectionType: fields[3],
                       inflectedForm: fields[4],
                       reading: fields[5],
                       phonetic: fields[6],
                       nodeType: fields[7]
                    ));
      }

      return result;
   }
}
