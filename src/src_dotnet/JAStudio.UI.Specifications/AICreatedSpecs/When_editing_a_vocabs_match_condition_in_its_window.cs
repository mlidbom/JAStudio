using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Editors;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The tree editor of a vocab's match-condition, in its match-condition window, with the vocab open in the workspace on its Matching tab: the add menu's<br/>
/// keys pick a subject, then a condition on it, which goes into the vocab's match-condition on that subject. The Matching tab shows what the tree changed,<br/>
/// as code, and the window shows a change made elsewhere, such as by the MCP server, when it's activated. Checked against the vocab's word in a sentence<br/>
/// typed in the window, each line of the tree is marked by whether the word meets its condition.
/// </summary>
public class When_editing_a_vocabs_match_condition_in_its_window : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly VocabNote _vocab;
   readonly VocabEditorSectionViewModel _editor;
   readonly MatchConditionWindowViewModel _window;
   readonly List<string> _vocabWhoseSentencesWereReparsed = [];
   ConditionTextsEditorViewModel? _editorOpenedByTheMenu;

   protected When_editing_a_vocabs_match_condition_in_its_window()
   {
      _vocab = CreateVocab("てよろしい", "may", "てよろしい");
      _editor = Edit<VocabEditorSectionViewModel>(_vocab);
      _window = new MatchConditionWindowViewModel(GetService<TemporaryServiceCollection>(), _vocab, _ => {}, vocab => _vocabWhoseSentencesWereReparsed.AddRange(vocab.Select(it => it.GetQuestion())));
   }

   ConditionTreeEditorViewModel Tree => _window.MatchConditionEditor;

   List<string> TheMatchingTabsCode() => [.. _editor.MatchingConfig.MatchCondition.Lines.Select(it => it.Text)];

   List<string> Lines() => [..Tree.Lines[0].ThisAndAllBelow().Select(it => it.Text)];

   /// <summary>The tree's lines, each with the mark in front of it while the tree is checked against a word: ✓ when the word meets its condition, ✗ when not.</summary>
   List<string> LinesWithTheirCheckMarks() => [..Tree.Lines[0].ThisAndAllBelow().Select(it => $"{it.CheckMark} {it.Text}")];

   List<string> TheWordsListedInTheSentence() => [.. _window.WordToCheckAgainst.Words.Select(it => it.Text)];

   string SelectedLine() => Tree.SelectedLine?.Text ?? throw new InvalidOperationException("No line is selected.");

   /// <summary>Types <paramref name="keys"/> in the add menu, one menu down for each, and does what the last one picks.</summary>
   void PickInTheAddMenu(params char[] keys)
   {
      IReadOnlyList<SpecMenuItem> menu = Tree.AddConditionMenu(it => _editorOpenedByTheMenu = it);
      foreach(var key in keys[..^1])
         menu = TheItem(key).Children;
      TheItem(keys[^1]).Action();

      SpecMenuItem TheItem(char key) => SpecMenuItem.ItemPickedBy(key, menu) ?? throw new InvalidOperationException($"No item in the menu has the key {key}.");
   }

   /// <summary>Types <paramref name="texts"/> into the editor the add menu opened, and accepts it.</summary>
   void TypeIntoTheEditorAndAccept(string texts)
   {
      var editor = EditorOpenedByTheMenu;
      editor.Text = texts;
      Tree.AddAConditionThroughAnEditor(editor, it => Task.FromResult(it.Condition));
   }

   ConditionTextsEditorViewModel EditorOpenedByTheMenu => _editorOpenedByTheMenu ?? throw new InvalidOperationException("The add menu opened no editor.");

   /// <summary>Waits for the reparse after a change, the window's after a change in the tree, or the Matching tab's after one on the tab.</summary>
   void WaitForTheReparseToFinish() =>
      SpinWait.SpinUntil(() => !_window.SentencesReparser.IsReparsingSentences && !_editor.SentencesReparser.IsReparsingSentences, TimeSpan.FromMinutes(1)).Must().BeTrue();

   public class for_a_vocab_with_no_match_condition : When_editing_a_vocabs_match_condition_in_its_window
   {
      [XF] public void the_window_is_titled_by_the_vocabs_question() => _window.Title.Must().Be("てよろしい - match-condition");
      [XF] public void the_tree_is_the_comment_heading_a_match_condition_alone() => Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:"]);
      [XF] public void the_root_is_selected() => SelectedLine().Must().Be("// A word matches the vocab when each of these holds:");
      [XF] public void the_Matching_tab_shows_the_comment_heading_alone() => TheMatchingTabsCode().Must().SequenceEqual(["// A word matches the vocab when each of these holds:"]);
      [XF] public void it_is_not_gone() => _window.IsGone.Must().BeFalse();
   }

   public class after_picking_the_preceding_token_then_fact_then_godan_verb : When_editing_a_vocabs_match_condition_in_its_window
   {
      public after_picking_the_preceding_token_then_fact_then_godan_verb()
      {
         PickInTheAddMenu('p', 'f', 'g');
         WaitForTheReparseToFinish();
      }

      [XF] public void the_vocabs_match_condition_is_that_the_preceding_token_is_a_godan_verb() =>
         _vocab.MatchingConfiguration.MatchCondition.Must().Be(ConditionGroup.AllOf(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));

      [XF] public void the_tree_shows_it_as_code_under_the_root() => Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "PrecedingToken.IsAGodanVerb"]);
      [XF] public void the_new_line_is_selected() => SelectedLine().Must().Be("PrecedingToken.IsAGodanVerb");

      [XF] public void the_Matching_tab_shows_it_as_code() =>
         TheMatchingTabsCode().Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "PrecedingToken.IsAGodanVerb"]);

      [XF] public void the_pages_of_the_vocab_are_told_its_sentences_were_reparsed() => _vocabWhoseSentencesWereReparsed.Must().SequenceEqual(["てよろしい"]);

      public class then_negating_it : after_picking_the_preceding_token_then_fact_then_godan_verb
      {
         public then_negating_it()
         {
            Tree.ToggleNegationOfTheSelectedLine();
            WaitForTheReparseToFinish();
         }

         [XF] public void the_tree_shows_the_negation_in_front_of_it_on_the_same_line() => Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "!PrecedingToken.IsAGodanVerb"]);

         [XF] public void the_Matching_tab_shows_the_negation_too() =>
            TheMatchingTabsCode().Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "!PrecedingToken.IsAGodanVerb"]);

         public class and_undoing : then_negating_it
         {
            public and_undoing()
            {
               Tree.Undo();
               WaitForTheReparseToFinish();
            }

            [XF] public void the_negation_is_gone_from_the_vocab() =>
               _vocab.MatchingConfiguration.MatchCondition.Must().Be(ConditionGroup.AllOf(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));
         }
      }

      public class then_removing_it : after_picking_the_preceding_token_then_fact_then_godan_verb
      {
         public then_removing_it()
         {
            Tree.RemoveTheSelectedLine();
            WaitForTheReparseToFinish();
         }

         [XF] public void the_vocab_has_no_match_condition() => _vocab.MatchingConfiguration.MatchCondition.IsEmpty.Must().BeTrue();
         [XF] public void the_root_is_selected() => SelectedLine().Must().Be("// A word matches the vocab when each of these holds:");
      }
   }

   public class after_picking_the_following_token_then_group_then_any_of : When_editing_a_vocabs_match_condition_in_its_window
   {
      public after_picking_the_following_token_then_group_then_any_of()
      {
         PickInTheAddMenu('o', 'g', 'o');
         WaitForTheReparseToFinish();
      }

      [XF] public void the_tree_shows_the_empty_group_on_the_following_token() => Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "FollowingToken.AnyOf()"]);
      [XF] public void the_group_is_selected() => SelectedLine().Must().Be("FollowingToken.AnyOf()");

      public class then_picking_part_of_speech_is_in_its_add_menu_and_typing_a_noun : after_picking_the_following_token_then_group_then_any_of
      {
         public then_picking_part_of_speech_is_in_its_add_menu_and_typing_a_noun()
         {
            PickInTheAddMenu('p');
            TypeIntoTheEditorAndAccept("名詞");
            WaitForTheReparseToFinish();
         }

         [XF] public void the_condition_goes_into_the_group_as_a_condition_on_a_token() =>
            _vocab.MatchingConfiguration.MatchCondition.Must().Be(ConditionGroup.AllOf(FollowingToken.Meets(ConditionGroup.AnyOf(TokenConditions.PartOfSpeech.Is("名詞")))));

         [XF] public void the_editor_was_titled_with_the_condition_test_alone_since_the_group_shows_its_subject() => EditorOpenedByTheMenu.Title.Must().Be("PartOfSpeech.Is");
      }
   }

   public class after_typing_a_sentence_with_the_vocab_in_it_after_the_ichidan_verb_食べる : When_editing_a_vocabs_match_condition_in_its_window
   {
      public after_typing_a_sentence_with_the_vocab_in_it_after_the_ichidan_verb_食べる() => _window.WordToCheckAgainst.Sentence = "食べてよろしい。";

      [XF] public void the_vocabs_word_in_it_is_listed_as_matching_the_vocab() => TheWordsListedInTheSentence().Must().SequenceEqual(["てよろしい, at 2: matches the vocab"]);
      [XF] public void the_word_is_picked() => _window.WordToCheckAgainst.Picked?.Text.Must().Be("てよろしい, at 2: matches the vocab");
      [XF] public void the_root_line_is_marked_as_met_by_it() => LinesWithTheirCheckMarks().Must().SequenceEqual(["✓ // A word matches the vocab when each of these holds:"]);

      public class then_requiring_the_preceding_token_to_be_a_godan_verb : after_typing_a_sentence_with_the_vocab_in_it_after_the_ichidan_verb_食べる
      {
         public then_requiring_the_preceding_token_to_be_a_godan_verb()
         {
            PickInTheAddMenu('p', 'f', 'g');
            WaitForTheReparseToFinish();
         }

         [XF] public void both_lines_are_marked_as_not_met_by_it() =>
            LinesWithTheirCheckMarks().Must().SequenceEqual(["✗ // A word matches the vocab when each of these holds:", "✗ PrecedingToken.IsAGodanVerb"]);

         [XF] public void the_word_is_listed_as_not_matching_the_vocab_once_its_sentences_were_reparsed() =>
            TheWordsListedInTheSentence().Must().Satisfy(it => it.Single().StartsWith("てよろしい, at 2: doesn't match: ", StringComparison.Ordinal));

         public class and_negating_it : then_requiring_the_preceding_token_to_be_a_godan_verb
         {
            public and_negating_it()
            {
               Tree.ToggleNegationOfTheSelectedLine();
               WaitForTheReparseToFinish();
            }

            [XF] public void both_lines_are_marked_as_met_by_it() =>
               LinesWithTheirCheckMarks().Must().SequenceEqual(["✓ // A word matches the vocab when each of these holds:", "✓ !PrecedingToken.IsAGodanVerb"]);

            [XF] public void the_word_is_listed_as_matching_the_vocab_again() => TheWordsListedInTheSentence().Must().SequenceEqual(["てよろしい, at 2: matches the vocab"]);
         }
      }

      public class then_typing_a_sentence_without_the_vocab_in_it : after_typing_a_sentence_with_the_vocab_in_it_after_the_ichidan_verb_食べる
      {
         public then_typing_a_sentence_without_the_vocab_in_it() => _window.WordToCheckAgainst.Sentence = "食べた。";

         [XF] public void no_word_is_listed() => TheWordsListedInTheSentence().Must().BeEmpty();
         [XF] public void it_says_no_word_in_the_sentence_is_one_of_the_vocabs_forms() => _window.WordToCheckAgainst.NoWordIsOneOfTheVocabsForms.Must().BeTrue();
         [XF] public void the_tree_is_no_longer_checked() => LinesWithTheirCheckMarks().Must().SequenceEqual([" // A word matches the vocab when each of these holds:"]);
      }
   }

   public class after_the_match_condition_is_changed_elsewhere : When_editing_a_vocabs_match_condition_in_its_window
   {
      public after_the_match_condition_is_changed_elsewhere() =>
         _vocab.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));

      [XF] public void the_tree_shows_it_only_once_the_window_is_activated() => Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:"]);

      public class then_activating_the_window : after_the_match_condition_is_changed_elsewhere
      {
         public then_activating_the_window() => _window.ShowItAsItIsNow();

         [XF] public void the_tree_shows_the_changed_match_condition() => Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "PrecedingToken.IsAGodanVerb"]);
      }

      public class then_activating_the_window_and_undoing_in_the_tree : after_the_match_condition_is_changed_elsewhere
      {
         public then_activating_the_window_and_undoing_in_the_tree()
         {
            _window.ShowItAsItIsNow();
            Tree.Undo();
            WaitForTheReparseToFinish();
         }

         [XF] public void the_vocab_has_no_match_condition_again() => _vocab.MatchingConfiguration.MatchCondition.IsEmpty.Must().BeTrue();
         [XF] public void the_Matching_tab_shows_the_comment_heading_alone() => TheMatchingTabsCode().Must().SequenceEqual(["// A word matches the vocab when each of these holds:"]);
      }
   }
}
