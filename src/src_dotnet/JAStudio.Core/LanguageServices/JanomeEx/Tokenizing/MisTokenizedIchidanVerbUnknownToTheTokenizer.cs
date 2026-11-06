using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// An ichidan verb missing from the tokenizer's dictionary, which the tokenizer therefore reads as a godan verb's e-stem<br/>
/// followed by an ending that can only follow an ichidan verb's stem: 磨げる ("can polish") becomes 磨げ, the e-stem of 磨ぐ,<br/>
/// followed by the classical auxiliary る.<br/>
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
/// Comparing the tokenizer's output for 泳げる and 磨げる across their forms shows that the endings themselves come out identically,<br/>
/// with three exceptions this corrects: the dictionary form's る and the hypothetical form's れ become separate tokens,<br/>
/// and ない after a stem read as an imperative becomes the adjective ない instead of the negative auxiliary.
/// </remarks>
static class MisTokenizedIchidanVerbUnknownToTheTokenizer
{
   const string IchidanDictionaryFormEnding = "る";

   /// <summary>The base forms of the endings that follow an ichidan verb's irrealis stem, as in 泳げない, 泳げまい and 泳げず.</summary>
   static readonly HashSet<string> BaseFormsOfEndingsAfterTheIrrealisStem = ["ない", "まい", "ぬ"];

   /// <summary>The base forms of the endings that follow an ichidan verb's continuative stem, as in 泳げた, 泳げます, 泳げて, 泳げてる and 泳げそう.</summary>
   static readonly HashSet<string> BaseFormsOfEndingsAfterTheContinuativeStem = ["た", "ます", "て", "てる", "そう"];

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
         var correction = index + 1 < tokens.Count ? CorrectionOrNull(tokens[index], tokens[index + 1]) : null;
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
   /// The tokens to replace <paramref name="token"/> and the <paramref name="ending"/> following it with,<br/>
   /// or null if they are not a mis-tokenized ichidan verb unknown to the tokenizer.
   /// </summary>
   static List<JNToken>? CorrectionOrNull(JNToken token, JNToken ending)
   {
      if(!IsGodanEStem(token)) return null;

      if(IsTheRestOfTheVerbItself(ending)) return [TheIchidanVerbMadeOf(token, ending)];

      if(IchidanStemFormBeforeOrNull(ending) is {} stemForm) return [TheIchidanStem(token, stemForm), AsTheEndingOfAnIchidanStem(ending)];

      return null;
   }

   static bool IsGodanEStem(JNToken token) =>
      token.IsGodanVerb &&
      (Equals(token.InflectedForm, InflectionForms.Hypothetical.GeneralHypotheticalKateikei) || Equals(token.InflectedForm, InflectionForms.ImperativeMeireikei.E));

   /// <summary>
   /// Whether <paramref name="ending"/> is the end of the ichidan verb itself, which the tokenizer only splits off for verbs missing from its dictionary:<br/>
   /// the dictionary form's る, read as a classical auxiliary, or the hypothetical form's れ, read as the verb る.
   /// </summary>
   static bool IsTheRestOfTheVerbItself(JNToken ending) => IsTheDictionaryFormEnding(ending) || IsTheHypotheticalFormEnding(ending);

   static bool IsTheDictionaryFormEnding(JNToken ending) =>
      ending.Surface == IchidanDictionaryFormEnding &&
      (Equals(ending.InflectionType, InflectionTypes.Bungo.Ri) || Equals(ending.InflectionType, InflectionTypes.Bungo.Ru));

   static bool IsTheHypotheticalFormEnding(JNToken ending) =>
      ending is { Surface: "れ", BaseForm: IchidanDictionaryFormEnding, IsIchidanVerb: true } &&
      Equals(ending.InflectedForm, InflectionForms.Hypothetical.GeneralHypotheticalKateikei);

   static InflectionForm? IchidanStemFormBeforeOrNull(JNToken ending)
   {
      if(IsTheAdjectiveNaiTheTokenizerUsesAfterAnImperative(ending) || IsAnEndingWithABaseFormIn(BaseFormsOfEndingsAfterTheIrrealisStem))
      {
         return InflectionForms.Irrealis.GeneralIrrealisMizenkei;
      }

      if(IsAnEndingWithABaseFormIn(BaseFormsOfEndingsAfterTheContinuativeStem))
      {
         return InflectionForms.Continuative.RenyoukeiMasuStem;
      }

      return null;

      bool IsAnEndingWithABaseFormIn(HashSet<string> baseForms) => ending.IsValidGodanPotentialFormInflection() && baseForms.Contains(ending.BaseForm);
   }

   static bool IsTheAdjectiveNaiTheTokenizerUsesAfterAnImperative(JNToken ending) => ending is { BaseForm: "ない", IsAdjective: true };

   static JNToken TheIchidanVerbMadeOf(JNToken godanEStem, JNToken ending) =>
      new(godanEStem.PartsOfSpeech,
          baseForm: godanEStem.Surface + IchidanDictionaryFormEnding,
          surface: godanEStem.Surface + ending.Surface,
          inflectionType: InflectionTypes.Ichidan.Regular,
          inflectedForm: IsTheHypotheticalFormEnding(ending) ? InflectionForms.Hypothetical.GeneralHypotheticalKateikei : InflectionForms.Basic.DictionaryForm,
          reading: godanEStem.Reading + ending.Reading,
          phonetic: godanEStem.Phonetic + ending.Phonetic);

   static JNToken TheIchidanStem(JNToken godanEStem, InflectionForm stemForm) =>
      new(godanEStem.PartsOfSpeech,
          baseForm: godanEStem.Surface + IchidanDictionaryFormEnding,
          surface: godanEStem.Surface,
          inflectionType: InflectionTypes.Ichidan.Regular,
          inflectedForm: stemForm,
          reading: godanEStem.Reading,
          phonetic: godanEStem.Phonetic);

   static JNToken AsTheEndingOfAnIchidanStem(JNToken ending) =>
      IsTheAdjectiveNaiTheTokenizerUsesAfterAnImperative(ending)
         ? new JNToken(JNPOS.BoundAuxiliary, ending.BaseForm, ending.Surface, InflectionTypes.Special.Nai, ending.InflectedForm, ending.Reading, ending.Phonetic)
         : ending;
}
