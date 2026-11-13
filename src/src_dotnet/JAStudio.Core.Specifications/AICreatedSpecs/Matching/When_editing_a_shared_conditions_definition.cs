using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.Parsing.Tokenizing;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static Compze.Must.MustActions;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The shared-conditions of <see cref="When_sharing_a_condition"/>, <c>Token.IsAVerb</c> and <c>Word.FollowsAVerb</c>, which uses it, and a third,<br/>
/// <c>Token.IsNotAVerb</c>, <c>!IsAVerb</c>. A shared-condition's definition is edited in the tree editor as an <see cref="EditedDefinition"/>.
/// </summary>
public class When_editing_a_shared_conditions_definition : When_sharing_a_condition
{
   protected readonly SharedCondition<AnalysisToken> _isNotAVerb;
   protected readonly EditedDefinition _definitionOfIsAVerb;

   public When_editing_a_shared_conditions_definition()
   {
      _isNotAVerb = _sharedConditions.Define(TokenConditions.Level, "IsNotAVerb", ConditionGroup.Not(_isAVerb));
      _definitionOfIsAVerb = new EditedDefinition(_isAVerb, _sharedConditions);
   }

   [XF] public void the_tree_is_an_all_of_holding_the_definition_when_the_definition_is_no_all_of() =>
      _definitionOfIsAVerb.Stored.Must().Be(ConditionGroup.AllOf(ConditionGroup.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb)));

   [XF] public void the_tree_is_on_the_shared_conditions_subject() => _definitionOfIsAVerb.Level.Must().Be(TokenConditions.Level);

   [XF] public void the_trees_root_line_is_a_comment_heading_saying_what_meets_the_shared_condition() =>
      ConditionCode.AsText([.. _definitionOfIsAVerb.LinesOf(_definitionOfIsAVerb.Stored).Take(1)]).Must().Be("// A token meets Token.IsAVerb when each of these holds:");

   [XF] public void neither_the_shared_condition_itself_nor_one_defined_through_it_is_offered_in_its_definition() =>
      FullNamesOf(_definitionOfIsAVerb.SharedConditionsOfferedOn(TokenConditions.Level)).Must().HaveCount(0);

   [XF] public void the_shared_conditions_on_its_subject_are_offered_in_the_definition_of_one_they_are_not_defined_through() =>
      FullNamesOf(new EditedDefinition(_followsAVerb, _sharedConditions).SharedConditionsOfferedOn(TokenConditions.Level)).Must().SequenceEqual(["Token.IsAVerb", "Token.IsNotAVerb"]);

   [XF] public void redefining_it_as_its_own_negation_throws_since_it_would_use_itself() =>
      Invoking(() => _sharedConditions.Redefine(_isAVerb, ConditionGroup.Not(_isAVerb))).Must().Throw<ArgumentAssertionFailedException>();

   [XF] public void redefining_it_as_a_shared_condition_defined_through_it_throws_since_it_would_use_itself() =>
      Invoking(() => _sharedConditions.Redefine(_isAVerb, _isNotAVerb)).Must().Throw<ArgumentAssertionFailedException>();

   public class after_storing_a_tree_of_one_condition : When_editing_a_shared_conditions_definition
   {
      public after_storing_a_tree_of_one_condition() => _definitionOfIsAVerb.Store(ConditionGroup.AllOf(TokenConditions.IsAGodanVerb));

      [XF] public void the_definition_is_that_condition() => _isAVerb.Definition.ToString().Must().Be("IsAGodanVerb");

      [XF] public void the_tree_is_an_all_of_holding_it() => _definitionOfIsAVerb.Stored.Must().Be(ConditionGroup.AllOf(TokenConditions.IsAGodanVerb));

      public class and_reloading_the_collection : after_storing_a_tree_of_one_condition
      {
         public and_reloading_the_collection() => GetService<JPCollection>().ReloadFromBackend();

         [XF] public void the_definition_is_read_back_from_its_file() =>
            _sharedConditions.All.Select(it => $"{it.FullName} = {it.Definition}").Must().Contain("Token.IsAVerb = IsAGodanVerb");
      }
   }

   public class after_storing_a_tree_of_one_all_of_group : When_editing_a_shared_conditions_definition
   {
      public after_storing_a_tree_of_one_all_of_group() => _definitionOfIsAVerb.Store(ConditionGroup.AllOf(ConditionGroup.AllOf(TokenConditions.IsAGodanVerb)));

      [XF] public void the_tree_is_as_stored_with_the_group_in_it() => _definitionOfIsAVerb.Stored.ToString().Must().Be("AllOf(AllOf(IsAGodanVerb))");
   }

   public class after_storing_a_tree_of_two_conditions : When_editing_a_shared_conditions_definition
   {
      public after_storing_a_tree_of_two_conditions() => _definitionOfIsAVerb.Store(ConditionGroup.AllOf(TokenConditions.IsAGodanVerb, TokenConditions.Exists));

      [XF] public void the_definition_is_the_tree() => _isAVerb.Definition.ToString().Must().Be("AllOf(IsAGodanVerb, Exists)");
   }

   public class with_a_vocab_using_FollowsAVerb_and_one_using_no_shared_condition : When_editing_a_shared_conditions_definition
   {
      public with_a_vocab_using_FollowsAVerb_and_one_using_no_shared_condition()
      {
         CreateVocab("ます", "polite ending", "ます").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(_followsAVerb));
         CreateVocab("です", "polite copula", "です").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));
      }

      [XF] public void the_vocab_whose_match_condition_depends_on_IsAVerbs_definition_is_the_one_using_FollowsAVerb() =>
         GetService<JPCollection>().Vocab.WhoseMatchConditionsDependOnTheDefinitionOf(_isAVerb).Select(it => it.GetQuestion()).Must().SequenceEqual(["ます"]);

      [XF] public void no_vocabs_match_condition_depends_on_IsNotAVerbs_definition() =>
         GetService<JPCollection>().Vocab.WhoseMatchConditionsDependOnTheDefinitionOf(_isNotAVerb).Select(it => it.GetQuestion()).Must().HaveCount(0);
   }
}
