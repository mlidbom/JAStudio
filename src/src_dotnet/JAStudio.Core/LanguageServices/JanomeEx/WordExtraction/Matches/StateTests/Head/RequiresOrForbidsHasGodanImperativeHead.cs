using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

static class RequiresOrForbidsHasGodanImperativeHead
{
   static readonly FailedMatchRequirement RequiredFailure = FailedMatchRequirement.Required("godan_imperative_prefix");
   static readonly FailedMatchRequirement ForbiddenFailure = FailedMatchRequirement.Forbids("godan_imperative_prefix");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = inspector.Match.RequiresForbids.GodanImperativeHead;
      if(requirement.IsActive)
      {
         var isInState = InternalIsInState(inspector);
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

   static bool InternalIsInState(VocabMatchInspector inspector)
   {
      if(inspector.PreviousLocation == null)
      {
         return false;
      }

      if(inspector.PreviousLocation.Token.IsGodanImperativeInflection)
      {
         return true;
      }

      return false;
   }
}
