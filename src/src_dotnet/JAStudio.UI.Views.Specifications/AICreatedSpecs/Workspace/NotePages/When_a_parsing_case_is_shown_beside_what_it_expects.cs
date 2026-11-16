using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Batches;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.UI.Web;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;
using JAStudio.UI.Views.Workspace.NotePages;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Workspace.NotePages;

/// <summary>
/// The vocab 走る, and the sentence 毎日走っている, made a parsing case while that was the only vocab. Then the vocab ている and 毎日 are added, and<br/>
/// the sentence is reparsed: it now displays words its case doesn't expect, and doesn't display some it does. Its page, in a window.
/// </summary>
public class When_a_parsing_case_is_shown_beside_what_it_expects : SpecificationOpeningWindows, IAIGeneratedSpec
{
   /// <summary>The CardServer doesn't run in these specifications. The workspace needs its address only to build the URLs of the note pages.</summary>
   const string AddressOfACardServerThatIsNotRunning = "http://localhost";

   readonly SentencePageViewModel _page;
   readonly Window _window;

   protected When_a_parsing_case_is_shown_beside_what_it_expects()
   {
      CardServerUrl.BaseUrl = AddressOfACardServerThatIsNotRunning;
      CreateVocab("走る", "to-run", "はしる");
      var sentence = CreateSentence("毎日走っている");
      sentence.MarkAsAParsingCase();
      sentence.NameTheParsingCase("the progressive");
      ReparseItsSentences(CreateVocab("ている", "is-doing", "ている"));
      ReparseItsSentences(CreateVocab("毎日", "every-day", "まいにち"));

      _page = OnTheUIThread(() =>
      {
         var workspace = new WorkspaceViewModel(Services);
         workspace.OpenNote(sentence);
         return (SentencePageViewModel)workspace.SelectedTab!.NotePage!;
      });
      _window = Open(() =>
      {
         var window = new Window { Width = 1300, Height = 900, Content = new ScrollViewer { Content = new SentencePageView { DataContext = _page } } };
         window.Show();
         return window;
      });
      OnTheUIThread(() => Dispatcher.UIThread.RunJobs());
   }

   void ReparseItsSentences(Core.Note.Vocabulary.VocabNote vocab) => GetService<LocalNoteUpdater>().ReparseSentencesForVocabWithoutProgressDialog(vocab);

   /// <summary>Each row: the word displayed, the word expected, <c>-</c> for none, then what differs, if anything.</summary>
   string[] Rows => OnTheUIThread(() => _page.RowsBesideTheExpected
                                             .Select(it => $"{it.Displayed?.Word ?? "-"} | {it.Expected?.Word ?? "-"}{(it.Differs ? $": {string.Join("; ", it.Differences)}" : "")}")
                                             .ToArray());

   /// <summary>What the rows marked as differing show on hover, in the order of the page: only the side of what the sentence parses to is marked.</summary>
   string?[] TheHoverTextsOfTheRowsMarkedAsDiffering => OnTheUIThread(() => _window.GetVisualDescendants()
                                                                                    .OfType<Border>()
                                                                                    .Where(it => it.Classes.Contains("breakdownSide") && it.Classes.Contains("differs"))
                                                                                    .Select(it => ToolTip.GetTip(it) as string)
                                                                                    .ToArray());

   public class rendered_in_a_window : When_a_parsing_case_is_shown_beside_what_it_expects
   {
      public rendered_in_a_window() => Render(_window, "ParsingCaseBesideWhatItExpects");

      [XF] public void the_page_shows_the_breakdown_beside_what_it_expects() => OnTheUIThread(() => _page.ShowsTheExpectedBesideTheBreakdown).Must().BeTrue();

      [XF] public void the_settings_shown_leave_out_the_kanji_and_compound_parts_which_are_on_but_not_shown_beside_what_it_expects() =>
         OnTheUIThread(() => _page.ViewToggles.Select(it => it.Abbreviation).ToArray()).Must().Satisfy(it => !it.Contains("SK") && !it.Contains("SCP"));

      [XF] public void each_row_says_how_what_is_displayed_differs_from_what_is_expected() =>
         Rows.Must().SequenceEqual(["毎日 | 毎日: matched to the vocab 毎日, but expected matched to no vocab",
                                    "走る | 走る",
                                    "ている | て: displayed, but expected not displayed; expected, but not displayed: て",
                                    "- | いる: expected, but not displayed: いる"]);

      [XF] public void the_three_differing_rows_are_marked_on_the_side_of_what_it_parses_to_only_and_say_on_hover_how_they_differ() =>
         TheHoverTextsOfTheRowsMarkedAsDiffering.Must().SequenceEqual(["matched to the vocab 毎日, but expected matched to no vocab",
                                                                       $"displayed, but expected not displayed{Environment.NewLine}expected, but not displayed: て",
                                                                       "expected, but not displayed: いる"]);
   }
}
