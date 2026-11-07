using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Sentences;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The results keep showing the same notes until the search changes or is refreshed, each row showing its note as it is now.<br/>
/// A refresh keeps the rows of the notes still found, so they stay where they are and selected if they were.
/// </summary>
public class When_keeping_the_results_of_a_search_up_to_date : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string SentenceAboutHome = "家に帰る";
   const string SentenceAboutSchool = "学校に行く";
   static readonly TimeSpan TimeToWaitForTheSearch = TimeSpan.FromSeconds(10);

   /// <summary>The Anki note id given to the sentence about home, so Anki can report a change to one of its cards.</summary>
   const long AnkiNoteIdOfTheSentenceAboutHome = 4711;

   readonly NoteSearchViewModel _search;
   readonly SentenceNote _sentenceAboutHome;
   readonly SentenceSearchResultViewModel _rowOfTheSentenceAboutHome;
   int _timesTheRowToldTheGridToReadItsValuesAgain;

   protected When_keeping_the_results_of_a_search_up_to_date()
   {
      _sentenceAboutHome = CreateTaggedSentence(SentenceAboutHome);
      _sentenceAboutHome.SetStudyingStatus(SuspendedReadingCard());
      GetService<ExternalNoteIdMap>().Register(AnkiNoteIdOfTheSentenceAboutHome, _sentenceAboutHome.GetId());

      _search = Workspace.Navigation.Search;
      _search.AddCondition(NoteProperties.Tags.Include("source::anime"));
      SpinWait.SpinUntil(() => !_search.IsSearching && FoundSentences().Count == 1, TimeToWaitForTheSearch).Must().BeTrue();
      _rowOfTheSentenceAboutHome = FoundSentences().Single();
      _rowOfTheSentenceAboutHome.PropertyChanged += (_, _) => Interlocked.Increment(ref _timesTheRowToldTheGridToReadItsValuesAgain);
   }

   static CardStudyingStatus SuspendedReadingCard() => new(AnkiNoteIdOfTheSentenceAboutHome, CardTypes.Reading, isSuspended: true, NoteTypes.Sentence);
   static CardStudyingStatus UnsuspendedReadingCard() => new(AnkiNoteIdOfTheSentenceAboutHome, CardTypes.Reading, isSuspended: false, NoteTypes.Sentence);

   bool TheRowToldTheGridToReadItsValuesAgain => Volatile.Read(ref _timesTheRowToldTheGridToReadItsValuesAgain) > 0;

   SentenceNote CreateTaggedSentence(string question)
   {
      var sentence = CreateTestSentence(question, "an answer");
      sentence.Tags.Set(Tag.FromName("source::anime"));
      return sentence;
   }

   List<SentenceSearchResultViewModel> FoundSentences() => [.._search.Results.Sentences.Results];

   void Refresh()
   {
      _search.RefreshCommand.Execute(null);
      SpinWait.SpinUntil(() => !_search.IsSearching, TimeToWaitForTheSearch).Must().BeTrue();
   }

   public class after_another_matching_sentence_is_created : When_keeping_the_results_of_a_search_up_to_date
   {
      public after_another_matching_sentence_is_created() => CreateTaggedSentence(SentenceAboutSchool);

      [XF] public void the_results_do_not_show_it_yet() => FoundSentences().Select(it => it.Question).Must().SequenceEqual([SentenceAboutHome]);

      public class and_refreshing_the_results : after_another_matching_sentence_is_created
      {
         public and_refreshing_the_results() => Refresh();

         [XF] public void the_results_show_both_sentences() => FoundSentences().Select(it => it.Question).Order().Must().SequenceEqual(new[] { SentenceAboutHome, SentenceAboutSchool }.Order());
         [XF] public void the_sentence_found_before_keeps_its_row() => FoundSentences().Must().Contain(_rowOfTheSentenceAboutHome);
      }
   }

   public class after_anki_reports_that_the_reading_card_of_a_found_sentence_was_unsuspended : When_keeping_the_results_of_a_search_up_to_date
   {
      public after_anki_reports_that_the_reading_card_of_a_found_sentence_was_unsuspended() =>
         GetService<JPCollection>().UpdateCardStudyingStatus(UnsuspendedReadingCard());

      [XF] public void the_row_tells_the_grid_to_read_its_values_again() => TheRowToldTheGridToReadItsValuesAgain.Must().BeTrue();
      [XF] public void the_row_shows_the_sentence_as_studying_reading() => _rowOfTheSentenceAboutHome.IsStudyingReading.Must().BeTrue();
   }

   public class after_the_studying_status_of_a_found_sentence_changes_without_any_notification : When_keeping_the_results_of_a_search_up_to_date
   {
      public after_the_studying_status_of_a_found_sentence_changes_without_any_notification() => _sentenceAboutHome.SetStudyingStatus(UnsuspendedReadingCard());

      [XF] public void the_row_does_not_tell_the_grid_to_read_its_values_again() => TheRowToldTheGridToReadItsValuesAgain.Must().BeFalse();

      public class and_refreshing_the_results : after_the_studying_status_of_a_found_sentence_changes_without_any_notification
      {
         public and_refreshing_the_results() => Refresh();

         [XF] public void the_row_tells_the_grid_to_read_its_values_again() => TheRowToldTheGridToReadItsValuesAgain.Must().BeTrue();
      }
   }
}
