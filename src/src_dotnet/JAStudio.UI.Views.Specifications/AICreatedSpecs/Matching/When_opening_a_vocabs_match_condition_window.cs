using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using JAStudio.UI.Views.Workspace.Editors.Matching;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The match-condition window of the vocab ます, whose match-condition is <c>AllOf(PrecedingToken.AnyOf(IsAGodanVerb, IsAnIchidanVerb), FollowsAVerb)</c>,<br/>
/// where <c>Word.FollowsAVerb</c> is a shared-condition, opened as a double-click on its match-condition on its Matching tab opens it. The specification<br/>
/// renders the window.
/// </summary>
public class When_opening_a_vocabs_match_condition_window : SpecificationOpeningWindows, IAIGeneratedSpec
{
   readonly VocabNote _ます;
   readonly MatchConditionWindow _window;

   public When_opening_a_vocabs_match_condition_window()
   {
      var followsAVerb = GetService<SharedConditions>().Define(WordConditions.Level, "FollowsAVerb", PrecedingToken.Meets(TokenConditions.IsAGodanVerb));
      _ます = CreateVocab("ます", "polite ending", "ます");
      _ます.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf<CandidateWordVariant>(
                                                      PrecedingToken.Meets(ConditionGroup.AnyOf<IAnalysisToken>(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb)),
                                                      followsAVerb));

      _window = Open(() => MatchConditionWindow.Open(_ます, Services));
      Render(_window, "MatchConditionWindow");
   }

   [XF] public void it_is_titled_by_the_vocabs_question() => OnTheUIThread(() => _window.Title ?? "").Must().Be("ます - match-condition");

   public class then_opening_it_again : When_opening_a_vocabs_match_condition_window
   {
      readonly MatchConditionWindow _openedAgain;

      public then_opening_it_again() => _openedAgain = Open(() => MatchConditionWindow.Open(_ます, Services));

      [XF] public void the_window_already_open_is_brought_to_the_front_rather_than_another_opened() => (_openedAgain == _window).Must().BeTrue();
   }

   public class then_closing_it_and_opening_it_again : When_opening_a_vocabs_match_condition_window
   {
      readonly MatchConditionWindow _openedAgain;

      public then_closing_it_and_opening_it_again()
      {
         OnTheUIThread(_window.Close);
         _openedAgain = Open(() => MatchConditionWindow.Open(_ます, Services));
      }

      [XF] public void a_new_window_is_opened_rather_than_the_closed_one_brought_to_the_front() => (_openedAgain == _window).Must().BeFalse();
   }
}
