using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Editors;
using JAStudio.UI.ViewModels.Workspace.Editors.MatchConditions;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The tree editor of a vocab's match-condition, on its Matching tab: the add menu's keys pick a subject, then a condition on it, which goes into<br/>
/// the vocab's match-condition on that subject, and the rules' editors on the same tab show what the tree changed, and the other way around.
/// </summary>
public class When_editing_a_vocabs_match_condition_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly VocabNote _vocab;
   readonly VocabEditorSectionViewModel _editor;
   ConditionTextsEditorViewModel? _editorOpenedByTheMenu;

   protected When_editing_a_vocabs_match_condition_in_the_workspace()
   {
      _vocab = CreateVocab("てよろしい", "may", "てよろしい");
      _editor = Edit<VocabEditorSectionViewModel>(_vocab);
   }

   MatchConditionEditorViewModel Tree => _editor.MatchingConfig.MatchConditionEditor;

   RequireForbidFlagEditorViewModel TheGodanRulesEditor => _editor.MatchingConfig.StemMatchingRules.Single(it => it.Label == "Godan");

   List<string> Lines() => [..Tree.Lines[0].ThisAndAllBelow().Select(it => it.Text)];

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

   void WaitForTheReparseToFinish() => SpinWait.SpinUntil(() => !_editor.SentencesReparser.IsReparsingSentences, TimeSpan.FromMinutes(1)).Must().BeTrue();

   public class for_a_vocab_with_no_match_condition : When_editing_a_vocabs_match_condition_in_the_workspace
   {
      [XF] public void the_tree_is_the_comment_heading_a_match_condition_alone() => Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:"]);
      [XF] public void the_root_is_selected() => SelectedLine().Must().Be("// A word matches the vocab when each of these holds:");
   }

   public class after_picking_the_preceding_token_then_fact_then_godan_verb : When_editing_a_vocabs_match_condition_in_the_workspace
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
      [XF] public void the_godan_rules_editor_shows_it_as_the_godan_rule_required() => TheGodanRulesEditor.IsRequired.Must().BeTrue();

      public class then_negating_it : after_picking_the_preceding_token_then_fact_then_godan_verb
      {
         public then_negating_it()
         {
            Tree.ToggleNegationOfTheSelectedLine();
            WaitForTheReparseToFinish();
         }

         [XF] public void the_tree_shows_the_negation_in_front_of_it_on_the_same_line() => Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "!PrecedingToken.IsAGodanVerb"]);
         [XF] public void the_godan_rules_editor_shows_it_as_the_godan_rule_forbidden() => TheGodanRulesEditor.IsForbidden.Must().BeTrue();

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

   public class after_picking_the_following_token_then_group_then_any_of : When_editing_a_vocabs_match_condition_in_the_workspace
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
            _vocab.MatchingConfiguration.MatchCondition.Must().Be(ConditionGroup.AllOf(FollowingToken.Meets(ConditionGroup.AnyOf<IAnalysisToken>(TokenConditions.PartOfSpeechIs("名詞")))));

         [XF] public void the_editor_was_titled_with_the_condition_test_alone_since_the_group_shows_its_subject() => EditorOpenedByTheMenu.Title.Must().Be("PartOfSpeech.Is");
      }
   }

   public class after_requiring_the_godan_rule_in_its_editor : When_editing_a_vocabs_match_condition_in_the_workspace
   {
      public after_requiring_the_godan_rule_in_its_editor()
      {
         TheGodanRulesEditor.IsRequired = true;
         WaitForTheReparseToFinish();
      }

      [XF] public void the_tree_shows_the_rules_condition() => Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "PrecedingToken.IsAGodanVerb"]);

      public class then_undoing_in_the_tree : after_requiring_the_godan_rule_in_its_editor
      {
         public then_undoing_in_the_tree()
         {
            Tree.Undo();
            WaitForTheReparseToFinish();
         }

         [XF] public void the_vocab_has_no_match_condition_again() => _vocab.MatchingConfiguration.MatchCondition.IsEmpty.Must().BeTrue();
         [XF] public void the_godan_rules_editor_shows_the_rule_unset() => TheGodanRulesEditor.IsUnset.Must().BeTrue();
      }
   }
}
