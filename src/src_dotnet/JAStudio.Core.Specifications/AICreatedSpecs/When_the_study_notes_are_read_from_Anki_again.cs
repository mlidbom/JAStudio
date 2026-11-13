using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.TestUtils;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Reading the study-notes from Anki again (<see cref="JPCollection.ReadTheStudyNotesFromAnkiAgain"/>), as after an Anki operation that changed notes,<br/>
/// gives each note what Anki now holds of its study-note, without loading the corpus again, and notifies each note whose study-note differs. The sentences<br/>
/// 家に帰る and 駅に行く, each with a study-note in Anki, untagged and with no card suspended, in a collection loaded as at startup.
/// </summary>
public class When_the_study_notes_are_read_from_Anki_again : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly JPCollection Collection;
   protected readonly PretendAnki Anki;
   protected readonly SentenceNote TheSentence;
   protected readonly List<JPNote> NotifiedAsChanged = [];

   public When_the_study_notes_are_read_from_Anki_again()
   {
      var sentenceId = WithAStudyNote(CreateTestSentence("家に帰る", "Go home")).GetId();
      WithAStudyNote(CreateTestSentence("駅に行く", "Go to the station"));
      Collection = GetService<JPCollection>();
      Anki = GetService<PretendAnki>();
      Collection.ReloadFromBackend();
      TheSentence = Collection.Sentences.WithIdOrNone(sentenceId) ?? throw new System.InvalidOperationException("The sentence is gone.");
      Collection.StudyNoteChanged.Subscribe(NotifiedAsChanged.Add);
   }

   public class with_nothing_changed_in_Anki : When_the_study_notes_are_read_from_Anki_again
   {
      public with_nothing_changed_in_Anki() => Collection.ReadTheStudyNotesFromAnkiAgain();

      [XF] public void no_note_is_notified() => NotifiedAsChanged.Must().BeEmpty();
   }

   public class after_the_student_tagged_the_sentences_study_note_in_Anki : When_the_study_notes_are_read_from_Anki_again
   {
      public after_the_student_tagged_the_sentences_study_note_in_Anki()
      {
         Anki.Tag(TheSentence, "leech", "marked");
         Collection.ReadTheStudyNotesFromAnkiAgain();
      }

      [XF] public void the_sentences_Anki_tags_are_the_new_ones() => TheSentence.AnkiTags.Select(it => it.Name).Order().Must().SequenceEqual(["anki::leech", "anki::marked"]);
      [XF] public void only_the_sentence_is_notified_once() => NotifiedAsChanged.Single().Must().Be(TheSentence);

      public class and_then_read_again_with_nothing_more_changed : after_the_student_tagged_the_sentences_study_note_in_Anki
      {
         public and_then_read_again_with_nothing_more_changed() => Collection.ReadTheStudyNotesFromAnkiAgain();

         [XF] public void no_further_note_is_notified() => NotifiedAsChanged.Count.Must().Be(1);
      }

      public class and_then_its_study_note_was_deleted_in_Anki : after_the_student_tagged_the_sentences_study_note_in_Anki
      {
         public and_then_its_study_note_was_deleted_in_Anki()
         {
            Anki.DeleteTheStudyNoteOf(TheSentence);
            Collection.ReadTheStudyNotesFromAnkiAgain();
         }

         [XF] public void the_sentence_no_longer_has_an_Anki_note() => TheSentence.HasAnAnkiNote().Must().BeFalse();
         [XF] public void the_sentence_no_longer_has_Anki_tags() => TheSentence.AnkiTags.Must().BeEmpty();
         [XF] public void the_sentence_is_notified_a_second_time() => NotifiedAsChanged.Count(it => it.Equals(TheSentence)).Must().Be(2);
      }
   }

   public class after_the_student_suspended_the_sentences_reading_card_in_Anki : When_the_study_notes_are_read_from_Anki_again
   {
      public after_the_student_suspended_the_sentences_reading_card_in_Anki()
      {
         Anki.SuspendTheCard(TheSentence, CardTypes.Reading);
         Collection.ReadTheStudyNotesFromAnkiAgain();
      }

      [XF] public void the_sentence_has_a_suspended_card() => TheSentence.HasSuspendedCards().Must().BeTrue();
      [XF] public void only_the_sentence_is_notified_once() => NotifiedAsChanged.Single().Must().Be(TheSentence);
   }
}
