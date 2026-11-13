using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Deleting a note in Anki means it's no longer studied in that Anki profile. The note stays in the corpus and in JAStudio:<br/>
/// Anki holds the notes being studied, and the corpus holds them all.
/// </summary>
public class When_a_vocab_is_deleted_in_Anki : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabCollection _vocab;
   readonly VocabNote _deletedInAnki;

   public When_a_vocab_is_deleted_in_Anki()
   {
      _vocab = GetService<VocabCollection>();
      _deletedInAnki = WithAStudyNote(CreateVocab("部屋", "room", "へや"));
      var ankiNoteId = GetService<ExternalNoteIdMap>().RequireExternalId(_deletedInAnki.GetId());
      GetService<JPCollection>().UpdateCardStudyingStatus(new CardStudyingStatus(ankiNoteId, CardTypes.Reading, isSuspended: false, NoteTypes.Vocab));
      _deletedInAnki.IsStudying().Must().BeTrue();

      _vocab.ExternalSyncHandler.ExternalNoteRemoved(ankiNoteId);
   }

   [XF] public void JAStudio_still_has_it() => _vocab.WithQuestion("部屋").Must().Contain(_deletedInAnki);
   [XF] public void it_is_no_longer_studied() => _deletedInAnki.IsStudying().Must().BeFalse();
   [XF] public void its_Anki_note_is_forgotten() => GetService<ExternalNoteIdMap>().ToExternalId(_deletedInAnki.GetId()).HasValue.Must().BeFalse();
}
