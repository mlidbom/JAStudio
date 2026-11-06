using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.Specifications.LanguageServices.TextAnalysis;
using JAStudio.Core.UI.Web.Sentence;

namespace JAStudio.Core.Specifications.AICreatedSpecs.IchidanVerbsUnknownToTheTokenizer;

/// <summary>
/// Base for the specifications of how text containing an ichidan verb missing from the tokenizer's dictionary is tokenized and displayed.<br/>
/// 磨げる, the potential form of 磨ぐ, is missing from the dictionary; 泳げる, the potential form of 泳ぐ, is in it.
/// </summary>
public abstract class IchidanVerbUnknownToTheTokenizerSpecification() : SentenceAnalysisViewModelCommon(DataNeeded.Vocabulary), IAIGeneratedSpec
{
   protected const string StemOfAPotentialFormUnknownToTheTokenizer = "磨げ";
   protected const string StemOfAPotentialFormKnownToTheTokenizer = "泳げ";

   protected static List<JNToken> TokensOf(string text) => JNTokenizer.GetInstance().Tokenize(text).Tokens;

   /// <summary>Everything about the tokens of <paramref name="text"/> that the analysis uses, except their readings.</summary>
   protected static List<string> TokenDescriptionsOf(string text) =>
      [..TokensOf(text).Select(it => $"{it.Surface}|{it.BaseForm}|{it.InflectionType}|{it.InflectedForm}|{it.PartsOfSpeech}")];

   protected List<string> DisplayedWordsOf(string text) =>
      [..new SentenceViewModel(CreateSentence(text), GetService<Settings>(), GetService<VocabCollection>()).DisplayedMatches.Select(SurfaceAndMatchForm)];

   /// <summary>
   /// <paramref name="descriptions"/> of text containing the potential form known to the tokenizer,<br/>
   /// rewritten to be about the potential form unknown to it instead.
   /// </summary>
   protected static List<string> AsIfAboutThePotentialFormUnknownToTheTokenizer(IEnumerable<string> descriptions) =>
      [..descriptions.Select(it => it.Replace("泳", "磨"))];
}
