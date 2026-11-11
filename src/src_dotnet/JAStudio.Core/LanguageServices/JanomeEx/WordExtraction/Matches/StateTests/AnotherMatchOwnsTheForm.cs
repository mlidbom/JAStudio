using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;

static class ForbidsAnotherMatchIsHigherPriority
{
   /// <summary>The failure of a match whose form another match owns, or that a match of a higher priority, by its custom requirements' weight, wins over.</summary>
   internal static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("another_match_owns_the_form_or_is_higher_priority_due_to_custom_requirements_weight");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      if(inspector.Match.AnotherMatchIsHigherPriority)
      {
         return Failed;
      }

      return null;
   }
}
