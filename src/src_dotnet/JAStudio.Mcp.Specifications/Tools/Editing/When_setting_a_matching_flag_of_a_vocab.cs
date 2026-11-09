using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using JAStudio.Mcp.Tools.Editing;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Editing;

/// <summary>The vocab 行った has the matching flag questionOverridesForm set, and no other.</summary>
public class When_setting_a_matching_flag_of_a_vocab : McpServerSpecification
{
   protected readonly VocabNote Vocab;

   public When_setting_a_matching_flag_of_a_vocab()
   {
      Vocab = CreateVocab("行った", "went", "いった");
      Vocab.MatchingConfiguration.Flags.QuestionOverridesForm.SetTo(true);
   }

   protected CallToolResult SetTheFlag(string flag, bool set) =>
      CallTool("set_a_matching_flag_of_a_vocab", new Dictionary<string, object?> { ["vocabNoteId"] = Vocab.GetId().ToString(), ["flag"] = flag, ["set"] = set });

   protected IEnumerable<string> FlagsStored() =>
      GetService<INoteRepository>().LoadAll().Vocab.Single(it => it.GetId() == Vocab.GetId()).MatchingConfiguration.Flags.All.Where(it => it.IsSet()).Select(it => it.Name);

   public class setting_isCompositionallyTransparentCompound : When_setting_a_matching_flag_of_a_vocab
   {
      readonly MatchingFlagsChange _change;

      public setting_isCompositionallyTransparentCompound() => _change = ResultOf<MatchingFlagsChange>(SetTheFlag("isCompositionallyTransparentCompound", set: true));

      [XF] public void the_change_names_the_vocab() => _change.Vocab.Must().Be("行った");
      [XF] public void the_change_has_the_flags_set_before() => _change.Before.Must().SequenceEqual(["questionOverridesForm"]);
      [XF] public void the_change_has_the_flags_set_after() => _change.After.Must().SequenceEqual(["questionOverridesForm", "isCompositionallyTransparentCompound"]);
      [XF] public void the_vocab_is_a_compositionally_transparent_compound() => Vocab.MatchingConfiguration.Flags.IsCompositionallyTransparentCompound.IsSet().Must().BeTrue();
      [XF] public void the_vocab_is_saved_with_both_flags() => FlagsStored().Must().SequenceEqual(["questionOverridesForm", "isCompositionallyTransparentCompound"]);
   }

   public class clearing_questionOverridesForm : When_setting_a_matching_flag_of_a_vocab
   {
      readonly MatchingFlagsChange _change;

      public clearing_questionOverridesForm() => _change = ResultOf<MatchingFlagsChange>(SetTheFlag("questionOverridesForm", set: false));

      [XF] public void the_change_has_no_flags_set_after() => _change.After.Must().BeEmpty();
      [XF] public void the_vocab_has_the_flag_cleared() => Vocab.MatchingConfiguration.Flags.QuestionOverridesForm.IsSet().Must().BeFalse();
      [XF] public void the_vocab_is_saved_with_no_flags_set() => FlagsStored().Must().BeEmpty();
   }

   public class naming_a_flag_a_vocab_does_not_have : When_setting_a_matching_flag_of_a_vocab
   {
      readonly CallToolResult _result;

      public naming_a_flag_a_vocab_does_not_have() => _result = SetTheFlag("isTransparent", set: true);

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_names_the_unknown_flag() => TextOf(_result).Must().Contain("A vocab has no matching flag named 'isTransparent'");
      [XF] public void the_error_lists_the_matching_flags_a_vocab_has() => TextOf(_result).Must().Contain("\"isCompositionallyTransparentCompound\"");
      [XF] public void the_vocab_keeps_its_flags() => FlagsStored().Must().SequenceEqual(["questionOverridesForm"]);
   }
}
