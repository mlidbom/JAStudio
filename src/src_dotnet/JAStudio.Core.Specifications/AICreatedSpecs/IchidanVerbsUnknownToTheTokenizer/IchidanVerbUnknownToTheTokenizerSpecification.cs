using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

namespace JAStudio.Core.Specifications.AICreatedSpecs.IchidanVerbsUnknownToTheTokenizer;

/// <summary>
/// Base for the specifications of how text containing an ichidan verb missing from the tokenizer's dictionary is tokenized.<br/>
/// 磨げる, the potential form of 磨ぐ, is missing from the dictionary; 泳げる, the potential form of 泳ぐ, is in it. How such text is displayed<br/>
/// is specified by parsing cases (<see cref="LanguageServices.ParsingCases.ParsingCase"/>) of the same sentences.
/// </summary>
public abstract class IchidanVerbUnknownToTheTokenizerSpecification : IAIGeneratedSpec
{
   protected const string StemOfAPotentialFormUnknownToTheTokenizer = "磨げ";
   protected const string StemOfAPotentialFormKnownToTheTokenizer = "泳げ";

   protected static List<JNToken> TokensOf(string text) => JNTokenizer.GetInstance().Tokenize(text).Tokens;

   /// <summary>Everything about the tokens of <paramref name="text"/> that the analysis uses, except their readings.</summary>
   protected static List<string> TokenDescriptionsOf(string text) =>
      [..TokensOf(text).Select(it => $"{it.Surface}|{it.BaseForm}|{it.InflectionType}|{it.InflectedForm}|{it.PartsOfSpeech}")];

   /// <summary>
   /// <paramref name="descriptions"/> of text containing the potential form known to the tokenizer,<br/>
   /// rewritten to be about the potential form unknown to it instead.
   /// </summary>
   protected static List<string> AsIfAboutThePotentialFormUnknownToTheTokenizer(IEnumerable<string> descriptions) =>
      [..descriptions.Select(it => it.Replace("泳", "磨"))];
}
