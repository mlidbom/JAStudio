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
/// server. The vocab ます's match-condition is <c>PrecedingToken.IsAVerb</c>.
/// </summary>
public class When_renaming_a_shared_condition : McpServerSpecification
{
   public When_renaming_a_shared_condition()
   {
      Define("Token", "IsAVerb", """{"fact": "godanVerb"}""");
      Define("Word", "FollowsAVerb", """{"precedingToken": {"sharedCondition": "IsAVerb"}}""");
      CreateVocab("ます", "polite ending", "ます").MatchingConfiguration
                                                .SetMatchCondition(MatchConditionJson.VocabsMatchConditionFromJsonText("""{"precedingToken": {"sharedCondition": "IsAVerb"}}""", GetService<SharedConditions>()));
   }

   void Define(string subject, string name, string definition) =>
      CallTool("define_a_shared_condition", new Dictionary<string, object?> { ["subject"] = subject, ["name"] = name, ["definition"] = definition });

   CallToolResult Rename(string newName) =>
      CallTool("rename_a_shared_condition", new Dictionary<string, object?> { ["subject"] = "Token", ["name"] = "IsAVerb", ["newName"] = newName });

   public class renaming_IsAVerb_IsAGodanVerbToo : When_renaming_a_shared_condition
   {
      readonly ListedSharedCondition _renamed;

      public renaming_IsAVerb_IsAGodanVerbToo() => _renamed = ResultOf<ListedSharedCondition>(Rename("IsAGodanVerbToo"));

      [XF] public void it_is_reported_by_its_new_name() => _renamed.FullName.Must().Be("Token.IsAGodanVerbToo");

      [XF] public void it_is_reported_with_the_vocab_using_it() => _renamed.VocabUsingIt.Must().DeepEqual(new List<string> { "ます" });

      [XF] public void it_is_reported_with_the_shared_condition_using_it() => _renamed.SharedConditionsUsingIt.Must().DeepEqual(new List<string> { "Word.FollowsAVerb" });
   }

   public class renaming_IsAVerb_IsAGodanVerb_a_facts_name : When_renaming_a_shared_condition
   {
      readonly CallToolResult _result;

      public renaming_IsAVerb_IsAGodanVerb_a_facts_name() => _result = Rename("IsAGodanVerb");

      [XF] public void fails() => _result.IsError.Must().Be(true);
   }
}
