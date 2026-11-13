using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests;

static class ForbidsDictionaryInflectionSurfaceWithBase
{
   static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("dictionary_form_verb_inflection");

   public static FailedMatchRequirement? ApplyTo(MatchInspector inspector)
   {
      if(inspector.Variant.IsSurface &&
         !inspector.Word.IsCompound &&
         inspector.Word.BaseVariant != null &&
         inspector.Word.EndLocation.Token.IsDictionaryVerbInflection)
      {
         return Failed;
      }

      return null;
   }
}
