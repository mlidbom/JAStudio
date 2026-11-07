using System.Collections.Generic;
using System.Text.RegularExpressions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

public class by_text : NoteSearchSpecification
{
   public class for_questions_containing_行く : by_text
   {
      readonly List<string> _found;
      public for_questions_containing_行く() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Question.Contains("行く"));

      [XF] public void the_sentence_and_the_vocab_whose_questions_contain_it_are_found() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutSchool, VocabToGo));
   }

   public class for_answers_containing_HOME_in_upper_case : by_text
   {
      readonly List<string> _found;
      public for_answers_containing_HOME_in_upper_case() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Answer.Contains("HOME"));

      [XF] public void the_case_is_ignored_so_the_sentence_whose_answer_is_Go_home_is_found() => _found.Must().SequenceEqual([SentenceAboutHome]);
   }

   public class for_questions_that_are_exactly_行 : by_text
   {
      readonly List<string> _found;
      public for_questions_that_are_exactly_行() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Question.Is("行"));

      [XF] public void only_the_kanji_is_found_not_the_notes_whose_question_merely_contains_it() => _found.Must().SequenceEqual([KanjiGo]);
   }

   public class for_answers_matching_the_wildcard_pattern_go_star : by_text
   {
      readonly List<string> _found;
      public for_answers_matching_the_wildcard_pattern_go_star() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Answer.MatchesWildcards("go*"));

      [XF] public void the_notes_whose_whole_answer_starts_with_go_are_found_ignoring_case() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutHome, KanjiGo));
   }

   public class for_answers_matching_the_wildcard_pattern_t_underscore_go : by_text
   {
      readonly List<string> _found;
      public for_answers_matching_the_wildcard_pattern_t_underscore_go() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Answer.MatchesWildcards("t_ go"));

      [XF] public void the_underscore_stands_for_one_character_so_the_vocab_whose_answer_is_to_go_is_found() => _found.Must().SequenceEqual([VocabToGo]);
   }

   public class for_answers_matching_a_regular_expression : by_text
   {
      readonly List<string> _found;
      public for_answers_matching_a_regular_expression() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Answer.MatchesRegex(@"\bgo\b.*\bto\b"));

      [XF] public void the_notes_with_a_part_of_their_answer_matching_it_are_found() => _found.Must().SequenceEqual([SentenceAboutSchool]);
   }

   public class for_a_regular_expression_that_is_not_valid : by_text
   {
      [XF] public void making_the_condition_throws_a_regex_parse_exception() =>
         Invoking(() => NoteProperties.Question.MatchesRegex("(")).Must().Throw<RegexParseException>();
   }
}
