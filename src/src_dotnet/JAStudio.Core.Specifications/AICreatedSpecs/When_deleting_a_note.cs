using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.KanjiTevents;
using JAStudio.Core.Note.Sentences.Tevents;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A vocab, a kanji and a sentence, each deleted from the collection (<see cref="JPCollection.Delete"/>): a deleted note publishes its class's<br/>
/// <see cref="INoteTevent.Deleted"/>, which its observers get once the deletion has committed.
/// </summary>
public class When_deleting_a_note : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly JPNote Vocab;
   protected readonly JPNote Kanji;
   protected readonly JPNote Sentence;

   public When_deleting_a_note()
   {
      Vocab = CreateVocab("食べる", "to eat", "たべる");
      Kanji = CreateKanji("食", "eat", "ショク", "た");
      Sentence = CreateTestSentence("ご飯を食べる", "eat a meal");
   }

   /// <summary>Each deleted tevent observed, as the class of note it names and the id of the note.</summary>
   protected IReadOnlyList<(string NoteClass, Guid NoteId)> DeletedTeventsObserved() =>
      [.. TeventsObservedSoFar().OfType<INoteTevent.Deleted>().Select(it => (ClassOfNoteIn(it), it.TaggregateId.Value))];

   static string ClassOfNoteIn(INoteTevent.Deleted tevent) => tevent switch
   {
      IVocabTevent.Deleted => "vocab",
      IKanjiTevent.Deleted => "kanji",
      ISentenceTevent.Deleted => "sentence",
      _ => throw new ArgumentOutOfRangeException(nameof(tevent), tevent.GetType().FullName, "a deleted tevent of no class of note")
   };

   public class before_any_is_deleted : When_deleting_a_note
   {
      [XF] public void no_deleted_tevent_is_observed() => DeletedTeventsObserved().Must().BeEmpty();
   }

   public class after_deleting_the_vocab_then_the_kanji_then_the_sentence : When_deleting_a_note
   {
      public after_deleting_the_vocab_then_the_kanji_then_the_sentence()
      {
         GetService<JPCollection>().Delete(Vocab, ItsStudyNote.IsDeleted);
         GetService<JPCollection>().Delete(Kanji, ItsStudyNote.IsDeleted);
         GetService<JPCollection>().Delete(Sentence, ItsStudyNote.IsDeleted);
      }

      [XF] public void the_deleted_tevent_of_each_notes_class_is_observed_with_its_id_in_the_order_deleted() =>
         DeletedTeventsObserved().Must().SequenceEqual([("vocab", Vocab.Id.Value), ("kanji", Kanji.Id.Value), ("sentence", Sentence.Id.Value)]);
   }

   public class in_a_unit_of_work_that_then_fails : When_deleting_a_note
   {
      public in_a_unit_of_work_that_then_fails() =>
         Invoking(() => GetService<JPCollection>().InAUnitOfWork(_ =>
                 {
                    GetService<JPCollection>().Delete(Vocab, ItsStudyNote.IsDeleted);
                    throw new InvalidOperationException("The unit of work fails.");
                 }))
           .Must().Throw<InvalidOperationException>();

      [XF] public void no_deleted_tevent_is_observed() => DeletedTeventsObserved().Must().BeEmpty();
      [XF] public void the_vocab_is_still_in_the_collection() => GetService<JPCollection>().NoteFromNoteId(Vocab.Id).Must().NotBeNull();
   }
}
