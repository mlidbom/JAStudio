using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests;

static class ForbidsIsGodanImperativeInflectionWithBase
{
   static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("godan_imperative_surface_with_base");

   public static FailedMatchRequirement? ApplyTo(MatchInspector inspector)
   {
      if(inspector.HasGodanImperativePart &&
         inspector.Word.LocationCount == 1 &&
         inspector.Variant.IsSurface &&
         inspector.Word.BaseVariant != null)
      {
         return Failed;
      }

      return null;
   }
}
