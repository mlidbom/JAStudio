using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// An ichidan verb missing from the tokenizer's dictionary, which the tokenizer therefore reads as a godan verb's e-stem<br/>
/// followed by an ending that can only follow an ichidan verb's stem: 磨げる ("can polish") becomes 磨げ, the e-stem of 磨ぐ,<br/>
/// followed by the classical auxiliary り (文語助動詞-リ) in its form る.<br/>
/// <see cref="JNTokenizer"/> corrects it into exactly the tokens the tokenizer produces for an ichidan verb in its dictionary, such as 泳げる.
/// </summary>
/// <remarks>
/// Nearly all such verbs are the potential forms of godan verbs. The dictionary lists most potential forms as ichidan verbs<br/>
/// of their own (泳げる, 書ける), but not all of them (磨げる, 嗅げる, 浮ける, 活かせる, しまえる, もらえる).<br/>
/// Uncorrected, the analysis shows a classical り where the dictionary form's る is, shows an imperative え before ない and まい,<br/>
/// and loses the potential altogether before た, ます and て.<br/>
/// Once corrected, telling a potential form from a genuine ichidan verb such as 点ける is left to the
/// <see cref="PreProcessingStage.IchidanGodanPotentialOrImperativeHybridSplitter"/>, exactly as for the verbs in the dictionary.
/// </remarks>
/// <remarks>
/// Only endings that cannot follow a godan e-stem in modern Japanese trigger the correction. Left alone are the conditional ば (磨げば),<br/>
/// the classical concessive ど and ども (待てど), the classical り in its own form (極まれり), and whatever follows an imperative (行け夏目, 飲めとか).<br/>
/// Comparing the tokenizer's output for 嗅げる, which its dictionary knows, and 磨げる across their forms shows that the endings themselves<br/>
/// come out identically, with two exceptions this corrects: the dictionary form's る and the hypothetical form's れ become separate tokens.<br/>
/// The tokenizer's dictionary, UniDic, gives the stem a form it guesses, 未然形 before most endings, so the e-stem is told by its kana.
/// </remarks>
static class MisTokenizedIchidanVerbUnknownToTheTokenizer
{
   const string IchidanDictionaryFormEnding = "る";

   /// <summary>The base forms of the endings that follow an ichidan verb's irrealis stem, as in 泳げない and 泳げず.</summary>
   static readonly HashSet<string> BaseFormsOfEndingsAfterTheIrrealisStem = ["ない", "ぬ", "ず"];

   /// <summary>
   /// The base forms of the endings that follow an ichidan verb's continuative stem, as in 泳げた, 泳げます, 泳げて, 泳げてる, 泳げそう and 泳げまい:<br/>
   /// the tokenizer's dictionary, UniDic, has まい after an ichidan verb's continuative.
   /// </summary>
   static readonly HashSet<string> BaseFormsOfEndingsAfterTheContinuativeStem = ["た", "ます", "て", "てる", "そう", "まい"];

   /// <summary>
   /// Returns <paramref name="tokens"/> with each mis-tokenized ichidan verb unknown to the tokenizer replaced by<br/>
   /// the tokens the tokenizer produces for an ichidan verb in its dictionary.
   /// </summary>
   public static List<JNToken> CorrectEachIn(List<JNToken> tokens)
   {
      var corrected = new List<JNToken>(tokens.Count);
      var index = 0;
      while(index < tokens.Count)
      {
         var correction = index + 1 < tokens.Count ? CorrectionOrNull(tokens[index], tokens[index + 1], tokens.ElementAtOrDefault(index + 2)) : null;
         if(correction != null)
         {
            corrected.AddRange(correction);
            index += 2;
         } else
         {
            corrected.Add(tokens[index]);
            index++;
         }
      }

      return corrected;
   }

   /// <summary>
   /// The tokens to replace <paramref name="token"/> and the <paramref name="ending"/> following it with, <paramref name="followingToken"/> after<br/>
   /// them, or null if they are not a mis-tokenized ichidan verb unknown to the tokenizer.
   /// </summary>
   static List<JNToken>? CorrectionOrNull(JNToken token, JNToken ending, JNToken? followingToken)
   {
      if(!IsGodanEStem(token)) return null;

      if(IsTheRestOfTheVerbItself(ending)) return [TheIchidanVerbMadeOf(token, ending, followingToken)];

      if(IchidanStemFormBeforeOrNull(ending) is {} stemForm) return [TheIchidanStem(token, stemForm), AsTheEndingOfAnIchidanStem(ending)];

      return null;
   }

   /// <summary>A godan verb's stem ending in the e-row, such as 磨げ, whatever form the tokenizer guessed it to be in.</summary>
   static bool IsGodanEStem(JNToken token) => token.IsGodanVerb && Conjugator.GodanImperativeVerbEndings.Contains(token.Surface[^1..]);

