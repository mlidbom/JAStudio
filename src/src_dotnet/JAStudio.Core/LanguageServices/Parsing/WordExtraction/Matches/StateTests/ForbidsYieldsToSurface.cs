using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests;

static class ForbidsYieldsToValidSurfaceSurface
{
   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var surfaces = inspector.Match.MatchingConfiguration.YieldToSurface.Surfaces;
      var surfaceVariant = inspector.SurfaceVariant;
      if(surfaceVariant.HasValidMatch && surfaces.Contains(surfaceVariant.Form))
      {
         return FailedMatchRequirement.Forbids($"valid_surface_in:{string.Join(",", surfaces)}");
      }

      return null;
   }
}
