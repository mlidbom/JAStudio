using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests;

sealed class ForbidsIsShadowed : MatchRequirement
{
   readonly MatchInspector _inspector;

   public ForbidsIsShadowed(MatchInspector inspector) => _inspector = inspector;

   public override bool IsFulfilled => !_inspector.Match.IsShadowed;

   public override string FailureReason
   {
      get
      {
         var shadowedBy = _inspector.Match.Word.ShadowedBy;
         return shadowedBy != "" ? $"forbids::shadowed_by:{shadowedBy}" : "";
      }
   }
}
