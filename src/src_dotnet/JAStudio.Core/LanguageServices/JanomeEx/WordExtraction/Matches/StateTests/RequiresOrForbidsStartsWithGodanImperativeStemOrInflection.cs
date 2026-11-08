using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;

static class RequiresOrForbidsStartsWithGodanImperativeStemOrInflection
{
   static readonly FailedMatchRequirement RequiredFailure = FailedMatchRequirement.Required("godan_imperative");
   static readonly FailedMatchRequirement ForbiddenFailure = FailedMatchRequirement.Forbids("godan_imperative");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = inspector.Match.RequiresForbids.GodanImperative;
      if(requirement.IsConfigured)
      {
         var isInState = inspector.Word.StartLocation.Token.IsGodanImperativeInflection ||
                         inspector.Word.StartLocation.Token.IsGodanImperativeStem;
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
