using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;

public sealed class FailedMatchRequirement : MatchRequirement
{
   public string Reason { get; }

   FailedMatchRequirement(string reason) => Reason = reason;

   public override string FailureReason => Reason;
   public override bool IsFulfilled => false;

   public static FailedMatchRequirement Forbids(string message) =>
      new($"forbids::{message}");

   /// <summary>The failure of a word to meet <paramref name="condition"/>, one of the conditions of a vocab's match-condition, given as the condition describes itself.</summary>
   public static FailedMatchRequirement NotMet(Condition<CandidateWordVariant> condition) =>
      new($"not_met::{condition}");

   public static FailedMatchRequirement Required(string message) =>
      new($"required::{message}");
}
