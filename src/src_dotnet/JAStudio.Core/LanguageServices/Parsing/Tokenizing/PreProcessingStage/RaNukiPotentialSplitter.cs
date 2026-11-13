using Compze.Internals.SystemCE;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.MeCab;

namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing.PreProcessingStage;

/// <summary>
/// Splits a ら抜き potential (ら抜き言葉, "ra-less words") the tokenizer's dictionary, UniDic, gives as one token: the colloquial potential form of an<br/>
/// ichidan verb, or of 来る, that drops the ら of られる, as 食べれる for 食べられる, 寝れない for 寝られない and 来れる for 来られる. UniDic gives it as<br/>
/// a verb of its own, 食べれる, whose lemma is the verb, 食べる. JAStudio's analysis, and its vocab, see it as the verb's stem followed by the<br/>
/// potential れる, as IPADIC, the tokenizer's former dictionary, cut it: 食べ and れる, 寝 and れ (<see cref="RaNukiPotentialStem"/>,<br/>
/// <see cref="RaNukiPotentialInflection"/>), and a dictionary form's れる further into れ and る, as every dictionary-form verb is.
/// </summary>
/// <remarks>
/// It's told from a godan verb's potential form, which looks the same, by the inflection type the dictionary gives its lemma: 走れる's lemma,<br/>
/// 走る, is a godan verb, so 走れる is 走る's potential, which <see cref="IchidanGodanPotentialOrImperativeHybridSplitter"/> splits. And it's told<br/>
/// from a verb whose own stem ends in れ, as 忘れる's does, by the lemma's spelling: the lemma is the token's base form with る in place of れる.<br/>
/// So a ら抜き potential spelled in kana, such as たべれる, whose lemma UniDic spells 食べる, isn't split.
/// </remarks>
static class RaNukiPotentialSplitter
{
   const string DictionaryFormEnding = "る";

   /// <summary>The stem and the potential of <paramref name="token"/> if the <paramref name="dictionary"/> it was tokenized with makes it a ら抜き potential, or null.</summary>
   public static List<AnalysisToken>? TrySplit(MeCabToken token, TokenizerDictionary dictionary) =>
      TheVerbItIsTheRaNukiPotentialOf(token, dictionary) is {} verb ? StemAndPotentialOf(token, verb) : null;

   /// <summary>
   /// The verb <paramref name="token"/> is the ら抜き potential of, as the <paramref name="dictionary"/> tokenizes the verb on its own: 食べる for<br/>
   /// 食べれる. Nothing if the token isn't a ら抜き potential.
   /// </summary>
   static RawMeCabToken? TheVerbItIsTheRaNukiPotentialOf(MeCabToken token, TokenizerDictionary dictionary) =>
      token.IsIchidanVerb &&
      token.BaseForm.EndsWithOrdinal(RaNukiPotentialStem.PotentialEnding) &&
      token.Lemma == StemOf(token) + DictionaryFormEnding &&
      dictionary.Tokenize(token.Lemma) is [var verb] &&
      IsAnIchidanVerbOrKuru(InflectionTypeOf(verb))
         ? verb
         : null;

   /// <summary>The verb's stem, which a ら抜き potential's base form adds れる to: the 食べ of 食べれる.</summary>
   static string StemOf(MeCabToken token) => token.BaseForm[..^RaNukiPotentialStem.PotentialEnding.Length];

   static UniDicInflectionType InflectionTypeOf(RawMeCabToken verb) => UniDicInflectionTypes.GetByName(verb.InflectionType);

   static bool IsAnIchidanVerbOrKuru(UniDicInflectionType type) => UniDicInflectionTypes.Ichidan.Includes(type) || type.Family == UniDicInflectionTypes.Kahen.Family;

   static List<AnalysisToken> StemAndPotentialOf(MeCabToken potential, RawMeCabToken verb)
   {
      var stem = StemOf(potential);
      var ending = potential.Surface[stem.Length..];
      var theStem = new RaNukiPotentialStem(potential, stem, potential.Lemma, InflectionTypeOf(verb));
      return potential.IsDictionaryForm()
                ? [theStem, new IchidanDictionaryFormStem(potential, ending[..^DictionaryFormEnding.Length], RaNukiPotentialStem.PotentialEnding), new IchidanDictionaryFormInflection(potential)]
                : [theStem, new RaNukiPotentialInflection(potential, ending)];
   }
}
