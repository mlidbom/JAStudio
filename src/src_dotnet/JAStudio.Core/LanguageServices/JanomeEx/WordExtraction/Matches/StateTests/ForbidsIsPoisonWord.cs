using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;

static class ForbidsIsPoisonWord
{
   static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("poison_word");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      if(inspector.Match.Vocab.MatchingConfiguration.Flags.IsPoisonWord.IsSet())
      {
         return Failed;
      }

      return null;
   }
}
