using System;
using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;

static class GodanImperativeSplitter
{
   public static List<IAnalysisToken>? TrySplit(JNToken token)
   {
      if(IsGodanImperative(token))
      {
         return SplitGodanImperative(token, token.BaseForm);
      }

      return null;
   }

   /// <summary>
   /// A godan verb's imperative, such as 書け and ください, or its hypothetical stem ending a statement, which can only be an imperative there,<br/>
   /// such as the 書け of 書け！
   /// </summary>
   static bool IsGodanImperative(JNToken token) =>
      token.IsGodanVerb &&
      (Equals(token.InflectedForm, InflectionForms.Imperative) || (Equals(token.InflectedForm, InflectionForms.Hypothetical) && token.IsEndOfStatement));

   static List<IAnalysisToken> SplitGodanImperative(JNToken token, string godanBase)
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
