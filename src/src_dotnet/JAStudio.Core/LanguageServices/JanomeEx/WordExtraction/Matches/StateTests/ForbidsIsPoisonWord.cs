using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;

static class ForbidsIsPoisonWord
{
   /// <summary>The failure of a match of a poison word, which is never to match: the vocab is there to keep its form from matching.</summary>
   internal static readonly FailedMatchRequirement Failed = FailedMatchRequirement.Forbids("poison_word");

   public static FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      if(inspector.Match.Vocab.MatchingConfiguration.Flags.IsPoisonWord.IsSet())
      {
         return Failed;
      }

      return null;
   }
}
