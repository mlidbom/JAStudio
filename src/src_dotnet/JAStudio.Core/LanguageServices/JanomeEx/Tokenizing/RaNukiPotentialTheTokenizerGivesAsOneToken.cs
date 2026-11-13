using Compze.Internals.SystemCE;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.TokenizerDictionaries;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// A ら抜き potential (ら抜き言葉, "ra-less words") the tokenizer's dictionary, UniDic, gives as one token: the colloquial potential form of an<br/>
/// ichidan verb, or of 来る, that drops the ら of られる, as 食べれる for 食べられる, 寝れない for 寝られない and 来れる for 来られる. UniDic gives it as<br/>
/// a verb of its own, 食べれる, whose lemma is the verb, 食べる. JAStudio's analysis, and its vocab, see it as the verb's stem followed by the<br/>
/// potential れる, as IPADIC, the tokenizer's former dictionary, cut it: 食べ and れる, 寝 and れ. <see cref="JNTokenizer"/> splits each so.
/// </summary>
/// <remarks>
/// It's told from a godan verb's potential form, which looks the same, by the inflection type the dictionary gives its lemma: 走れる's lemma,<br/>
/// 走る, is a godan verb, so 走れる is 走る's potential, which the preprocessing stage splits. And it's told from a verb whose own stem ends in<br/>
/// れ, as 忘れる's does, by the lemma's spelling: the lemma is the token's base form with る in place of れる. So a ら抜き potential spelled in<br/>
/// kana, such as たべれる, whose lemma UniDic spells 食べる, isn't split.
/// </remarks>
static class RaNukiPotentialTheTokenizerGivesAsOneToken
{
   const string RaNukiEnding = "れる";
   const string DictionaryFormEnding = "る";

   /// <summary>Returns <paramref name="tokens"/> with each ら抜き potential the <paramref name="dictionary"/> gave as one token split into the verb's stem and れる.</summary>
   public static List<JNToken> SplitEachIn(List<JNToken> tokens, TokenizerDictionary dictionary) =>
      [.. tokens.SelectMany(it => TheVerbItIsTheRaNukiPotentialOf(it, dictionary) is {} verb ? StemAndReruOf(it, verb) : [it])];

   /// <summary>
   /// The verb <paramref name="token"/> is the ら抜き potential of, as the <paramref name="dictionary"/> tokenizes the verb on its own: 食べる for<br/>
   /// 食べれる. Nothing if the token isn't a ら抜き potential.
   /// </summary>
   static MeCabToken? TheVerbItIsTheRaNukiPotentialOf(JNToken token, TokenizerDictionary dictionary) =>
      token.IsIchidanVerb &&
      token.BaseForm.EndsWithOrdinal(RaNukiEnding) &&
      token.Lemma == StemOf(token) + DictionaryFormEnding &&
      dictionary.Tokenize(token.Lemma) is [var verb] &&
      IsAnIchidanVerbOrKuru(InflectionTypeOf(verb))
         ? verb
         : null;

   /// <summary>The verb's stem, which a ら抜き potential's base form adds れる to: the 食べ of 食べれる.</summary>
   static string StemOf(JNToken token) => token.BaseForm[..^RaNukiEnding.Length];

   static InflectionType InflectionTypeOf(MeCabToken verb) => InflectionTypes.GetByName(verb.InflectionType);

   static bool IsAnIchidanVerbOrKuru(InflectionType type) => InflectionTypes.Ichidan.Includes(type) || type.Family == InflectionTypes.Kahen.Family;

   static JNToken[] StemAndReruOf(JNToken potential, MeCabToken verb)
   {
      var stem = StemOf(potential);
      var ending = potential.Surface[stem.Length..];
      return
      [
         new JNToken(potential.PartsOfSpeech,
             baseForm: potential.Lemma,
             surface: stem,
             inflectionType: InflectionTypeOf(verb),
             inflectedForm: InflectionForms.Irrealis,
             reading: WithoutTheEnding(potential.Reading),
             phonetic: WithoutTheEnding(potential.Phonetic),
             lemma: potential.Lemma),
         new JNToken(JNPOS.BoundAuxiliary,
             baseForm: RaNukiEnding,
             surface: ending,
             inflectionType: InflectionTypes.Auxiliary.Reru,
             inflectedForm: potential.InflectedForm,
             reading: TheEnding(potential.Reading),
             phonetic: TheEnding(potential.Phonetic),
             lemma: RaNukiEnding)
      ];

      string WithoutTheEnding(string kana) => kana[..^ending.Length];
      string TheEnding(string kana) => kana[^ending.Length..];
   }
}
