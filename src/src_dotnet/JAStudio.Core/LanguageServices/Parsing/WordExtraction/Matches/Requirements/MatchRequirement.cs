namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;

public abstract class MatchRequirement
{
   public abstract bool IsFulfilled { get; }
   public abstract string FailureReason { get; }

   public override string ToString() => FailureReason;
}
