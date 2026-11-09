using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;

static class ForbidsConfiguredToHideAllCompounds
{
   static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("configured_to_hide_all_compounds");

   public static FailedMatchRequirement? ApplyTo(MatchInspector inspector)
   {
      if(inspector.BreakdownParsingOptions is { HideAllCompounds: true } &&
         inspector.Word.IsCompound &&
         inspector.CompoundLocationsAllHaveValidNonCompoundMatches &&
         !inspector.IsVerbDictionaryFormCompound &&
         !inspector.IsIchidanCoveringGodanPotential)
      {
         return Failed;
      }

      return null;
   }
}
