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

/// <summary>The vocab 要する has the parts of speech godan verb, special-class, suru verb and transitive.</summary>
public class When_setting_the_parts_of_speech_of_a_vocab : McpServerSpecification
{
   protected readonly VocabNote Vocab;

   public When_setting_the_parts_of_speech_of_a_vocab()
   {
      Vocab = CreateVocab("要する", "to require", "ようする");
      Vocab.PartsOfSpeech.Set([PartOfSpeech.GodanVerb, PartOfSpeech.SpecialClass, PartOfSpeech.SuruVerb, PartOfSpeech.Transitive]);
   }

   protected CallToolResult SetThePartsOfSpeech(params string[] partsOfSpeech) =>
      CallTool("set_the_parts_of_speech_of_a_vocab", new Dictionary<string, object?> { ["vocabNoteId"] = Vocab.GetId().ToString(), ["partsOfSpeech"] = partsOfSpeech });

   protected IEnumerable<string> PartsOfSpeechStored() =>
      GetService<INoteRepository>().LoadAll().Vocab.Single(it => it.GetId() == Vocab.GetId()).PartsOfSpeech.All().Select(it => it.Name);

   public class to_suru_verb_transitive_and_special_class : When_setting_the_parts_of_speech_of_a_vocab
   {
      readonly PartsOfSpeechChange _change;

      public to_suru_verb_transitive_and_special_class() => _change = ResultOf<PartsOfSpeechChange>(SetThePartsOfSpeech("suru verb", "transitive", "special-class"));

      [XF] public void the_change_names_the_vocab() => _change.Vocab.Must().Be("要する");
      [XF] public void the_change_has_the_parts_of_speech_before() => _change.Before.Must().SequenceEqual(["godan verb", "special-class", "suru verb", "transitive"]);
      [XF] public void the_change_has_the_parts_of_speech_after_in_alphabetical_order() => _change.After.Must().SequenceEqual(["special-class", "suru verb", "transitive"]);
      [XF] public void the_vocab_has_them() => Vocab.PartsOfSpeech.All().Select(it => it.Name).Must().SequenceEqual(["special-class", "suru verb", "transitive"]);
      [XF] public void the_vocab_is_saved_with_them() => PartsOfSpeechStored().Must().SequenceEqual(["special-class", "suru verb", "transitive"]);
   }

   public class to_a_list_with_a_part_of_speech_JAStudio_does_not_know : When_setting_the_parts_of_speech_of_a_vocab
   {
      readonly CallToolResult _result;

      public to_a_list_with_a_part_of_speech_JAStudio_does_not_know() => _result = SetThePartsOfSpeech("suru verb", "suru verb - special class");

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_names_the_unknown_part_of_speech() => TextOf(_result).Must().Contain("There is no part of speech named 'suru verb - special class'");
      [XF] public void the_error_lists_the_parts_of_speech_JAStudio_knows() => TextOf(_result).Must().Contain("\"godan verb\"");
      [XF] public void the_vocab_keeps_its_parts_of_speech() => Vocab.PartsOfSpeech.All().Select(it => it.Name).Must().SequenceEqual(["godan verb", "special-class", "suru verb", "transitive"]);
   }
}
