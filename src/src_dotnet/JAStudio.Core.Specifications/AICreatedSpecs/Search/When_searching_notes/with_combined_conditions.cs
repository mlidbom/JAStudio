using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Grouping;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

public class with_combined_conditions : NoteSearchSpecification
{
   public class that_is_an_empty_search : with_combined_conditions
   {
      readonly List<string> _found;
      public that_is_an_empty_search() => _found = QuestionsOfTheNotesFoundBy(ConditionGroup.AllOf());

      [XF] public void every_note_is_found() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutSchool, SentenceAboutHome, VocabToGo, KanjiGo));
   }

   public class that_must_all_match : with_combined_conditions
   {
      readonly List<string> _found;
      public that_must_all_match() => _found = QuestionsOfTheNotesFoundBy(
                                                  ConditionGroup.AllOf(
                                                     NoteProperties.Tags.Include("source::anime"),
                                                     NoteProperties.StudyingReading.Is(false)));

      [XF] public void only_the_notes_matching_every_condition_are_found() => _found.Must().SequenceEqual([SentenceAboutHome]);
   }

   public class of_which_any_may_match : with_combined_conditions
   {
      readonly List<string> _found;
      public of_which_any_may_match() => _found = QuestionsOfTheNotesFoundBy(
                                                     ConditionGroup.AnyOf(
                                                        NoteProperties.NoteType.Is<KanjiNote>(),
                                                        NoteProperties.Tags.Include("source::wani")));

      [XF] public void the_notes_matching_at_least_one_condition_are_found() => _found.Must().SequenceEqual(InOrdinalOrder(VocabToGo, KanjiGo));
   }

   public class with_a_negated_condition : with_combined_conditions
   {
      readonly List<string> _found;
      public with_a_negated_condition() => _found = QuestionsOfTheNotesFoundBy(ConditionGroup.Not(NoteProperties.Tags.Include("source::anime")));

      [XF] public void the_notes_not_matching_it_are_found() => _found.Must().SequenceEqual(InOrdinalOrder(VocabToGo, KanjiGo));
   }

   public class with_groups_nested_in_groups : with_combined_conditions
   {
      readonly List<string> _found;
      public with_groups_nested_in_groups() => _found = QuestionsOfTheNotesFoundBy(
                                                           ConditionGroup.AllOf(
                                                              NoteProperties.Tags.Include("source"),
                                                              ConditionGroup.AnyOf(
                                                                 NoteProperties.StudyingReading.Is(true),
                                                                 NoteProperties.NoteType.Is<VocabNote>()),
                                                              ConditionGroup.Not(NoteProperties.Tags.Include("-::priority::low"))));

      [XF] public void each_group_combines_its_own_conditions() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutSchool, VocabToGo));
   }

   public class with_an_empty_group_among_conditions_of_which_any_may_match : with_combined_conditions
   {
      readonly List<string> _found;
      public with_an_empty_group_among_conditions_of_which_any_may_match() => _found = QuestionsOfTheNotesFoundBy(
                                                                                          ConditionGroup.AnyOf(
                                                                                             NoteProperties.NoteType.Is<KanjiNote>(),
                                                                                             ConditionGroup.AllOf()));

      [XF] public void the_empty_group_is_ignored_rather_than_matching_every_note() => _found.Must().SequenceEqual([KanjiGo]);
   }

   public class with_the_negation_of_an_empty_group : with_combined_conditions
   {
      readonly List<string> _found;
      public with_the_negation_of_an_empty_group() => _found = QuestionsOfTheNotesFoundBy(
                                                                  ConditionGroup.AllOf(
                                                                     NoteProperties.NoteType.Is<SentenceNote>(),
                                                                     ConditionGroup.Not(ConditionGroup.AnyOf())));

      [XF] public void the_negation_is_empty_too_so_it_is_ignored_rather_than_matching_no_note() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutSchool, SentenceAboutHome));
   }
}
