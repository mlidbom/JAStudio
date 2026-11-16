using System;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A note of the collection kept by its reference, <c>note.Reference()</c> (<see cref="ReferringToANote"/>): on each read, the note as the collection<br/>
/// holds it now, which an edit replaces with the changed copy it commits. For update, the copy the running edit's unit of work may change, and a<br/>
/// failure outside any edit.
/// </summary>
public class When_a_note_is_referred_to : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly VocabNote Vocab;
   protected readonly VocabNote OtherVocab;
   protected readonly ITaggregateReference<VocabNote> Reference;

   public When_a_note_is_referred_to()
   {
      Vocab = CreateVocab("走る", "to run", "はしる");
      OtherVocab = CreateVocab("歩く", "to walk", "あるく");
      Reference = Vocab.Reference();
   }

   [XF] public void the_reference_has_the_notes_id() => Reference.Id.Must().Be(Vocab.Id);
   [XF] public void the_reference_gives_the_note() => Reference.Current.Id.Must().Be(Vocab.Id);

   public class once_the_note_was_edited : When_a_note_is_referred_to
   {
      public once_the_note_was_edited() => Vocab.Edit(it => it.CorpusAnswer.Set("to dash"));

      [XF] public void the_reference_gives_the_note_as_it_is_now() => Reference.Current.GetAnswer().Must().Be("to dash");
   }

   public class getting_the_note_for_update_in_an_edit_of_it : When_a_note_is_referred_to
   {
      NoteId? _idOfTheNoteGotForUpdate;

      public getting_the_note_for_update_in_an_edit_of_it() => Vocab.Edit(_ => _idOfTheNoteGotForUpdate = Reference.ForUpdate().Id);

      [XF] public void it_gives_the_note_being_edited() => _idOfTheNoteGotForUpdate.Must().Be(Vocab.Id);
   }

   public class getting_the_note_for_update_in_an_edit_of_another_note : When_a_note_is_referred_to
   {
      public getting_the_note_for_update_in_an_edit_of_another_note() => OtherVocab.Edit(_ => Reference.ForUpdate().CorpusAnswer.Set("to dash"));

      [XF] public void the_change_to_it_is_committed_with_the_edit() => Reference.Current.GetAnswer().Must().Be("to dash");
   }

   public class getting_the_note_for_update : When_a_note_is_referred_to
   {
      [XF] public void outside_any_edit_fails() => Invoking(() => Reference.ForUpdate()).Must().Throw<StateAssertionFailedException>();
   }

   public class once_the_note_was_deleted : When_a_note_is_referred_to
   {
      public once_the_note_was_deleted() => GetService<JPCollection>().Delete(Vocab, ItsStudyNote.IsDeleted);

      [XF] public void reading_the_note_fails() => Invoking(() => _ = Reference.Current).Must().Throw<InvalidOperationException>();
   }
}
