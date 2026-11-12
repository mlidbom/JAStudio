using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// The tokenizer's dictionary, IPADIC, gives each word an inflection type, such as 五段・ラ行, and an inflected form, such as 連用形. JAStudio must know<br/>
/// every one the dictionary has: a token of one it doesn't know fails to tokenize, and so fails the whole sentence's analysis. Classical types such<br/>
/// as 下二・ガ行 did, found when the sentences of anime subtitles were first analyzed in bulk (2026-11-11).
/// </summary>
public class When_the_tokenizer_gives_a_word_of_any_inflection_type_its_dictionary_has : IAIGeneratedSpec
{
   /// <summary>
   /// The features of every word in the compiled dictionary, as MeCab stores them: comma-separated text, the inflection type the fifth field and<br/>
   /// the inflected form the sixth, among the dictionary's other, binary, data.
   /// </summary>
   static readonly IReadOnlyList<string[]> FeaturesOfEveryWord = ReadTheFeaturesOfEveryWord();

   readonly JNTokenizer _tokenizer = JNTokenizer.GetInstance();

   [XF] public void JAStudio_knows_every_inflection_type_in_the_dictionary() =>
      FeaturesOfEveryWord.Select(it => it[4]).Distinct().Where(it => !InflectionTypes.IsKnown(it)).Must().BeEmpty();

   [XF] public void JAStudio_knows_every_inflected_form_in_the_dictionary() =>
      FeaturesOfEveryWord.Select(it => it[5]).Distinct().Where(it => !InflectionForms.IsKnown(it)).Must().BeEmpty();

   [XF] public void the_dictionary_was_read_with_its_hundreds_of_thousands_of_words() => FeaturesOfEveryWord.Count.Must().BeGreaterThan(300_000);

   [XF] public void a_sentence_with_a_classical_lower_bigrade_verb_is_tokenized() =>
      _tokenizer.Tokenize("君に捧ぐ").Tokens.Select(it => it.InflectionType.Name).Must().Contain("下二・ガ行");

   static IReadOnlyList<string[]> ReadTheFeaturesOfEveryWord()
   {
      var dictionary = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "dic", "sys.dic"));
      return [.. SplitAtZeroBytes(dictionary).Select(TextOrNull).OfType<string>().Select(it => it.Split(',')).Where(IsAWordsFeatures)];

      // A word's features start with its part of speech, in Japanese: kana or kanji.
      static bool IsAWordsFeatures(string[] fields) => fields.Length >= 7 && fields[0].Any(character => character is >= '\u3040' and <= '\u9FFF');
   }

   static IEnumerable<ArraySegment<byte>> SplitAtZeroBytes(byte[] bytes)
   {
      var start = 0;
      for(var index = 0; index < bytes.Length; index++)
      {
         if(bytes[index] != 0) continue;
         if(index > start) yield return new ArraySegment<byte>(bytes, start, index - start);
         start = index + 1;
      }
   }

   /// <summary><paramref name="bytes"/> as text, or null when they aren't UTF-8 text, as the dictionary's binary data's runs of bytes aren't: they're no word's features.</summary>
   static string? TextOrNull(ArraySegment<byte> bytes) => System.Text.Unicode.Utf8.IsValid(bytes) ? Encoding.UTF8.GetString(bytes) : null;
}
