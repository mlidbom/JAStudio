using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Editing;
using JAStudio.Core.Note.Search.Grouping;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search;

/// <summary>The changes the search editor makes to an <see cref="EditableSearch"/>, and undoing them.</summary>
public class When_editing_a_search : IAIGeneratedSpec
{
   readonly EditableSearch _search = new();

   static readonly NoteCondition TaggedAnime = NoteProperties.Tags.Include("source::anime");
   static readonly NoteCondition StudyingReading = NoteProperties.StudyingReading.Is(true);

   public class that_was_just_started : When_editing_a_search
   {
      [XF] public void it_is_an_empty_all_of_group() => _search.Current.Must().Be(ConditionGroup.AllOf());
      [XF] public void there_is_nothing_to_undo() => _search.CanUndo.Must().BeFalse();
      [XF] public void removing_the_root_throws() => Invoking(() => _search.RemoveCondition(ConditionPosition.Root)).Must().Throw<InvalidOperationException>();
   }

   public class after_adding_two_conditions_to_the_root : When_editing_a_search
   {
      readonly ConditionPosition _positionOfTheSecond;

      public after_adding_two_conditions_to_the_root()
      {
         _search.AddCondition(ConditionPosition.Root, TaggedAnime);
         _positionOfTheSecond = _search.AddCondition(ConditionPosition.Root, StudyingReading);
      }

      [XF] public void the_root_holds_both_in_the_order_they_were_added() => _search.Current.Must().Be(ConditionGroup.AllOf(TaggedAnime, StudyingReading));
      [XF] public void adding_returns_the_position_of_the_added_condition() => _positionOfTheSecond.Must().Be(ConditionPosition.Root.Child(1));
      [XF] public void adding_to_a_condition_that_is_not_a_group_throws() =>
         Invoking(() => _search.AddCondition(ConditionPosition.Root.Child(0), StudyingReading)).Must().Throw<ArgumentException>();

      public class after_undoing_the_last_change : after_adding_two_conditions_to_the_root
      {
         public after_undoing_the_last_change() => _search.Undo();

         [XF] public void only_the_first_condition_is_left() => _search.Current.Must().Be(ConditionGroup.AllOf(TaggedAnime));

         public class and_undoing_again : after_undoing_the_last_change
         {
            public and_undoing_again() => _search.Undo();

            [XF] public void the_search_is_empty_again() => _search.Current.Must().Be(ConditionGroup.AllOf());
            [XF] public void there_is_nothing_more_to_undo() => _search.CanUndo.Must().BeFalse();
         }
      }

      public class after_removing_the_first : after_adding_two_conditions_to_the_root
      {
         public after_removing_the_first() => _search.RemoveCondition(ConditionPosition.Root.Child(0));

         [XF] public void only_the_second_is_left() => _search.Current.Must().Be(ConditionGroup.AllOf(StudyingReading));
      }

      public class after_negating_the_first : after_adding_two_conditions_to_the_root
      {
         public after_negating_the_first() => _search.ToggleNegation(ConditionPosition.Root.Child(0));

         [XF] public void it_is_negated() => _search.Current.Must().Be(ConditionGroup.AllOf(ConditionGroup.Not(TaggedAnime), StudyingReading));

         public class and_toggling_the_negation_again : after_negating_the_first
         {
            public and_toggling_the_negation_again() => _search.ToggleNegation(ConditionPosition.Root.Child(0));

            [XF] public void the_negation_is_gone() => _search.Current.Must().Be(ConditionGroup.AllOf(TaggedAnime, StudyingReading));
         }

         public class and_removing_the_condition_it_negates : after_negating_the_first
         {
            public and_removing_the_condition_it_negates() => _search.RemoveCondition(ConditionPosition.Root.Child(0).Child(0));

            [XF] public void the_negation_goes_with_it() => _search.Current.Must().Be(ConditionGroup.AllOf(StudyingReading));
         }
      }

      public class after_switching_the_root : after_adding_two_conditions_to_the_root
      {
         public after_switching_the_root() => _search.SwitchGroup(ConditionPosition.Root);

         [XF] public void it_is_an_any_of_group_with_the_same_conditions() => _search.Current.Must().Be(ConditionGroup.AnyOf(TaggedAnime, StudyingReading));
      }

      public class after_replacing_the_second : after_adding_two_conditions_to_the_root
      {
         public after_replacing_the_second() => _search.ReplaceCondition(ConditionPosition.Root.Child(1), NoteProperties.StudyingListening.Is(false));

         [XF] public void the_replacement_is_in_its_place() =>
            _search.Current.Must().Be(ConditionGroup.AllOf(TaggedAnime, NoteProperties.StudyingListening.Is(false)));
      }

      public class after_adding_a_group_and_then_a_condition_to_that_group : after_adding_two_conditions_to_the_root
      {
         readonly ConditionPosition _positionInTheGroup;

         public after_adding_a_group_and_then_a_condition_to_that_group()
         {
            var groupPosition = _search.AddCondition(ConditionPosition.Root, ConditionGroup.AnyOf());
            _positionInTheGroup = _search.AddCondition(groupPosition, NoteProperties.Question.Contains("行く"));
         }

         [XF] public void the_condition_is_in_the_group() =>
            _search.Current.Must().Be(ConditionGroup.AllOf(TaggedAnime, StudyingReading, ConditionGroup.AnyOf(NoteProperties.Question.Contains("行く"))));

         [XF] public void adding_returns_its_position_in_the_group() => _positionInTheGroup.Must().Be(ConditionPosition.Root.Child(2).Child(0));
      }
   }
}
