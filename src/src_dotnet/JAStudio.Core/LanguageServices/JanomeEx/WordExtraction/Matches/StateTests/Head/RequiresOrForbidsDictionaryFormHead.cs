using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

static class RequiresOrForbidsDictionaryFormHead
{
   static readonly FailedMatchRequirement RequiredFailure = FailedMatchRequirement.Required("dictionary_form_prefix");
   static readonly FailedMatchRequirement ForbiddenFailure = FailedMatchRequirement.Forbids("dictionary_form_prefix");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = inspector.Match.RequiresForbids.DictionaryFormHead;
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
      if(inspector.PreviousLocation != null && inspector.PreviousLocation.Token.IsDictionaryVerbInflection)
      {
         return true;
      }

      return false;
   }
}
