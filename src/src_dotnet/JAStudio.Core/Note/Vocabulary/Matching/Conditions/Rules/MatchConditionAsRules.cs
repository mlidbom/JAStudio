using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;

/// <summary>
/// A vocab's match-condition seen as the matching rules (<see cref="MatchingRules"/>) its conditions are, as the Matching tab shows it: which rules are<br/>
/// required or forbidden, and the texts of the others. While it's made of rules only (<see cref="IsMadeOfRulesOnly"/>), it's edited rule by rule here.
/// </summary>
public sealed class MatchConditionAsRules
{
   readonly VocabNoteMatchingConfiguration _configuration;

   internal MatchConditionAsRules(VocabNoteMatchingConfiguration configuration) => _configuration = configuration;

   /// <summary>
   /// Whether each condition of the match-condition is one rule's, and no rule's is there twice. Every vocab migrated from the rules is, and stays so<br/>
   /// while edited here. A match-condition given other conditions can only be edited as a whole.
   /// </summary>
   public bool IsMadeOfRulesOnly => Conditions.All(condition => MatchingRules.All.Count(rule => rule.Recognizes(condition)) == 1) &&
                                    MatchingRules.All.All(rule => Conditions.Count(rule.Recognizes) <= 1);

   /// <summary>Whether the match-condition requires <paramref name="rule"/>, forbids it, or neither.</summary>
   public RequireForbidChoice ChoiceOf(RequireForbidMatchingRule rule) => ConditionOf(rule) is {} condition ? rule.ChoiceOf(condition) : RequireForbidChoice.Unset;

   /// <summary>Makes the match-condition require <paramref name="rule"/>, forbid it, or neither, and saves the vocab.</summary>
   public void Choose(RequireForbidMatchingRule rule, RequireForbidChoice choice) =>
      Replace(rule, choice == RequireForbidChoice.Unset ? null : rule.ConditionFor(choice));

   /// <summary>The texts <paramref name="rule"/> has in the match-condition: none when the match-condition doesn't hold it.</summary>
   public IReadOnlySet<string> TextsOf(TextsMatchingRule rule) => ConditionOf(rule) is {} condition ? rule.TextsIn(condition)!.Texts.ToHashSet() : new HashSet<string>();

   /// <summary>Gives <paramref name="rule"/> <paramref name="texts"/> in the match-condition, and saves the vocab: with none, the match-condition doesn't hold it.</summary>
   public void SetTexts(TextsMatchingRule rule, IEnumerable<string> texts)
   {
      var textsGiven = texts.ToList();
      Replace(rule, textsGiven.Count == 0 ? null : rule.ConditionOn(textsGiven));
   }

   /// <summary>What the match-condition's conditions add to the vocab's priority: what each rule's weighs, as the rule did before match-conditions.</summary>
   internal int Weight => Conditions.Sum(condition => MatchingRules.All.Sum(rule => rule.WeightOf(condition)));

   IReadOnlyList<Condition<CandidateWordVariant>> Conditions => _configuration.MatchCondition.Conditions;

   Condition<CandidateWordVariant>? ConditionOf(MatchingRule rule) => Conditions.FirstOrDefault(rule.Recognizes);

   /// <summary>The match-condition with <paramref name="rule"/>'s condition replaced by <paramref name="condition"/>, or removed when it's null, each rule's in the order of <see cref="MatchingRules.All"/>.</summary>
   void Replace(MatchingRule rule, Condition<CandidateWordVariant>? condition)
   {
      Contract.State.Assert(IsMadeOfRulesOnly, () => $"The match-condition has conditions that aren't matching rules', so it can't be edited rule by rule, only as a whole:{System.Environment.NewLine}{_configuration.MatchCondition}");
      _configuration.SetMatchCondition(ConditionGroup.AllOf([.. MatchingRules.All.Select(it => it == rule ? condition : ConditionOf(it)).OfType<Condition<CandidateWordVariant>>()]));
   }
}
