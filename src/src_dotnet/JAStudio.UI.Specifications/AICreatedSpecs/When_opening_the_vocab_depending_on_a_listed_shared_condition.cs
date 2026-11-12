using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The window listing the shared-conditions, with three: <c>Token.IsAVerb</c>, <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, <c>Word.FollowsAVerb</c>,<br/>
/// <c>PrecedingToken.IsAVerb</c>, which the vocab ます and です use, and <c>Token.IsIchidan</c>, <c>IsAnIchidanVerb</c>, which no vocab uses. Each listed<br/>
/// one's button lists the vocab depending on it in the workspace's "List" tab.
/// </summary>
public class When_opening_the_vocab_depending_on_a_listed_shared_condition : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly SharedConditionsWindowViewModel _list;

   public When_opening_the_vocab_depending_on_a_listed_shared_condition()
   {
      var sharedConditions = GetService<SharedConditions>();
      var isAVerb = sharedConditions.Define(TokenConditions.Level, "IsAVerb", ConditionGroup.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb));
      var followsAVerb = sharedConditions.Define(Level, "FollowsAVerb", PrecedingToken.Meets(isAVerb));
      sharedConditions.Define(TokenConditions.Level, "IsIchidan", TokenConditions.IsAnIchidanVerb);
      CreateVocab("ます", "polite ending", "ます").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(followsAVerb));
      CreateVocab("です", "polite copula", "です").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(followsAVerb));

      _list = new SharedConditionsWindowViewModel(GetService<TemporaryServiceCollection>(), Workspace.Navigation.ShowNoteList);
   }

   private protected ListedSharedConditionViewModel Listed(string fullName) => _list.SharedConditions.Single(it => it.SharedCondition.FullName == fullName);

   private protected IReadOnlyList<string> QuestionsOfTheNotesTheWorkspacesListTabLists() =>
      [.. Workspace.Navigation.NoteList.Notes.OfEachType.SelectMany(it => it.Notes).Select(it => it.Question)];

   [XF] public void the_vocab_depending_on_IsAVerb_can_be_opened() => Listed("Token.IsAVerb").OpenTheVocabDependingOnItCommand.CanExecute(null).Must().BeTrue();

   [XF] public void the_vocab_depending_on_IsIchidan_cannot_be_opened_since_there_are_none() =>
      Listed("Token.IsIchidan").OpenTheVocabDependingOnItCommand.CanExecute(null).Must().BeFalse();

   public class after_opening_the_vocab_depending_on_IsAVerb : When_opening_the_vocab_depending_on_a_listed_shared_condition
   {
      public after_opening_the_vocab_depending_on_IsAVerb() => Listed("Token.IsAVerb").OpenTheVocabDependingOnItCommand.Execute(null);

      [XF] public void the_workspace_shows_its_List_tab() => Workspace.Navigation.ShownTab.Must().Be(NavigationPaneTab.NoteList);

      [XF] public void the_List_tab_is_titled_by_the_shared_condition_and_counts_them() => Workspace.Navigation.NoteList.Title.Must().Be("Vocab depending on Token.IsAVerb (2)");

      [XF] public void it_lists_です_and_ます_which_use_it_through_FollowsAVerb() => QuestionsOfTheNotesTheWorkspacesListTabLists().Must().SequenceEqual(["です", "ます"]);
   }
}
