using System;
using System.IO;
using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using static Compze.Must.MustActions;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The shared-conditions of <see cref="When_sharing_a_condition"/>, <c>Token.IsAVerb</c>, <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, and <c>Word.FollowsAVerb</c>,<br/>
/// <c>PrecedingToken.IsAVerb</c>, and a third, <c>Word.FollowsAVerbAsASingleToken</c>, <c>AllOf(FollowsAVerb, IsASingleToken)</c>. The vocab ます's match-condition is<br/>
/// <c>AllOf(FollowsAVerbAsASingleToken, IsMatchedByItsSurface)</c>, and を's <c>PrecedingToken.IsAVerb</c>.
/// </summary>
public class When_deleting_a_shared_condition : When_sharing_a_condition
{
   readonly SharedCondition<CandidateWordVariant> _followsAVerbAsASingleToken;
   readonly VocabNote _ます;
   readonly VocabNote _を;

   public When_deleting_a_shared_condition()
   {
      _followsAVerbAsASingleToken = _sharedConditions.Define(Level, "FollowsAVerbAsASingleToken", ConditionGroup.AllOf(_followsAVerb, IsASingleToken));
      _ます = CreateVocab("ます", "polite ending", "ます");
      _ます.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(_followsAVerbAsASingleToken, IsMatchedByItsSurface));
      _を = CreateVocab("を", "object marker", "を");
      _を.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(PrecedingToken.Meets(_isAVerb)));
   }

   void Delete(ISharedCondition sharedCondition) => _sharedConditions.Delete(sharedCondition);

   public class after_deleting_FollowsAVerbAsASingleToken : When_deleting_a_shared_condition
   {
      public after_deleting_FollowsAVerbAsASingleToken() => Delete(_followsAVerbAsASingleToken);

      [XF] public void the_vocab_using_it_has_its_definitions_conditions_in_its_place_in_its_all_of() =>
         _ます.MatchingConfiguration.MatchCondition.ToString().Must().Be("AllOf(FollowsAVerb, IsASingleToken, IsMatchedByItsSurface)");

      [XF] public void it_is_no_shared_condition_of_the_corpus() => FullNamesOf(_sharedConditions.All).Must().SequenceEqual(["Word.FollowsAVerb", "Token.IsAVerb"]);

      [XF] public void its_file_is_deleted() =>
         new DirectoryInfo(TheFolderOfTheSharedConditions).GetFiles().Select(it => it.Name).Order(StringComparer.Ordinal).Must().SequenceEqual(["Token.IsAVerb.json", "Word.FollowsAVerb.json"]);

      [XF] public void setting_a_match_condition_using_it_throws_as_an_undo_in_a_tree_editor_would() =>
         Invoking(() => _ます.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(_followsAVerbAsASingleToken))).Must().Throw<ArgumentAssertionFailedException>();

      public class and_defining_one_anew_by_its_name : after_deleting_FollowsAVerbAsASingleToken
      {
         public and_defining_one_anew_by_its_name() => _sharedConditions.Define(Level, "FollowsAVerbAsASingleToken", IsASingleToken);

         [XF] public void the_deleted_one_is_not_held_though_one_has_its_name() => _sharedConditions.Holds(_followsAVerbAsASingleToken).Must().BeFalse();

         [XF] public void redefining_the_deleted_one_throws_as_an_edit_in_its_window_would() =>
            Invoking(() => _sharedConditions.Redefine(_followsAVerbAsASingleToken, IsMatchedByItsSurface)).Must().Throw<ArgumentAssertionFailedException>();
      }
   }

   public class after_deleting_IsAVerb : When_deleting_a_shared_condition
   {
      public after_deleting_IsAVerb() => Delete(_isAVerb);

      [XF] public void the_definition_using_it_has_its_definition_written_out_in_its_place() => _followsAVerb.Definition.ToString().Must().Be("PrecedingToken.AnyOf(IsAGodanVerb, IsAnIchidanVerb)");

      [XF] public void the_vocab_using_it_has_its_definition_written_out_in_its_place() => _を.MatchingConfiguration.MatchCondition.ToString().Must().Be("AllOf(PrecedingToken.AnyOf(IsAGodanVerb, IsAnIchidanVerb))");

      [XF] public void the_vocab_using_it_only_through_another_is_unchanged() =>
         _ます.MatchingConfiguration.MatchCondition.ToString().Must().Be("AllOf(FollowsAVerbAsASingleToken, IsMatchedByItsSurface)");

      public class and_reloading_the_collection : after_deleting_IsAVerb
      {
         public and_reloading_the_collection() => GetService<JPCollection>().ReloadFromBackend();

         [XF] public void the_shared_conditions_left_are_read_back_from_their_files() =>
            _sharedConditions.All.Select(it => $"{it.FullName} = {it.Definition}")
                             .Must().SequenceEqual(["Word.FollowsAVerb = PrecedingToken.AnyOf(IsAGodanVerb, IsAnIchidanVerb)", "Word.FollowsAVerbAsASingleToken = AllOf(FollowsAVerb, IsASingleToken)"]);
      }
   }
}
