using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Mcp.Tools.Matching;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Matching;

/// <summary>
/// Three vocab, ない, せる and させる, require an irrealis form and no ichidan verb, among other conditions or not, and the shared-condition<br/>
/// <c>Word.FollowsAGodanIrrealisForm</c> is defined as those two, through the MCP server, before the conditions are looked for.
/// </summary>
public class When_finding_the_conditions_vocab_share : McpServerSpecification
{
   readonly FoundSharedConditionCandidates _found;

   public When_finding_the_conditions_vocab_share()
   {
      CreateVocab("ない", "not", "ない").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(MatchingRules.Irrealis.WhenRequired, MatchingRules.SingleToken.WhenRequired, MatchingRules.Ichidan.WhenForbidden));
      CreateVocab("せる", "make", "せる").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(MatchingRules.Irrealis.WhenRequired, MatchingRules.Ichidan.WhenForbidden));
      CreateVocab("させる", "make", "させる").MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(MatchingRules.Ichidan.WhenForbidden, MatchingRules.Irrealis.WhenRequired));
      CallTool("define_a_shared_condition",
               new Dictionary<string, object?> { ["subject"] = "Word", ["name"] = "FollowsAGodanIrrealisForm", ["definition"] = """{"allOf": [{"precedingToken": {"fact": "irrealis"}}, {"not": {"precedingToken": {"fact": "ichidanVerb"}}}]}""" });

      _found = ResultOf<FoundSharedConditionCandidates>(CallTool("find_the_conditions_vocab_share", new Dictionary<string, object?> { ["heldByAtLeast"] = 3 }));
   }

   [XF] public void one_condition_is_found() => _found.Found.Must().Be(1);

   [XF] public void it_is_on_a_word() => _found.Listed[0].Subject.Must().Be("Word");

   [XF] public void it_is_held_by_the_three_vocab() => _found.Listed[0].Vocab.Must().DeepEqual(new List<string> { "させる", "せる", "ない" });

   [XF] public void it_names_the_shared_condition_whose_definition_means_the_same() => _found.Listed[0].Must().Satisfy(it => it.SameAsTheSharedCondition == "Word.FollowsAGodanIrrealisForm");
}
