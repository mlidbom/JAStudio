using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Must;
using Compze.Teventive;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.KanjiTevents;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.Tevents;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Tevents;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A note is a Compze taggregate: a new note publishes its created tevent, which gives it its id, and a note's text field publishes the<br/>
/// <c>PropertyUpdated</c> tevent of its new value, its source data's (<see cref="VocabSourceData"/>, <see cref="KanjiSourceData"/>,<br/>
/// <see cref="SentenceSourceData"/>) included. Most of a note's parts change its state directly still, and a note whose stored data they<br/>
/// changed publishes <see cref="INoteTevent.PartWithoutTeventsChanged"/>, so that a taggregate store would save it.
/// </summary>
public class When_notes_publish_tevents : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   /// <summary>The tevents <paramref name="note"/> published since this was last asked, as a taggregate store takes them when it commits the note.</summary>
   static IReadOnlyList<INoteTevent> TheTeventsPublishedBy(JPNote note)
   {
      IReadOnlyList<INoteTevent> published = [];
      ((ITaggregate)note).Commit(tevents => published = [.. tevents.Select(it => (INoteTevent)it.Tevent)]);
      return published;
   }

   public class given_a_new_vocab : When_notes_publish_tevents
   {
      readonly VocabNote _vocab;
      readonly IReadOnlyList<INoteTevent> _published;

      public given_a_new_vocab()
      {
         _vocab = new VocabNote(NoteServices, "走る");
         _published = TheTeventsPublishedBy(_vocab);
      }

      [XF] public void it_published_its_created_tevent_alone() => _published.Must().Satisfy(it => it is [IVocabTevent.Created]);
      [XF] public void the_created_tevent_carries_its_question() => ((IVocabTevent.Created)_published[0]).Text.Must().Be("走る");
      [XF] public void the_created_tevent_gave_it_its_id() => _vocab.GetId().Must().Be(_published[0].TaggregateId);
      [XF] public void its_question_is_the_created_tevents() => _vocab.GetQuestion().Must().Be("走る");
   }

   public class given_a_new_kanji : When_notes_publish_tevents
   {
      readonly KanjiNote _kanji;
      readonly IReadOnlyList<INoteTevent> _published;

      public given_a_new_kanji()
      {
         _kanji = new KanjiNote(NoteServices, "漢");
         _published = TheTeventsPublishedBy(_kanji);
      }

      [XF] public void it_published_its_created_tevent_alone() => _published.Must().Satisfy(it => it is [IKanjiTevent.Created]);
      [XF] public void the_created_tevent_gave_it_its_id() => _kanji.GetId().Must().Be(_published[0].TaggregateId);
      [XF] public void its_question_is_the_created_tevents() => _kanji.GetQuestion().Must().Be("漢");
   }

   public class given_a_new_sentence : When_notes_publish_tevents
   {
      readonly SentenceNote _sentence;
      readonly IReadOnlyList<INoteTevent> _published;

      public given_a_new_sentence()
      {
         _sentence = new SentenceNote(NoteServices, "家に帰る");
         _published = TheTeventsPublishedBy(_sentence);
      }

      [XF] public void it_published_its_created_tevent_alone() => _published.Must().Satisfy(it => it is [ISentenceTevent.Created]);
      [XF] public void the_created_tevent_carries_its_source_question() => ((ISentenceTevent.Created)_published[0]).Question.Must().Be("家に帰る");
      [XF] public void the_created_tevent_gave_it_its_id() => _sentence.GetId().Must().Be(_published[0].TaggregateId);
      [XF] public void its_question_is_the_created_tevents() => _sentence.GetQuestion().Must().Be("家に帰る");
   }

   public class given_a_kanji_in_the_collection : When_notes_publish_tevents
   {
      readonly ITaggregateReference<KanjiNote> _kanjiReference;
      KanjiNote _kanji => _kanjiReference.Current;

      public given_a_kanji_in_the_collection() => _kanjiReference = CreateKanji("走", "run", "ソウ", "はし.る").Reference();

      /// <summary>The tevents the kanji's copy publishes as <paramref name="change"/> changes it, in an edit, as the unit of work dispatches them.</summary>
      protected IReadOnlyList<INoteTevent> TheTeventsPublishedAs(Action<KanjiNote> change)
      {
         List<INoteTevent> published = [];
         _kanji.Edit(it =>
         {
            using var _ = ((ITaggregate)it).TeventStream.Subscribe(tevent => published.Add((INoteTevent)tevent.Tevent));
            change(it);
         });
         return published;
      }

      public class when_its_answer_is_changed : given_a_kanji_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_answer_is_changed() => _published = TheTeventsPublishedAs(it => it.SetCorpusAnswer(" run | dash "));

         [XF] public void it_publishes_that_its_corpus_answer_was_updated_to_the_new_answer_trimmed() =>
            _published.Must().Satisfy(it => it is [IKanjiTevent.PropertyUpdated.CorpusAnswer { CorpusAnswer: "run | dash" }]);

         [XF] public void the_committed_kanji_has_the_new_answer() => _kanji.CorpusAnswer.Must().Be("run | dash");
      }

      public class when_its_source_answer_is_changed : given_a_kanji_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_source_answer_is_changed() => _published = TheTeventsPublishedAs(it => it.SourceData.SetAnswer(" running "));

         [XF] public void it_publishes_that_its_source_answer_was_updated_to_the_new_answer_trimmed() =>
            _published.Must().Satisfy(it => it is [IKanjiTevent.SourceData.PropertyUpdated.Answer { Answer: "running" }]);

         [XF] public void the_committed_kanji_has_the_new_source_answer() => _kanji.SourceData.Answer.Must().Be("running");
      }
   }

   public class given_a_sentence_in_the_collection : When_notes_publish_tevents
   {
      readonly ITaggregateReference<SentenceNote> _sentenceReference;
      SentenceNote _sentence => _sentenceReference.Current;

      public given_a_sentence_in_the_collection() => _sentenceReference = CreateSentence("家に帰る").Reference();

      /// <summary>The tevents the sentence's copy publishes as <paramref name="change"/> changes it, in an edit, as the unit of work dispatches them.</summary>
      protected IReadOnlyList<INoteTevent> TheTeventsPublishedAs(Action<SentenceNote> change)
      {
         List<INoteTevent> published = [];
         _sentence.Edit(it =>
         {
            using var _ = ((ITaggregate)it).TeventStream.Subscribe(tevent => published.Add((INoteTevent)tevent.Tevent));
            change(it);
         });
         return published;
      }

      public class when_its_answer_is_changed : given_a_sentence_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_answer_is_changed() => _published = TheTeventsPublishedAs(it => it.SetCorpusAnswer(" I go home. "));

         [XF] public void it_publishes_that_its_corpus_answer_was_updated_to_the_new_answer_trimmed() =>
            _published.Must().Satisfy(it => it is [ISentenceTevent.PropertyUpdated.CorpusAnswer { CorpusAnswer: "I go home." }]);

         [XF] public void the_committed_sentence_has_the_new_answer() => _sentence.CorpusAnswer.Must().Be("I go home.");
      }

      public class when_it_is_reparsed_to_the_result_it_has : given_a_sentence_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_reparsed_to_the_result_it_has()
         {
            _sentence.Edit(it => it.UpdateParsedWords(force: true));
            _published = TheTeventsPublishedAs(it => it.UpdateParsedWords(force: true));
         }

         [XF] public void it_publishes_nothing() => _published.Must().BeEmpty();
      }

      public class when_its_source_question_is_changed : given_a_sentence_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_source_question_is_changed() => _published = TheTeventsPublishedAs(it => it.SourceData.SetQuestion(" 家へ帰る "));

         [XF] public void it_publishes_that_its_source_question_was_updated_to_the_new_question_trimmed() =>
            _published.Must().Satisfy(it => it is [ISentenceTevent.SourceData.PropertyUpdated.Question { Question: "家へ帰る" }]);

         [XF] public void the_committed_sentence_shows_the_new_question() => _sentence.GetQuestion().Must().Be("家へ帰る");
      }
   }

   public class given_a_vocab_in_the_collection : When_notes_publish_tevents
   {
      readonly ITaggregateReference<VocabNote> _vocabReference;
      VocabNote _vocab => _vocabReference.Current;

      public given_a_vocab_in_the_collection() => _vocabReference = CreateVocab("走る", "to run", "はしる").Reference();

      /// <summary>The tevents the vocab's copy publishes as <paramref name="change"/> changes it, in an edit, as the unit of work dispatches them.</summary>
      protected IReadOnlyList<INoteTevent> TheTeventsPublishedAs(Action<VocabNote> change)
      {
         List<INoteTevent> published = [];
         _vocab.Edit(it =>
         {
            using var _ = ((ITaggregate)it).TeventStream.Subscribe(tevent => published.Add((INoteTevent)tevent.Tevent));
            change(it);
         });
         return published;
      }

      public class when_its_answer_is_changed : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_answer_is_changed() => _published = TheTeventsPublishedAs(it => it.SetCorpusAnswer(" to dash "));

         [XF] public void it_publishes_that_its_corpus_answer_was_updated_to_the_new_answer_trimmed() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.PropertyUpdated.CorpusAnswer { CorpusAnswer: "to dash" }]);

         [XF] public void the_committed_vocab_has_the_new_answer() => _vocab.CorpusAnswer.Must().Be("to dash");
      }

      public class when_its_readings_are_changed : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_readings_are_changed() => _published = TheTeventsPublishedAs(it => it.SetReadings(["はしる", "かける"]));

         [XF] public void it_publishes_that_its_readings_were_updated_to_the_new_readings() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.PropertyUpdated.Readings { Readings: ["はしる", "かける"] }]);

         [XF] public void the_committed_vocab_has_the_new_readings() => _vocab.GetReadings().Must().SequenceEqual(["はしる", "かける"]);
      }

      public class when_its_readings_are_set_to_the_readings_it_has : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_readings_are_set_to_the_readings_it_has() => _published = TheTeventsPublishedAs(it => it.SetReadings(["はしる"]));

         [XF] public void it_publishes_nothing() => _published.Must().BeEmpty();
      }

      public class when_its_source_answer_is_changed : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_source_answer_is_changed() => _published = TheTeventsPublishedAs(it => it.SourceData.SetAnswer(" to sprint "));

         [XF] public void it_publishes_that_its_source_answer_was_updated_to_the_new_answer_trimmed() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.SourceData.PropertyUpdated.Answer { Answer: "to sprint" }]);

         [XF] public void the_committed_vocab_has_the_new_source_answer() => _vocab.SourceData.Answer.Must().Be("to sprint");
      }

      public class when_its_answer_is_set_to_the_answer_it_has : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_answer_is_set_to_the_answer_it_has() => _published = TheTeventsPublishedAs(it => it.SetCorpusAnswer("to run"));

         [XF] public void it_publishes_nothing() => _published.Must().BeEmpty();
      }

      public class when_a_tag_is_set : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_a_tag_is_set() => _published = TheTeventsPublishedAs(it => it.Tags.Set(Tags.Source.Jastudio));

         [XF] public void it_publishes_that_a_part_without_tevents_changed() =>
            _published.Must().Satisfy(it => it is [INoteTevent.PartWithoutTeventsChanged]);
      }
   }
}
