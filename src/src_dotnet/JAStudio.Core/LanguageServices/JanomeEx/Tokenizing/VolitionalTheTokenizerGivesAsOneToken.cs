using Compze.Internals.SystemCE;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// A volitional the tokenizer's dictionary, UniDic, gives as one token in its volitional form (意志推量形): 書こう, 食べよう, しよう, and the<br/>
/// conjectural だろう, でしょう and ましょう. JAStudio's analysis, and its vocab, see the volitional as the irrealis stem followed by the<br/>
/// auxiliary う, as IPADIC, the tokenizer's former dictionary, cut it: 書こ and う, だろ and う. <see cref="JNTokenizer"/> splits each so.
/// </summary>
/// <remarks>
/// Without the split, the vocab of the volitional's う (おう) and the stems the vocab of だろう and でしょう match by (だろ, でしょ) aren't<br/>
/// matched. A volitional form ending in よ instead, such as 食べよ, is the classical imperative, which the imperative splitter takes.
/// </remarks>
static class VolitionalTheTokenizerGivesAsOneToken
{
   const string U = "う";

   /// <summary>Returns <paramref name="tokens"/> with each volitional given as one token split into its stem and う.</summary>
   public static List<JNToken> SplitEachIn(List<JNToken> tokens) => [.. tokens.SelectMany(it => IsOne(it) ? StemAndUOf(it) : [it])];

   static bool IsOne(JNToken token) =>
      Equals(token.InflectedForm, InflectionForms.Volitional) && token.Surface.Length > 1 && token.Surface.EndsWithOrdinal(U);

   static JNToken[] StemAndUOf(JNToken volitional) =>
   [
      new(volitional.PartsOfSpeech,
          baseForm: volitional.BaseForm,
          surface: volitional.Surface[..^1],
          inflectionType: volitional.InflectionType,
          inflectedForm: InflectionForms.Irrealis,
          reading: WithoutItsLastKana(volitional.Reading),
          phonetic: WithoutItsLastKana(volitional.Phonetic),
          lemma: volitional.Lemma),
      new(JNPOS.BoundAuxiliary,
          baseForm: U,
          surface: U,
          inflectionType: InflectionTypes.Uninflecting,
          inflectedForm: InflectionForms.GetByName("終止形-一般"),
          reading: "ウ",
          phonetic: "ウ",
          lemma: U)
   ];

   /// <summary><paramref name="kana"/> without the ウ, or the ー of a pronunciation such as ショー, that the volitional's う is read as.</summary>
   static string WithoutItsLastKana(string kana) => kana.Length > 0 ? kana[..^1] : kana;
}
