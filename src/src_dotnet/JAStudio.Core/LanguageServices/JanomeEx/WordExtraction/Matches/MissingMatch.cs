using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;

/// <summary>A single token no vocab and no dictionary entry has: never valid, but displayed when nothing valid covers it, so that no part of the sentence goes missing from the breakdown.</summary>
public sealed class MissingMatch : Match
{
   internal MissingMatch(CandidateWordVariant wordVariant) : base(wordVariant) {}

   public override string Answer => "---";
   public override string MatchForm => "[MISSING]"; // Change this so the tests can distinguish that this is a missing match
   public override bool IsValid => false;
   public override List<string> Readings => [];

   public override List<string> FailureReasons =>
      [..base.FailureReasons, "no_dictionary_or_vocabulary_match_found"];
}
