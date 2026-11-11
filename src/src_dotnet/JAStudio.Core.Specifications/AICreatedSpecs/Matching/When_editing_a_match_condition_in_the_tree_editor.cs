using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using static Compze.Must.MustActions;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The changes the Matching tab's tree editor makes to a vocab's match-condition (<see cref="EditableMatchCondition"/>), and undoing them. Unlike a<br/>
/// search's tree, a match-condition holds conditions on several subjects: a condition on a word holds one on its preceding token.
/// </summary>
public class When_editing_a_match_condition_in_the_tree_editor : IAIGeneratedSpec
{
   readonly EditableMatchCondition _matchCondition = new(ConditionGroup.AllOf<CandidateWordVariant>());

   static ConditionPosition Root => ConditionPosition.Root;

   public class that_is_empty : When_editing_a_match_condition_in_the_tree_editor
   {
      [XF] public void the_conditions_at_the_root_are_those_on_a_word() => _matchCondition.LevelAt(Root).Must().Be(WordConditions.Level);
      [XF] public void there_is_nothing_to_undo() => _matchCondition.CanUndo.Must().BeFalse();
      [XF] public void removing_the_root_throws() => Invoking(() => _matchCondition.RemoveCondition(Root)).Must().Throw<InvalidOperationException>();

      [XF] public void switching_the_root_to_any_of_throws_since_a_match_condition_is_always_all_of() =>
         Invoking(() => _matchCondition.SwitchGroup(Root)).Must().Throw<InvalidOperationException>();

      [XF] public void negating_the_root_throws_since_a_match_condition_is_always_all_of() =>
         Invoking(() => _matchCondition.ToggleNegation(Root)).Must().Throw<InvalidOperationException>();
   }

   public class after_adding_that_the_preceding_token_is_a_godan_verb_and_negating_it : When_editing_a_match_condition_in_the_tree_editor
   {
      static readonly ConditionPosition TheNegation = Root.Child(0);
      static readonly ConditionPosition TheFact = TheNegation.Child(0).Child(0);

      public after_adding_that_the_preceding_token_is_a_godan_verb_and_negating_it() =>
         _matchCondition.ToggleNegation(_matchCondition.AddCondition(Root, PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));

