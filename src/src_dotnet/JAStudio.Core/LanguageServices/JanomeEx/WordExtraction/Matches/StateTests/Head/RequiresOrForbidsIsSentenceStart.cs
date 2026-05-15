using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

static class RequiresOrForbidsIsSentenceStart
{
   static readonly FailedMatchRequirement RequiredFailure = FailedMatchRequirement.Required("sentence_start");
   static readonly FailedMatchRequirement ForbiddenFailure = FailedMatchRequirement.Forbids("sentence_start");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = inspector.Match.MatchingConfiguration.RequiresForbids.SentenceStart;
      if(requirement.IsActive)
      {
         var isInState = inspector.Prefix.Length == 0 ||
                         AnalysisConstants.SentenceStartCharacters.Contains(inspector.Prefix[^1]);
         if(requirement.IsRequired && !isInState)
         {
            return RequiredFailure;
         }

         if(requirement.IsForbidden && isInState)
         {
            return ForbiddenFailure;
         }
      }

      return null;
   }
}
