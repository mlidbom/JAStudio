using System.Linq;
using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A vocab with require/forbid rules, a matching flag and a register, written as the corpus's JSON and read back, as JAStudio saves and loads<br/>
/// the corpus. They're stored as the vocab's own data, not as tags.
/// </summary>
public class When_a_vocab_with_matching_configuration_and_a_register_is_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly JsonElement _json;
   readonly VocabNote _readBack;

   public When_a_vocab_with_matching_configuration_and_a_register_is_saved_to_the_corpus()
   {
      var vocab = CreateVocab("あれる", "get-_/is-_", "あれる");
      vocab.MatchingConfiguration.RequiresForbids.Irrealis.Choose(RequireForbidChoice.Required);
      vocab.MatchingConfiguration.RequiresForbids.GodanPotential.Choose(RequireForbidChoice.Forbidden);
      vocab.MatchingConfiguration.Flags.IsInflectingWord.SetTo(true);
      vocab.Register.Humble.SetTo(true);
      var serializer = GetService<NoteSerializer>();
      var json = serializer.Serialize(vocab);
      _json = JsonDocument.Parse(json).RootElement;
      _readBack = serializer.DeserializeVocab(json);
   }

   JsonElement StoredRules => _json.GetProperty("matching").GetProperty("requiredOrForbidden");

   [XF] public void the_corpus_json_stores_a_required_rule_as_required() => StoredRules.GetProperty("irrealis").ToString().Must().Be("required");
   [XF] public void the_corpus_json_stores_a_forbidden_rule_as_forbidden() => StoredRules.GetProperty("godanPotential").ToString().Must().Be("forbidden");
   [XF] public void the_corpus_json_stores_no_rule_that_is_unset() => StoredRules.EnumerateObject().Select(it => it.Name).Must().SequenceEqual(["godanPotential", "irrealis"]);
   [XF] public void the_corpus_json_stores_the_names_of_the_matching_flags_that_are_set() => _json.GetProperty("matching").GetProperty("flags").EnumerateArray().Select(it => it.ToString()).Must().SequenceEqual(["isInflectingWord"]);
   [XF] public void the_corpus_json_stores_the_names_of_the_registers() => _json.GetProperty("register").EnumerateArray().Select(it => it.ToString()).Must().SequenceEqual(["humble"]);
   [XF] public void the_corpus_json_stores_no_tags() => _json.GetProperty("tags").EnumerateArray().Must().BeEmpty();

   [XF] public void it_is_read_back_requiring_the_required_rule() => _readBack.MatchingConfiguration.RequiresForbids.Irrealis.Choice.Must().Be(RequireForbidChoice.Required);
   [XF] public void it_is_read_back_forbidding_the_forbidden_rule() => _readBack.MatchingConfiguration.RequiresForbids.GodanPotential.Choice.Must().Be(RequireForbidChoice.Forbidden);
   [XF] public void it_is_read_back_with_the_other_rules_unset() => _readBack.MatchingConfiguration.RequiresForbids.Godan.Choice.Must().Be(RequireForbidChoice.Unset);
   [XF] public void it_is_read_back_with_the_matching_flag_set() => _readBack.MatchingConfiguration.Flags.IsInflectingWord.IsSet().Must().BeTrue();
   [XF] public void it_is_read_back_with_the_other_matching_flags_unset() => _readBack.MatchingConfiguration.Flags.IsPoisonWord.IsSet().Must().BeFalse();
   [XF] public void it_is_read_back_in_the_register() => _readBack.Register.Humble.IsSet().Must().BeTrue();
   [XF] public void it_is_read_back_in_no_other_register() => _readBack.Register.Polite.IsSet().Must().BeFalse();
}
