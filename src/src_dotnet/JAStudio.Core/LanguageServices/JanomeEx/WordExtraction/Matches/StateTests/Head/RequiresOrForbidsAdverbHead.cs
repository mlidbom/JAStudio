using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

static class RequiresOrForbidsAdverbHead
{
   static readonly FailedMatchRequirement RequiredReason = FailedMatchRequirement.Required("preceding-adverb");
   static readonly FailedMatchRequirement ForbiddenReason = FailedMatchRequirement.Forbids("preceding-adverb");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      if(inspector.RequiresForbids.AdverbHead.IsRequired && !inspector.HasAdverbHead)
      {
         return RequiredReason;
      }

      if(inspector.RequiresForbids.AdverbHead.IsForbidden && inspector.HasAdverbHead)
      {
         return ForbiddenReason;
      }

      return null;
   }
}
