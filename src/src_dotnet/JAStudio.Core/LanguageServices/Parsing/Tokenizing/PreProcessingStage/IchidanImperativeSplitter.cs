using Compze.Internals.SystemCE;
using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing.PreProcessingStage;

static class IchidanImperativeSplitter
{
   public static List<AnalysisToken>? TrySplit(MeCabToken token)
   {
      if(IsIchidanImperative(token))
      {
         return SplitIchidanImperative(token);
      }

      return null;
   }

   /// <summary>
   /// An imperative ending in ろ or よ, as an ichidan verb's does, such as 食べろ, and as する's does, しろ; or an ichidan verb's classical imperative<br/>
   /// in よ, such as 食べよ, which the tokenizer's dictionary, UniDic, gives as a volitional.
   /// </summary>
   static bool IsIchidanImperative(MeCabToken token) =>
      (Equals(token.InflectedForm, UniDicInflectionForms.Imperative) && EndsAsAnIchidanImperative(token)) ||
      (Equals(token.InflectedForm, UniDicInflectionForms.Volitional) && token.IsIchidanVerb && token.Surface.EndsWithOrdinal("よ"));

   /// <summary>Whether <paramref name="imperative"/> ends in ろ or よ, as an ichidan verb's imperative does.</summary>
   internal static bool EndsAsAnIchidanImperative(MeCabToken imperative) => imperative.Surface.EndsWithOrdinal("ろ") || imperative.Surface.EndsWithOrdinal("よ");

   static List<AnalysisToken> SplitIchidanImperative(MeCabToken token)
   {
      var ichidanSurface = token.Surface[..^1];
      var ichidanImperativePart = token.Surface[^1..];
      return
      [
         new SplitToken(token, ichidanSurface, token.BaseForm, isInflectableWord: true, isIchidanImperativeStem: true),
         new SplitToken(token, ichidanImperativePart, ichidanImperativePart, isInflectableWord: true, isIchidanImperativeInflection: true)
      ];
   }
}
