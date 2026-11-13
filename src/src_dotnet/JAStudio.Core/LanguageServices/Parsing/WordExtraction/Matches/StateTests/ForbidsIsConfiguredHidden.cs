using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests;

static class ForbidsIsConfiguredHidden
{
   static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("configured_hidden");

   public static FailedMatchRequirement? ApplyTo(MatchInspector inspector)
   {
      if(inspector.Configuration.HiddenMatches.ExcludesAtIndex(inspector.Match.ExclusionForm,
                                                               inspector.Match.StartIndex))
      {
         return Failed;
      }

      return null;
   }
}
