using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.UI.Web.Sentence;

namespace JAStudio.Core.Specifications.AICreatedSpecs.IchidanVerbsUnknownToTheTokenizer;

/// <summary>
/// Base for the specifications of how the pre-processing stage corrects text containing an ichidan verb missing from the tokenizer's dictionary.<br/>
/// 磨げる, the potential form of 磨ぐ, is missing from the dictionary; 泳げる, the potential form of 泳ぐ, is in it. How such text is displayed<br/>
/// is specified by parsing cases (<see cref="LanguageServices.ParsingCases.ParsingCase"/>) of the same sentences.
/// </summary>
public abstract class IchidanVerbUnknownToTheTokenizerSpecification : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected const string StemOfAPotentialFormUnknownToTheTokenizer = "磨げ";
   protected const string StemOfAPotentialFormKnownToTheTokenizer = "泳げ";

   /// <summary>The tokens JAStudio's analysis works with for <paramref name="text"/>: the tokenizer's, as the pre-processing stage corrected and split them.</summary>
   protected List<IAnalysisToken> PreProcessedTokensOf(string text) => TextAnalysis.FromText(GetService<AnalysisServices>(), text).PreProcessedTokens;

   /// <summary>
   /// Everything about the pre-processed tokens of <paramref name="text"/> that the analysis uses, except their readings: each token's surface, base<br/>
   /// form and facts, with the parts of speech and inflection of the token it came from.
   /// </summary>
   protected List<string> TokenDescriptionsOf(string text) =>
      [..PreProcessedTokensOf(text).Select(it => $"{it.Surface}|{it.BaseForm}|{new TokenViewModel(it).TrueFlagsListed}|{it.SourceToken.InflectionType}|{it.SourceToken.InflectedForm}|{it.SourceToken.PartsOfSpeech}")];

   /// <summary>
   /// <paramref name="descriptions"/> of text containing the potential form known to the tokenizer,<br/>
   /// rewritten to be about the potential form unknown to it instead.
   /// </summary>
   protected static List<string> AsIfAboutThePotentialFormUnknownToTheTokenizer(IEnumerable<string> descriptions) =>
      [..descriptions.Select(it => it.Replace("泳", "磨"))];
}
