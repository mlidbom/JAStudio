using System;
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
/// holds it now, which an edit replaces with the changed copy it commits.
/// </summary>
public class When_a_note_is_referred_to : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly VocabNote Vocab;
   protected readonly ITaggregateReference<VocabNote> Reference;

   public When_a_note_is_referred_to()
   {
      Vocab = CreateVocab("走る", "to run", "はしる");
      Reference = Vocab.Reference();
   }

   [XF] public void the_reference_has_the_notes_id() => Reference.Id.Must().Be(Vocab.Id);
   [XF] public void the_reference_gives_the_note() => Reference.Current.Id.Must().Be(Vocab.Id);

   public class once_the_note_was_edited : When_a_note_is_referred_to
   {
      public once_the_note_was_edited() => Vocab.Edit(it => it.SetCorpusAnswer("to dash"));

      [XF] public void the_reference_gives_the_note_as_it_is_now() => Reference.Current.GetAnswer().Must().Be("to dash");
   }

   public class once_the_note_was_deleted : When_a_note_is_referred_to
   {
      public once_the_note_was_deleted() => GetService<JPCollection>().Delete(Vocab, ItsStudyNote.IsDeleted);

      [XF] public void reading_the_note_fails() => Invoking(() => _ = Reference.Current).Must().Throw<InvalidOperationException>();
   }
}
