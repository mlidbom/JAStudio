using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Mcp.Tools.Matching;
using ModelContextProtocol.Protocol;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Matching;

/// <summary>
/// Defines the shared-conditions <c>Token.IsAVerb</c>, <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, and <c>Word.FollowsAVerb</c>, with that written out,<br/>
/// <c>PrecedingToken.AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, through the MCP server. There are two vocab ます, told apart by the disambiguation<br/>
/// <c>polite</c> on one: its match-condition is <c>PrecedingToken.AnyOf(IsAnIchidanVerb, IsAGodanVerb)</c>, and the other's is that in the other order.<br/>
/// を's is <c>PrecedingToken.IsAMasuStem</c>.
/// </summary>
public class When_using_a_shared_condition_wherever_its_definition_is : McpServerSpecification
{
   public When_using_a_shared_condition_wherever_its_definition_is()
   {
      Define("Token", "IsAVerb", """{"anyOf": [{"fact": "godanVerb"}, {"fact": "ichidanVerb"}]}""");
      Define("Word", "FollowsAVerb", """{"precedingToken": {"anyOf": [{"fact": "godanVerb"}, {"fact": "ichidanVerb"}]}}""");
      var politeます = CreateVocab("ます", "polite ending", "ます");
      politeます.Question.SetDisambiguation("polite");
      politeます.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(PrecedingToken.AnyOf(TokenConditions.IsAnIchidanVerb, TokenConditions.IsAGodanVerb)));
      CreateVocab("ます", "polite ending, again", "ます").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(PrecedingToken.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb)));
      CreateVocab("を", "object marker", "を").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(PrecedingToken.IsAMasuStem));
   }

   void Define(string subject, string name, string definition) =>
      CallTool("define_a_shared_condition", new Dictionary<string, object?> { ["subject"] = subject, ["name"] = name, ["definition"] = definition });

   protected CallToolResult Use(string subject, string name) =>
      CallTool("use_a_shared_condition_wherever_its_definition_is", new Dictionary<string, object?> { ["subject"] = subject, ["name"] = name });

   public class using_IsAVerb : When_using_a_shared_condition_wherever_its_definition_is
   {
      readonly SharedConditionPutToUse _putToUse;

      public using_IsAVerb() => _putToUse = ResultOf<SharedConditionPutToUse>(Use("Token", "IsAVerb"));

      [XF] public void both_vocab_ます_whose_match_conditions_hold_its_definition_in_either_order_are_reported_made_to_use_it_by_their_questions_with_their_disambiguations() =>
         _putToUse.VocabMadeToUseIt.Must().DeepEqual(new List<string> { "ます", "ます:polite" });

      [XF] public void the_shared_condition_whose_definition_holds_it_is_reported_made_to_use_it() =>
         _putToUse.SharedConditionsMadeToUseIt.Must().DeepEqual(new List<string> { "Word.FollowsAVerb" });

      [XF] public void it_is_reported_as_listed_with_what_uses_it() =>
         _putToUse.SharedCondition.Must().DeepEqual(new ListedSharedCondition("Token.IsAVerb",
                                                                               $"Token.IsAVerb ={System.Environment.NewLine}   AnyOf({System.Environment.NewLine}      IsAGodanVerb,{System.Environment.NewLine}      IsAnIchidanVerb)",
                                                                               new List<string> { "ます", "ます:polite" },
                                                                               new List<string> { "Word.FollowsAVerb" }));

      public class then_using_FollowsAVerb : using_IsAVerb
      {
         readonly SharedConditionPutToUse _followsAVerbPutToUse;

         public then_using_FollowsAVerb() => _followsAVerbPutToUse = ResultOf<SharedConditionPutToUse>(Use("Word", "FollowsAVerb"));

         [XF] public void both_vocab_ます_whose_match_conditions_now_use_IsAVerb_under_the_preceding_token_are_made_to_use_it() =>
            _followsAVerbPutToUse.VocabMadeToUseIt.Must().DeepEqual(new List<string> { "ます", "ます:polite" });
      }
   }

   public class using_one_there_is_none_of : When_using_a_shared_condition_wherever_its_definition_is
   {
      readonly CallToolResult _result;

      public using_one_there_is_none_of() => _result = Use("Word", "IsLong");

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);

      [XF] public void the_error_names_the_shared_conditions_there_are() => TextOf(_result).Must().Contain("Word.FollowsAVerb, Token.IsAVerb");
   }
}
