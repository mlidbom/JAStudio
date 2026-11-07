using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The menu of the search results selected in the grid, which acts on all their notes at once, in the background.<br/>
/// The results keep showing the same notes afterwards, with their changes, until the search is refreshed.
/// </summary>
public class When_acting_on_selected_search_results : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string UnsuspendAllCards = "_u Unsuspend all cards";
   const string SuspendAllCards = "_s Suspend all cards";
   static readonly TimeSpan TimeToWaitForTheSearch = TimeSpan.FromSeconds(10);

   readonly NoteSearchViewModel _search;
   readonly List<SentenceSearchResultViewModel> _rowsBeforeActing;

   /// <summary>The rows that told the grid to read their values again. They're told on the thread that changed their note, so this is safe to fill from any thread.</summary>
   readonly ConcurrentDictionary<SearchResultViewModel, bool> _rowsThatToldTheGridToReadTheirValuesAgain = new();

   protected When_acting_on_selected_search_results()
   {
      foreach(var question in new[] { "家に帰る", "学校に行く" })
      {
         var sentence = CreateTestSentence(question, "an answer");
         sentence.Tags.Set(Tag.FromName("source::anime"));
         sentence.SetStudyingStatus(new CardStudyingStatus(0, CardTypes.Reading, isSuspended: true, NoteTypes.Sentence));
      }

      _search = Workspace.Navigation.Search;
      _search.AddCondition(NoteProperties.Tags.Include("source::anime"));
      _search.AddCondition(NoteProperties.StudyingReading.Is(false));
      SpinWait.SpinUntil(() => !_search.IsSearching && FoundSentences().Count == 2, TimeToWaitForTheSearch).Must().BeTrue();
      _rowsBeforeActing = FoundSentences();
      foreach(var row in _rowsBeforeActing)
         row.PropertyChanged += (_, _) => _rowsThatToldTheGridToReadTheirValuesAgain[row] = true;
   }

   List<SentenceSearchResultViewModel> FoundSentences() => [.._search.Results.Sentences.Results];

   /// <summary>The item named <paramref name="name"/> in the menu of all the found sentences, selected together.</summary>
   SpecMenuItem ItemInTheMenuOfAllTheFoundSentences(string name) => _search.Results.SelectionMenu(FoundSentences()).Single(it => it.Name == name);

   public class whose_reading_cards_are_all_suspended_found_by_a_search_for_sentences_not_studying_reading : When_acting_on_selected_search_results
   {
      [XF] public void the_menu_offers_to_unsuspend_all_cards() => ItemInTheMenuOfAllTheFoundSentences(UnsuspendAllCards).IsEnabled.Must().BeTrue();
      [XF] public void the_menu_does_not_offer_to_suspend_all_cards() => ItemInTheMenuOfAllTheFoundSentences(SuspendAllCards).IsEnabled.Must().BeFalse();

      public class after_unsuspending_all_cards : whose_reading_cards_are_all_suspended_found_by_a_search_for_sentences_not_studying_reading
      {
         public after_unsuspending_all_cards()
         {
            ItemInTheMenuOfAllTheFoundSentences(UnsuspendAllCards).Action();
            SpinWait.SpinUntil(() => _rowsThatToldTheGridToReadTheirValuesAgain.Count == 2, TimeToWaitForTheSearch);
         }

         [XF] public void both_sentences_are_studying_reading() => _rowsBeforeActing.Select(it => it.Note.IsStudyingRead()).Must().SequenceEqual([true, true]);
         [XF] public void both_rows_tell_the_grid_to_read_their_values_again() => _rowsBeforeActing.All(_rowsThatToldTheGridToReadTheirValuesAgain.ContainsKey).Must().BeTrue();
         [XF] public void the_results_still_show_the_same_rows_although_the_search_would_no_longer_find_them() => FoundSentences().Must().SequenceEqual(_rowsBeforeActing);
         [XF] public void the_rows_show_both_sentences_as_studying_reading() => FoundSentences().Select(it => it.IsStudyingReading).Must().SequenceEqual([true, true]);

         public class and_refreshing_the_results : after_unsuspending_all_cards
         {
            public and_refreshing_the_results()
            {
               _search.RefreshCommand.Execute(null);
               SpinWait.SpinUntil(() => !_search.IsSearching, TimeToWaitForTheSearch).Must().BeTrue();
            }

            [XF] public void the_sentences_the_search_no_longer_finds_are_gone() => FoundSentences().Must().BeEmpty();
         }
      }
   }
}
