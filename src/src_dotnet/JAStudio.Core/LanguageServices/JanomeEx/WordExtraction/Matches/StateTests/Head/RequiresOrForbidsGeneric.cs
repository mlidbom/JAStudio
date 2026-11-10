using System;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.Note.Vocabulary.Matching;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

sealed class RequiresOrForbids
{
   public static readonly RequiresOrForbids Irrealis = new("irrealis", it => it.RequiresForbids.Irrealis, it => it.PreviousLocationIsIrrealis);
   public static readonly RequiresOrForbids Godan = new("godan", it => it.RequiresForbids.Godan, it => it.PreviousLocationIsGodan);
   public static readonly RequiresOrForbids Ichidan = new("ichidan", it => it.RequiresForbids.Ichidan, it => it.PreviousLocationIsIchidan);
   public static readonly RequiresOrForbids TeFormStemHead = new("te_form_prefix", it => it.RequiresForbids.TeFormStemHead, it => it.HasTeFormStemHead);

   readonly Func<VocabMatchInspector, RequireForbidFlagField> _getRequirement;
   readonly Func<VocabMatchInspector, bool> _isInState;
   readonly FailedMatchRequirement _requiredFailure;
   readonly FailedMatchRequirement _forbiddenFailure;

   RequiresOrForbids(
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
