using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Migration;

/// <summary>
/// One of a vocab's matching rules as the corpus stores them today, a require/forbid rule (<see cref="RequireForbidFlagField"/>) or a configurable<br/>
/// rule (<see cref="VocabNoteMatchingRules"/>), and the condition it becomes in the vocab's match-condition. Each is defined once, in<br/>
/// <see cref="TodaysMatchingRules"/>.
/// </summary>
/// <remarks>
/// It also knows the requirement that tests the rule today, so that the side-by-side check (<see cref="ConditionAgreesWithTodaysRequirementOn"/>) can<br/>
/// tell which rule a disagreement comes from, where a diff of the corpus would show only that some sentence changed.
/// </remarks>
public sealed class TodaysMatchingRule
{
   readonly Func<VocabNoteMatchingConfiguration, bool> _isSetIn;
   readonly Func<VocabNoteMatchingConfiguration, Condition<CandidateWordVariant>> _conditionIn;
   readonly Func<VocabMatchInspector, FailedMatchRequirement?> _todaysRequirement;

   TodaysMatchingRule(string name,
                      Func<VocabNoteMatchingConfiguration, bool> isSetIn,
                      Func<VocabNoteMatchingConfiguration, Condition<CandidateWordVariant>> conditionIn,
                      Func<VocabMatchInspector, FailedMatchRequirement?> todaysRequirement)
   {
      Name = name;
      _isSetIn = isSetIn;
      _conditionIn = conditionIn;
      _todaysRequirement = todaysRequirement;
   }

   /// <summary>The rule's name in the corpus, such as <c>masuStem</c> or <c>prefixIsNot</c>.</summary>
   public string Name { get; }

   /// <summary>Whether <paramref name="configuration"/> sets the rule: requires or forbids it, or gives it texts. A rule that isn't set tests nothing.</summary>
   public bool IsSetIn(VocabNoteMatchingConfiguration configuration) => _isSetIn(configuration);

   /// <summary>The condition the rule becomes in the match-condition of a vocab whose <paramref name="configuration"/> sets it.</summary>
   public Condition<CandidateWordVariant> ConditionIn(VocabNoteMatchingConfiguration configuration)
   {
      Contract.Argument.Assert(IsSetIn(configuration), () => $"The rule {Name} isn't set, so it becomes no condition.");
      return _conditionIn(configuration);
   }

   /// <summary>
   /// Whether the rule's condition and the requirement that tests it today agree on whether <paramref name="match"/>, whose vocab sets the rule, is<br/>
   /// valid. When they agree on every match, the match-condition changes nothing in the parsing.
   /// </summary>
   public bool ConditionAgreesWithTodaysRequirementOn(VocabMatch match) =>
      ConditionIn(match.MatchingConfiguration).IsMetBy(match.Variant) == TodaysRequirementIsMetBy(match);

   /// <summary>Whether the requirement that tests the rule today lets <paramref name="match"/> be valid.</summary>
   public bool TodaysRequirementIsMetBy(VocabMatch match) => _todaysRequirement(match.VocabInspector) == null;

   /// <summary>
   /// The require/forbid rule <paramref name="rule"/>, named <paramref name="name"/> in the corpus, whose fact the condition <paramref name="whenRequired"/><br/>
   /// tests: forbidden, it becomes that condition negated.
   /// </summary>
   internal static TodaysMatchingRule RequireForbid(string name,
                                                    Func<VocabMatchingRequireForbidFlags, RequireForbidFlagField> rule,
                                                    Condition<CandidateWordVariant> whenRequired,
                                                    Func<VocabMatchInspector, FailedMatchRequirement?> todaysRequirement) =>
      RequireForbid(name, rule, whenRequired, ConditionGroup.Not(whenRequired), todaysRequirement);

   /// <summary>A require/forbid rule that tests one fact when required, <paramref name="whenRequired"/>, and another when forbidden, <paramref name="whenForbidden"/>.</summary>
   internal static TodaysMatchingRule RequireForbid(string name,
                                                    Func<VocabMatchingRequireForbidFlags, RequireForbidFlagField> rule,
                                                    Condition<CandidateWordVariant> whenRequired,
                                                    Condition<CandidateWordVariant> whenForbidden,
                                                    Func<VocabMatchInspector, FailedMatchRequirement?> todaysRequirement)
   {
      return new TodaysMatchingRule(name,
                                    it => RuleIn(it).IsConfigured,
                                    it => RuleIn(it).IsRequired ? whenRequired : whenForbidden,
                                    todaysRequirement);

      RequireForbidFlagField RuleIn(VocabNoteMatchingConfiguration configuration)
      {
         var field = rule(configuration.RequiresForbids);
         Contract.State.Assert(field.Name == name, () => $"The rule named {name} reads the rule {field.Name}.");
         return field;
      }
   }

   /// <summary>A configurable rule, the set of texts <paramref name="rule"/>: it becomes the condition <paramref name="conditionOn"/> makes of the texts.</summary>
   internal static TodaysMatchingRule Texts(string name,
                                            Func<VocabNoteMatchingRules, IReadOnlySet<string>> rule,
                                            Func<string[], Condition<CandidateWordVariant>> conditionOn,
                                            Func<VocabMatchInspector, FailedMatchRequirement?> todaysRequirement) =>
      new(name,
          it => rule(it.ConfigurableRules).Count > 0,
          it => conditionOn([.. rule(it.ConfigurableRules)]),
          todaysRequirement);

   public override string ToString() => Name;
}
