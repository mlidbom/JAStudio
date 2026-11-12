using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
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
   readonly List<string> _definitionsOpened = [];

   protected When_editing_a_shared_conditions_definition_in_its_window()
   {
      var sharedConditions = GetService<SharedConditions>();
      _isAVerb = sharedConditions.Define(TokenConditions.Level, "IsAVerb", ConditionGroup.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb));
      _followsAVerb = sharedConditions.Define(Level, "FollowsAVerb", PrecedingToken.Meets(_isAVerb));
      CreateVocab("ます", "polite ending", "ます").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(_followsAVerb));
   }

   private protected SharedConditionWindowViewModel WindowOf(ISharedCondition sharedCondition) =>
      new(GetService<TemporaryServiceCollection>(), sharedCondition, it => _definitionsOpened.Add(it.FullName));

   private protected static List<string> LinesOf(SharedConditionWindowViewModel window) => [.. window.Definition.Lines[0].ThisAndAllBelow().Select(it => it.Text)];

   private protected static void SelectTheFirstLineUnderTheRoot(SharedConditionWindowViewModel window) => window.Definition.SelectedLine = window.Definition.Lines[0].Children[0];

   private protected static void WaitForTheReparseToFinish(SharedConditionWindowViewModel window) =>
      SpinWait.SpinUntil(() => !window.IsReparsingSentences, TimeSpan.FromMinutes(1)).Must().BeTrue();

   public class the_window_of_FollowsAVerb : When_editing_a_shared_conditions_definition_in_its_window
   {
      private protected readonly SharedConditionWindowViewModel _window;

      public the_window_of_FollowsAVerb() => _window = WindowOf(_followsAVerb);

      [XF] public void is_titled_by_its_full_name() => _window.Title.Must().Be("Word.FollowsAVerb - shared-condition");

      [XF] public void shows_the_definition_under_a_comment_heading_saying_what_meets_it() =>
         LinesOf(_window).Must().SequenceEqual(["// A word meets Word.FollowsAVerb when each of these holds:", "PrecedingToken.IsAVerb"]);

      [XF] public void says_how_many_vocabs_match_conditions_depend_on_it() =>
         _window.WhoUsesIt.Must().Be("The match-conditions of 1 vocab depend on it: a change to its definition reparses their sentences.");

      public class after_pressing_F12_on_the_line_using_IsAVerb : the_window_of_FollowsAVerb
      {
         public after_pressing_F12_on_the_line_using_IsAVerb()
         {
            SelectTheFirstLineUnderTheRoot(_window);
            _window.Definition.OpenTheDefinitionOfTheSelectedLine();
         }

         [XF] public void IsAVerbs_window_is_opened() => _definitionsOpened.Must().SequenceEqual(["Token.IsAVerb"]);
      }

      public class after_negating_the_line_using_IsAVerb : the_window_of_FollowsAVerb
      {
         public after_negating_the_line_using_IsAVerb()
         {
            SelectTheFirstLineUnderTheRoot(_window);
            _window.Definition.ToggleNegationOfTheSelectedLine();
            WaitForTheReparseToFinish(_window);
         }

         [XF] public void the_shared_conditions_definition_is_the_negation() => _followsAVerb.Definition.ToString().Must().Be("!PrecedingToken.IsAVerb");

         [XF] public void the_tree_shows_the_negation() =>
            LinesOf(_window).Must().SequenceEqual(["// A word meets Word.FollowsAVerb when each of these holds:", "!PrecedingToken.IsAVerb"]);

         [XF] public void the_vocab_still_depends_on_it() =>
            _window.WhoUsesIt.Must().Be("The match-conditions of 1 vocab depend on it: a change to its definition reparses their sentences.");

         public class then_undoing : after_negating_the_line_using_IsAVerb
         {
            public then_undoing()
            {
               _window.Definition.Undo();
               WaitForTheReparseToFinish(_window);
            }

            [XF] public void the_definition_is_as_before() => _followsAVerb.Definition.ToString().Must().Be("PrecedingToken.IsAVerb");
         }
      }
   }

   public class the_window_of_IsAVerb : When_editing_a_shared_conditions_definition_in_its_window
   {
      readonly SharedConditionWindowViewModel _window;

      public the_window_of_IsAVerb() => _window = WindowOf(_isAVerb);

      [XF] public void shows_the_definition_under_a_comment_heading_saying_what_meets_it() =>
         LinesOf(_window).Must().SequenceEqual(["// A token meets Token.IsAVerb when each of these holds:", "AnyOf(", "IsAGodanVerb,", "IsAnIchidanVerb)"]);

      [XF] public void counts_the_vocab_using_it_through_FollowsAVerb() =>
         _window.WhoUsesIt.Must().Be("The match-conditions of 1 vocab depend on it: a change to its definition reparses their sentences.");

      [XF] public void its_add_menu_offers_no_shared_condition_since_the_only_one_on_a_token_is_itself() =>
         _window.Definition.AddConditionMenu(_ => {}).Select(it => it.Name).Must().Satisfy(names => !names.Any(it => it.Contains("Shared")));
   }
}
