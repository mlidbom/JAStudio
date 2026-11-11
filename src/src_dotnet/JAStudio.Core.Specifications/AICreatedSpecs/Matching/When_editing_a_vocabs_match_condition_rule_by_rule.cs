using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static Compze.Must.MustActions;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// A vocab's match-condition edited through the matching rules it's made of (<see cref="MatchConditionAsRules"/>), as the Matching tab and the MCP<br/>
/// server edit it: each rule set is its condition, in the order of <see cref="MatchingRules.All"/>, and weighs what the rule did before match-conditions.
/// </summary>
public class When_editing_a_vocabs_match_condition_rule_by_rule : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;

   public When_editing_a_vocabs_match_condition_rule_by_rule() => _vocab = CreateVocab("あれる", "get-_/is-_", "あれる");

   VocabNoteMatchingConfiguration Configuration => _vocab.MatchingConfiguration;

   public class for_a_new_vocab : When_editing_a_vocabs_match_condition_rule_by_rule
   {
      [XF] public void the_match_condition_is_empty_so_every_word_meets_it() => Configuration.MatchCondition.IsEmpty.Must().BeTrue();
      [XF] public void it_is_made_of_rules_only() => Configuration.Rules.IsMadeOfRulesOnly.Must().BeTrue();
      [XF] public void every_require_forbid_rule_is_unset() =>
         MatchingRules.All.OfType<RequireForbidMatchingRule>().Select(Configuration.Rules.ChoiceOf).Distinct().Must().SequenceEqual([RequireForbidChoice.Unset]);

      [XF] public void every_texts_rule_has_no_texts() => MatchingRules.All.OfType<TextsMatchingRule>().SelectMany(Configuration.Rules.TextsOf).Must().BeEmpty();
      [XF] public void it_weighs_nothing() => Configuration.CustomRequirementsWeight.Must().Be(0);
   }

   public class after_requiring_a_masu_stem_forbidding_a_godan_verb_and_adding_ん_to_prefix_is_not : When_editing_a_vocabs_match_condition_rule_by_rule
   {
      public after_requiring_a_masu_stem_forbidding_a_godan_verb_and_adding_ん_to_prefix_is_not()
      {
         Configuration.Rules.Choose(MatchingRules.MasuStem, RequireForbidChoice.Required);
         Configuration.Rules.Choose(MatchingRules.Godan, RequireForbidChoice.Forbidden);
         Configuration.Rules.AddText(MatchingRules.PrefixIsNot, "ん");
      }

      [XF] public void the_match_condition_is_all_of_the_rules_conditions_a_forbidden_one_negated_in_the_rules_order() =>
         Configuration.MatchCondition.Must().Be(ConditionGroup.AllOf(ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)),
                                                                     ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん")))),
                                                                     PrecedingToken.Meets(TokenConditions.IsAMasuStem)));

      [XF] public void the_required_rule_reads_as_required() => Configuration.Rules.ChoiceOf(MatchingRules.MasuStem).Must().Be(RequireForbidChoice.Required);
      [XF] public void the_forbidden_rule_reads_as_forbidden() => Configuration.Rules.ChoiceOf(MatchingRules.Godan).Must().Be(RequireForbidChoice.Forbidden);
      [XF] public void the_texts_rule_reads_its_texts() => Configuration.Rules.TextsOf(MatchingRules.PrefixIsNot).Must().SequenceEqual(["ん"]);

      [XF] public void it_weighs_what_the_rules_did_before_match_conditions_50_required_1_forbidden_and_2_for_texts() =>
         Configuration.CustomRequirementsWeight.Must().Be(53);

      public class and_then_unsetting_the_required_rule_and_removing_ん_from_prefix_is_not : after_requiring_a_masu_stem_forbidding_a_godan_verb_and_adding_ん_to_prefix_is_not
      {
         public and_then_unsetting_the_required_rule_and_removing_ん_from_prefix_is_not()
         {
            Configuration.Rules.Choose(MatchingRules.MasuStem, RequireForbidChoice.Unset);
            Configuration.Rules.RemoveText(MatchingRules.PrefixIsNot, "ん");
         }

         [XF] public void only_the_forbidden_rules_condition_is_left() =>
            Configuration.MatchCondition.Must().Be(ConditionGroup.AllOf(ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAGodanVerb))));
      }
   }

   public class after_forbidding_the_surface : When_editing_a_vocabs_match_condition_rule_by_rule
   {
      public after_forbidding_the_surface() => Configuration.Rules.Choose(MatchingRules.Surface, RequireForbidChoice.Forbidden);

      [XF] public void the_condition_is_that_it_is_not_matched_by_an_inflected_surface_since_that_is_what_the_rule_forbids() =>
         Configuration.MatchCondition.Must().Be(ConditionGroup.AllOf(ConditionGroup.Not(IsMatchedByAnInflectedSurface)));
   }

   public class after_giving_the_match_condition_a_condition_that_is_no_rules : When_editing_a_vocabs_match_condition_rule_by_rule
   {
      public after_giving_the_match_condition_a_condition_that_is_no_rules() =>
         Configuration.SetMatchCondition(ConditionGroup.AllOf(FirstToken.Meets(TokenConditions.PartOfSpeechIs("名詞"))));

      [XF] public void it_is_not_made_of_rules_only() => Configuration.Rules.IsMadeOfRulesOnly.Must().BeFalse();
      [XF] public void choosing_a_rule_throws_since_it_can_only_be_edited_as_a_whole() =>
         Invoking(() => Configuration.Rules.Choose(MatchingRules.MasuStem, RequireForbidChoice.Required)).Must().Throw<StateAssertionFailedException>();

      [XF] public void the_condition_weighs_nothing_since_priority_knows_only_the_rules() => Configuration.CustomRequirementsWeight.Must().Be(0);
   }
}
