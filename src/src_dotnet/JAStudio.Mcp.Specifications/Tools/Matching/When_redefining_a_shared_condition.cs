using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Mcp.Tools.Matching;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Matching;

/// <summary>
/// Defines the shared-conditions <c>Token.IsAVerb</c>, <c>IsAGodanVerb</c>, and <c>Word.FollowsAVerb</c>, <c>PrecedingToken.IsAVerb</c>, through the MCP<br/>
/// server. The vocab ます uses <c>FollowsAVerb</c>, and を no shared-condition.
/// </summary>
public class When_redefining_a_shared_condition : McpServerSpecification
{
   public When_redefining_a_shared_condition()
   {
      Define("Token", "IsAVerb", """{"fact": "godanVerb"}""");
      Define("Word", "FollowsAVerb", """{"precedingToken": {"sharedCondition": "IsAVerb"}}""");
      CreateVocab("ます", "polite ending", "ます").MatchingConfiguration
                                                .SetMatchCondition(MatchConditionJson.VocabsMatchConditionFromJsonText("""{"sharedCondition": "FollowsAVerb"}""", GetService<SharedConditions>()));
      CreateVocab("を", "object marker", "を");
   }

   void Define(string subject, string name, string definition) =>
      CallTool("define_a_shared_condition", new Dictionary<string, object?> { ["subject"] = subject, ["name"] = name, ["definition"] = definition });

   CallToolResult Redefine(string subject, string name, string definition) =>
      CallTool("redefine_a_shared_condition", new Dictionary<string, object?> { ["subject"] = subject, ["name"] = name, ["definition"] = definition });

   public class redefining_IsAVerb_as_either_a_godan_or_an_ichidan_verb : When_redefining_a_shared_condition
   {
      readonly RedefinedSharedCondition _redefined;

      public redefining_IsAVerb_as_either_a_godan_or_an_ichidan_verb() =>
         _redefined = ResultOf<RedefinedSharedCondition>(Redefine("Token", "IsAVerb", """{"anyOf": [{"fact": "godanVerb"}, {"fact": "ichidanVerb"}]}"""));

      [XF] public void it_is_reported_with_its_new_definition() =>
         _redefined.SharedCondition.Code.Must().Be($"Token.IsAVerb ={System.Environment.NewLine}   AnyOf(IsAGodanVerb,{System.Environment.NewLine}         IsAnIchidanVerb)");

      [XF] public void the_sentences_of_ます_were_reparsed_since_its_match_condition_depends_on_it_through_FollowsAVerb() =>
         _redefined.VocabWhoseSentencesWereReparsed.Must().DeepEqual(new List<string> { "ます" });
   }

   public class redefining_IsAVerb_so_that_it_uses_itself : When_redefining_a_shared_condition
   {
      readonly CallToolResult _result;

      public redefining_IsAVerb_so_that_it_uses_itself() => _result = Redefine("Token", "IsAVerb", """{"not": {"sharedCondition": "IsAVerb"}}""");

      [XF] public void fails() => _result.IsError.Must().Be(true);
   }
}
