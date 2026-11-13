using System;
using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using JAStudio.Mcp.Tools.Matching;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Matching;

/// <summary>
/// Defines the shared-condition <c>Token.IsAVerb</c>, <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, then <c>Word.FollowsAVerb</c>, <c>PrecedingToken.IsAVerb</c>, through<br/>
/// the MCP server, and makes the vocab ます's match-condition <c>FollowsAVerb</c>.
/// </summary>
public class When_defining_a_shared_condition : McpServerSpecification
{
   readonly ListedSharedCondition _defined;

   public When_defining_a_shared_condition()
   {
      Define("Token", "IsAVerb", """{"anyOf": [{"fact": "godanVerb"}, {"fact": "ichidanVerb"}]}""");
      _defined = Define("Word", "FollowsAVerb", """{"precedingToken": {"sharedCondition": "IsAVerb"}}""");
      var followsAVerb = (SharedCondition<CandidateWordVariant>)GetService<SharedConditions>().On(WordConditions.Level)[0];
      CreateVocab("ます", "polite ending", "ます").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf<CandidateWordVariant>(followsAVerb));
   }

   ListedSharedCondition Define(string subject, string name, string definition) =>
      ResultOf<ListedSharedCondition>(CallTool("define_a_shared_condition", new Dictionary<string, object?> { ["subject"] = subject, ["name"] = name, ["definition"] = definition }));

   [XF] public void the_definition_is_reported_as_code_under_the_full_name() => _defined.Code.Must().Be($"Word.FollowsAVerb ={Environment.NewLine}   PrecedingToken.IsAVerb");

   public class then_listing_the_shared_conditions : When_defining_a_shared_condition
   {
      readonly IReadOnlyList<ListedSharedCondition> _listed;

      public then_listing_the_shared_conditions() => _listed = ResultOf<List<ListedSharedCondition>>(CallTool("list_the_shared_conditions", new Dictionary<string, object?>()));

      [XF] public void the_one_on_a_word_is_listed_first_with_the_vocab_using_it() =>
         _listed[0].Must().DeepEqual(new ListedSharedCondition("Word.FollowsAVerb", $"Word.FollowsAVerb ={Environment.NewLine}   PrecedingToken.IsAVerb", new List<string> { "ます" }, new List<string>()));

      [XF] public void the_one_on_a_token_is_listed_next_with_the_shared_condition_using_it() =>
         _listed[1].Must().DeepEqual(new ListedSharedCondition("Token.IsAVerb",
                                                                $"Token.IsAVerb ={Environment.NewLine}   AnyOf(IsAGodanVerb,{Environment.NewLine}         IsAnIchidanVerb)",
                                                                new List<string>(),
                                                                new List<string> { "Word.FollowsAVerb" }));
   }

   public class then_defining_one_on_Sentence_which_is_no_subject_a_condition_is_on : When_defining_a_shared_condition
   {
      readonly CallToolResult _result;

      public then_defining_one_on_Sentence_which_is_no_subject_a_condition_is_on() =>
         _result = CallTool("define_a_shared_condition", new Dictionary<string, object?> { ["subject"] = "Sentence", ["name"] = "IsLong", ["definition"] = """{"fact": "singleToken"}""" });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);

      [XF] public void the_error_names_the_subjects_there_are() => TextOf(_result).Must().Contain("Word, Token, Text");
   }
}
