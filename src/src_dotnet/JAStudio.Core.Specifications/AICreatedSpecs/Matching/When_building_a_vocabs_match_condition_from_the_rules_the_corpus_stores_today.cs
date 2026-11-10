using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Migration;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// A vocab's match-condition, built from the matching rules the corpus stores today (<see cref="TodaysMatchingRules"/>): all of the conditions its<br/>
/// rules become. That it changes nothing in the parsing is checked on the whole corpus, by the side-by-side check and a full rebuild.
/// </summary>
public class When_building_a_vocabs_match_condition_from_the_rules_the_corpus_stores_today : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;

   public When_building_a_vocabs_match_condition_from_the_rules_the_corpus_stores_today() => _vocab = CreateVocab("あれる", "get-_/is-_", "あれる");

   AllOfGroup<CandidateWordVariant> MatchCondition => TodaysMatchingRules.MatchConditionOf(_vocab.MatchingConfiguration);

   VocabMatchingRequireForbidFlags RequireForbidRules => _vocab.MatchingConfiguration.RequiresForbids;

   VocabNoteMatchingRules ConfigurableRules => _vocab.MatchingConfiguration.ConfigurableRules;

   [XF] public void a_vocab_that_sets_no_rule_gets_an_empty_match_condition_which_every_word_meets() => MatchCondition.IsEmpty.Must().BeTrue();

   public class for_a_vocab_that_requires_a_masu_stem_and_forbids_a_godan_verb_and_the_prefix_ん : When_building_a_vocabs_match_condition_from_the_rules_the_corpus_stores_today
   {
      public for_a_vocab_that_requires_a_masu_stem_and_forbids_a_godan_verb_and_the_prefix_ん()
      {
         RequireForbidRules.MasuStem.Choose(RequireForbidChoice.Required);
         RequireForbidRules.Godan.Choose(RequireForbidChoice.Forbidden);
         ConfigurableRules.PrefixIsNot.Add("ん");
      }

      [XF] public void each_rule_becomes_a_condition_a_forbidden_one_negated_in_the_order_matching_tests_them() =>
         MatchCondition.Must().Be(ConditionGroup.AllOf(ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)),
                                                       ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん")))),
                                                       PrecedingToken.Meets(TokenConditions.IsAMasuStem)));
   }

   public class for_a_vocab_that_forbids_the_surface : When_building_a_vocabs_match_condition_from_the_rules_the_corpus_stores_today
   {
      public for_a_vocab_that_forbids_the_surface() => RequireForbidRules.Surface.Choose(RequireForbidChoice.Forbidden);

      [XF] public void the_condition_is_that_it_is_not_matched_by_an_inflected_surface_since_that_is_what_the_rule_forbids() =>
         MatchCondition.Must().Be(ConditionGroup.AllOf(ConditionGroup.Not(IsMatchedByAnInflectedSurface)));
   }

   public class for_a_vocab_that_requires_dictionary_form_start : When_building_a_vocabs_match_condition_from_the_rules_the_corpus_stores_today
   {
      public for_a_vocab_that_requires_dictionary_form_start() => RequireForbidRules.DictionaryFormStart.Choose(RequireForbidChoice.Required);

      [XF] public void the_match_condition_does_not_hold_it_since_every_other_vocab_forbids_it_by_default() => MatchCondition.IsEmpty.Must().BeTrue();
   }
}
