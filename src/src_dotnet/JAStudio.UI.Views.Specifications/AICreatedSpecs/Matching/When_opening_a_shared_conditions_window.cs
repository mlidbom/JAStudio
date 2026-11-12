using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using JAStudio.UI.Views.Workspace.Editors.Matching;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The window of the shared-condition <c>Word.FollowsAVerb</c>, defined as <c>AllOf(PrecedingToken.IsAVerb, !IsASingleToken)</c>, where <c>Token.IsAVerb</c><br/>
/// is <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, opened as F12 on it in a tree opens it. The vocab ます uses it. The specification renders the window.
/// </summary>
public class When_opening_a_shared_conditions_window : SpecificationOpeningWindows, IAIGeneratedSpec
{
   readonly SharedCondition<CandidateWordVariant> _followsAVerb;
   readonly SharedConditionWindow _window;

   public When_opening_a_shared_conditions_window()
   {
      var sharedConditions = GetService<SharedConditions>();
      var isAVerb = sharedConditions.Define(TokenConditions.Level, "IsAVerb", ConditionGroup.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb));
      _followsAVerb = sharedConditions.Define(WordConditions.Level, "FollowsAVerb", ConditionGroup.AllOf(PrecedingToken.Meets(isAVerb), ConditionGroup.Not(IsASingleToken)));
      CreateVocab("ます", "polite ending", "ます").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf<CandidateWordVariant>(_followsAVerb));

      _window = Open(() => SharedConditionWindow.Open(_followsAVerb, Services));
      Render(_window, "SharedConditionWindow");
   }

   [XF] public void it_is_titled_by_the_shared_conditions_full_name() => OnTheUIThread(() => _window.Title ?? "").Must().Be("Word.FollowsAVerb - shared-condition");

   [XF] public void it_says_how_many_vocabs_match_conditions_depend_on_it() =>
      OnTheUIThread(() => ViewModelOf<SharedConditionWindowViewModel>(_window).WhoDependsOnIt)
         .Must().Be("The match-conditions of 1 vocab depend on it: a change to its definition reparses their sentences.");

   public class then_opening_it_again : When_opening_a_shared_conditions_window
   {
      readonly SharedConditionWindow _openedAgain;

      public then_opening_it_again() => _openedAgain = Open(() => SharedConditionWindow.Open(_followsAVerb, Services));

      [XF] public void the_window_already_open_is_brought_to_the_front_rather_than_another_opened() => (_openedAgain == _window).Must().BeTrue();
   }
}
