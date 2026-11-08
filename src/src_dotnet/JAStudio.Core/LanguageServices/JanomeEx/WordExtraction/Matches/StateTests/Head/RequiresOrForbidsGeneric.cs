using System;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.Note.Vocabulary.Matching;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

sealed class RequiresOrForbids
{
   readonly Func<VocabMatchInspector, RequireForbidFlagField> _getRequirement;
   readonly Func<VocabMatchInspector, bool> _isInState;
   readonly FailedMatchRequirement _requiredFailure;
   readonly FailedMatchRequirement _forbiddenFailure;

   public RequiresOrForbids(
      string name,
      Func<VocabMatchInspector, RequireForbidFlagField> getRequirement,
      Func<VocabMatchInspector, bool> isInState)
   {
      _getRequirement = getRequirement;
      _isInState = isInState;
      _requiredFailure = FailedMatchRequirement.Required(name);
      _forbiddenFailure = FailedMatchRequirement.Forbids(name);
   }

   public FailedMatchRequirement? ApplyTo(VocabMatchInspector inspector)
   {
      var requirement = _getRequirement(inspector);

      if(requirement.IsRequired && !_isInState(inspector))
      {
         return _requiredFailure;
      }

      if(requirement.IsForbidden && _isInState(inspector))
      {
         return _forbiddenFailure;
      }

      return null;
   }
}
