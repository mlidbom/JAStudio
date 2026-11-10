using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Migration;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>
/// The tools for moving the vocab's matching rules to match-conditions (dev_docs/matching-configuration.md): the side-by-side check that each<br/>
/// rule's condition agrees with the requirement that tests it today.
/// </summary>
/// <remarks>Goes with <see cref="TodaysMatchingRules"/>, once the corpus stores match-conditions.</remarks>
[McpServerToolType]
public partial class MatchConditionMigrationTools
{
   readonly JPCollection _collection;

   public MatchConditionMigrationTools(JPCollection collection) => _collection = collection;

   [McpServerTool(Name = "compare_match_conditions_with_todays_rules", ReadOnly = true, Idempotent = true)]
   [Description("""
                The side-by-side check of the move to match-conditions. Analyzes every sentence as its stored parsing result is made, and for every
                vocab match, tests each of the vocab's matching rules both ways: by the condition the rule becomes in the vocab's match-condition,
                and by the requirement that tests it today. Reports, for each rule, how many matches it was tested on and those where the two
                disagree on whether the match is valid. None anywhere means the match-conditions would change nothing in the parsing.
                """)]
   public MatchConditionComparison CompareMatchConditionsWithTodaysRules(
      [Description("The disagreements listed for each rule. Every one is counted either way.")] int maxDisagreementsListedPerRule = 10)
   {
      var stopwatch = Stopwatch.StartNew();
      var sentences = _collection.Sentences.All();
      var testsOfRules = sentences.SelectMany(TestsOfTheRulesIn).ToList();

      return new MatchConditionComparison(stopwatch.Elapsed.TotalSeconds,
                                          sentences.Count,
                                          testsOfRules.Select(it => it.Match).Distinct().Count(),
                                          [.. TodaysMatchingRules.ThatAMatchConditionReplaces.Select(ComparisonOf)]);

      RuleComparison ComparisonOf(TodaysMatchingRule rule)
      {
         var tests = testsOfRules.Where(it => it.Rule == rule).ToList();
         var disagreements = tests.Where(it => !it.Agrees).ToList();
         return new RuleComparison(rule.Name, tests.Count, disagreements.Count, [.. disagreements.Take(maxDisagreementsListedPerRule).Select(it => it.Disagreement())]);
      }
   }

   static IEnumerable<TestOfARule> TestsOfTheRulesIn(SentenceNote sentence) =>
      sentence.CreateAnalysis()
              .Locations
              .SelectMany(it => it.CandidateWords)
              .SelectMany(it => it.Variants)
              .SelectMany(it => it.VocabMatches)
              .SelectMany(match => TodaysMatchingRules.SetIn(match.MatchingConfiguration).Select(rule => new TestOfARule(sentence, match, rule)));
}
