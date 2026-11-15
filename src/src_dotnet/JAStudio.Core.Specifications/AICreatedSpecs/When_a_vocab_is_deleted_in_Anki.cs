using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Studying;
using JAStudio.Core.TestUtils;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Deleting a note in Anki means it's no longer studied in that Anki profile. The note stays in the corpus and in JAStudio:<br/>
/// Anki holds the notes being studied, and the corpus holds them all. Nothing tells JAStudio of the deletion: it finds the study-note gone at its<br/>
/// next sync with Anki (<see cref="SRSSync"/>). The vocab 部屋, studied in a collection loaded as at startup, is deleted in Anki.
/// </summary>
public class When_a_vocab_is_deleted_in_Anki : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabCollection _vocab;
   readonly VocabNote _deletedInAnki;

   public When_a_vocab_is_deleted_in_Anki()
   {
      _vocab = GetService<VocabCollection>();
      var idOfTheVocab = WithAStudyNote(CreateVocab("部屋", "room", "へや")).GetId();
      GetService<JPCollection>().ReloadFromBackend();
      _deletedInAnki = _vocab.WithIdOrNone(idOfTheVocab) ?? throw new InvalidOperationException("The vocab is gone.");
      _deletedInAnki.SetStudyingStatus(new CardStudyingStatus(GetService<StudyNoteIds>().RequiredStudyNoteIdOf(idOfTheVocab), CardTypes.Reading, isSuspended: false, NoteTypes.Vocab));

      GetService<PretendAnki>().DeleteTheStudyNoteOf(_deletedInAnki);
   }

   [XF] public void until_the_next_sync_JAStudio_still_has_it_as_studied() => _deletedInAnki.IsStudying().Must().BeTrue();

   public class after_the_next_sync : When_a_vocab_is_deleted_in_Anki
   {
      public after_the_next_sync() => GetService<SRSSync>().Run();

      [XF] public void JAStudio_still_has_it() => _vocab.WithQuestion("部屋").Must().Contain(_deletedInAnki);
      [XF] public void it_is_no_longer_studied() => _deletedInAnki.IsStudying().Must().BeFalse();
      [XF] public void its_Anki_note_is_forgotten() => GetService<StudyNoteIds>().StudyNoteIdOf(_deletedInAnki.GetId()).HasValue.Must().BeFalse();
   }
}
