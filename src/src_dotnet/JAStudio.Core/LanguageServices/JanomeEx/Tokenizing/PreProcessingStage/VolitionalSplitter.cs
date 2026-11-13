using Compze.Internals.SystemCE;
using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;

/// <summary>
/// Splits a volitional the tokenizer's dictionary, UniDic, gives as one token in its volitional form (意志推量形): 書こう, 食べよう, しよう, and the<br/>
/// conjectural だろう, でしょう and ましょう. JAStudio's analysis, and its vocab, see the volitional as the irrealis stem followed by the auxiliary う,<br/>
/// as IPADIC, the tokenizer's former dictionary, cut it: 書こ and う, だろ and う (<see cref="VolitionalStem"/>, <see cref="VolitionalInflection"/>).
/// </summary>
/// <remarks>
/// Without the split, the vocab of the volitional's う (おう) and the stems the vocab of だろう and でしょう match by (だろ, でしょ) aren't<br/>
/// matched. A volitional form ending in よ instead, such as 食べよ, is the classical imperative, which <see cref="IchidanImperativeSplitter"/> takes.
/// </remarks>
static class VolitionalSplitter
{
   /// <summary>The stem and the う of <paramref name="token"/> if it's a volitional ending in う, or null.</summary>
   public static List<IAnalysisToken>? TrySplit(JNToken token) =>
      IsAVolitionalEndingInU(token) ? [new VolitionalStem(token), new VolitionalInflection(token)] : null;

   static bool IsAVolitionalEndingInU(JNToken token) =>
      Equals(token.InflectedForm, InflectionForms.Volitional) &&
      token.Surface.Length > VolitionalInflection.U.Length &&
      token.Surface.EndsWithOrdinal(VolitionalInflection.U);
}
