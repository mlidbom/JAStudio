using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing;
using JAStudio.Core.LanguageServices.Parsing.Tokenizing;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.UI.Web.Sentence;

namespace JAStudio.Core.Specifications.AICreatedSpecs.IchidanVerbsUnknownToTheTokenizer;

/// <summary>
/// Base for the specifications of how JAStudio reads and splits text containing an ichidan verb missing from the tokenizer's dictionary.<br/>
/// 磨げる, the potential form of 磨ぐ, is missing from the dictionary; 泳げる, the potential form of 泳ぐ, is in it. How such text is displayed<br/>
/// is specified by parsing cases (<see cref="LanguageServices.ParsingCases.ParsingCase"/>) of the same sentences.
/// </summary>
public abstract class IchidanVerbUnknownToTheTokenizerSpecification : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected const string StemOfAPotentialFormUnknownToTheTokenizer = "磨げ";
   protected const string StemOfAPotentialFormKnownToTheTokenizer = "泳げ";

   /// <summary>The tokens JAStudio's analysis works with for <paramref name="text"/>: the tokenizer's, as the pre-processing stage split them.</summary>
   protected List<AnalysisToken> PreProcessedTokensOf(string text) => TextAnalysis.FromText(GetService<AnalysisServices>(), text).PreProcessedTokens;

   /// <summary>The tokens the tokenizer cut <paramref name="text"/> into, as it gave them.</summary>
   protected List<MeCabToken> TheTokenizersTokensOf(string text) => TextAnalysis.FromText(GetService<AnalysisServices>(), text).TokenizedText.Tokens;

   /// <summary>
   /// What the analysis works with of the pre-processed tokens of <paramref name="text"/>: each token's type, surface, base form and facts. Not the<br/>
   /// tags of the tokenizer's token it was split off, which tell what the tokenizer read, not what JAStudio reads.
   /// </summary>
   protected List<string> TokenDescriptionsOf(string text) =>
      [..PreProcessedTokensOf(text).Select(it => $"{it.GetType().Name}|{it.Surface}|{it.BaseForm}|{new TokenViewModel(it).TrueFlagsListed}")];

   /// <summary>
   /// <paramref name="descriptions"/> of text containing the potential form known to the tokenizer,<br/>
   /// rewritten to be about the potential form unknown to it instead.
   /// </summary>
   protected static List<string> AsIfAboutThePotentialFormUnknownToTheTokenizer(IEnumerable<string> descriptions) =>
      [..descriptions.Select(it => it.Replace("泳", "磨"))];
}
