using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests;

static class ForbidsIsGodanPotentialInflectionWithBase
{
   static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("godan_potential_surface");

   public static FailedMatchRequirement? ApplyTo(MatchInspector inspector)
   {
      if(inspector.HasGodanPotentialStart &&
         inspector.Word.LocationCount == 1 &&
         inspector.Variant.IsSurface &&
         inspector.Word.BaseVariant != null)
      {
         return Failed;
      }

      return null;
   }
}
