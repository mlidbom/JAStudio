using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests;

static class ForbidsCompositionallyTransparentCompound
{
   static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("configured_to_hide_compositionally_transparent_compounds");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var match = inspector.Match;
      if(inspector.BreakdownParsingOptions is { HideCompositionallyTransparentCompounds: true } &&
         match.Word.IsCompound &&
         inspector.CompoundLocationsAllHaveValidNonCompoundMatches &&
         match.Vocab.MatchingConfiguration.Flags.IsCompositionallyTransparentCompound.IsSet())
      {
         return Failed;
      }

      return null;
   }
}