   /// <summary>
   /// Whether <paramref name="ending"/> is the end of the ichidan verb itself, which the tokenizer only splits off for verbs missing from its dictionary:<br/>
   /// the dictionary form's る, read as an auxiliary, or the hypothetical form's れ, read as the passive auxiliary れる, which can't<br/>
   /// follow an e-stem: the passive follows the a-stem, as in 書かれる.
   /// </summary>
   static bool IsTheRestOfTheVerbItself(JNToken ending) => IsTheDictionaryFormEnding(ending) || IsTheHypotheticalFormEnding(ending);

   /// <summary>
   /// The dictionary form's る, which the written-language UniDic reads as the classical auxiliary り, and the spoken-language one as an<br/>
   /// uninflecting auxiliary.
   /// </summary>
   static bool IsTheDictionaryFormEnding(JNToken ending) =>
      ending.Surface == IchidanDictionaryFormEnding &&
      (Equals(ending.InflectionType, InflectionTypes.Classical.Ri) || (Equals(ending.PartsOfSpeech, JNPOS.BoundAuxiliary) && Equals(ending.InflectionType, InflectionTypes.Uninflecting)));

   static bool IsTheHypotheticalFormEnding(JNToken ending) =>
      ending.Surface == "れ" && Equals(ending.InflectionType, InflectionTypes.Auxiliary.Reru);

   static InflectionForm? IchidanStemFormBeforeOrNull(JNToken ending)
   {
      if(IsTheAdjectiveNaiTheTokenizerUsesAfterAnImperative(ending) || IsAnEndingWithABaseFormIn(BaseFormsOfEndingsAfterTheIrrealisStem))
      {
         return InflectionForms.Irrealis;
      }

      if(IsAnEndingWithABaseFormIn(BaseFormsOfEndingsAfterTheContinuativeStem))
      {
         return InflectionForms.Continuative;
      }

      return null;

      bool IsAnEndingWithABaseFormIn(HashSet<string> baseForms) => ending.IsValidGodanPotentialFormInflection() && baseForms.Contains(ending.BaseForm);
   }

   static bool IsTheAdjectiveNaiTheTokenizerUsesAfterAnImperative(JNToken ending) => ending is { BaseForm: "ない", IsAdjective: true };

   static JNToken TheIchidanVerbMadeOf(JNToken godanEStem, JNToken ending, JNToken? followingToken) =>
      new(godanEStem.PartsOfSpeech,
          baseForm: godanEStem.Surface + IchidanDictionaryFormEnding,
          surface: godanEStem.Surface + ending.Surface,
          inflectionType: ThePotentialFormsTypeOf(godanEStem),
          inflectedForm: IsTheHypotheticalFormEnding(ending) ? InflectionForms.Hypothetical : TheDictionaryFormBefore(followingToken),
          reading: godanEStem.Reading + ending.Reading,
          phonetic: godanEStem.Phonetic + ending.Phonetic,
          lemma: godanEStem.Lemma);

   static JNToken TheIchidanStem(JNToken godanEStem, InflectionForm stemForm) =>
      new(godanEStem.PartsOfSpeech,
          baseForm: godanEStem.Surface + IchidanDictionaryFormEnding,
          surface: godanEStem.Surface,
          inflectionType: ThePotentialFormsTypeOf(godanEStem),
          inflectedForm: stemForm,
          reading: godanEStem.Reading,
          phonetic: godanEStem.Phonetic,
          lemma: godanEStem.Lemma);

   /// <summary>
   /// The dictionary form the tokenizer gives a verb before <paramref name="followingToken"/>: the attributive (連体形-一般) before a noun, or<br/>
   /// before の and ん, which make what precedes them a noun, as in 泳げる人 and 泳げるの; otherwise the conclusive (終止形-一般), as in 泳げる and<br/>
   /// 泳げるか.
   /// </summary>
   static InflectionForm TheDictionaryFormBefore(JNToken? followingToken) =>
      InflectionForms.GetByName(followingToken is {} following && (following.PartsOfSpeech.IsNoun() || Equals(following.PartsOfSpeech, JNPOS.Particle.Nominalizing))
                                   ? "連体形-一般"
                                   : "終止形-一般");

   /// <summary>
   /// The inflection type of the potential form of <paramref name="godanEStem"/>'s verb: a lower ichidan verb of the same row, as the dictionary<br/>
   /// has the potential forms it knows, such as 嗅げる of 嗅ぐ (下一段-ガ行); the wa-row godan verbs' are of the a-row, such as もらえる (下一段-ア行).
   /// </summary>
   static InflectionType ThePotentialFormsTypeOf(JNToken godanEStem)
   {
      var row = godanEStem.InflectionType.Name.Split('-')[1];
      return InflectionTypes.GetByName($"{InflectionTypes.Ichidan.LowerFamily}-{(row == "ワア行" ? "ア行" : row)}");
   }

   static JNToken AsTheEndingOfAnIchidanStem(JNToken ending) =>
      IsTheAdjectiveNaiTheTokenizerUsesAfterAnImperative(ending)
         ? new JNToken(JNPOS.BoundAuxiliary, ending.BaseForm, ending.Surface, InflectionTypes.Auxiliary.Nai, ending.InflectedForm, ending.Reading, ending.Phonetic, ending.Lemma)
         : ending;
}
