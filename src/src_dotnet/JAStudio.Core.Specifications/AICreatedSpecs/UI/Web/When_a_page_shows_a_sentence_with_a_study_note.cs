using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.TestUtils;
using JAStudio.Core.UI.Web;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.UI.Web;

/// <summary>
/// A page showing a note is outdated by the changes <see cref="ChangesThatOutdateNotePages"/> names, and by no others. The sentences 家に帰る and<br/>
/// 駅に行く, each with a study-note in Anki, in a collection loaded as at startup, and a page that shows 家に帰る and follows the changes that outdate it.
/// </summary>
public class When_a_page_shows_a_sentence_with_a_study_note : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly JPCollection Collection;
   protected readonly SentenceNote TheShownSentence;
   protected readonly SentenceNote TheOtherSentence;
   protected readonly IDisposable ThePagesFollowingOfTheChanges;
   protected int TimesThePageWasOutdated;

   public When_a_page_shows_a_sentence_with_a_study_note()
   {
      var shownSentenceId = WithAStudyNote(CreateTestSentence("家に帰る", "Go home")).GetId();
      var otherSentenceId = WithAStudyNote(CreateTestSentence("駅に行く", "Go to the station")).GetId();
      Collection = GetService<JPCollection>();
      Collection.ReloadFromBackend();
      TheShownSentence = Collection.Sentences.WithIdOrNone(shownSentenceId) ?? throw new InvalidOperationException("The shown sentence is gone.");
      TheOtherSentence = Collection.Sentences.WithIdOrNone(otherSentenceId) ?? throw new InvalidOperationException("The other sentence is gone.");
      ThePagesFollowingOfTheChanges = GetService<ChangesThatOutdateNotePages>().Subscribe(TheShownSentence.GetId(), () => TimesThePageWasOutdated++);
   }

   public class and_nothing_changes : When_a_page_shows_a_sentence_with_a_study_note
   {
      [XF] public void the_page_is_not_outdated() => TimesThePageWasOutdated.Must().Be(0);
   }

   public class and_the_shown_sentences_answer_is_changed : When_a_page_shows_a_sentence_with_a_study_note
   {
      public and_the_shown_sentences_answer_is_changed() => TheShownSentence.CorpusAnswer.Set("Return home");

      [XF] public void the_page_is_outdated_once() => TimesThePageWasOutdated.Must().Be(1);
   }

   public class and_the_other_sentences_answer_is_changed : When_a_page_shows_a_sentence_with_a_study_note
   {
      public and_the_other_sentences_answer_is_changed() => TheOtherSentence.CorpusAnswer.Set("Head for the station");

      [XF] public void the_page_is_not_outdated() => TimesThePageWasOutdated.Must().Be(0);
   }

   public class and_the_shown_sentence_is_tagged_in_Anki_and_the_study_notes_are_read_from_Anki_again : When_a_page_shows_a_sentence_with_a_study_note
   {
      public and_the_shown_sentence_is_tagged_in_Anki_and_the_study_notes_are_read_from_Anki_again()
      {
         GetService<PretendAnki>().Tag(TheShownSentence, "marked");
         Collection.ReadTheStudyNotesFromTheSRSAgain();
      }

      [XF] public void the_page_is_outdated_once() => TimesThePageWasOutdated.Must().Be(1);
   }

   public class and_the_shown_sentence_is_deleted_with_its_study_note : When_a_page_shows_a_sentence_with_a_study_note
   {
      public and_the_shown_sentence_is_deleted_with_its_study_note() => Collection.Delete(TheShownSentence, ItsStudyNote.IsDeleted);

      [XF] public void the_page_is_outdated_once() => TimesThePageWasOutdated.Must().Be(1);
   }

   public class and_a_setting_is_changed : When_a_page_shows_a_sentence_with_a_study_note
   {
      public and_a_setting_is_changed()
      {
         var setting = GetService<JapaneseConfig>().AutoadvanceVocabStartingSeconds;
         setting.Value += 1;
      }

      [XF] public void the_page_is_outdated_once() => TimesThePageWasOutdated.Must().Be(1);
   }

   public class and_the_collection_is_loaded_again : When_a_page_shows_a_sentence_with_a_study_note
   {
      public and_the_collection_is_loaded_again() => Collection.ReloadFromBackend();

      [XF] public void the_page_is_outdated_once() => TimesThePageWasOutdated.Must().Be(1);
   }

   public class and_the_page_stops_following_the_changes : When_a_page_shows_a_sentence_with_a_study_note
   {
      public and_the_page_stops_following_the_changes() => ThePagesFollowingOfTheChanges.Dispose();

      public class and_then_the_shown_sentences_answer_is_changed : and_the_page_stops_following_the_changes
      {
         public and_then_the_shown_sentences_answer_is_changed() => TheShownSentence.CorpusAnswer.Set("Return home");

         [XF] public void the_page_is_not_outdated() => TimesThePageWasOutdated.Must().Be(0);
      }
   }
}
