using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A note of the collection is changed only in an edit of it, <c>note.Edit(it => ...)</c> (<see cref="EditingANote"/>): a change outside any edit, or in<br/>
/// an edit of another note, fails before it changes anything. A note being created isn't in the collection yet, and is changed freely.
/// </summary>
/// <remarks>
/// The state store that keeps the notes checks this: the notes it serves to readers refuse every change, and an edit changes the copy it gets for<br/>
/// update (dev_docs/taggregates/shared/how-jastudio-edits-a-note.md).
/// </remarks>
public class When_a_note_is_changed : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly ITaggregateReference<VocabNote> VocabReference;
   protected VocabNote Vocab => VocabReference.Current;
   protected readonly ITaggregateReference<VocabNote> OtherVocabReference;
   protected VocabNote OtherVocab => OtherVocabReference.Current;

   public When_a_note_is_changed()
   {
      VocabReference = CreateVocab("走る", "to run", "はしる").Reference();
      OtherVocabReference = CreateVocab("歩く", "to walk", "あるく").Reference();
   }

   protected void ChangeTheVocabsAnswer() => Vocab.SetCorpusAnswer("to dash");

   public class in_an_edit_of_it : When_a_note_is_changed
   {
      public in_an_edit_of_it() => Vocab.Edit(it => it.SetCorpusAnswer("to dash"));

      [XF] public void the_change_is_made() => Vocab.GetAnswer().Must().Be("to dash");
   }

   public class outside_any_edit : When_a_note_is_changed
   {
      [XF] public void the_change_fails() => Invoking(ChangeTheVocabsAnswer).Must().Throw<StateAssertionFailedException>();

      public class once_the_change_failed : outside_any_edit
      {
         public once_the_change_failed() => Invoking(ChangeTheVocabsAnswer).Must().Throw<StateAssertionFailedException>();

         [XF] public void the_note_is_as_it_was() => Vocab.GetAnswer().Must().Be("to run");
      }
   }

   public class in_an_edit_of_another_note : When_a_note_is_changed
   {
      [XF] public void the_change_fails() => Invoking(() => OtherVocab.Edit(_ => ChangeTheVocabsAnswer())).Must().Throw<StateAssertionFailedException>();
   }

   public class before_it_is_added_to_the_collection : When_a_note_is_changed
   {
      readonly VocabNote _beingCreated;

      public before_it_is_added_to_the_collection()
      {
         _beingCreated = new VocabNote(NoteServices, "泳ぐ");
         _beingCreated.SetCorpusAnswer("to swim");
      }

      [XF] public void the_change_is_made_outside_any_edit() => _beingCreated.GetAnswer().Must().Be("to swim");
   }
}
