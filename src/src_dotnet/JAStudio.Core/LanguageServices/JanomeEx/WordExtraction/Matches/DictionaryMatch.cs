using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;

/// <summary>A match of a candidate word to an entry in the dictionary, when no vocab decides whether the word matches.</summary>
public sealed class DictionaryMatch : Match
{
   static readonly List<System.Func<MatchInspector, FailedMatchRequirement?>> RequirementsList =
   [
      new Forbids("dict_match_with_dictionary_form_stem",
                  it => it.StartLocationIsDictionaryVerbInflection).ApplyTo
   ];

   static readonly List<System.Func<MatchInspector, FailedMatchRequirement?>> CombinedRequirements;

   static DictionaryMatch()
   {
      var matchRequirements = typeof(Match).GetField("MatchPrimaryValidityRequirements",
                                                     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                                .GetValue(null) as List<System.Func<MatchInspector, FailedMatchRequirement?>>;

      CombinedRequirements = new List<System.Func<MatchInspector, FailedMatchRequirement?>>(
         matchRequirements ?? []);
      CombinedRequirements.AddRange(RequirementsList);
   }

   public DictEntry DictionaryEntry { get; }

   internal DictionaryMatch(CandidateWordVariant wordVariant, DictEntry dictionaryEntry) : base(wordVariant) => DictionaryEntry = dictionaryEntry;

   public override string Answer => DictionaryEntry.FormatAnswer();
   public override List<string> Readings => DictionaryEntry.KanaFormsText();

   protected override bool IsPrimarilyValid()
   {
      return !CombinedRequirements
             .Select(requirement => requirement(Inspector))
             .Any(failure => failure != null);
   }

   protected override List<FailedMatchRequirement> CreatePrimaryValidityFailures()
   {
      return CombinedRequirements
            .Select(requirement => requirement(Inspector))
            .Where(failure => failure != null)
            .Cast<FailedMatchRequirement>()
            .ToList();
   }
}
