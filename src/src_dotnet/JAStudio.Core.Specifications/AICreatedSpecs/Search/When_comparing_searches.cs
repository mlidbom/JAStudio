using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Grouping;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search;

/// <summary>Searches are values, so a spec can compare the search an operation produced with one written out.</summary>
public class When_comparing_searches : IAIGeneratedSpec
{
   static AllOfGroup SentencesAboutGoing() =>
      ConditionGroup.AllOf(
         NoteProperties.Tags.Include("source::anime"),
         ConditionGroup.Not(NoteProperties.StudyingReading.Is(true)),
         NoteProperties.Answer.MatchesRegex("go(ing)?"));

   public class built_from_the_same_conditions_in_the_same_order : When_comparing_searches
   {
      readonly NoteCondition _first = SentencesAboutGoing();
      readonly NoteCondition _second = SentencesAboutGoing();

      [XF] public void they_are_equal() => _first.Must().Be(_second);
      [XF] public void they_have_the_same_hash_code() => _first.GetHashCode().Must().Be(_second.GetHashCode());
   }

   public class built_from_the_same_conditions_in_another_order : When_comparing_searches
   {
      readonly NoteCondition _first = ConditionGroup.AllOf(NoteProperties.Tags.Include("source::anime"), NoteProperties.StudyingReading.Is(true));
      readonly NoteCondition _second = ConditionGroup.AllOf(NoteProperties.StudyingReading.Is(true), NoteProperties.Tags.Include("source::anime"));

      [XF] public void they_are_not_equal_because_a_search_is_a_tree_in_the_order_the_editor_shows() => _first.Must().NotBe(_second);
   }

   public class with_the_same_conditions_combined_by_all_of_and_by_any_of : When_comparing_searches
   {
      readonly NoteCondition _allOf = ConditionGroup.AllOf(NoteProperties.Tags.Include("source::anime"), NoteProperties.StudyingReading.Is(true));
      readonly NoteCondition _anyOf = ConditionGroup.AnyOf(NoteProperties.Tags.Include("source::anime"), NoteProperties.StudyingReading.Is(true));

      [XF] public void they_are_not_equal() => _allOf.Must().NotBe(_anyOf);
   }

   public class differing_only_in_a_regular_expression : When_comparing_searches
   {
      readonly NoteCondition _first = NoteProperties.Answer.MatchesRegex("go(ing)?");
      readonly NoteCondition _second = NoteProperties.Answer.MatchesRegex("went");

      [XF] public void they_are_not_equal() => _first.Must().NotBe(_second);
   }
}
