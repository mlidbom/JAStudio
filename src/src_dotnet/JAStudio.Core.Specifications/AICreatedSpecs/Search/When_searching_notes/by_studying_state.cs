using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

public class by_studying_state : NoteSearchSpecification
{
   public class for_notes_whose_reading_is_studied : by_studying_state
   {
      readonly List<string> _found;
      public for_notes_whose_reading_is_studied() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.StudyingReading.Is(true));

      [XF] public void only_the_sentence_with_an_unsuspended_reading_card_is_found() => _found.Must().SequenceEqual([SentenceAboutSchool]);
   }

   public class for_notes_whose_reading_is_not_studied : by_studying_state
   {
      readonly List<string> _found;
      public for_notes_whose_reading_is_not_studied() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.StudyingReading.Is(false));

      [XF] public void every_other_note_is_found() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutHome, VocabToGo, KanjiGo));
   }

   public class for_notes_whose_listening_is_studied : by_studying_state
   {
      readonly List<string> _found;
      public for_notes_whose_listening_is_studied() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.StudyingListening.Is(true));

      [XF] public void no_note_is_found_because_none_has_an_unsuspended_listening_card() => _found.Must().BeEmpty();
   }
}
