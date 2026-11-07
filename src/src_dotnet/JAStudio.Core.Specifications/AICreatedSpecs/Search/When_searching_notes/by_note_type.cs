using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

public class by_note_type : NoteSearchSpecification
{
   public class for_sentences : by_note_type
   {
      readonly List<string> _found;
      public for_sentences() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.NoteType.Is<SentenceNote>());

      [XF] public void both_sentences_are_found() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutSchool, SentenceAboutHome));
   }

   public class for_kanji : by_note_type
   {
      readonly List<string> _found;
      public for_kanji() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.NoteType.Is<KanjiNote>());

      [XF] public void only_the_kanji_is_found() => _found.Must().SequenceEqual([KanjiGo]);
   }
}
