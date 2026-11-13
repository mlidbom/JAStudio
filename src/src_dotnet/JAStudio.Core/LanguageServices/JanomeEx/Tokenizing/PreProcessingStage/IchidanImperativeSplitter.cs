using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;

static class IchidanImperativeSplitter
{
   public static List<IAnalysisToken>? TrySplit(JNToken token)
   {
      if(IsIchidanImperative(token))
      {
         return SplitIchidanImperative(token);
      }

      return null;
   }

   /// <summary>An ichidan verb's imperative, such as 食べろ.</summary>
   static bool IsIchidanImperative(JNToken token) => token.IsIchidanVerb && Equals(token.InflectedForm, InflectionForms.Imperative);

   static List<IAnalysisToken> SplitIchidanImperative(JNToken token)
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
