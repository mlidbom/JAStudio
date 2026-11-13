using System.Collections.Generic;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests;

static class ForbidsSurfaceIfBaseIsValidAndContextIndicatesAVerb
{
   static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("inflected_surface_with_valid_base");
   static readonly HashSet<string> PrefixesThatIndicatesVerbIfFollowedByEndOfStatement = ["を", "が"];

   public static FailedMatchRequirement? ApplyTo(MatchInspector inspector)
   {
      if(inspector.Variant.IsSurface &&
         inspector.Word.HasBaseVariantWithValidMatch &&
         (inspector.Word.IsInflectedWord ||
          (inspector.Prefix != "" &&
           PrefixesThatIndicatesVerbIfFollowedByEndOfStatement.Contains(inspector.Prefix[^1].ToString()) &&
           inspector.IsEndOfStatement)))
      {
         return Failed;
      }

      return null;
   }
}
