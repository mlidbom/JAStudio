using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;

static class RequiresOrForbidsIsSingleToken
{
   static readonly FailedMatchRequirement RequiredFailure = FailedMatchRequirement.Required("single_token");
   static readonly FailedMatchRequirement ForbiddenFailure = FailedMatchRequirement.Forbids("single_token");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = inspector.Match.RequiresForbids.SingleToken;
      if(requirement.IsConfigured)
      {
         var isInState = !inspector.Word.IsCompound;
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
