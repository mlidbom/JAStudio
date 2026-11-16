using System;
using System.Linq;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Batches;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Menus.Notes.Sentence;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Editors;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences.ParsingCases;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The vocab 走る and ている, and the sentence 毎日走っている, a parsing case named "the progressive" that nobody has verified, opened in the workspace<br/>
/// with its editor.
/// </summary>
public class When_a_parsing_case_is_shown_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly ITaggregateReference<SentenceNote> _sentenceReference;
   SentenceNote _sentence => _sentenceReference.Current;
   readonly SentenceEditorSectionViewModel _editor;

   protected When_a_parsing_case_is_shown_in_the_workspace()
   {
      CreateVocab("走る", "to-run", "はしる");
      CreateVocab("ている", "is-doing", "ている");
      _sentenceReference = CreateSentence("毎日走っている").Reference();
      _sentence.Edit(it =>
      {
         it.MarkAsAParsingCase();
         it.NameTheParsingCase("the progressive");
      });
      _editor = Edit<SentenceEditorSectionViewModel>(_sentence);
   }

   SentencePageViewModel Page => Workspace.SelectedTab?.NotePage as SentencePageViewModel ?? throw new InvalidOperationException("The workspace shows no sentence's page.");

   SentenceParsingCaseViewModel TheCase => Page.ParsingCase ?? throw new InvalidOperationException("The page has no parsing case section.");

   string TheVerdictChosen => TheCase.Verdicts.Single(it => it.IsChosen).Text;

   void Choose(Verdict verdict) => TheCase.Verdicts.Single(it => it.Verdict == verdict).ChooseCommand.Execute(null);

   /// <summary>The page's rows of the breakdown beside the expected one: the word displayed, then the word expected, <c>-</c> for none, and <c>(differs)</c> after a row that differs.</summary>
   string[] RowsBesideTheExpected => [.. Page.RowsBesideTheExpected.Select(it => $"{it.Displayed?.Word ?? "-"} | {it.Expected?.Word ?? "-"}{(it.Differs ? " (differs)" : "")}")];

   /// <summary>The labels of the items of the "Parsing case" submenu of the sentence's menu, without their keys.</summary>
   string[] TheParsingCaseMenu =>
      [.. new SentenceNoteMenus(GetService<Core.TemporaryServiceCollection>()).BuildNoteActionsMenuSpec("Sentence", _sentence)
                                                                              .Children.Single(it => it.Name.EndsWith("Parsing case", StringComparison.Ordinal))
                                                                              .Children.Select(it => it.Name[(it.Name.IndexOf(' ') + 1)..])];

   TextFieldEditorViewModel EditorField(string label) => _editor.ParsingCase.Fields.Single(it => it.Label == label);

   public class at_first : When_a_parsing_case_is_shown_in_the_workspace
   {
      [XF] public void the_page_has_a_parsing_case_section() => Page.IsAParsingCase.Must().BeTrue();
      [XF] public void the_section_shows_the_cases_name() => TheCase.Name.Must().Be("the progressive");
      [XF] public void the_section_shows_no_why_since_none_is_given() => TheCase.Why.Must().BeNull();
      [XF] public void the_verdict_chosen_is_unverified() => TheVerdictChosen.Must().Be("unverified");
      [XF] public void the_section_says_it_parses_as_expected() => TheCase.OutcomeText.Must().Be("Parses as expected.");
      [XF] public void the_page_shows_the_breakdown_alone() => Page.ShowsTheExpectedBesideTheBreakdown.Must().BeFalse();
      [XF] public void the_editor_shows_the_parsing_cases_name_and_why() => _editor.ParsingCase.IsAParsingCase.Must().BeTrue();
      [XF] public void the_editors_name_field_has_the_name() => EditorField("Name").Text.Must().Be("the progressive");
      [XF] public void the_menu_offers_the_other_two_verdicts_accepting_and_stopping() =>
         TheParsingCaseMenu.Must().SequenceEqual(["The displayed vocab are verified", "The displayed vocab are broken", "Accept what it parses to now", "Stop being a parsing case"]);
   }

   public class after_the_verdict_verified_is_chosen_on_the_page : When_a_parsing_case_is_shown_in_the_workspace
   {
      public after_the_verdict_verified_is_chosen_on_the_page() => Choose(Verdict.Verified);

      [XF] public void the_cases_displayed_vocab_are_verified() => _sentence.ParsingCase!.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Verified);
      [XF] public void the_page_made_anew_shows_verified_as_chosen() => TheVerdictChosen.Must().Be("verified");
   }

   public class after_a_why_is_typed_in_the_editor : When_a_parsing_case_is_shown_in_the_workspace
   {
      public after_a_why_is_typed_in_the_editor() => EditorField("Why").Text = "ている was matched as て and いる";

      [XF] public void the_case_has_the_why() => _sentence.ParsingCase!.Why.Must().Be("ている was matched as て and いる");
      [XF] public void the_page_made_anew_shows_the_why() => TheCase.Why.Must().Be("ている was matched as て and いる");
   }

   public class after_a_vocab_is_added_that_the_breakdown_displays_and_its_sentences_are_reparsed : When_a_parsing_case_is_shown_in_the_workspace
   {
      public after_a_vocab_is_added_that_the_breakdown_displays_and_its_sentences_are_reparsed()
      {
         var everyDay = CreateVocab("毎日", "every-day", "まいにち");
         GetService<LocalNoteUpdater>().ReparseSentencesForVocabWithoutProgressDialog(everyDay);
      }

      [XF] public void the_section_says_it_is_to_review() => TheCase.IsToReview.Must().BeTrue();
      [XF] public void the_section_lists_the_differences() => TheCase.Differences.Must().NotBeEmpty();
      [XF] public void the_page_shows_the_breakdown_beside_the_expected_one_with_the_row_of_the_added_vocab_differing() =>
         RowsBesideTheExpected.Must().SequenceEqual(["毎日 | 毎日 (differs)", "走る | 走る", "ている | ている"]);

      public class and_showing_what_it_expects_beside_the_breakdown_is_switched_off : after_a_vocab_is_added_that_the_breakdown_displays_and_its_sentences_are_reparsed
      {
         public and_showing_what_it_expects_beside_the_breakdown_is_switched_off() => TheCase.ShowsWhatItExpectsBesideTheBreakdown = false;

         [XF] public void the_page_made_anew_shows_the_breakdown_alone() => Page.ShowsTheExpectedBesideTheBreakdown.Must().BeFalse();
         [XF] public void the_section_still_lists_the_differences() => TheCase.Differences.Must().NotBeEmpty();
      }

      public class and_the_breakdown_is_switched_to_edit_mode : after_a_vocab_is_added_that_the_breakdown_displays_and_its_sentences_are_reparsed
      {
         public and_the_breakdown_is_switched_to_edit_mode() => GetService<JapaneseConfig>().ShowSentenceBreakdownInEditMode.Value = true;

         [XF] public void the_page_shows_the_breakdown_alone_as_edit_mode_lists_every_match() => Page.ShowsTheExpectedBesideTheBreakdown.Must().BeFalse();
      }

      public class and_what_it_parses_to_is_accepted_on_the_page : after_a_vocab_is_added_that_the_breakdown_displays_and_its_sentences_are_reparsed
      {
         public and_what_it_parses_to_is_accepted_on_the_page() => TheCase.AcceptWhatItParsesToCommand.Execute(null);

         [XF] public void the_section_says_it_parses_as_expected() => TheCase.ParsesAsExpected.Must().BeTrue();
         [XF] public void the_page_shows_the_breakdown_alone() => Page.ShowsTheExpectedBesideTheBreakdown.Must().BeFalse();
      }
   }

   public class after_it_stops_being_a_parsing_case : When_a_parsing_case_is_shown_in_the_workspace
   {
      public after_it_stops_being_a_parsing_case() => _sentence.Edit(it => it.StopBeingAParsingCase());

      [XF] public void the_page_has_no_parsing_case_section() => Page.IsAParsingCase.Must().BeFalse();
      [XF] public void the_editor_no_longer_shows_a_parsing_cases_name_and_why() => _editor.ParsingCase.IsAParsingCase.Must().BeFalse();
      [XF] public void the_menu_offers_only_marking_it_as_a_parsing_case() => TheParsingCaseMenu.Must().SequenceEqual(["Mark as a parsing case"]);

      public class and_is_then_marked_as_one_on_the_page : after_it_stops_being_a_parsing_case
      {
         public and_is_then_marked_as_one_on_the_page() => Page.MarkAsAParsingCaseCommand.Execute(null);

         [XF] public void it_is_a_parsing_case_again() => _sentence.ParsingCase.Must().NotBeNull();
         [XF] public void the_page_has_a_parsing_case_section_again() => Page.IsAParsingCase.Must().BeTrue();
         [XF] public void nobody_has_named_the_new_case() => TheCase.IsNamed.Must().BeFalse();
      }
   }
}
