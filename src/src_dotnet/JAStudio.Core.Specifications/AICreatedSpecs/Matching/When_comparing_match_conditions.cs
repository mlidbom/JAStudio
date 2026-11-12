using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>Match-conditions are values: two built from the same parts, in the same order, are equal, so a migrated one can be compared with one written out.</summary>
public class When_comparing_match_conditions : IAIGeneratedSpec
{
   [XF] public void two_built_the_same_are_equal() =>
      ConditionGroup.AllOf(PrecedingToken.Meets(TokenConditions.PartOfSpeech.Is("名詞")), ConditionGroup.Not(IsASingleToken))
                    .Must().Be(ConditionGroup.AllOf(PrecedingToken.Meets(TokenConditions.PartOfSpeech.Is("名詞")), ConditionGroup.Not(IsASingleToken)));

   [XF] public void the_same_conditions_in_another_order_are_not_equal() =>
      ConditionGroup.AllOf(IsASingleToken, IsMatchedByItsSurface).Must().NotBe(ConditionGroup.AllOf(IsMatchedByItsSurface, IsASingleToken));

   [XF] public void conditions_on_different_subjects_are_not_equal() =>
      PrecedingToken.Meets(TokenConditions.IsAGodanVerb).Must().NotBe(FollowingToken.Meets(TokenConditions.IsAGodanVerb));

   [XF] public void an_all_of_group_and_an_any_of_group_of_the_same_conditions_are_not_equal() =>
      ConditionGroup.AllOf(IsASingleToken).Equals(ConditionGroup.AnyOf(IsASingleToken)).Must().BeFalse();

   [XF] public void text_comparisons_with_the_same_texts_given_in_another_order_and_repeated_are_equal() =>
      TextConditions.EndsWithOneOf("ん", "い").Must().Be(TextConditions.EndsWithOneOf("い", "ん", "い"));

   [XF] public void regular_expressions_with_the_same_pattern_are_equal() =>
      TextConditions.MatchesRegularExpression("^よ.$").Must().Be(TextConditions.MatchesRegularExpression("^よ.$"));
}
