using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>A vocab added in Anki, as Yomitan adds them through AnkiConnect, brings only its question and a few other fields.</summary>
public class When_a_vocab_is_added_in_Anki : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   VocabNote AddInAnki(string question)
   {
      var vocab = GetService<VocabCollection>();
      vocab.ExternalSyncHandler.ExternalNoteAdded(1, new AnkiNoteData(null, new Dictionary<string, string> { [AnkiFieldNames.Vocab.Question] = question }, []));
      return vocab.WithQuestion(question).Single();
   }

   public class with_its_question_in_kana : When_a_vocab_is_added_in_Anki
   {
      readonly VocabNote _vocab;

      public with_its_question_in_kana() => _vocab = AddInAnki("ありがとう");

      [XF] public void it_takes_its_reading_from_its_question() => _vocab.GetReadings().Must().Contain("ありがとう");
      [XF] public void it_gets_its_kanji_forms_from_the_dictionary() => _vocab.Forms.EnteredFormsSet().Must().Contain("有難う");
   }

   public class with_its_question_in_kanji : When_a_vocab_is_added_in_Anki
   {
      readonly VocabNote _vocab;

      public with_its_question_in_kanji() => _vocab = AddInAnki("部屋");

      [XF] public void it_has_no_reading_to_look_up_its_forms_with_so_it_gets_none() => _vocab.Forms.EnteredFormsList().Must().BeEmpty();
   }
}
