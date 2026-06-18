using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

static class RequiresOrForbidsDictionaryFormStart
{
   static readonly FailedMatchRequirement RequiredFailure = FailedMatchRequirement.Required("dictionary_form_stem");
   static readonly FailedMatchRequirement ForbiddenFailure = FailedMatchRequirement.Forbids("dictionary_form_stem");
   static readonly FailedMatchRequirement ForbiddenByDefaultFailure = FailedMatchRequirement.Forbids("dictionary_form_stem_forbidden_by_default");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = inspector.Match.RequiresForbids.DictionaryFormStart;

      var isInState = InternalIsInState(inspector);
      if(isInState)
      {
         if(requirement.IsRequired)
         {
            return null;
         }

         if(requirement.IsForbidden)
         {
            return ForbiddenFailure;
         }

         return ForbiddenByDefaultFailure;
      }

      if(requirement.IsRequired)
      {
         return RequiredFailure;
      }

      return null;
   }

   static bool InternalIsInState(VocabMatchInspector inspector)
   {
      if(inspector.StartLocationIsDictionaryVerbInflection)
      {
         return true;
      }

      return false;
   }
}
