using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.MeCab;

namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing;

public sealed class MeCabTokenizer
{
   static readonly MeCabTokenizer Instance = new();

   public static MeCabTokenizer GetInstance() => Instance;

   static readonly HashSet<string> CharactersThatMayConfuseTokenizerSoWeReplaceThemWithOrdinaryFullWidthSpaces =
      ["!", "！", "|", "（", "）", " "];

   MeCabTokenizer() {}

   public MeCabTokenizedText Tokenize(string text)
   {
      var dictionary = TokenizerDictionary.UniDic;
      return new MeCabTokenizedText(text, TokenizeToLinkedTokenList(ReplaceCharactersThatConfuseTheTokenizer(text), dictionary), dictionary);
   }

   static string ReplaceCharactersThatConfuseTheTokenizer(string text)
   {
      // The tokenizer does not fully understand that invisible spaces are word separators,
      // so we replace them with a sentinel token since they are not anything that should need to be parsed
      var tokenizerInput = text.Replace(StringExtensions.InvisibleSpace.ToString(), MeCabToken.SplitterTokenText);

      foreach(var character in CharactersThatMayConfuseTokenizerSoWeReplaceThemWithOrdinaryFullWidthSpaces)
      {
         tokenizerInput = tokenizerInput.Replace(character, "　");
      }

      return tokenizerInput;
   }

   /// <summary>
   /// The tokens the <paramref name="dictionary"/> cuts <paramref name="tokenizerInput"/> into, each translated from MeCab's and nothing more, linked as<br/>
   /// neighbours. Correcting and splitting them is the pre-processing stage's (<see cref="PreProcessingStage.PreProcessingStage"/>), which leaves these as they are.
   /// </summary>
   static List<MeCabToken> TokenizeToLinkedTokenList(string tokenizerInput, TokenizerDictionary dictionary)
   {
      List<MeCabToken> tokens = [.. dictionary.Tokenize(tokenizerInput).Select(TokenOf)];
      MeCabToken.LinkAsNeighbours(tokens);
      return tokens;
   }

   /// <summary>MeCab's <paramref name="token"/> as a MeCabToken: its tags looked up in JAStudio's registries, its texts as they are.</summary>
   static MeCabToken TokenOf(RawMeCabToken token) =>
      new(UniDicPartOfSpeech.Fetch(token.PartOfSpeech),
          baseForm: Sanitize(token.BaseForm),
          surface: Sanitize(token.Surface),
          inflectionType: UniDicInflectionTypes.GetByName(token.InflectionType),
          inflectedForm: UniDicInflectionForms.GetByName(token.InflectionForm),
          reading: ValueOrEmpty(token.Reading),
          phonetic: ValueOrEmpty(token.Pronunciation),
          lemma: ValueOrEmpty(token.Lemma));

   /// <summary><paramref name="value"/>, or nothing for MeCab's "nothing", <c>*</c>, which a word the tokenizer dictionary lacks has for its reading.</summary>
   static string ValueOrEmpty(string? value) => value is null or RawMeCabToken.Nothing ? "" : Sanitize(value);

   static string Sanitize(string value) => value.Replace("\n", " ").Replace("\r", " ");
}
