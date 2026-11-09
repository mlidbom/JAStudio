using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Note.Collection;

/// <summary>The collection holds the vocab 行く and 来る, saved where the corpus keeps notes, before it's reloaded.</summary>
public abstract class When_reloading_the_collection : SpecificationStartingWithAnEmptyCollection
{
   protected readonly JPCollection _collection;
   protected readonly VocabNote _vocab行く;

   protected When_reloading_the_collection()
   {
      _collection = GetService<JPCollection>();
      _vocab行く = CreateVocab("行く", "to go", "いく");
      CreateVocab("来る", "to come", "くる");
   }

   protected IReadOnlyList<string> QuestionsOfTheVocabHeld() => [.. _collection.Vocab.All().Select(it => it.GetQuestion()).Order(StringComparer.Ordinal)];

   /// <summary>Behind the collection's back, as when the corpus checkout changes on disk, 行く's stored data is replaced by a copy whose answer is "to walk".</summary>
   public class after_a_notes_file_changed_on_disk : When_reloading_the_collection
   {
      readonly List<string> _questionsOfTheNotesPassedOnAsUpdated = [];
      readonly IReadOnlyList<JPNote> _notesChangedOnDisk;

      public after_a_notes_file_changed_on_disk()
      {
         GetService<INoteRepository>().Save(ACopyOf行くWithTheAnswer("to walk"));
         _collection.NoteUpdated.Subscribe(it => _questionsOfTheNotesPassedOnAsUpdated.Add(it.GetQuestion()));

         _notesChangedOnDisk = _collection.ReloadFromBackend();
      }

      VocabNote ACopyOf行くWithTheAnswer(string answer)
      {
         var serializer = GetService<NoteSerializer>();
         var copy = serializer.DeserializeVocab(serializer.Serialize(_vocab行く));
         copy.CorpusAnswer.Set(answer);
         return copy;
      }

      [XF] public void the_collection_holds_the_vocab_as_changed_on_disk() => _collection.Vocab.All().Single(it => it.GetId() == _vocab行く.GetId()).GetAnswer().Must().Be("to walk");
      [XF] public void the_vocab_changed_on_disk_is_the_one_note_reported() => _notesChangedOnDisk.Select(it => it.GetQuestion()).Must().SequenceEqual(["行く"]);
      [XF] public void the_vocab_changed_on_disk_is_the_one_note_passed_on_as_updated() => _questionsOfTheNotesPassedOnAsUpdated.Must().SequenceEqual(["行く"]);
   }

   /// <summary>The reload makes a change to the corpus checkout first, which notes which vocab the collection holds while it's made.</summary>
   public class after_a_change_to_the_corpus_checkout : When_reloading_the_collection
   {
      readonly IReadOnlyList<string> _questionsOfTheVocabHeldDuringTheChange;

      public after_a_change_to_the_corpus_checkout()
      {
         IReadOnlyList<string> questionsOfTheVocabHeld = [];
         _collection.ReloadFromBackendAfter(() => questionsOfTheVocabHeld = QuestionsOfTheVocabHeld());
         _questionsOfTheVocabHeldDuringTheChange = questionsOfTheVocabHeld;
      }

      [XF] public void the_collection_holds_no_notes_during_the_change() => _questionsOfTheVocabHeldDuringTheChange.Must().BeEmpty();
      [XF] public void the_collection_holds_the_notes_again_afterwards() => QuestionsOfTheVocabHeld().Must().SequenceEqual(["来る", "行く"]);
   }

   /// <summary>The change to the corpus checkout throws, as git's fast-forward does when git refuses it.</summary>
   public class after_a_change_to_the_corpus_checkout_that_fails : When_reloading_the_collection
   {
      readonly InvalidOperationException _failure;

      public after_a_change_to_the_corpus_checkout_that_fails() =>
         _failure = Invoking(() => _collection.ReloadFromBackendAfter(() => throw new InvalidOperationException("git refused the change"))).Must().Throw<InvalidOperationException>().Which;

      [XF] public void the_failure_reaches_the_caller() => _failure.Message.Must().Be("git refused the change");
      [XF] public void the_collection_holds_the_notes_again() => QuestionsOfTheVocabHeld().Must().SequenceEqual(["来る", "行く"]);
   }
}
