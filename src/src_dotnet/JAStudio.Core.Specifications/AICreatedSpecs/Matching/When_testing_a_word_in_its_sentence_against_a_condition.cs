using System;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// A word in its sentence, a candidate word's surface or base form as JAStudio's analysis of a real sentence finds it, meets the conditions of a<br/>
/// match-condition or doesn't: conditions on its tokens, on its texts, and facts about it, combined with all of, any of and not.
/// </summary>
public abstract class When_testing_a_word_in_its_sentence_against_a_condition : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   /// <summary>The word the specifications test, set by each context's constructor.</summary>
   protected abstract CandidateWordVariant Word { get; }

   /// <summary>The surface of the candidate word <paramref name="word"/> in <paramref name="sentence"/>.</summary>
   protected CandidateWordVariant TheSurfaceOf(string word, string sentence) => TheCandidateWord(word, sentence).SurfaceVariant;

   /// <summary>The base form of the candidate word whose surface is <paramref name="word"/> in <paramref name="sentence"/>.</summary>
   protected CandidateWordVariant TheBaseFormOf(string word, string sentence) =>
      TheCandidateWord(word, sentence).BaseVariant ?? throw new InvalidOperationException($"{word} has no base form of its own in {sentence}.");

   protected bool Meets(Condition<CandidateWordVariant> condition) => condition.IsMetBy(Word);

   CandidateWord TheCandidateWord(string surface, string sentence) =>
      CreateSentence(sentence).CreateAnalysis().Locations.SelectMany(it => it.CandidateWords).Single(it => it.SurfaceForm == surface);

   public class the_word_様です_in_夏目様ですよね : When_testing_a_word_in_its_sentence_against_a_condition
   {
      public the_word_様です_in_夏目様ですよね() => Word = TheSurfaceOf("様です", "夏目様ですよね");

      protected override CandidateWordVariant Word { get; }

      [XF] public void its_first_token_is_a_suffix_to_a_persons_name() => Meets(FirstToken.Meets(TokenConditions.PartOfSpeechIs("名詞", "接尾", "人名"))).Must().BeTrue();
      [XF] public void its_first_token_is_a_kind_of_suffix_noun() => Meets(FirstToken.Meets(TokenConditions.PartOfSpeechIs("名詞", "接尾"))).Must().BeTrue();
      [XF] public void its_first_token_is_a_kind_of_noun() => Meets(FirstToken.Meets(TokenConditions.PartOfSpeechIs("名詞"))).Must().BeTrue();
      [XF] public void its_first_token_is_not_a_general_noun() => Meets(FirstToken.Meets(TokenConditions.PartOfSpeechIs("名詞", "一般"))).Must().BeFalse();
      [XF] public void its_preceding_token_is_a_proper_noun() => Meets(PrecedingToken.Meets(TokenConditions.PartOfSpeechIs("名詞", "固有名詞"))).Must().BeTrue();
      [XF] public void its_last_token_has_the_inflection_type_of_です() => Meets(LastToken.Meets(TokenConditions.InflectionTypeIsOneOf("特殊・デス"))).Must().BeTrue();
      [XF] public void its_last_token_has_an_inflection_type_in_the_family_of_the_special_ones() => Meets(LastToken.Meets(TokenConditions.InflectionTypeIsOneOf("特殊"))).Must().BeTrue();
      [XF] public void its_last_token_has_no_godan_inflection_type() => Meets(LastToken.Meets(TokenConditions.InflectionTypeIsOneOf("五段"))).Must().BeFalse();
      [XF] public void its_following_token_is_a_sentence_ending_particle() => Meets(FollowingToken.Meets(TokenConditions.PartOfSpeechIs("助詞", "終助詞"))).Must().BeTrue();
      [XF] public void the_text_preceding_it_is_夏目() => Meets(PrecedingText.Meets(TextConditions.In("夏目"))).Must().BeTrue();
      [XF] public void the_text_following_it_starts_with_one_of_よ_and_か() => Meets(FollowingText.Meets(TextConditions.StartsWithOneOf("よ", "か"))).Must().BeTrue();
      [XF] public void the_text_following_it_ends_with_ね() => Meets(FollowingText.Meets(TextConditions.EndsWithOneOf("ね"))).Must().BeTrue();
      [XF] public void the_text_following_it_contains_よ() => Meets(FollowingText.Meets(TextConditions.ContainsOneOf("よ"))).Must().BeTrue();
      [XF] public void the_text_following_it_is_not_よ_alone() => Meets(FollowingText.Meets(TextConditions.In("よ"))).Must().BeFalse();
      [XF] public void the_text_following_it_is_matched_by_the_regular_expression_for_よ_and_one_more_character() => Meets(FollowingText.Meets(TextConditions.MatchesRegularExpression("^よ.$"))).Must().BeTrue();
      [XF] public void the_text_following_it_is_not_empty() => Meets(FollowingText.Meets(TextConditions.IsEmpty)).Must().BeFalse();
      [XF] public void its_surface_is_様です() => Meets(Surface.Meets(TextConditions.In("様です"))).Must().BeTrue();
      [XF] public void it_is_not_a_single_token() => Meets(IsASingleToken).Must().BeFalse();
      [XF] public void it_is_matched_by_its_surface() => Meets(IsMatchedByItsSurface).Must().BeTrue();

      public class which_is_matched_by_its_surface_and_is_not_a_single_token_against_groups_of_those_two_facts : the_word_様です_in_夏目様ですよね
      {
         static readonly Condition<CandidateWordVariant> Met = IsMatchedByItsSurface;
         static readonly Condition<CandidateWordVariant> NotMet = IsASingleToken;

         [XF] public void all_of_is_met_when_every_condition_in_it_is() => Meets(ConditionGroup.AllOf(Met, Met)).Must().BeTrue();
         [XF] public void all_of_is_not_met_when_one_condition_in_it_is_not() => Meets(ConditionGroup.AllOf(Met, NotMet)).Must().BeFalse();
         [XF] public void any_of_is_met_when_one_condition_in_it_is() => Meets(ConditionGroup.AnyOf(NotMet, Met)).Must().BeTrue();
         [XF] public void any_of_is_not_met_when_no_condition_in_it_is() => Meets(ConditionGroup.AnyOf(NotMet, NotMet)).Must().BeFalse();
         [XF] public void not_is_met_when_its_condition_is_not() => Meets(ConditionGroup.Not(NotMet)).Must().BeTrue();
         [XF] public void not_is_not_met_when_its_condition_is() => Meets(ConditionGroup.Not(Met)).Must().BeFalse();
         [XF] public void a_group_with_no_conditions_adds_nothing_so_an_empty_any_of_is_met() => Meets(ConditionGroup.AnyOf<CandidateWordVariant>()).Must().BeTrue();
         [XF] public void an_empty_group_in_a_group_is_ignored() => Meets(ConditionGroup.AnyOf(ConditionGroup.AllOf<CandidateWordVariant>(), NotMet)).Must().BeFalse();
      }
   }

   public class the_word_夏目_at_the_start_of_夏目様ですよね : When_testing_a_word_in_its_sentence_against_a_condition
   {
      public the_word_夏目_at_the_start_of_夏目様ですよね() => Word = TheSurfaceOf("夏目", "夏目様ですよね");

      protected override CandidateWordVariant Word { get; }

      [XF] public void its_preceding_token_does_not_exist() => Meets(PrecedingToken.Meets(TokenConditions.Exists)).Must().BeFalse();
      [XF] public void its_preceding_token_is_no_godan_verb() => Meets(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)).Must().BeFalse();
      [XF] public void its_preceding_token_meets_a_negated_fact_as_the_empty_token_has_no_facts() => Meets(PrecedingToken.Meets(ConditionGroup.Not(TokenConditions.IsAGodanVerb))).Must().BeTrue();
      [XF] public void the_negation_of_a_fact_about_its_preceding_token_is_met_too() => Meets(ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAGodanVerb))).Must().BeTrue();
      [XF] public void its_preceding_token_has_no_part_of_speech() => Meets(PrecedingToken.Meets(TokenConditions.PartOfSpeechIs("名詞"))).Must().BeFalse();
      [XF] public void its_preceding_tokens_surface_is_empty() => Meets(PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.IsEmpty))).Must().BeTrue();
      [XF] public void its_preceding_tokens_reading_is_empty() => Meets(PrecedingToken.Meets(TokenConditions.Reading.Meets(TextConditions.IsEmpty))).Must().BeTrue();
      [XF] public void a_condition_on_its_preceding_token_that_adds_nothing_is_met() => Meets(PrecedingToken.Meets(ConditionGroup.AllOf<IAnalysisToken>())).Must().BeTrue();
      [XF] public void its_first_token_exists() => Meets(FirstToken.Meets(TokenConditions.Exists)).Must().BeTrue();
      [XF] public void the_text_preceding_it_is_empty() => Meets(PrecedingText.Meets(TextConditions.IsEmpty)).Must().BeTrue();
      [XF] public void it_is_a_single_token() => Meets(IsASingleToken).Must().BeTrue();
   }

   public class the_word_よね_at_the_end_of_夏目様ですよね : When_testing_a_word_in_its_sentence_against_a_condition
   {
      public the_word_よね_at_the_end_of_夏目様ですよね() => Word = TheSurfaceOf("よね", "夏目様ですよね");

      protected override CandidateWordVariant Word { get; }

      [XF] public void its_following_token_does_not_exist() => Meets(FollowingToken.Meets(TokenConditions.Exists)).Must().BeFalse();
      [XF] public void its_following_token_is_no_non_word_character() => Meets(FollowingToken.Meets(TokenConditions.IsANonWordCharacter)).Must().BeFalse();
      [XF] public void its_following_token_does_not_end_a_statement_as_the_empty_token_has_no_facts() => Meets(FollowingToken.Meets(TokenConditions.EndsAStatement)).Must().BeFalse();
      [XF] public void the_text_following_it_is_empty() => Meets(FollowingText.Meets(TextConditions.IsEmpty)).Must().BeTrue();
      [XF] public void its_last_token_ends_a_statement() => Meets(LastToken.Meets(TokenConditions.EndsAStatement)).Must().BeTrue();
   }

   public class the_word_食べる_in_食べるな_which_JAStudio_splits_into_the_verbs_stem_and_its_ending : When_testing_a_word_in_its_sentence_against_a_condition
   {
      public the_word_食べる_in_食べるな_which_JAStudio_splits_into_the_verbs_stem_and_its_ending() => Word = TheSurfaceOf("食べる", "食べるな");

      protected override CandidateWordVariant Word { get; }

      [XF] public void its_first_token_is_a_dictionary_form_verbs_stem() => Meets(FirstToken.Meets(TokenConditions.IsADictionaryVerbFormStem)).Must().BeTrue();
      [XF] public void its_last_token_is_a_dictionary_form_verbs_ending() => Meets(LastToken.Meets(TokenConditions.IsADictionaryVerbInflection)).Must().BeTrue();
      [XF] public void its_ending_has_the_part_of_speech_of_the_verb_it_was_split_from() => Meets(LastToken.Meets(TokenConditions.PartOfSpeechIs("動詞"))).Must().BeTrue();
      [XF] public void its_stems_surface_is_食べ() => Meets(FirstToken.Meets(TokenConditions.Surface.Meets(TextConditions.In("食べ")))).Must().BeTrue();
      [XF] public void its_stems_base_form_is_食べる() => Meets(FirstToken.Meets(TokenConditions.BaseForm.Meets(TextConditions.In("食べる")))).Must().BeTrue();
      [XF] public void its_stems_reading_is_that_of_the_whole_verb() => Meets(FirstToken.Meets(TokenConditions.Reading.Meets(TextConditions.In("タベル")))).Must().BeTrue();
      [XF] public void it_is_not_matched_by_an_inflected_surface() => Meets(IsMatchedByAnInflectedSurface).Must().BeFalse();
   }

   public class the_word_な_in_食べるな : When_testing_a_word_in_its_sentence_against_a_condition
   {
      public the_word_な_in_食べるな() => Word = TheSurfaceOf("な", "食べるな");

      protected override CandidateWordVariant Word { get; }

      [XF] public void its_preceding_token_is_a_dictionary_form_verbs_ending() => Meets(PrecedingToken.Meets(TokenConditions.IsADictionaryVerbInflection)).Must().BeTrue();
      [XF] public void its_preceding_token_exists() => Meets(PrecedingToken.Meets(TokenConditions.Exists)).Must().BeTrue();
   }

   public class the_surface_of_the_word_食べ_in_もう食べた : When_testing_a_word_in_its_sentence_against_a_condition
   {
      public the_surface_of_the_word_食べ_in_もう食べた() => Word = TheSurfaceOf("食べ", "もう食べた");

      protected override CandidateWordVariant Word { get; }

      [XF] public void it_is_matched_by_an_inflected_surface() => Meets(IsMatchedByAnInflectedSurface).Must().BeTrue();
      [XF] public void its_preceding_token_is_an_adverb() => Meets(PrecedingToken.Meets(TokenConditions.IsAnAdverb)).Must().BeTrue();
      [XF] public void its_last_token_is_in_the_continuative_form() => Meets(LastToken.Meets(TokenConditions.InflectedFormIsOneOf("連用形"))).Must().BeTrue();
      [XF] public void its_last_token_is_a_masu_stem() => Meets(LastToken.Meets(TokenConditions.IsAMasuStem)).Must().BeTrue();
      [XF] public void its_last_token_has_the_ichidan_inflection_type() => Meets(LastToken.Meets(TokenConditions.InflectionTypeIsOneOf("一段"))).Must().BeTrue();
      [XF] public void its_following_token_is_a_past_tense_marker() => Meets(FollowingToken.Meets(TokenConditions.IsAPastTenseMarker)).Must().BeTrue();
      [XF] public void its_base_form_is_食べる() => Meets(BaseForm.Meets(TextConditions.In("食べる"))).Must().BeTrue();
   }

   public class the_base_form_of_the_word_食べ_in_もう食べた : When_testing_a_word_in_its_sentence_against_a_condition
   {
      public the_base_form_of_the_word_食べ_in_もう食べた() => Word = TheBaseFormOf("食べ", "もう食べた");

      protected override CandidateWordVariant Word { get; }

      [XF] public void it_is_not_matched_by_its_surface() => Meets(IsMatchedByItsSurface).Must().BeFalse();
      [XF] public void it_is_not_matched_by_an_inflected_surface() => Meets(IsMatchedByAnInflectedSurface).Must().BeFalse();
   }
}
