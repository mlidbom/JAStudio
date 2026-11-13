using System;
using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing.PreProcessingStage;

static class GodanImperativeSplitter
{
   public static List<AnalysisToken>? TrySplit(MeCabToken token)
   {
      if(IsGodanImperative(token))
      {
         return SplitGodanImperative(token, token.BaseForm);
      }

      return null;
   }

   /// <summary>
   /// An imperative that ends as a godan verb's does, in the e-row or in い, such as 書け, ください and 来い, and also くれ, which IPADIC, the<br/>
   /// tokenizer's former dictionary, gave godan imperative forms; or a godan verb's hypothetical stem ending a statement, which can only be an<br/>
   /// imperative there, such as the 書け of 書け！
   /// </summary>
   static bool IsGodanImperative(MeCabToken token) =>
      (Equals(token.InflectedForm, UniDicInflectionForms.Imperative) && !IchidanImperativeSplitter.EndsAsAnIchidanImperative(token)) ||
      (token.IsGodanVerb && Equals(token.InflectedForm, UniDicInflectionForms.Hypothetical) && token.IsEndOfStatement);

   static List<AnalysisToken> SplitGodanImperative(MeCabToken token, string godanBase)
   {
      var godanSurface = token.Surface[..^1];
      var imperativePart = token.Surface[^1..];
      var imperativeBase = imperativePart == "い" ? "い" : "え";
      return
      [
         new SplitToken(token, godanSurface, godanBase, isInflectableWord: true, isGodanImperativeStem: true),
         new SplitToken(token, imperativePart, imperativeBase, isInflectableWord: true, isGodanImperativeInflection: true)
      ];
   }
}
