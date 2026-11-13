using System;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;

sealed class Forbids
{
   readonly Func<MatchInspector, bool> _isInState;
   readonly FailedMatchRequirement _requiredFailure;

   public Forbids(string name, Func<MatchInspector, bool> isInState)
   {
      _isInState = isInState;
      _requiredFailure = FailedMatchRequirement.Forbids(name);
   }

   public FailedMatchRequirement? ApplyTo(MatchInspector inspector)
   {
      if(_isInState(inspector))
      {
         return _requiredFailure;
      }

      return null;
   }
}
