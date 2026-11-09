using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Tools.Dictionary;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Dictionary;

public class When_looking_up_in_the_dictionary : McpServerSpecification
{
   protected CallToolResult LookUp(Dictionary<string, object?> arguments) => CallTool("look_up_in_the_dictionary", arguments);

   public class the_word_要する : When_looking_up_in_the_dictionary
   {
      readonly DictionaryLookup _lookup;

      public the_word_要する() => _lookup = ResultOf<DictionaryLookup>(LookUp(new Dictionary<string, object?> { ["word"] = "要する" }));

      [XF] public void the_one_entry_found_has_the_kanji_form_要する() => _lookup.Entries.Single().KanjiForms.Must().SequenceEqual(["要する"]);
      [XF] public void the_one_entry_found_has_the_kana_form_ようする() => _lookup.Entries.Single().KanaForms.Must().SequenceEqual(["ようする"]);
      [XF] public void a_sense_of_it_has_the_dictionarys_own_part_of_speech_suru_verb_special_class() =>
         _lookup.Entries.Single().Senses.SelectMany(it => it.PartsOfSpeech).Must().Contain("suru verb - special class");
      [XF] public void the_parts_of_speech_JAStudio_maps_the_entry_to_are_special_class_suru_verb_and_transitive() =>
         _lookup.Entries.Single().PartsOfSpeech.Must().SequenceEqual(["special-class", "suru verb", "transitive"]);
      [XF] public void the_parts_of_speech_of_all_the_entries_together_are_special_class_suru_verb_and_transitive() =>
         _lookup.PartsOfSpeech.Must().SequenceEqual(["special-class", "suru verb", "transitive"]);
   }

   public class the_vocab_要する_by_its_id : When_looking_up_in_the_dictionary
   {
      readonly DictionaryLookup _lookup;

      public the_vocab_要する_by_its_id()
      {
         var vocab = CreateVocab("要する", "to require", "ようする");
         _lookup = ResultOf<DictionaryLookup>(LookUp(new Dictionary<string, object?> { ["vocabNoteId"] = vocab.GetId().ToString() }));
      }

      [XF] public void the_one_entry_found_has_the_kanji_form_要する() => _lookup.Entries.Single().KanjiForms.Must().SequenceEqual(["要する"]);
   }

   public class giving_both_a_word_and_a_vocabs_id : When_looking_up_in_the_dictionary
   {
      readonly CallToolResult _result;

      public giving_both_a_word_and_a_vocabs_id()
      {
         var vocab = CreateVocab("要する", "to require", "ようする");
         _result = LookUp(new Dictionary<string, object?> { ["word"] = "要する", ["vocabNoteId"] = vocab.GetId().ToString() });
      }

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_says_to_give_either_a_word_or_a_vocabs_id() => TextOf(_result).Must().Contain("Give either word or vocabNoteId");
   }
}
