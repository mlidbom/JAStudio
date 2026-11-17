using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A unit of work publishes, once its work is done, for each note it added or changed, its class's <see cref="INoteChangedInAUnitOfWork"/>: one<br/>
/// per note, however many tevents it published. The vocab てよろしい, ていい and んでいい are one web of perfect synonyms, beside a kanji and a sentence.
/// </summary>
public class When_notes_change_in_a_unit_of_work : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly ITaggregateReference<VocabNote> TeyoroshiiReference;
   protected VocabNote Teyoroshii => TeyoroshiiReference.Current;
   protected readonly ITaggregateReference<VocabNote> TeiiReference;
   protected VocabNote Teii => TeiiReference.Current;
   protected readonly KanjiNote Kanji;
   protected readonly SentenceNote Sentence;
   readonly int _teventsObservedInTheSetup;

   public When_notes_change_in_a_unit_of_work()
   {
      TeyoroshiiReference = VocabNote.Create(NoteServices, "てよろしい", "may", "てよろしい").Reference();
      TeiiReference = VocabNote.Create(NoteServices, "ていい", "may", "ていい").Reference();
      VocabNote.Create(NoteServices, "んでいい", "may", "んでいい");
      Teyoroshii.Edit(it =>
      {
         it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(new WordReference("ていい"));
         it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(new WordReference("んでいい"));
      });
      Kanji = CreateKanji("良", "good", "リョウ", "よ");
      Sentence = CreateTestSentence("これでいい", "this will do");
      _teventsObservedInTheSetup = TeventsObservedSoFar().Count;
   }

   /// <summary>Each <see cref="INoteChangedInAUnitOfWork"/> observed since the setup, as its note's class and question.</summary>
   protected IReadOnlyList<string> NotesChangedInAUnitOfWorkObservedSinceTheSetup() =>
      [.. TeventsObservedSoFar().Skip(_teventsObservedInTheSetup).OfType<INoteChangedInAUnitOfWork>().Select(Described)];

   string Described(INoteChangedInAUnitOfWork tevent) =>
      $"{ClassOfNoteIn(tevent)} {GetService<JPCollection>().NoteFromNoteId(tevent.NoteId)?.GetQuestion() ?? "<no longer in the collection>"}";

   static string ClassOfNoteIn(INoteChangedInAUnitOfWork tevent) => tevent switch
   {
      INoteChangedInAUnitOfWork.Vocab => "vocab",
      INoteChangedInAUnitOfWork.Kanji => "kanji",
      INoteChangedInAUnitOfWork.Sentence => "sentence",
      _ => throw new ArgumentOutOfRangeException(nameof(tevent), tevent.GetType().FullName, "a note changed in a unit of work of no class of note")
   };

   public class the_setup_itself : When_notes_change_in_a_unit_of_work
   {
      [XF] public void each_note_created_is_observed_as_added() =>
         TeventsObservedSoFar().OfType<INoteChangedInAUnitOfWork>().Select(Described).Distinct().Order()
                               .Must().SequenceEqual(["kanji 良", "sentence これでいい", "vocab ていい", "vocab てよろしい", "vocab んでいい"]);
   }

   public class creating_a_vocab : When_notes_change_in_a_unit_of_work
   {
      public creating_a_vocab() => CreateVocab("食べる", "to eat", "たべる");

      [XF] public void it_alone_is_observed_as_added() => NotesChangedInAUnitOfWorkObservedSinceTheSetup().Must().SequenceEqual(["vocab 食べる"]);
   }

   public class editing_several_properties_of_the_kanji_in_one_edit : When_notes_change_in_a_unit_of_work
   {
      public editing_several_properties_of_the_kanji_in_one_edit() =>
         Kanji.Edit(it =>
         {
            it.SetCorpusAnswer("good, fine");
            it.SetMnemonic("a good mnemonic");
            it.Tags.Set(Tag.FromName("source::grammar"));
         });

      [XF] public void the_kanji_is_observed_as_changed_once() => NotesChangedInAUnitOfWorkObservedSinceTheSetup().Must().SequenceEqual(["kanji 良"]);
   }

   public class editing_only_the_tags_of_the_sentence : When_notes_change_in_a_unit_of_work
   {
      public editing_only_the_tags_of_the_sentence() => Sentence.Edit(it => it.Tags.Set(Tag.FromName("source::grammar")));

      [XF] public void the_sentence_is_observed_as_a_changed_sentence() => NotesChangedInAUnitOfWorkObservedSinceTheSetup().Must().SequenceEqual(["sentence これでいい"]);
   }

   public class an_edit_that_sets_what_the_note_holds_already : When_notes_change_in_a_unit_of_work
   {
      public an_edit_that_sets_what_the_note_holds_already() => Kanji.Edit(it => it.SetCorpusAnswer("good"));

      [XF] public void no_note_is_observed_as_changed() => NotesChangedInAUnitOfWorkObservedSinceTheSetup().Must().BeEmpty();
   }

   public class editing_the_sentence_then_the_kanji_then_the_sentence_again_in_one_unit_of_work : When_notes_change_in_a_unit_of_work
   {
      public editing_the_sentence_then_the_kanji_then_the_sentence_again_in_one_unit_of_work() =>
         GetService<JPCollection>().InAUnitOfWork(_ =>
         {
            Sentence.Edit(it => it.SetCorpusAnswer("this is fine"));
            Kanji.Edit(it => it.SetCorpusAnswer("good, fine"));
            Sentence.Edit(it => it.SetComments("a comment"));
         });

      [XF] public void each_is_observed_as_changed_once_in_the_order_first_changed() =>
         NotesChangedInAUnitOfWorkObservedSinceTheSetup().Must().SequenceEqual(["sentence これでいい", "kanji 良"]);
   }

   public class giving_てよろしい_another_answer : When_notes_change_in_a_unit_of_work
   {
      public giving_てよろしい_another_answer() => Teyoroshii.Edit(it => it.SetCorpusAnswer("may, be allowed to"));

      [XF] public void it_and_the_perfect_synonyms_it_gave_the_answer_are_observed_as_changed() =>
         NotesChangedInAUnitOfWorkObservedSinceTheSetup().Must().SequenceEqual(["vocab てよろしい", "vocab ていい", "vocab んでいい"]);
   }

   public class deleting_ていい : When_notes_change_in_a_unit_of_work
   {
      public deleting_ていい() => GetService<JPCollection>().Delete(Teii, ItsStudyNote.IsDeleted);

      [XF] public void only_the_perfect_synonyms_it_left_are_observed_as_changed() =>
         NotesChangedInAUnitOfWorkObservedSinceTheSetup().Order().Must().SequenceEqual(["vocab てよろしい", "vocab んでいい"]);
   }

   public class editing_the_kanji_in_a_unit_of_work_that_then_fails : When_notes_change_in_a_unit_of_work
   {
      public editing_the_kanji_in_a_unit_of_work_that_then_fails() =>
         Invoking(() => GetService<JPCollection>().InAUnitOfWork(_ =>
                 {
                    Kanji.Edit(it => it.SetCorpusAnswer("good, fine"));
                    throw new InvalidOperationException("The unit of work fails.");
                 }))
           .Must().Throw<InvalidOperationException>();

      [XF] public void no_note_is_observed_as_changed() => NotesChangedInAUnitOfWorkObservedSinceTheSetup().Must().BeEmpty();
   }
}