      [XF] public void the_root_holds_not_the_preceding_token_is_a_godan_verb() =>
         _matchCondition.Current.Must().Be(ConditionGroup.AllOf(ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAGodanVerb))));

      [XF] public void the_conditions_where_the_negation_is_are_those_on_a_word() => _matchCondition.LevelAt(TheNegation).Must().Be(WordConditions.Level);
      [XF] public void the_conditions_inside_the_condition_on_the_preceding_token_are_those_on_a_token() => _matchCondition.LevelAt(TheFact).Must().Be(TokenConditions.Level);

      public class then_removing_the_fact_inside_them : after_adding_that_the_preceding_token_is_a_godan_verb_and_negating_it
      {
         public then_removing_the_fact_inside_them() => _matchCondition.RemoveCondition(TheFact);

         [XF] public void the_negation_and_the_condition_on_the_preceding_token_go_with_it_since_they_held_only_it() =>
            _matchCondition.Current.Must().Be(ConditionGroup.AllOf<CandidateWordVariant>());
      }

      public class then_toggling_the_negation_of_the_negation : after_adding_that_the_preceding_token_is_a_godan_verb_and_negating_it
      {
         public then_toggling_the_negation_of_the_negation() => _matchCondition.ToggleNegation(TheNegation);

         [XF] public void the_negation_goes() => _matchCondition.Current.Must().Be(ConditionGroup.AllOf(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));
      }

      public class then_negating_the_fact_inside_the_condition_on_the_preceding_token : after_adding_that_the_preceding_token_is_a_godan_verb_and_negating_it
      {
         public then_negating_the_fact_inside_the_condition_on_the_preceding_token() => _matchCondition.ToggleNegation(TheFact);

         [XF] public void both_negations_are_kept_since_they_mean_different_things_where_there_is_no_preceding_token() =>
            _matchCondition.Current.Must().Be(ConditionGroup.AllOf(ConditionGroup.Not(PrecedingToken.Meets(ConditionGroup.Not(TokenConditions.IsAGodanVerb)))));
      }

      public class then_undoing : after_adding_that_the_preceding_token_is_a_godan_verb_and_negating_it
      {
         public then_undoing() => _matchCondition.Undo();

         [XF] public void the_negation_is_undone() => _matchCondition.Current.Must().Be(ConditionGroup.AllOf(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));
      }
   }

   public class after_adding_an_any_of_group_on_the_preceding_token_with_two_conditions_in_it : When_editing_a_match_condition_in_the_tree_editor
   {
      readonly ConditionPosition _theGroup;

      public after_adding_an_any_of_group_on_the_preceding_token_with_two_conditions_in_it()
      {
         _theGroup = _matchCondition.AddCondition(Root, PrecedingToken.Meets(ConditionGroup.AnyOf<IAnalysisToken>())).Child(0);
         _matchCondition.AddCondition(_theGroup, TokenConditions.IsAnAdverb);
         _matchCondition.AddCondition(_theGroup, TokenConditions.PartOfSpeechIs("形容詞"));
      }

      [XF] public void the_group_holds_both_in_the_order_they_were_added() =>
         _matchCondition.Current.Must().Be(ConditionGroup.AllOf(PrecedingToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAnAdverb, TokenConditions.PartOfSpeechIs("形容詞")))));

      [XF] public void the_conditions_in_the_group_are_those_on_a_token() => _matchCondition.LevelAt(_theGroup.Child(1)).Must().Be(TokenConditions.Level);

      [XF] public void adding_a_condition_on_a_word_to_the_group_on_a_token_throws() =>
         Invoking(() => _matchCondition.AddCondition(_theGroup, IsASingleToken)).Must().Throw<ArgumentException>();

      [XF] public void adding_to_a_condition_that_is_not_a_group_throws() =>
         Invoking(() => _matchCondition.AddCondition(_theGroup.Child(0), TokenConditions.IsAGodanVerb)).Must().Throw<ArgumentException>();

      public class then_switching_the_group : after_adding_an_any_of_group_on_the_preceding_token_with_two_conditions_in_it
      {
         public then_switching_the_group() => _matchCondition.SwitchGroup(_theGroup);

         [XF] public void it_is_an_all_of_group_with_the_same_conditions() =>
            _matchCondition.Current.Must().Be(ConditionGroup.AllOf(PrecedingToken.Meets(ConditionGroup.AllOf(TokenConditions.IsAnAdverb, TokenConditions.PartOfSpeechIs("形容詞")))));
      }

      public class then_replacing_the_second_condition_in_it : after_adding_an_any_of_group_on_the_preceding_token_with_two_conditions_in_it
      {
         public then_replacing_the_second_condition_in_it() => _matchCondition.ReplaceCondition(_theGroup.Child(1), TokenConditions.PartOfSpeechIs("副詞"));

         [XF] public void the_replacement_takes_its_place() =>
            _matchCondition.Current.Must().Be(ConditionGroup.AllOf(PrecedingToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAnAdverb, TokenConditions.PartOfSpeechIs("副詞")))));
      }

      public class then_removing_the_first_condition_in_it : after_adding_an_any_of_group_on_the_preceding_token_with_two_conditions_in_it
      {
         public then_removing_the_first_condition_in_it() => _matchCondition.RemoveCondition(_theGroup.Child(0));

         [XF] public void only_the_second_is_left_in_the_group() =>
            _matchCondition.Current.Must().Be(ConditionGroup.AllOf(PrecedingToken.Meets(ConditionGroup.AnyOf(TokenConditions.PartOfSpeechIs("形容詞")))));
      }
   }

   public class after_the_match_condition_changed_elsewhere : When_editing_a_match_condition_in_the_tree_editor
   {
      public after_the_match_condition_changed_elsewhere() => _matchCondition.ChangedElsewhere(ConditionGroup.AllOf(IsASingleToken));

      [XF] public void it_is_the_current_one() => _matchCondition.Current.Must().Be(ConditionGroup.AllOf(IsASingleToken));

      public class then_undoing : after_the_match_condition_changed_elsewhere
      {
         public then_undoing() => _matchCondition.Undo();

         [XF] public void the_match_condition_is_the_one_from_before() => _matchCondition.Current.Must().Be(ConditionGroup.AllOf<CandidateWordVariant>());
      }

      public class then_changing_to_the_same_one_elsewhere_and_undoing : after_the_match_condition_changed_elsewhere
      {
         public then_changing_to_the_same_one_elsewhere_and_undoing()
         {
            _matchCondition.ChangedElsewhere(ConditionGroup.AllOf(IsASingleToken));
            _matchCondition.Undo();
         }

         [XF] public void the_match_condition_is_the_one_from_before_the_first_change_since_the_same_one_is_no_change() =>
            _matchCondition.Current.Must().Be(ConditionGroup.AllOf<CandidateWordVariant>());
      }
   }
}
