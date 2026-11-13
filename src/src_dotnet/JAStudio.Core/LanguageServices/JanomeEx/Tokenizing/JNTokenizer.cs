using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.TokenizerDictionaries;

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
      //refactor: the tokenizer is MeCab, with the UniDic dictionary, but the namespace JanomeEx and the types JNTokenizer, JNToken,
      //   JNTokenizedText, JNPartsOfSpeech and JNPOS are named for Janome, the Python tokenizer JAStudio used before. So is the comment on
      //   JNTokenizedText.RawTokens. The names should say what they are.
      return new JNTokenizedText(text, TokenizeToLinkedTokenList(ReplaceCharactersThatConfuseTheTokenizer(text)));
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

   static List<JNToken> TokenizeToLinkedTokenList(string tokenizerInput)
   {
      var dictionary = TokenizerDictionary.UniDic;
      List<JNToken> tokens = [.. dictionary.Tokenize(tokenizerInput).Select(TokenOf)];
      tokens = MisTokenizedIchidanVerbUnknownToTheTokenizer.CorrectEachIn(tokens);
      tokens = RaNukiPotentialTheTokenizerGivesAsOneToken.SplitEachIn(tokens, dictionary);
      tokens = VolitionalTheTokenizerGivesAsOneToken.SplitEachIn(tokens);
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

   static JNToken TokenOf(MeCabToken token) =>
      new(JNPartsOfSpeech.Fetch(token.PartOfSpeech),
          baseForm: Sanitize(token.BaseForm),
          surface: Sanitize(token.Surface),
          inflectionType: InflectionTypes.GetByName(token.InflectionType),
          inflectedForm: InflectionForms.GetByName(token.InflectionForm),
          reading: ValueOrEmpty(token.Reading),
          phonetic: ValueOrEmpty(token.Pronunciation),
          lemma: ValueOrEmpty(token.Lemma));

   /// <summary><paramref name="value"/>, or nothing for MeCab's "nothing", <c>*</c>, which a word the tokenizer dictionary lacks has for its reading.</summary>
   static string ValueOrEmpty(string? value) => value is null or MeCabToken.Nothing ? "" : Sanitize(value);

   static string Sanitize(string value) => value.Replace("\n", " ").Replace("\r", " ");
}
