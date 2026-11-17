using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Must;
using Compze.Teventive;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note;
using JAStudio.Core.Note.KanjiTevents;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.Tevents;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Tevents;
using JAStudio.Core.Storage.Dto;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A note is a Compze taggregate: a new note publishes its created tevent, which gives it its id, and a note's text field publishes the<br/>
/// <c>PropertyUpdated</c> tevent of its new value, its source data's (<see cref="VocabSourceData"/>, <see cref="KanjiSourceData"/>,<br/>
/// <see cref="SentenceSourceData"/>) included, as do the parts converted so far, such as a vocab's <see cref="VocabNoteRegister"/>, the flags<br/>
/// and rules of its <see cref="VocabNote.MatchingConfiguration"/>, its related vocab, a kanji's relationships to other kanji, and the notes' media. Most of a note's parts change its state directly still, and a note whose stored data they<br/>
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

      public class when_an_on_reading_is_added_in_katakana : given_a_kanji_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_an_on_reading_is_added_in_katakana() => _published = TheTeventsPublishedAs(it => it.Readings.On.Add("シュ"));

         [XF] public void it_publishes_its_on_readings_with_the_new_one_last_in_hiragana_and_secondary() =>
            _published.Must().Satisfy(it => it is [IKanjiTevent.Readings.PropertyUpdated.On { On: [_, { Kana: "しゅ", IsPrimary: false }] }]);

         [XF] public void the_committed_kanji_has_it() => _kanji.Readings.On.AllKana().Must().SequenceEqual(["そう", "しゅ"]);

         public class and_then_made_primary : when_an_on_reading_is_added_in_katakana
         {
            readonly IReadOnlyList<INoteTevent> _publishedWhenMadePrimary;

            public and_then_made_primary() => _publishedWhenMadePrimary = TheTeventsPublishedAs(it => it.Readings.On.MakePrimary("しゅ"));

            [XF] public void it_publishes_its_on_readings_with_it_primary() =>
               _publishedWhenMadePrimary.Must().Satisfy(it => it is [IKanjiTevent.Readings.PropertyUpdated.On { On: [_, { Kana: "しゅ", IsPrimary: true }] }]);
         }
      }

      public class when_an_image_is_added : given_a_kanji_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_an_image_is_added() => _published = TheTeventsPublishedAs(it => it.Images.Add(new ImageReference("run.png")));

         [XF] public void it_publishes_its_images() =>
            _published.Must().Satisfy(it => it is [IKanjiTevent.PropertyUpdated.Images { Images: [{ FileName: "run.png" }] }]);

         [XF] public void the_committed_kanji_has_it() => _kanji.Images.All.Must().SequenceEqual([new ImageReference("run.png")]);
      }

      public class when_it_is_given_the_same_radical_twice : given_a_kanji_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_given_the_same_radical_twice() => _published = TheTeventsPublishedAs(it =>
         {
            it.Radicals.Add("土");
            it.Radicals.Add("土");
         });

         [XF] public void each_publishes_the_radicals_it_has_in_their_order() =>
            _published.Must().Satisfy(it => it is [IKanjiTevent.PropertyUpdated.Radicals { Radicals: ["土"] }, IKanjiTevent.PropertyUpdated.Radicals { Radicals: ["土", "土"] }]);

         [XF] public void the_committed_kanji_has_it_twice() => _kanji.Radicals.All.Must().SequenceEqual(["土", "土"]);
      }

      public class when_a_vocab_is_made_its_last_primary_vocab : given_a_kanji_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_a_vocab_is_made_its_last_primary_vocab() => _published = TheTeventsPublishedAs(it => it.PrimaryVocab.PutLast(new WordReference("走る")));

         [XF] public void it_publishes_its_primary_vocab() =>
            _published.Must().Satisfy(it => it is [IKanjiTevent.PropertyUpdated.PrimaryVocab primaryVocab] && primaryVocab.PrimaryVocab.SequenceEqual([new WordReference("走る")]));

         [XF] public void the_committed_kanji_has_it() => _kanji.PrimaryVocab.Words.Must().SequenceEqual([new WordReference("走る")]);

         public class and_then_made_its_last_again : when_a_vocab_is_made_its_last_primary_vocab
         {
            readonly IReadOnlyList<INoteTevent> _publishedTheSecondTime;

            public and_then_made_its_last_again() => _publishedTheSecondTime = TheTeventsPublishedAs(it => it.PrimaryVocab.PutLast(new WordReference("走る")));

            [XF] public void it_publishes_nothing() => _publishedTheSecondTime.Must().BeEmpty();
         }
      }

      public class when_it_is_given_a_kanji_of_similar_meaning_that_has_no_note : given_a_kanji_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_given_a_kanji_of_similar_meaning_that_has_no_note() => _published = TheTeventsPublishedAs(it => it.SimilarMeaning.Add("駆"));

         [XF] public void it_publishes_the_kanji_it_has_a_similar_meaning_to() =>
            _published.Must().Satisfy(it => it is [IKanjiTevent.PropertyUpdated.SimilarMeaning { SimilarMeaning: ["駆"] }]);

         [XF] public void the_committed_kanji_has_it() => _kanji.SimilarMeaning.Kanji.Must().SequenceEqual(["駆"]);

         public class and_then_the_same_kanji_again : when_it_is_given_a_kanji_of_similar_meaning_that_has_no_note
         {
            readonly IReadOnlyList<INoteTevent> _publishedTheSecondTime;

            public and_then_the_same_kanji_again() => _publishedTheSecondTime = TheTeventsPublishedAs(it => it.SimilarMeaning.Add("駆"));

            [XF] public void it_publishes_nothing() => _publishedTheSecondTime.Must().BeEmpty();
         }
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

      public class when_a_screenshot_is_added : given_a_sentence_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_a_screenshot_is_added() => _published = TheTeventsPublishedAs(it => it.Screenshots.Add(new ImageReference("home.png")));

         [XF] public void it_publishes_its_screenshots() =>
            _published.Must().Satisfy(it => it is [ISentenceTevent.PropertyUpdated.Screenshots { Screenshots: [{ FileName: "home.png" }] }]);

         [XF] public void the_committed_sentence_has_it() => _sentence.Screenshots.All.Must().SequenceEqual([new ImageReference("home.png")]);
      }

      public class when_a_word_is_highlighted : given_a_sentence_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_a_word_is_highlighted() => _published = TheTeventsPublishedAs(it => it.Configuration.AddHighlightedWord(" 家 "));

         [XF] public void it_publishes_its_highlighted_words_with_the_word_trimmed_first() =>
            _published.Must().Satisfy(it => it is [ISentenceTevent.Configuration.PropertyUpdated.HighlightedWords { HighlightedWords: ["家"] }, ..]);

         [XF] public void the_committed_sentence_highlights_it() => _sentence.Configuration.HighlightedWords.Must().SequenceEqual(["家"]);

         public class and_then_the_highlighted_words_are_reset : when_a_word_is_highlighted
         {
            readonly IReadOnlyList<INoteTevent> _publishedWhenReset;

            public and_then_the_highlighted_words_are_reset() => _publishedWhenReset = TheTeventsPublishedAs(it => it.Configuration.ResetHighlightedWords());

            [XF] public void it_publishes_that_it_highlights_no_words_first() =>
               _publishedWhenReset.Must().Satisfy(it => it is [ISentenceTevent.Configuration.PropertyUpdated.HighlightedWords { HighlightedWords: [] }, ..]);

            [XF] public void the_committed_sentence_highlights_nothing() => _sentence.Configuration.HighlightedWords.Must().BeEmpty();
         }
      }

      public class when_a_word_is_marked_an_incorrect_match_everywhere_in_it : given_a_sentence_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_a_word_is_marked_an_incorrect_match_everywhere_in_it() => _published = TheTeventsPublishedAs(it => it.Configuration.IncorrectMatches.AddGlobal("帰る"));

         [XF] public void it_publishes_its_incorrect_matches_then_its_parsing_result_without_the_word() =>
            _published.Must().Satisfy(it => it is [ISentenceTevent.Configuration.PropertyUpdated.IncorrectMatches { IncorrectMatches: var incorrectMatches },
                                                   ISentenceTevent.PropertyUpdated.ParsingResult { ParsingResult: var parsingResult }]
                                          && incorrectMatches.SetEquals([WordExclusion.Global("帰る")])
                                          && parsingResult.ParsedWords.All(word => word.ParsedForm != "帰る"));

         [XF] public void the_committed_sentence_has_it() => _sentence.Configuration.IncorrectMatches.Get().Must().SequenceEqual([WordExclusion.Global("帰る")]);

         public class and_then_marked_again : when_a_word_is_marked_an_incorrect_match_everywhere_in_it
         {
            readonly IReadOnlyList<INoteTevent> _publishedTheSecondTime;

            public and_then_marked_again() => _publishedTheSecondTime = TheTeventsPublishedAs(it => it.Configuration.IncorrectMatches.AddGlobal("帰る"));

            [XF] public void it_publishes_nothing() => _publishedTheSecondTime.Must().BeEmpty();
         }
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

      public class when_it_is_made_a_transitive_godan_verb : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_made_a_transitive_godan_verb() => _published = TheTeventsPublishedAs(it => it.PartsOfSpeech.Set([PartOfSpeech.Transitive, PartOfSpeech.GodanVerb]));

         [XF] public void it_publishes_its_parts_of_speech_in_alphabetical_order() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.PropertyUpdated.PartsOfSpeech partsOfSpeech]
                                          && partsOfSpeech.PartsOfSpeech.SequenceEqual([PartOfSpeech.GodanVerb, PartOfSpeech.Transitive]));

         [XF] public void the_committed_vocab_has_them() => _vocab.PartsOfSpeech.All().Must().SequenceEqual([PartOfSpeech.GodanVerb, PartOfSpeech.Transitive]);
      }

      public class when_its_primary_compound_parts_are_set : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_primary_compound_parts_are_set() => _published = TheTeventsPublishedAs(it => it.CompoundParts.SetPrimary([new WordReference("走"), new WordReference("る")]));

         [XF] public void it_publishes_them_in_their_order() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.CompoundParts.PropertyUpdated.Primary primary]
                                          && primary.Primary.SequenceEqual([new WordReference("走"), new WordReference("る")]));

         [XF] public void the_committed_vocab_has_them() => _vocab.CompoundParts.Primary().Must().SequenceEqual([new WordReference("走"), new WordReference("る")]);
      }

      public class when_audio_is_added_to_what_its_source_gave_it : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_audio_is_added_to_what_its_source_gave_it() => _published = TheTeventsPublishedAs(it => it.SourceData.Audio.Add(new AudioReference("hashiru.mp3")));

         [XF] public void it_publishes_its_sources_audio() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.SourceData.PropertyUpdated.Audio { Audio: [{ FileName: "hashiru.mp3" }] }]);

         [XF] public void the_committed_vocab_has_it() => _vocab.SourceData.Audio.All.Must().SequenceEqual([new AudioReference("hashiru.mp3")]);
      }

      public class when_it_is_given_a_synonym_no_vocab_has : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_given_a_synonym_no_vocab_has() => _published = TheTeventsPublishedAs(it => it.RelatedNotes.Synonyms.Add(new WordReference("駆ける")));

         [XF] public void it_publishes_its_synonyms() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.RelatedVocab.PropertyUpdated.Synonyms synonyms] && synonyms.Synonyms.SetEquals([new WordReference("駆ける")]));

         [XF] public void the_committed_vocab_has_it() => _vocab.RelatedNotes.Synonyms.Words.Must().SequenceEqual([new WordReference("駆ける")]);

         public class and_then_the_same_synonym_again : when_it_is_given_a_synonym_no_vocab_has
         {
            readonly IReadOnlyList<INoteTevent> _publishedTheSecondTime;

            public and_then_the_same_synonym_again() => _publishedTheSecondTime = TheTeventsPublishedAs(it => it.RelatedNotes.Synonyms.Add(new WordReference("駆ける")));

            [XF] public void it_publishes_nothing() => _publishedTheSecondTime.Must().BeEmpty();
         }
      }

      public class when_the_word_it_is_derived_from_is_set : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_the_word_it_is_derived_from_is_set() => _published = TheTeventsPublishedAs(it => it.RelatedNotes.DerivedFrom.Set(new WordReference("走")));

         [XF] public void it_publishes_the_word() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.RelatedVocab.PropertyUpdated.DerivedFrom derivedFrom] && derivedFrom.DerivedFrom == new WordReference("走"));

         [XF] public void the_committed_vocab_is_derived_from_it() => _vocab.RelatedNotes.DerivedFrom.Word.Must().Be(new WordReference("走"));
      }

      public class when_it_is_put_in_two_registers : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_put_in_two_registers() => _published = TheTeventsPublishedAs(it =>
         {
            it.Register.Slang.SetTo(true);
            it.Register.Formal.SetTo(true);
         });

         [XF] public void each_publishes_the_names_of_the_registers_it_is_in_in_ordinal_order() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.Register.PropertyUpdated.NamesOfTheSetFlags { NamesOfTheSetFlags: ["slang"] },
                                                   IVocabTevent.Register.PropertyUpdated.NamesOfTheSetFlags { NamesOfTheSetFlags: ["formal", "slang"] }]);

         [XF] public void the_committed_vocab_is_in_both_and_in_no_other() => ((VocabDto)_vocab.ToDto()).Register.Must().SequenceEqual(["formal", "slang"]);
      }

      public class when_it_is_taken_out_of_a_register_it_is_not_in : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_taken_out_of_a_register_it_is_not_in() => _published = TheTeventsPublishedAs(it => it.Register.Humble.SetTo(false));

         [XF] public void it_publishes_nothing() => _published.Must().BeEmpty();
      }

      public class when_it_is_made_a_poison_word : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_made_a_poison_word() => _published = TheTeventsPublishedAs(it => it.MatchingConfiguration.Flags.IsPoisonWord.SetTo(true));

         [XF] public void it_publishes_the_names_of_its_set_matching_flags() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.MatchingConfiguration.PropertyUpdated.NamesOfTheSetFlags { NamesOfTheSetFlags: ["isPoisonWord"] }]);

         [XF] public void the_committed_vocab_is_a_poison_word() => _vocab.MatchingConfiguration.Flags.IsPoisonWord.IsSet().Must().BeTrue();
      }

      public class when_it_is_made_to_require_the_dictionary_form_start : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_made_to_require_the_dictionary_form_start() =>
            _published = TheTeventsPublishedAs(it => it.MatchingConfiguration.RequiresForbids.DictionaryFormStart.Choose(RequireForbidChoice.Required));

         [XF] public void it_publishes_the_rules_new_choice() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.MatchingConfiguration.PropertyUpdated.DictionaryFormStart { DictionaryFormStart: RequireForbidChoice.Required }]);

         [XF] public void the_committed_vocab_requires_it() => _vocab.MatchingConfiguration.RequiresForbids.DictionaryFormStart.Choice.Must().Be(RequireForbidChoice.Required);
      }

      public class when_it_is_made_to_forbid_yielding_its_last_token : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_made_to_forbid_yielding_its_last_token() =>
            _published = TheTeventsPublishedAs(it => it.MatchingConfiguration.RequiresForbids.YieldLastToken.Choose(RequireForbidChoice.Forbidden));

         [XF] public void it_publishes_the_rules_new_choice() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.MatchingConfiguration.PropertyUpdated.YieldLastToken { YieldLastToken: RequireForbidChoice.Forbidden }]);

         [XF] public void the_committed_vocab_forbids_it() => _vocab.MatchingConfiguration.RequiresForbids.YieldLastToken.Choice.Must().Be(RequireForbidChoice.Forbidden);
      }

      public class when_it_is_made_to_yield_to_a_surface : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_made_to_yield_to_a_surface() => _published = TheTeventsPublishedAs(it => it.MatchingConfiguration.YieldToSurface.Add("走っ"));

         [XF] public void it_publishes_the_surfaces_it_yields_to() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.MatchingConfiguration.PropertyUpdated.YieldToSurface { YieldToSurface: ["走っ"] }]);

         [XF] public void the_committed_vocab_yields_to_it() => _vocab.MatchingConfiguration.YieldToSurface.Surfaces.Must().SequenceEqual(["走っ"]);

         public class and_then_to_the_same_surface_again : when_it_is_made_to_yield_to_a_surface
         {
            readonly IReadOnlyList<INoteTevent> _publishedTheSecondTime;

            public and_then_to_the_same_surface_again() => _publishedTheSecondTime = TheTeventsPublishedAs(it => it.MatchingConfiguration.YieldToSurface.Add("走っ"));

            [XF] public void it_publishes_nothing() => _publishedTheSecondTime.Must().BeEmpty();
         }
      }

      public class when_its_match_condition_is_set : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_its_match_condition_is_set() => _published = TheTeventsPublishedAs(it => it.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(IsMatchedByItsSurface)));

         /// <summary>The <paramref name="tevents"/>, each a match-condition's tevent with the match-condition as code, or another tevent by its type: data, as a match-condition isn't.</summary>
         static IEnumerable<string> AsData(IReadOnlyList<INoteTevent> tevents) =>
            tevents.Select(it => it is IVocabTevent.MatchingConfiguration.PropertyUpdated.MatchCondition updated ? $"MatchCondition: {updated.MatchCondition}" : it.GetType().Name);

         [XF] public void it_publishes_its_new_match_condition() => AsData(_published).Must().SequenceEqual(["MatchCondition: AllOf(IsMatchedByItsSurface)"]);

         [XF] public void the_committed_vocab_has_it() => _vocab.MatchingConfiguration.MatchCondition.ToString().Must().Be("AllOf(IsMatchedByItsSurface)");

         public class and_then_set_to_the_same_again : when_its_match_condition_is_set
         {
            readonly IReadOnlyList<INoteTevent> _publishedTheSecondTime;

            public and_then_set_to_the_same_again() =>
               _publishedTheSecondTime = TheTeventsPublishedAs(it => it.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(IsMatchedByItsSurface)));

            [XF] public void it_publishes_nothing() => AsData(_publishedTheSecondTime).Must().BeEmpty();
         }
      }

      public class when_a_tag_is_set : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_a_tag_is_set() => _published = TheTeventsPublishedAs(it => it.Tags.Set(Tags.Source.Jastudio));

         [XF] public void it_publishes_its_tags() =>
            _published.Must().Satisfy(it => it is [INoteTevent.PropertyUpdated.Tags tags] && tags.Tags.Contains(Tags.Source.Jastudio));

         [XF] public void the_committed_vocab_has_it() => _vocab.Tags.ToStringList().Must().Satisfy(it => it.Contains(Tags.Source.Jastudio.Name));

         public class and_then_set_again : when_a_tag_is_set
         {
            readonly IReadOnlyList<INoteTevent> _publishedTheSecondTime;

            public and_then_set_again() => _publishedTheSecondTime = TheTeventsPublishedAs(it => it.Tags.Set(Tags.Source.Jastudio));

            [XF] public void it_publishes_nothing() => _publishedTheSecondTime.Must().BeEmpty();
         }
      }

      public class when_it_is_renamed_to_a_form_it_has : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_renamed_to_a_form_it_has()
         {
            _vocab.Edit(it => it.Forms.SetEnteredForms([EnteredForm.Owned("奔る"), EnteredForm.NotOwned("走")]));
            _published = TheTeventsPublishedAs(it => it.Question.SetText("奔る"));
         }

         [XF] public void it_publishes_that_it_was_renamed_from_its_old_name_and_then_its_entered_forms_without_the_new_question() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.Question.Renamed { From.Text: "走る", Text: "奔る", Disambiguation: "" },
                                                   IVocabTevent.Forms.PropertyUpdated.EnteredForms { EnteredForms: [{ Text: "走" }] }]);

         [XF] public void the_committed_vocab_has_the_new_question() => _vocab.GetQuestion().Must().Be("奔る");
         [XF] public void the_committed_vocab_has_the_other_form_only() => _vocab.Forms.EnteredFormsList().Must().SequenceEqual(["走"]);
         [XF] public void the_committed_vocab_matches_the_new_question() => _vocab.Forms.MatchedFormsList().Must().SequenceEqual(["奔る", "走"]);
      }

      public class when_it_is_given_a_disambiguation : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_it_is_given_a_disambiguation() => _published = TheTeventsPublishedAs(it => it.Question.SetDisambiguation("run"));

         [XF] public void it_publishes_that_it_was_renamed_with_the_same_text() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.Question.Renamed { From.Disambiguation: "", Text: "走る", Disambiguation: "run" }]);

         [XF] public void the_committed_vocab_has_it() => _vocab.Question.DisambiguationName.Must().Be("走る:run");

         public class and_then_the_same_again : when_it_is_given_a_disambiguation
         {
            readonly IReadOnlyList<INoteTevent> _publishedTheSecondTime;

            public and_then_the_same_again() => _publishedTheSecondTime = TheTeventsPublishedAs(it => it.Question.SetDisambiguation("run"));

            [XF] public void it_publishes_nothing() => _publishedTheSecondTime.Must().BeEmpty();
         }
      }

      public class when_a_form_is_added : given_a_vocab_in_the_collection
      {
         readonly IReadOnlyList<INoteTevent> _published;

         public when_a_form_is_added() => _published = TheTeventsPublishedAs(it => it.Forms.Add("奔る"));

         [XF] public void it_publishes_that_the_form_was_added_with_its_entered_forms_the_form_not_owned() =>
            _published.Must().Satisfy(it => it is [IVocabTevent.Forms.PropertyUpdated.EnteredForms.Added { Form: "奔る", EnteredForms: [{ Text: "奔る", IsOwned: false }] }]);

         [XF] public void the_committed_vocab_has_it() => _vocab.Forms.EnteredForms().Must().SequenceEqual([EnteredForm.NotOwned("奔る")]);

         public class and_then_added_again : when_a_form_is_added
         {
            readonly IReadOnlyList<INoteTevent> _publishedTheSecondTime;

            public and_then_added_again() => _publishedTheSecondTime = TheTeventsPublishedAs(it => it.Forms.Add("奔る"));

            [XF] public void it_publishes_nothing() => _publishedTheSecondTime.Must().BeEmpty();
         }

         public class and_then_owned : when_a_form_is_added
         {
            readonly IReadOnlyList<INoteTevent> _publishedWhenOwned;

            public and_then_owned() => _publishedWhenOwned = TheTeventsPublishedAs(it => it.Forms.Own("奔る"));

            [XF] public void it_publishes_its_entered_forms_with_the_form_owned() =>
               _publishedWhenOwned.Must().Satisfy(it => it is [IVocabTevent.Forms.PropertyUpdated.EnteredForms { EnteredForms: [{ Text: "奔る", IsOwned: true }] }
                                                                  and not IVocabTevent.Forms.PropertyUpdated.EnteredForms.Added]);

            [XF] public void the_committed_vocab_owns_it() => _vocab.Forms.EnteredForms().Must().SequenceEqual([EnteredForm.Owned("奔る")]);
         }

         public class and_then_removed : when_a_form_is_added
         {
            readonly IReadOnlyList<INoteTevent> _publishedWhenRemoved;

            public and_then_removed() => _publishedWhenRemoved = TheTeventsPublishedAs(it => it.Forms.Remove("奔る"));

            [XF] public void it_publishes_that_the_form_was_removed_with_no_entered_forms_left() =>
               _publishedWhenRemoved.Must().Satisfy(it => it is [IVocabTevent.Forms.PropertyUpdated.EnteredForms.Removed { Form: "奔る", EnteredForms: [] }]);

            [XF] public void the_committed_vocab_has_no_entered_forms() => _vocab.Forms.EnteredForms().Must().BeEmpty();
         }
      }
   }
}
