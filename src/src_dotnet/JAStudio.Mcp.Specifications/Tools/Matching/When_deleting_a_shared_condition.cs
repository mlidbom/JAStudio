using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Mcp.Tools.Matching;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Matching;

/// <summary>
/// Defines the shared-condition <c>Token.IsAVerb</c>, <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, through the MCP server. The vocab ます's match-condition is<br/>
/// <c>PrecedingToken.IsAVerb</c>. Then deletes <c>IsAVerb</c>.
/// </summary>
public class When_deleting_a_shared_condition : McpServerSpecification
{
   readonly VocabNote _ます;
   readonly ListedSharedCondition _deleted;

   public When_deleting_a_shared_condition()
   {
      CallTool("define_a_shared_condition",
               new Dictionary<string, object?> { ["subject"] = "Token", ["name"] = "IsAVerb", ["definition"] = """{"anyOf": [{"fact": "godanVerb"}, {"fact": "ichidanVerb"}]}""" });
      _ます = CreateVocab("ます", "polite ending", "ます");
      _ます.MatchingConfiguration.SetMatchCondition(MatchConditionJson.VocabsMatchConditionFromJsonText("""{"precedingToken": {"sharedCondition": "IsAVerb"}}""", GetService<SharedConditions>()));

      _deleted = ResultOf<ListedSharedCondition>(CallTool("delete_a_shared_condition", new Dictionary<string, object?> { ["subject"] = "Token", ["name"] = "IsAVerb" }));
   }

   [XF] public void it_is_reported_with_the_vocab_that_used_it() => _deleted.VocabUsingIt.Must().DeepEqual(new List<string> { "ます" });

   [XF] public void the_vocab_has_its_definition_written_out_in_its_place() =>
      _ます.MatchingConfiguration.MatchCondition.ToString().Must().Be("AllOf(PrecedingToken.AnyOf(IsAGodanVerb, IsAnIchidanVerb))");

   [XF] public void there_is_no_shared_condition_left() => GetService<SharedConditions>().All.Select(it => it.FullName).Must().HaveCount(0);
}
