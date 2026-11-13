using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing.PreProcessingStage;

static class DictionaryFormVerbSplitter
{
   public static List<AnalysisToken>? TrySplit(MeCabToken token)
   {
      if(token.IsDictionaryForm() && !token.IsProgressiveForm())
      {
         if(token.IsIchidanVerb)
         {
            return SplitIchidanDictionaryForm(token);
         }

         if(token.IsGodanVerb)
         {
            return SplitGodanDictionaryForm(token);
         }

         if(token.IsKuruVerb)
         {
            return SplitKuruVerb(token);
         }

         if(token.IsSuruVerb)
         {
            return SplitSuruVerb(token);
         }
      }

      return null;
   }

   static List<AnalysisToken> SplitIchidanDictionaryForm(MeCabToken token)
   {
      var ichidanSurface = token.Surface[..^1];
      return
      [
         new IchidanDictionaryFormStem(token, ichidanSurface, token.BaseForm),
         new IchidanDictionaryFormInflection(token)
      ];
   }

   static List<AnalysisToken> SplitGodanDictionaryForm(MeCabToken token)
   {
      var godanSurface = token.Surface[..^1];
      var godanDictionaryEnding = token.Surface[^1..];
      return
      [
         new GodanDictionaryFormStem(token, godanSurface, token.BaseForm),
         new GodanDictionaryFormInflection(token, godanDictionaryEnding, "う")
      ];
   }

   static List<AnalysisToken> SplitKuruVerb(MeCabToken token)
   {
      var surface = token.Surface[..^1];
      return
      [
         new KuruVerbDictionaryFormStem(token, surface, token.BaseForm),
         new KuruVerbDictionaryFormInflection(token)
      ];
   }

   static List<AnalysisToken> SplitSuruVerb(MeCabToken token)
   {
      var surface = token.Surface[..^1];
      return
      [
         new SuruVerbDictionaryFormStem(token, surface, token.BaseForm),
         new SuruVerbDictionaryFormInflection(token)
      ];
   }
}
