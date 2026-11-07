using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

public class by_searchable_text : NoteSearchSpecification
{
   public class for_a_vocabs_reading_in_kana : by_searchable_text
   {
      readonly List<string> _found;
      public for_a_vocabs_reading_in_kana() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.SearchableText.Contains("いく"));

      [XF] public void the_vocab_with_that_reading_is_found() => _found.Must().SequenceEqual([VocabToGo]);
   }

   public class for_a_vocabs_reading_in_romaji : by_searchable_text
   {
      readonly List<string> _found;
      public for_a_vocabs_reading_in_romaji() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.SearchableText.Contains("iku"));

      [XF] public void the_vocab_with_that_reading_is_found() => _found.Must().SequenceEqual([VocabToGo]);
   }

   public class for_a_kanjis_reading_in_romaji : by_searchable_text
   {
      readonly List<string> _found;
      public for_a_kanjis_reading_in_romaji() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.SearchableText.Contains("kou"));

      [XF] public void the_kanji_with_that_reading_is_found() => _found.Must().SequenceEqual([KanjiGo]);
   }

   public class for_a_word_in_a_sentences_answer : by_searchable_text
   {
      readonly List<string> _found;
      public for_a_word_in_a_sentences_answer() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.SearchableText.Contains("home"));

      [XF] public void the_sentence_is_found() => _found.Must().SequenceEqual([SentenceAboutHome]);
   }

   public class for_a_text_in_the_questions : by_searchable_text
   {
      readonly List<string> _found;
      public for_a_text_in_the_questions() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.SearchableText.Contains("行"));

      [XF] public void every_note_whose_question_contains_it_is_found() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutSchool, VocabToGo, KanjiGo));
   }
}
