using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>Deleting a note in JAStudio deletes it for good, unlike deleting it in Anki: see <see cref="When_a_vocab_is_deleted_in_Anki"/>.</summary>
public class When_a_vocab_is_deleted_in_JAStudio : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public When_a_vocab_is_deleted_in_JAStudio() => GetService<JPCollection>().Delete(VocabNote.Create(NoteServices, "部屋", "room", "へや"), ItsAnkiNote.IsDeleted);

   [XF] public void JAStudio_no_longer_has_it() => GetService<VocabCollection>().WithQuestion("部屋").Must().BeEmpty();
}
