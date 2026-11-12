using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.UI.ViewModels.Workspace.Editors;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// Two shared-conditions, <c>Token.IsAVerb</c>, <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, and <c>Word.FollowsAVerb</c>, <c>PrecedingToken.IsAVerb</c>,<br/>
/// and the vocab ます, whose match-condition uses <c>FollowsAVerb</c>. A shared-condition's window shows its definition in the tree editor, where every<br/>
/// change is stored as its definition and reparses the sentences of the vocab depending on it.
/// </summary>
public class When_editing_a_shared_conditions_definition_in_its_window : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly SharedCondition<IAnalysisToken> _isAVerb;
   readonly SharedCondition<CandidateWordVariant> _followsAVerb;
   readonly VocabNote _ます;
   readonly List<string> _definitionsOpened = [];

   protected When_editing_a_shared_conditions_definition_in_its_window()
   {
      var sharedConditions = GetService<SharedConditions>();
      _isAVerb = sharedConditions.Define(TokenConditions.Level, "IsAVerb", ConditionGroup.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb));
      _followsAVerb = sharedConditions.Define(Level, "FollowsAVerb", PrecedingToken.Meets(_isAVerb));
      _ます = CreateVocab("ます", "polite ending", "ます");
      _ます.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(_followsAVerb));
   }

   private protected SharedConditionWindowViewModel WindowOf(ISharedCondition sharedCondition) =>
      new(GetService<TemporaryServiceCollection>(), sharedCondition, it => _definitionsOpened.Add(it.FullName), Workspace.Navigation.ShowNoteList);

   private protected IReadOnlyList<string> QuestionsOfTheNotesTheWorkspacesListTabLists() =>
      [.. Workspace.Navigation.NoteList.Notes.OfEachType.SelectMany(it => it.Notes).Select(it => it.Question)];

   private protected static List<string> LinesOf(SharedConditionWindowViewModel window) => [.. window.DefinitionEditor.Lines[0].ThisAndAllBelow().Select(it => it.Text)];

   private protected static void SelectTheFirstLineUnderTheRoot(SharedConditionWindowViewModel window) => window.DefinitionEditor.SelectedLine = window.DefinitionEditor.Lines[0].Children[0];

   private protected static void WaitForTheReparseToFinish(SharedConditionWindowViewModel window) =>
      SpinWait.SpinUntil(() => !window.SentencesReparser.IsReparsingSentences, TimeSpan.FromMinutes(1)).Must().BeTrue();

   public class the_window_of_FollowsAVerb : When_editing_a_shared_conditions_definition_in_its_window
   {
      private protected readonly SharedConditionWindowViewModel _window;

      public the_window_of_FollowsAVerb() => _window = WindowOf(_followsAVerb);

      [XF] public void is_titled_by_its_full_name() => _window.Title.Must().Be("Word.FollowsAVerb - shared-condition");

      [XF] public void shows_the_definition_under_a_comment_heading_saying_what_meets_it() =>
         LinesOf(_window).Must().SequenceEqual(["// A word meets Word.FollowsAVerb when each of these holds:", "PrecedingToken.IsAVerb"]);

      [XF] public void says_how_many_vocabs_match_conditions_depend_on_it() =>
         _window.WhoDependsOnIt.Must().Be("The match-conditions of 1 vocab depend on it: a change to its definition reparses their sentences.");

      public class after_pressing_F12_on_the_line_using_IsAVerb : the_window_of_FollowsAVerb
      {
         public after_pressing_F12_on_the_line_using_IsAVerb()
         {
            SelectTheFirstLineUnderTheRoot(_window);
            _window.DefinitionEditor.OpenTheDefinitionOfTheSelectedLine();
         }

         [XF] public void IsAVerbs_window_is_opened() => _definitionsOpened.Must().SequenceEqual(["Token.IsAVerb"]);
      }

      public class after_negating_the_line_using_IsAVerb : the_window_of_FollowsAVerb
      {
         public after_negating_the_line_using_IsAVerb()
         {
            SelectTheFirstLineUnderTheRoot(_window);
            _window.DefinitionEditor.ToggleNegationOfTheSelectedLine();
            WaitForTheReparseToFinish(_window);
         }

         [XF] public void the_shared_conditions_definition_is_the_negation() => _followsAVerb.Definition.ToString().Must().Be("!PrecedingToken.IsAVerb");

         [XF] public void the_tree_shows_the_negation() =>
            LinesOf(_window).Must().SequenceEqual(["// A word meets Word.FollowsAVerb when each of these holds:", "!PrecedingToken.IsAVerb"]);

         [XF] public void the_vocab_still_depends_on_it() =>
            _window.WhoDependsOnIt.Must().Be("The match-conditions of 1 vocab depend on it: a change to its definition reparses their sentences.");

         public class then_undoing : after_negating_the_line_using_IsAVerb
         {
            public then_undoing()
            {
               _window.DefinitionEditor.Undo();
               WaitForTheReparseToFinish(_window);
            }

            [XF] public void the_definition_is_as_before() => _followsAVerb.Definition.ToString().Must().Be("PrecedingToken.IsAVerb");
         }
      }
   }

   public class the_window_of_FollowsAVerb_with_ます_open_on_its_Matching_tab : When_editing_a_shared_conditions_definition_in_its_window
   {
      readonly SharedConditionWindowViewModel _window;
      readonly VocabEditorSectionViewModel _ますsEditor;

      public the_window_of_FollowsAVerb_with_ます_open_on_its_Matching_tab()
      {
         _ますsEditor = Edit<VocabEditorSectionViewModel>(_ます);
         _window = WindowOf(_followsAVerb);
      }

      [XF] public void its_rename_dialog_starts_with_its_name_which_it_cannot_be_renamed() =>
         _window.RenameDialog().Must().Satisfy(it => it is { TypedName: "FollowsAVerb", WhyItCannotBeRenamed: "That's its name now." });

      [XF] public void deleting_it_says_where_its_definition_is_written_out() =>
         _window.WhatDeletingItDoes().Must().Be("Delete Word.FollowsAVerb? Its definition is written out in its place wherever it's used: in the match-conditions of 1 vocab, "
                                              + "and the definitions of 0 other shared-conditions. None of them then means anything else.");

      [XF] public void it_can_be_renamed_or_deleted_since_no_reparse_runs() => _window.CanBeRenamedOrDeleted.Must().BeTrue();

      [XF] public void it_is_not_gone() => _window.IsGone.Must().BeFalse();

      public class after_deleting_it : the_window_of_FollowsAVerb_with_ます_open_on_its_Matching_tab
      {
         public after_deleting_it() => _window.Delete();

         [XF] public void it_is_gone_so_that_the_window_closes_when_activated() => _window.IsGone.Must().BeTrue();

         [XF] public void the_vocabs_tree_on_its_Matching_tab_shows_its_definition_written_out() =>
            _ますsEditor.MatchingConfig.MatchConditionEditor.Lines[0].ThisAndAllBelow().Select(it => it.Text)
                       .Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "PrecedingToken.IsAVerb"]);
      }

      public class after_renaming_it_FollowsAVerbOfEitherKind : the_window_of_FollowsAVerb_with_ます_open_on_its_Matching_tab
      {
         public after_renaming_it_FollowsAVerbOfEitherKind() => _window.RenameTo("FollowsAVerbOfEitherKind");

         [XF] public void the_window_is_titled_by_the_new_name() => _window.Title.Must().Be("Word.FollowsAVerbOfEitherKind - shared-condition");

         [XF] public void the_heading_of_the_definition_has_the_new_name() =>
            LinesOf(_window)[0].Must().Be("// A word meets Word.FollowsAVerbOfEitherKind when each of these holds:");

         [XF] public void the_vocabs_tree_on_its_Matching_tab_shows_it_by_the_new_name() =>
            _ますsEditor.MatchingConfig.MatchConditionEditor.Lines[0].ThisAndAllBelow().Select(it => it.Text)
                       .Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "FollowsAVerbOfEitherKind"]);
      }
   }

   public class the_window_of_IsAVerb : When_editing_a_shared_conditions_definition_in_its_window
   {
      private protected readonly SharedConditionWindowViewModel _window;

      public the_window_of_IsAVerb() => _window = WindowOf(_isAVerb);

      [XF] public void shows_the_definition_under_a_comment_heading_saying_what_meets_it() =>
         LinesOf(_window).Must().SequenceEqual(["// A token meets Token.IsAVerb when each of these holds:", "AnyOf(", "IsAGodanVerb,", "IsAnIchidanVerb)"]);

      [XF] public void counts_the_vocab_using_it_through_FollowsAVerb() =>
         _window.WhoDependsOnIt.Must().Be("The match-conditions of 1 vocab depend on it: a change to its definition reparses their sentences.");

      [XF] public void its_add_menu_offers_no_shared_condition_since_the_only_one_on_a_token_is_itself() =>
         _window.DefinitionEditor.AddConditionMenu(_ => {}).Select(it => it.Name).Must().Satisfy(names => !names.Any(it => it.Contains("Shared")));

      [XF] public void the_vocab_depending_on_it_can_be_opened() => _window.OpenTheVocabDependingOnItCommand.CanExecute(null).Must().BeTrue();

      public class after_opening_the_vocab_depending_on_it : the_window_of_IsAVerb
      {
         public after_opening_the_vocab_depending_on_it() => _window.OpenTheVocabDependingOnItCommand.Execute(null);

         [XF] public void the_workspace_shows_its_List_tab() => Workspace.Navigation.ShownTab.Must().Be(NavigationPaneTab.NoteList);

         [XF] public void the_List_tab_is_titled_by_the_shared_condition_and_counts_them() => Workspace.Navigation.NoteList.Title.Must().Be("Vocab depending on Token.IsAVerb (1)");

         [XF] public void it_lists_ます_which_uses_it_through_FollowsAVerb() => QuestionsOfTheNotesTheWorkspacesListTabLists().Must().SequenceEqual(["ます"]);
      }
   }
}
