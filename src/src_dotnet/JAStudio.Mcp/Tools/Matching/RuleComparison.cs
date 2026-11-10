using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>How one rule's condition compared with the requirement that tests it today.</summary>
/// <param name="Rule">The rule's name in the corpus, such as <c>masuStem</c>.</param>
/// <param name="MatchesTested">The vocab matches whose vocab sets the rule.</param>
/// <param name="Disagreements">The matches where the condition and today's requirement disagree on whether the match is valid.</param>
/// <param name="DisagreementsListed">The first of them.</param>
public record RuleComparison(string Rule, int MatchesTested, int Disagreements, IReadOnlyList<RuleDisagreement> DisagreementsListed);
