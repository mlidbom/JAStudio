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

   static bool IsGodanImperative(JNToken token)
   {
      if(InflectionForms.ImperativeMeireikei.GodanForms.Contains(token.InflectedForm))
      {
         return true;
      }

      if(token.InflectionType.Family == InflectionTypes.Godan.Family &&
         Equals(token.InflectedForm, InflectionForms.Hypothetical.GeneralHypotheticalKateikei) &&
         token.IsEndOfStatement)
      {
         return true;
      }

      return false;
   }

   static List<IAnalysisToken> SplitGodanImperative(JNToken token, string godanBase)
   {
      if(Equals(token.InflectedForm, InflectionForms.ImperativeMeireikei.Yo))
      {
         // Handles cases like 放せよ which janome turns into a single token and believes is an ichidan よ imperative
         var godanSurface = token.Surface[..^2];
         var imperativePart = token.Surface[^2..^1];
         return
         [
            new SplitToken(token, godanSurface, godanBase, isInflectableWord: true, isGodanImperativeStem: true),
            new SplitToken(token, imperativePart, "え", isInflectableWord: true, isGodanImperativeInflection: true),
            new SplitToken(token, "よ", "よ", isInflectableWord: false)
         ];
      } else if(Equals(token.InflectedForm, InflectionForms.ImperativeMeireikei.Ro))
      {
         throw new Exception("I doubt this ever happens, but let's explode if it does so we can add support");
      } else
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
}
