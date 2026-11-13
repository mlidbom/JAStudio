using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Menus.Notes.Sentence;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The sentence パンを食べる。, with the vocab 食べる, opened in the workspace, its breakdown in edit mode, which lists every match whatever the sentence<br/>
/// marks it as. Right-clicking the entry of 食べる's match gives the match's menu (<see cref="SentencePageElementMenus"/>): the items that apply to it as<br/>
/// it is, each on the key it always has, then the vocab's menu and the sentence's. Its items mark the match by hand in the sentence, at its position.<br/>
/// パン, which no vocab has, is the dictionary's match, whose menu creates its vocab.
/// </summary>
public class When_a_breakdown_entry_is_right_clicked : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly SentenceNote _sentence;
   readonly VocabNote _toEat;

   public When_a_breakdown_entry_is_right_clicked()
   {
      _toEat = CreateVocab("食べる", "to eat", "たべる");
      _sentence = CreateTestSentence("パンを食べる。", "I eat bread.");
      GetService<JapaneseConfig>().ShowSentenceBreakdownInEditMode.Value = true;
      Workspace.OpenNote(_sentence);
   }

   SentencePageViewModel Page =>
      (Workspace.SelectedTab ?? throw new InvalidOperationException("The workspace shows no tab.")).NotePage as SentencePageViewModel
      ?? throw new InvalidOperationException("The tab shows no sentence's page.");

   /// <summary>The menu of the entry of 食べる's match, on the page as it is now, with <paramref name="selection"/> selected on it.</summary>
   List<SpecMenuItem> TheMenuOf食べる(string selection = "") =>
      Page.MenuOf(Page.Entries.First(it => it.Match is VocabMatch match && match.Vocab.GetId().Equals(_toEat.GetId())), selection);

   List<string> TheItemsOf食べる() => [.. TheMenuOf食べる().Select(it => it.Name)];

   List<string> TheItemsOfTheDictionarysパン() =>
      [.. Page.MenuOf(Page.Entries.First(it => it.Match is DictionaryMatch { ParsedForm: "パン" }), "").Select(it => it.Name)];

   void Pick(string item) => TheMenuOf食べる().Single(it => it.Name == item).Action();

   List<string> IncorrectMatches() => [.. _sentence.Configuration.IncorrectMatches.Get().Select(it => it.ToString())];

   List<string> HiddenMatches() => [.. _sentence.Configuration.HiddenMatches.Get().Select(it => it.ToString())];

   [XF] public void its_menu_checks_the_match_condition_against_it_marks_it_and_opens_the_vocab() =>
      TheItemsOf食べる().Must().SequenceEqual(["_u Match-condition of 食べる, checked against this word",
                                               "_e Mark incorrect here",
                                               "_o Hide here",
                                               "_a Highlight",
                                               "_i Open 食べる",
                                               "_p Open 食べる in a background tab",
                                               "_k Vocab: 食べる",
                                               "_j Sentence"]);

   [XF] public void the_menu_of_パン_which_only_the_dictionary_has_marks_it_creates_its_vocab_and_searches_the_web_for_it() =>
      TheItemsOfTheDictionarysパン().Must().SequenceEqual(["_e Mark incorrect here",
                                                           "_o Hide here",
                                                           "_a Highlight",
                                                           "_ö Create the vocab パン from the dictionary",
                                                           "_ä Web: パン",
                                                           "_j Sentence"]);

   [XF] public void with_text_selected_on_it_its_menu_ends_with_what_can_be_done_with_the_text() =>
      TheMenuOf食べる("食べ").Last().Name.Must().Be("_q Selection: \"食べ\"");

   public class after_marking_it_incorrect_here : When_a_breakdown_entry_is_right_clicked
   {
      public after_marking_it_incorrect_here() => Pick("_e Mark incorrect here");

      [XF] public void the_sentence_marks_食べる_incorrect_at_its_position_only() => IncorrectMatches().Must().SequenceEqual(["WordExclusion('食べる', 3)"]);
      [XF] public void its_menu_unmarks_it_where_it_marked_it() => TheItemsOf食べる().Must().Contain("_e Unmark incorrect");
      [XF] public void its_menu_no_longer_hides_it_since_it_isnt_shown() => TheItemsOf食べる().Must().Satisfy(it => !it.Contains("_o Hide here"));

      public class then_unmarking_it : after_marking_it_incorrect_here
      {
         public then_unmarking_it() => Pick("_e Unmark incorrect");

         [XF] public void the_sentence_marks_nothing_incorrect() => IncorrectMatches().Must().BeEmpty();
         [XF] public void its_menu_marks_it_incorrect_again() => TheItemsOf食べる().Must().Contain("_e Mark incorrect here");
      }
   }

   public class after_the_sentence_marks_食べる_incorrect_everywhere_in_it : When_a_breakdown_entry_is_right_clicked
   {
      public after_the_sentence_marks_食べる_incorrect_everywhere_in_it() => _sentence.Configuration.IncorrectMatches.AddGlobal("食べる");

      [XF] public void its_menu_says_unmarking_it_unmarks_it_everywhere_in_the_sentence() =>
         TheItemsOf食べる().Must().Contain("_e Unmark incorrect, everywhere in the sentence");

      public class then_unmarking_it : after_the_sentence_marks_食べる_incorrect_everywhere_in_it
      {
         public then_unmarking_it() => Pick("_e Unmark incorrect, everywhere in the sentence");

         [XF] public void the_sentence_marks_nothing_incorrect() => IncorrectMatches().Must().BeEmpty();
      }
   }

   public class after_hiding_it_here : When_a_breakdown_entry_is_right_clicked
   {
      public after_hiding_it_here() => Pick("_o Hide here");

      [XF] public void the_sentence_hides_食べる_at_its_position_only() => HiddenMatches().Must().SequenceEqual(["WordExclusion('食べる', 3)"]);
      [XF] public void its_menu_unhides_it() => TheItemsOf食べる().Must().Contain("_o Unhide");

      public class then_unhiding_it : after_hiding_it_here
      {
         public then_unhiding_it() => Pick("_o Unhide");

         [XF] public void the_sentence_hides_nothing() => HiddenMatches().Must().BeEmpty();
      }
   }

   public class after_highlighting_it : When_a_breakdown_entry_is_right_clicked
   {
      public after_highlighting_it() => Pick("_a Highlight");

      [XF] public void the_sentence_highlights_食べる() => _sentence.Configuration.HighlightedWords.Must().SequenceEqual(["食べる"]);
      [XF] public void its_menu_removes_the_highlight() => TheItemsOf食べる().Must().Contain("_a Remove the highlight");

      public class then_removing_the_highlight : after_highlighting_it
      {
         public then_removing_the_highlight() => Pick("_a Remove the highlight");

         [XF] public void the_sentence_highlights_nothing() => _sentence.Configuration.HighlightedWords.Must().BeEmpty();
      }
   }
}
