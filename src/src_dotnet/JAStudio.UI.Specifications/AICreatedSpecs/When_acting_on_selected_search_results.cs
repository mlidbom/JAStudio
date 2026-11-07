using System;
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
/// The menu of the search results selected in the grid, which acts on all their notes at once, in the background,<br/>
/// and then runs the search again so the results show the change.
/// </summary>
public class When_acting_on_selected_search_results : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string UnsuspendAllCards = "_u Unsuspend all cards";
   const string SuspendAllCards = "_s Suspend all cards";
   static readonly TimeSpan TimeToWaitForTheSearch = TimeSpan.FromSeconds(10);

   readonly NoteSearchViewModel _search;

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
      SpinWait.SpinUntil(() => FoundSentences().Count == 2, TimeToWaitForTheSearch).Must().BeTrue();
   }

   List<SentenceSearchResultViewModel> FoundSentences() => [.._search.Results.Sentences.Results];

   /// <summary>The item named <paramref name="name"/> in the menu of all the found sentences, selected together.</summary>
   SpecMenuItem ItemInTheMenuOfAllTheFoundSentences(string name) => _search.Results.SelectionMenu(FoundSentences()).Single(it => it.Name == name);

   public class whose_reading_cards_are_all_suspended : When_acting_on_selected_search_results
   {
      [XF] public void the_menu_offers_to_unsuspend_all_cards() => ItemInTheMenuOfAllTheFoundSentences(UnsuspendAllCards).IsEnabled.Must().BeTrue();
      [XF] public void the_menu_does_not_offer_to_suspend_all_cards() => ItemInTheMenuOfAllTheFoundSentences(SuspendAllCards).IsEnabled.Must().BeFalse();

      public class after_unsuspending_all_cards : whose_reading_cards_are_all_suspended
      {
         public after_unsuspending_all_cards()
         {
            ItemInTheMenuOfAllTheFoundSentences(UnsuspendAllCards).Action();
            SpinWait.SpinUntil(() => FoundSentences() is { Count: 2 } found && found.All(it => it.IsStudyingReading), TimeToWaitForTheSearch);
         }

         [XF] public void the_results_show_both_sentences_as_studying_reading() => FoundSentences().Select(it => it.IsStudyingReading).Must().SequenceEqual([true, true]);
         [XF] public void both_sentences_are_studying_reading() => FoundSentences().Select(it => it.Note.IsStudyingRead()).Must().SequenceEqual([true, true]);
      }
   }
}
