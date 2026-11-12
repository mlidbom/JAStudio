using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping.ConditionGroup;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// Whether two conditions mean the same (<see cref="Condition.MeansTheSameAs"/>): whether they're built of the same conditions, but for the order of those<br/>
/// in a group. Equality says more, so that reordering a group in the editor is a change (<see cref="When_comparing_match_conditions"/>). How a shared-condition<br/>
/// means what its definition does is in <see cref="When_using_a_shared_condition_wherever_its_definition_is"/>.
/// </summary>
public class When_comparing_what_conditions_mean : IAIGeneratedSpec
{
   [XF] public void two_built_the_same_mean_the_same() =>
      AllOf(IsASingleToken, !IsMatchedByItsSurface).MeansTheSameAs(AllOf(IsASingleToken, !IsMatchedByItsSurface)).Must().BeTrue();

   [XF] public void a_groups_conditions_in_another_order_mean_the_same() =>
      AllOf(IsASingleToken, IsMatchedByItsSurface).MeansTheSameAs(AllOf(IsMatchedByItsSurface, IsASingleToken)).Must().BeTrue();

   [XF] public void a_group_in_another_order_negated_under_a_token_means_the_same() =>
      (!PrecedingToken.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb))
     .MeansTheSameAs(!PrecedingToken.AnyOf(TokenConditions.IsAnIchidanVerb, TokenConditions.IsAGodanVerb)).Must().BeTrue();

   [XF] public void an_all_of_group_in_an_all_of_group_means_the_same_as_its_conditions_in_the_outer_group() =>
      AllOf(IsASingleToken, AllOf(IsMatchedByItsSurface, !IsMatchedByAnInflectedSurface)).MeansTheSameAs(AllOf(!IsMatchedByAnInflectedSurface, IsASingleToken, IsMatchedByItsSurface)).Must().BeTrue();

   [XF] public void an_any_of_group_in_an_all_of_group_does_not_mean_the_same_as_its_conditions_in_the_outer_group() =>
      AllOf(IsASingleToken, AnyOf(IsMatchedByItsSurface, !IsMatchedByAnInflectedSurface)).MeansTheSameAs(AllOf(IsASingleToken, IsMatchedByItsSurface, !IsMatchedByAnInflectedSurface)).Must().BeFalse();

   [XF] public void an_all_of_group_and_an_any_of_group_of_the_same_conditions_do_not_mean_the_same() =>
      AllOf(IsASingleToken, IsMatchedByItsSurface).MeansTheSameAs(AnyOf(IsASingleToken, IsMatchedByItsSurface)).Must().BeFalse();

   [XF] public void a_group_with_a_condition_more_does_not_mean_the_same() =>
      AllOf(IsASingleToken, IsMatchedByItsSurface).MeansTheSameAs(AllOf(IsASingleToken, IsMatchedByItsSurface, IsMatchedByAnInflectedSurface)).Must().BeFalse();

   [XF] public void a_group_holding_one_condition_twice_does_not_mean_the_same_as_one_holding_it_and_another() =>
      AllOf(IsASingleToken, IsASingleToken).MeansTheSameAs(AllOf(IsASingleToken, IsMatchedByItsSurface)).Must().BeFalse();

   [XF] public void the_same_condition_on_different_subjects_does_not_mean_the_same() =>
      PrecedingToken.Meets(TokenConditions.IsAGodanVerb).MeansTheSameAs(FollowingToken.Meets(TokenConditions.IsAGodanVerb)).Must().BeFalse();

   [XF] public void a_condition_and_its_negation_do_not_mean_the_same() => IsASingleToken.MeansTheSameAs(!IsASingleToken).Must().BeFalse();

   [XF] public void groups_meaning_the_same_in_another_order_and_flattened_have_the_same_hash_code_by_meaning() =>
      ConditionMeaningComparer.Instance.GetHashCode(!PrecedingToken.AnyOf(TokenConditions.IsAGodanVerb, AnyOf(TokenConditions.IsAnIchidanVerb, TokenConditions.IsAnAdverb)))
                              .Must().Be(ConditionMeaningComparer.Instance.GetHashCode(!PrecedingToken.AnyOf(TokenConditions.IsAnAdverb, TokenConditions.IsAnIchidanVerb, TokenConditions.IsAGodanVerb)));
}
