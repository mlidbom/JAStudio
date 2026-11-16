using System;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.ViewModels.Workspace.Navigation.ParsingCases;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The vocab 走る and ている, and three sentences: 毎日走っている, a parsing case named "the progressive" that nobody has verified; 走った, a parsing case<br/>
/// named "the past tense" whose displayed vocab are verified; and 毎日走る, which is no parsing case. The navigation pane shows its Parsing cases tab.
/// </summary>
public class When_listing_the_parsing_cases_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string TheProgressive = "毎日走っている";
   const string ThePastTense = "走った";
   static readonly TimeSpan TimeToWaitForTheComparison = TimeSpan.FromSeconds(10);

   readonly ParsingCasesViewModel _parsingCases;
   readonly ITaggregateReference<SentenceNote> _theProgressiveReference;
   SentenceNote _theProgressive => _theProgressiveReference.Current;
   readonly ITaggregateReference<SentenceNote> _thePastTenseReference;
   SentenceNote _thePastTense => _thePastTenseReference.Current;
   readonly ITaggregateReference<SentenceNote> _theOrdinarySentenceReference;
   SentenceNote _theOrdinarySentence => _theOrdinarySentenceReference.Current;

   protected When_listing_the_parsing_cases_in_the_workspace()
   {
      CreateVocab("走る", "to-run", "はしる");
      CreateVocab("ている", "is-doing", "ている");
      _theProgressiveReference = AParsingCase(TheProgressive, "the progressive").Reference();
      _thePastTenseReference = AParsingCase(ThePastTense, "the past tense").Reference();
      _thePastTense.Edit(it => it.SetTheVerdictOnTheDisplayedVocab(Verdict.Verified));
      _theOrdinarySentenceReference = CreateSentence("毎日走る").Reference();

      _parsingCases = Workspace.Navigation.ParsingCases;
      Workspace.Navigation.ShownTab = NavigationPaneTab.ParsingCases;
      WaitForTheComparison();
   }

   SentenceNote AParsingCase(string question, string name)
   {
      var sentence = CreateSentence(question);
      sentence.Edit(it =>
      {
         it.MarkAsAParsingCase();
         it.NameTheParsingCase(name);
      });
      return sentence;
   }

   void WaitForTheComparison() => SpinWait.SpinUntil(() => !_parsingCases.IsComparing, TimeToWaitForTheComparison).Must().BeTrue();

   void Refresh()
   {
      _parsingCases.RefreshCommand.Execute(null);
      WaitForTheComparison();
   }

   /// <summary>The sentences of the cases listed, in the order of their questions.</summary>
   string[] SentencesListed => [.. _parsingCases.Cases.Select(it => it.Question).Order(StringComparer.Ordinal)];

   ListedParsingCaseViewModel RowOf(string sentence) => _parsingCases.Cases.Single(it => it.Question == sentence);

   ParsingCaseFilterViewModel Filter(string label) => _parsingCases.ByOutcome.Filters.Concat(_parsingCases.ByVerdict.Filters).Single(it => it.Label == label);

   public class at_first : When_listing_the_parsing_cases_in_the_workspace
   {
      [XF] public void the_two_parsing_cases_are_listed_and_the_ordinary_sentence_is_not() => SentencesListed.Must().SequenceEqual([TheProgressive, ThePastTense]);
      [XF] public void a_case_is_listed_with_its_name() => RowOf(TheProgressive).Name.Must().Be("the progressive");
      [XF] public void a_case_that_parses_as_it_expects_is_listed_as_parsing_as_expected() => RowOf(TheProgressive).OutcomeText.Must().Be("as expected");
      [XF] public void the_unverified_case_is_listed_as_unverified() => RowOf(TheProgressive).VerdictText.Must().Be("unverified");
      [XF] public void the_verified_case_is_listed_as_verified() => RowOf(ThePastTense).VerdictText.Must().Be("verified");
      [XF] public void the_status_says_how_many_parsing_cases_there_are() => _parsingCases.StatusText.Must().Be("2 parsing cases");
      [XF] public void the_filter_for_those_that_parse_as_expected_counts_both() => Filter("as expected").Text.Must().Be("as expected (2)");
      [XF] public void the_filter_for_the_unverified_counts_one() => Filter("unverified").Text.Must().Be("unverified (1)");
      [XF] public void the_filter_for_those_to_review_counts_none() => Filter("to review").Text.Must().Be("to review (0)");
   }

   public class with_the_filter_for_the_unverified_on : When_listing_the_parsing_cases_in_the_workspace
   {
      public with_the_filter_for_the_unverified_on() => Filter("unverified").IsOn = true;

      [XF] public void only_the_unverified_case_is_listed() => SentencesListed.Must().SequenceEqual([TheProgressive]);
      [XF] public void the_status_says_how_many_of_the_parsing_cases_are_listed() => _parsingCases.StatusText.Must().Be("1 of 2 parsing cases");

      public class and_the_filter_for_the_verified_on_too : with_the_filter_for_the_unverified_on
      {
         public and_the_filter_for_the_verified_on_too() => Filter("verified").IsOn = true;

         [XF] public void the_cases_of_either_verdict_are_listed() => SentencesListed.Must().SequenceEqual([TheProgressive, ThePastTense]);
      }

      public class and_the_filter_for_those_to_review_on_too : with_the_filter_for_the_unverified_on
      {
         public and_the_filter_for_those_to_review_on_too() => Filter("to review").IsOn = true;

         [XF] public void no_case_is_listed_since_none_is_both_unverified_and_to_review() => SentencesListed.Must().BeEmpty();
      }

      public class after_the_listed_cases_displayed_vocab_are_verified : with_the_filter_for_the_unverified_on
      {
         public after_the_listed_cases_displayed_vocab_are_verified() => _theProgressive.Edit(it => it.SetTheVerdictOnTheDisplayedVocab(Verdict.Verified));

         [XF] public void the_case_stays_listed() => SentencesListed.Must().SequenceEqual([TheProgressive]);
         [XF] public void its_row_shows_it_as_verified() => RowOf(TheProgressive).VerdictText.Must().Be("verified");
         [XF] public void the_filter_for_the_unverified_counts_none() => Filter("unverified").Text.Must().Be("unverified (0)");

         public class and_the_list_is_refreshed : after_the_listed_cases_displayed_vocab_are_verified
         {
            public and_the_list_is_refreshed() => Refresh();

            [XF] public void the_case_is_no_longer_listed() => SentencesListed.Must().BeEmpty();
         }
      }
   }

   public class with_a_text_filter_that_a_part_of_one_cases_name_matches_in_another_letter_case : When_listing_the_parsing_cases_in_the_workspace
   {
      public with_a_text_filter_that_a_part_of_one_cases_name_matches_in_another_letter_case() => _parsingCases.TextFilter = "PAST";

      [XF] public void only_that_case_is_listed() => SentencesListed.Must().SequenceEqual([ThePastTense]);
   }

   public class with_a_text_filter_that_a_part_of_one_cases_sentence_matches : When_listing_the_parsing_cases_in_the_workspace
   {
      public with_a_text_filter_that_a_part_of_one_cases_sentence_matches() => _parsingCases.TextFilter = "毎日";

      [XF] public void only_that_case_is_listed() => SentencesListed.Must().SequenceEqual([TheProgressive]);
   }

   public class after_a_vocab_is_added_that_the_breakdown_of_the_unverified_case_displays_and_the_list_is_refreshed : When_listing_the_parsing_cases_in_the_workspace
   {
      public after_a_vocab_is_added_that_the_breakdown_of_the_unverified_case_displays_and_the_list_is_refreshed()
      {
         CreateVocab("毎日", "every-day", "まいにち");
         Refresh();
      }

      [XF] public void the_case_is_listed_as_to_review() => RowOf(TheProgressive).OutcomeText.Must().Be("to review");
      [XF] public void the_case_is_listed_with_its_differences() => RowOf(TheProgressive).Differences.Must().NotBeEmpty();
      [XF] public void the_other_case_is_still_listed_as_parsing_as_expected() => RowOf(ThePastTense).OutcomeText.Must().Be("as expected");
      [XF] public void the_filter_for_those_to_review_counts_one() => Filter("to review").Text.Must().Be("to review (1)");

      public class and_what_the_case_parses_to_is_accepted : after_a_vocab_is_added_that_the_breakdown_of_the_unverified_case_displays_and_the_list_is_refreshed
      {
         public and_what_the_case_parses_to_is_accepted() => _theProgressive.Edit(it => it.AcceptWhatItParsesToAsAParsingCase());

         [XF] public void the_case_is_listed_as_parsing_as_expected_at_once() => RowOf(TheProgressive).OutcomeText.Must().Be("as expected");
         [XF] public void the_filter_for_those_to_review_counts_none() => Filter("to review").Text.Must().Be("to review (0)");
      }
   }

   public class after_the_ordinary_sentence_is_marked_as_a_parsing_case : When_listing_the_parsing_cases_in_the_workspace
   {
      public after_the_ordinary_sentence_is_marked_as_a_parsing_case() => _theOrdinarySentence.Edit(it => it.MarkAsAParsingCase());

      [XF] public void it_is_listed_too() => SentencesListed.Must().SequenceEqual([TheProgressive, "毎日走る", ThePastTense]);
      [XF] public void the_status_counts_it() => _parsingCases.StatusText.Must().Be("3 parsing cases");
   }

   public class after_a_case_stops_being_a_parsing_case : When_listing_the_parsing_cases_in_the_workspace
   {
      public after_a_case_stops_being_a_parsing_case() => _thePastTense.Edit(it => it.StopBeingAParsingCase());

      [XF] public void it_is_no_longer_listed() => SentencesListed.Must().SequenceEqual([TheProgressive]);
      [XF] public void the_status_no_longer_counts_it() => _parsingCases.StatusText.Must().Be("1 parsing case");
   }

   public class opening_a_listed_case : When_listing_the_parsing_cases_in_the_workspace
   {
      public opening_a_listed_case() => _parsingCases.Open(RowOf(ThePastTense));

      [XF] public void the_workspace_shows_the_cases_sentence() => Workspace.SelectedTab!.ShownNoteId.Must().Be(_thePastTense.GetId());
   }
}
