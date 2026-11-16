using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

public class by_being_a_parsing_case : NoteSearchSpecification
{
   protected by_being_a_parsing_case() => GetService<JPCollection>().Sentences.All().Single(it => it.GetQuestion() == SentenceAboutHome).Edit(it => it.MarkAsAParsingCase());

   public class for_notes_that_are_parsing_cases : by_being_a_parsing_case
   {
      readonly List<string> _found;
      public for_notes_that_are_parsing_cases() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.ParsingCase.Is(true));

      [XF] public void only_the_sentence_marked_as_a_parsing_case_is_found() => _found.Must().SequenceEqual([SentenceAboutHome]);
   }

   public class for_notes_that_are_not_parsing_cases : by_being_a_parsing_case
   {
      readonly List<string> _found;
      public for_notes_that_are_not_parsing_cases() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.ParsingCase.Is(false));

      [XF] public void every_other_note_is_found() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutSchool, VocabToGo, KanjiGo));
   }
}
