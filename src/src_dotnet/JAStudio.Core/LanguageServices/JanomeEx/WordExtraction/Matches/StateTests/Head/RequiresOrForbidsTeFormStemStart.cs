using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

static class RequiresOrForbidsTeFormStemStart
{
   static readonly FailedMatchRequirement RequiredFailure = FailedMatchRequirement.Required("te_form_stem");
   static readonly FailedMatchRequirement ForbiddenFailure = FailedMatchRequirement.Forbids("te_form_stem");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = inspector.Match.RequiresForbids.TeFormStemStart;

      if(requirement.IsRequired && !inspector.HasTeFormStem)
      {
         return RequiredFailure;
      }

      if(requirement.IsForbidden && inspector.HasTeFormStem)
      {
         return ForbiddenFailure;
      }

      return null;
   }
}
