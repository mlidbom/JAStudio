using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Tail;

static class RequiresOrForbidsIsSentenceEnd
{
   static readonly FailedMatchRequirement RequiredFailure = FailedMatchRequirement.Required("sentence_end");
   static readonly FailedMatchRequirement ForbiddenFailure = FailedMatchRequirement.Forbids("sentence_end");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = inspector.Match.RequiresForbids.SentenceEnd;
      if(requirement.IsConfigured)
      {
         var isInState = inspector.IsEndOfStatement;
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
