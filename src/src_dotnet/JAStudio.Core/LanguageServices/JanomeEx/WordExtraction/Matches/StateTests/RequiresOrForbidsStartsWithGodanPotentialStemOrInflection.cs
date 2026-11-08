using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;

static class RequiresOrForbidsStartsWithGodanPotentialStemOrInflection
{
   static readonly FailedMatchRequirement RequiredFailure = FailedMatchRequirement.Required("godan_potential");
   static readonly FailedMatchRequirement ForbiddenFailure = FailedMatchRequirement.Forbids("godan_potential");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = inspector.Match.RequiresForbids.GodanPotential;
      if(requirement.IsConfigured)
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
      if(inspector.Word.StartLocation.Token.IsGodanPotentialInflection ||
         inspector.Word.StartLocation.Token.IsGodanPotentialStem)
      {
         return true;
      }

      return false;
   }
}
