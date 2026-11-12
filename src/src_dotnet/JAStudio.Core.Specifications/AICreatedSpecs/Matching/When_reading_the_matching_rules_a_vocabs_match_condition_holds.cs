using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// What JAStudio reads off the matching rules (<see cref="MatchingRules"/>) a vocab's match-condition holds, now that it's edited as a tree: what it<br/>
/// weighs in on the vocab's priority, as the rules did before match-conditions, which rules it holds, and which it requires.
/// </summary>
public class When_reading_the_matching_rules_a_vocabs_match_condition_holds : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;

   public When_reading_the_matching_rules_a_vocabs_match_condition_holds() => _vocab = CreateVocab("あれる", "get-_/is-_", "あれる");

   VocabNoteMatchingConfiguration Configuration => _vocab.MatchingConfiguration;

   string[] NamesOfTheRulesItHolds() => [.. MatchingRules.All.Where(Configuration.MatchConditionHolds).Select(it => it.Name)];

   string[] NamesOfTheRulesItRequires() => [.. MatchingRules.All.OfType<RequireForbidMatchingRule>().Where(Configuration.MatchConditionRequires).Select(it => it.Name)];

   public class for_a_new_vocab : When_reading_the_matching_rules_a_vocabs_match_condition_holds
   {
      [XF] public void it_weighs_nothing() => Configuration.CustomRequirementsWeight.Must().Be(0);
      [XF] public void it_holds_no_rule() => NamesOfTheRulesItHolds().Must().BeEmpty();
   }

   public class with_a_masu_stem_required_a_godan_verb_forbidden_and_ん_a_prefix_it_must_not_follow : When_reading_the_matching_rules_a_vocabs_match_condition_holds
   {
      public with_a_masu_stem_required_a_godan_verb_forbidden_and_ん_a_prefix_it_must_not_follow() =>
         Configuration.SetMatchCondition(ConditionGroup.AllOf(MatchingRules.Godan.WhenForbidden,
                                                              !PrecedingToken.Surface.EndsWithOneOf("ん"),
                                                              MatchingRules.MasuStem.WhenRequired));

      [XF] public void it_weighs_what_the_rules_did_before_match_conditions_50_required_1_forbidden_and_2_for_texts() =>
         Configuration.CustomRequirementsWeight.Must().Be(53);

      [XF] public void it_holds_the_three_rules() => NamesOfTheRulesItHolds().Must().SequenceEqual(["godan", "prefixIsNot", "masuStem"]);
      [XF] public void it_requires_only_the_masu_stem() => NamesOfTheRulesItRequires().Must().SequenceEqual(["masuStem"]);
   }

   public class with_a_condition_that_is_no_rules : When_reading_the_matching_rules_a_vocabs_match_condition_holds
   {
      public with_a_condition_that_is_no_rules() => Configuration.SetMatchCondition(ConditionGroup.AllOf(FirstToken.Meets(TokenConditions.PartOfSpeech.Is("名詞"))));

      [XF] public void it_weighs_nothing_since_priority_knows_only_the_rules() => Configuration.CustomRequirementsWeight.Must().Be(0);
      [XF] public void it_holds_no_rule() => NamesOfTheRulesItHolds().Must().BeEmpty();
   }
}
