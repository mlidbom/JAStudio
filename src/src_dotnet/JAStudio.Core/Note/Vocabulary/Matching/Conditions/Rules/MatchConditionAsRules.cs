using System;
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
   public bool IsMadeOfRulesOnly => EachConditionIsExactlyOneRules && NoRulesConditionIsThereTwice;

   /// <summary>Whether the match-condition requires <paramref name="rule"/>, forbids it, or neither.</summary>
   public RequireForbidChoice ChoiceOf(RequireForbidMatchingRule rule) => ConditionOf(rule) is {} condition ? rule.ChoiceOf(condition) : RequireForbidChoice.Unset;

   /// <summary>Makes the match-condition require <paramref name="rule"/>, forbid it, or neither, and saves the vocab.</summary>
   public void Choose(RequireForbidMatchingRule rule, RequireForbidChoice choice) => Replace(rule, rule.ConditionFor(choice));

   /// <summary>The texts <paramref name="rule"/> has in the match-condition, in ordinal order: none when the match-condition doesn't hold it.</summary>
   public IReadOnlyList<string> TextsOf(TextsMatchingRule rule) => ConditionOf(rule) is {} condition ? rule.TextsIn(condition) : [];

   /// <summary>Adds <paramref name="text"/> to the texts <paramref name="rule"/> has in the match-condition, and saves the vocab.</summary>
   public void AddText(TextsMatchingRule rule, string text) => SetTexts(rule, [.. TextsOf(rule), text]);

   /// <summary>Removes <paramref name="text"/> from the texts <paramref name="rule"/> has in the match-condition, and saves the vocab: with none left, the match-condition no longer holds the rule.</summary>
   public void RemoveText(TextsMatchingRule rule, string text) => SetTexts(rule, [.. TextsOf(rule).Where(it => it != text)]);

   /// <summary>
   /// What the match-condition's conditions add to the vocab's priority: what each rule's weighs, as the rule did before match-conditions. Each of the<br/>
   /// match-condition's flattened conditions is weighed (<see cref="ConditionGroup{TSubject}.FlattenedConditions"/>), and each that means the same as a<br/>
   /// rule's (<see cref="Condition.MeansTheSameAs"/>) is that rule's, so a shared-condition weighs what its definition weighs.
   /// </summary>
   internal int MatchWeight => _configuration.MatchCondition.FlattenedConditions.Sum(condition => MatchingRules.All.Sum(rule => rule.WeightOf(condition)));

   IReadOnlyList<Condition<CandidateWordVariant>> Conditions => _configuration.MatchCondition.Conditions;

   bool EachConditionIsExactlyOneRules => Conditions.All(condition => MatchingRules.All.Count(rule => rule.Recognizes(condition)) == 1);

   bool NoRulesConditionIsThereTwice => MatchingRules.All.All(rule => Conditions.Count(rule.Recognizes) <= 1);

   Condition<CandidateWordVariant>? ConditionOf(MatchingRule rule) => Conditions.FirstOrDefault(rule.Recognizes);

   void SetTexts(TextsMatchingRule rule, IReadOnlyCollection<string> texts) => Replace(rule, rule.ConditionOn(texts));

   /// <summary>
   /// Makes <paramref name="condition"/> <paramref name="rule"/>'s condition in the match-condition, or removes the rule's when it's null, and saves the<br/>
   /// vocab. The match-condition holds the rules' conditions in the order of <see cref="MatchingRules.All"/>.
   /// </summary>
   void Replace(MatchingRule rule, Condition<CandidateWordVariant>? condition)
   {
      Contract.State.Assert(IsMadeOfRulesOnly, () => $"The match-condition has conditions that aren't matching rules', so it can't be edited rule by rule, only as a whole:{Environment.NewLine}{_configuration.MatchCondition}");
      _configuration.SetMatchCondition(ConditionGroup.AllOf([.. MatchingRules.All.Select(ConditionAfterTheChange).OfType<Condition<CandidateWordVariant>>()]));
      return;

      Condition<CandidateWordVariant>? ConditionAfterTheChange(MatchingRule each) => each == rule ? condition : ConditionOf(each);
   }
}
