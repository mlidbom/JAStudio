using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>What <see cref="MatchConditionMigrationTools.CompareMatchConditionsWithTodaysRules"/> reports.</summary>
/// <param name="SecondsTaken">How long the check took.</param>
/// <param name="SentencesAnalyzed">Every sentence in the collection.</param>
/// <param name="VocabMatchesTested">The vocab matches whose vocab sets at least one of the rules.</param>
/// <param name="Rules">Each rule a match-condition replaces, with how it compared.</param>
public record MatchConditionComparison(double SecondsTaken, int SentencesAnalyzed, int VocabMatchesTested, IReadOnlyList<RuleComparison> Rules);
