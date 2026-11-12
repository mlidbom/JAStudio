using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A vocab with matching rules in its match-condition, a require/forbid rule outside it, a matching flag and a register, written as the corpus's<br/>
/// JSON and read back, as JAStudio saves and loads the corpus. They're stored as the vocab's own data, not as tags.
/// </summary>
public class When_a_vocab_with_matching_configuration_and_a_register_is_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;
   readonly JsonElement _json;
   readonly VocabNote _readBack;

   public When_a_vocab_with_matching_configuration_and_a_register_is_saved_to_the_corpus()
   {
      _vocab = CreateVocab("あれる", "get-_/is-_", "あれる");
      _vocab.MatchingConfiguration.Rules.Choose(MatchingRules.Irrealis, RequireForbidChoice.Required);
      _vocab.MatchingConfiguration.Rules.Choose(MatchingRules.GodanPotential, RequireForbidChoice.Forbidden);
      _vocab.MatchingConfiguration.RequiresForbids.DictionaryFormStart.Choose(RequireForbidChoice.Forbidden);
      _vocab.MatchingConfiguration.Flags.IsInflectingWord.SetTo(true);
      _vocab.Register.Humble.SetTo(true);
      var serializer = GetService<NoteSerializer>();
      var json = serializer.Serialize(_vocab);
      _json = JsonDocument.Parse(json).RootElement;
      _readBack = serializer.DeserializeVocab(json);
   }

   JsonElement Matching => _json.GetProperty("matching");

   [XF] public void the_corpus_json_stores_the_match_condition_as_an_all_of_the_rules_conditions() =>
      JsonNode.DeepEquals(JsonNode.Parse(Matching.GetProperty("condition").GetRawText()),
                          JsonNode.Parse("""
                                         {"allOf": [
                                            {"precedingToken": {"fact": "irrealis"}},
                                            {"not": {"firstToken": {"anyOf": [{"fact": "godanPotentialStem"}, {"fact": "godanPotentialInflection"}]}}}
                                         ]}
                                         """))
              .Must().BeTrue();

   [XF] public void the_corpus_json_stores_only_the_require_forbid_rules_outside_the_match_condition_that_are_set() =>
      Matching.GetProperty("requiredOrForbidden").EnumerateObject().Select(it => $"{it.Name}: {it.Value}").Must().SequenceEqual(["dictionaryFormStart: forbidden"]);

   [XF] public void the_corpus_json_stores_the_names_of_the_matching_flags_that_are_set() => Matching.GetProperty("flags").EnumerateArray().Select(it => it.ToString()).Must().SequenceEqual(["isInflectingWord"]);
   [XF] public void the_corpus_json_stores_no_rules_in_the_format_from_before_match_conditions() => Matching.Must().Satisfy(it => !it.TryGetProperty("rules", out _));
   [XF] public void the_corpus_json_stores_the_names_of_the_registers() => _json.GetProperty("register").EnumerateArray().Select(it => it.ToString()).Must().SequenceEqual(["humble"]);
   [XF] public void the_corpus_json_stores_no_tags() => _json.GetProperty("tags").EnumerateArray().Must().BeEmpty();

   [XF] public void it_is_read_back_with_the_same_match_condition() => _readBack.MatchingConfiguration.MatchCondition.Must().Be(_vocab.MatchingConfiguration.MatchCondition);
   [XF] public void it_is_read_back_requiring_the_required_rule() => _readBack.MatchingConfiguration.Rules.ChoiceOf(MatchingRules.Irrealis).Must().Be(RequireForbidChoice.Required);
   [XF] public void it_is_read_back_forbidding_the_forbidden_rule() => _readBack.MatchingConfiguration.Rules.ChoiceOf(MatchingRules.GodanPotential).Must().Be(RequireForbidChoice.Forbidden);
   [XF] public void it_is_read_back_with_the_other_rules_unset() => _readBack.MatchingConfiguration.Rules.ChoiceOf(MatchingRules.Godan).Must().Be(RequireForbidChoice.Unset);
   [XF] public void it_is_read_back_forbidding_dictionary_form_start() => _readBack.MatchingConfiguration.RequiresForbids.DictionaryFormStart.Choice.Must().Be(RequireForbidChoice.Forbidden);
   [XF] public void it_is_read_back_with_the_matching_flag_set() => _readBack.MatchingConfiguration.Flags.IsInflectingWord.IsSet().Must().BeTrue();
   [XF] public void it_is_read_back_with_the_other_matching_flags_unset() => _readBack.MatchingConfiguration.Flags.IsPoisonWord.IsSet().Must().BeFalse();
   [XF] public void it_is_read_back_in_the_register() => _readBack.Register.Humble.IsSet().Must().BeTrue();
   [XF] public void it_is_read_back_in_no_other_register() => _readBack.Register.Polite.IsSet().Must().BeFalse();
}
