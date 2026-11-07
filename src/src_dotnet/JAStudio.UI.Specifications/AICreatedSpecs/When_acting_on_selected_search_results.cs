using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;
using JAStudio.UI.ViewModels.Workspace.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The menu of the search results selected in the grid, which acts on all their notes at once, in the background:<br/>
/// adding or removing a tag picked in a dialog, and unsuspending or suspending their cards.<br/>
/// The results keep showing the same notes afterwards, with their changes, until the search is refreshed.
/// </summary>
public class When_acting_on_selected_search_results : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string AddATag = "_a Add a tag... (Ctrl+T)";
   const string RemoveATag = "_r Remove a tag... (Ctrl+Shift+T)";
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

   /// <summary>The tag path a person picks in the tag picker dialog, or null to cancel it.</summary>
   string? _tagPathToPickInTheDialog;

   /// <summary>The tag picker dialog the menu showed last.</summary>
   TagPickerDialogViewModel? _dialogShown;

   Task<string?> PickATagInTheDialog(TagPickerDialogViewModel dialog)
   {
      _dialogShown = dialog;
      return Task.FromResult(_tagPathToPickInTheDialog);
   }

   TagPickerDialogViewModel DialogShown => _dialogShown ?? throw new InvalidOperationException("No tag picker dialog was shown.");

   /// <summary>The item named <paramref name="name"/> in the menu of all the found sentences, selected together.</summary>
   SpecMenuItem ItemInTheMenuOfAllTheFoundSentences(string name) =>
      _search.Results.ActionsOnTheSelected.Menu(FoundSentences(), PickATagInTheDialog).Single(it => it.Name == name);

   bool BothSentencesHave(string tagPath) => _rowsBeforeActing.All(it => it.Note.Tags.Contains(Tag.FromName(tagPath)));

   bool NeitherSentenceHas(string tagPath) => _rowsBeforeActing.All(it => !it.Note.Tags.Contains(Tag.FromName(tagPath)));

   public class after_adding_a_new_tag_through_the_menu : When_acting_on_selected_search_results
   {
      const string NewTag = "-::todo::check-the-translations";

      public after_adding_a_new_tag_through_the_menu()
      {
         _tagPathToPickInTheDialog = NewTag;
         ItemInTheMenuOfAllTheFoundSentences(AddATag).Action();
         SpinWait.SpinUntil(() => BothSentencesHave(NewTag), TimeToWaitForTheSearch);
      }

      [XF] public void both_sentences_have_the_tag() => BothSentencesHave(NewTag).Must().BeTrue();
      [XF] public void the_dialog_said_it_adds_a_tag_to_2_notes() => DialogShown.Title.Must().Be("Add a tag to 2 notes");
      [XF] public void both_rows_tell_the_grid_to_read_their_values_again() => _rowsBeforeActing.All(_rowsThatToldTheGridToReadTheirValuesAgain.ContainsKey).Must().BeTrue();

      public class and_removing_it_again_through_the_menu : after_adding_a_new_tag_through_the_menu
      {
         public and_removing_it_again_through_the_menu()
         {
            ItemInTheMenuOfAllTheFoundSentences(RemoveATag).Action();
            SpinWait.SpinUntil(() => NeitherSentenceHas(NewTag), TimeToWaitForTheSearch);
         }

         [XF] public void neither_sentence_has_the_tag() => NeitherSentenceHas(NewTag).Must().BeTrue();
         [XF] public void the_dialog_offered_the_tags_the_sentences_have() =>
            DialogShown.Picker.Choices.Select(it => it.TagPath).Must().SequenceEqual([NewTag, "source::anime"]);
      }
   }

   public class after_cancelling_the_dialog_for_adding_a_tag : When_acting_on_selected_search_results
   {
      public after_cancelling_the_dialog_for_adding_a_tag()
      {
         _tagPathToPickInTheDialog = null;
         ItemInTheMenuOfAllTheFoundSentences(AddATag).Action();
      }

      [XF] public void the_sentences_still_have_only_their_source_tag() =>
         _rowsBeforeActing.Select(it => string.Join(" ", it.Note.Tags.Select(tag => tag.Name))).Must().SequenceEqual(["source::anime", "source::anime"]);
   }

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
