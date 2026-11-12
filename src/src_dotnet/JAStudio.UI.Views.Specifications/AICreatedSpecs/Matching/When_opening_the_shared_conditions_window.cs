using System.Linq;
using Avalonia.Controls;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using JAStudio.UI.Menus;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The window listing the shared-conditions, opened as from the workspace's main menu, with two shared-conditions: <c>Token.IsAVerb</c>,<br/>
/// <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, and <c>Word.FollowsAVerb</c>, <c>PrecedingToken.IsAVerb</c>, which the vocab ます uses. The specification<br/>
/// renders the window.
/// </summary>
public class When_opening_the_shared_conditions_window : SpecificationOpeningWindows, IAIGeneratedSpec
{
   readonly Window _window;

   public When_opening_the_shared_conditions_window()
   {
      var sharedConditions = GetService<SharedConditions>();
      var isAVerb = sharedConditions.Define(TokenConditions.Level, "IsAVerb", ConditionGroup.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb));
      var followsAVerb = sharedConditions.Define(WordConditions.Level, "FollowsAVerb", PrecedingToken.Meets(isAVerb));
      CreateVocab("ます", "polite ending", "ます").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(followsAVerb));

      _window = Open(() => MainMenuWindows.SharedConditions.Open(Services));
      Render(_window, "SharedConditionsWindow");
   }

   SharedConditionsWindowViewModel ViewModel => ViewModelOf<SharedConditionsWindowViewModel>(_window);

   [XF] public void it_lists_each_as_it_is_defined_with_a_groups_first_condition_after_its_parenthesis_and_the_others_under_it_those_on_a_word_first() =>
      OnTheUIThread(() => ViewModel.SharedConditions.Select(it => it.Code.ReplaceLineEndings("|")).ToList())
         .Must().SequenceEqual(["Word.FollowsAVerb =|   PrecedingToken.IsAVerb", "Token.IsAVerb =|   AnyOf(IsAGodanVerb,|         IsAnIchidanVerb)"]);

   [XF] public void it_says_how_many_vocab_depend_on_each_through_others_too() =>
      OnTheUIThread(() => ViewModel.SharedConditions.Select(it => it.WhoDependsOnIt).ToList())
         .Must().SequenceEqual(["The match-conditions of 1 vocab depend on it.", "The match-conditions of 1 vocab depend on it."]);

   [XF] public void the_first_is_selected() => OnTheUIThread(() => ViewModel.Selected?.SharedCondition.FullName ?? "").Must().Be("Word.FollowsAVerb");
}
