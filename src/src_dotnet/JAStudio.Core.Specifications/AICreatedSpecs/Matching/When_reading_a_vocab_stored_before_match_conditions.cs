using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Storage;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// A vocab file in the format from before match-conditions, with require/forbid rules and configurable rules, read as the migration of the corpus<br/>
/// reads every vocab: its rules become its match-condition, and saving it writes the new format.
/// </summary>
/// <remarks>Goes once the corpus is migrated, with the reading of the old format (increment 3 in dev_docs/matching-configuration.md).</remarks>
public class When_reading_a_vocab_stored_before_match_conditions : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _readBack;
   readonly JsonElement _writtenAgain;

   public When_reading_a_vocab_stored_before_match_conditions()
   {
      var serializer = GetService<NoteSerializer>();
      var vocabFile = JsonNode.Parse(serializer.Serialize(CreateVocab("あれる", "get-_/is-_", "あれる"))) as JsonObject ?? throw new JsonException("A vocab file is an object.");
      vocabFile["matching"] = JsonNode.Parse("""
                                             {"requiredOrForbidden": {"masuStem": "required", "godan": "forbidden", "dictionaryFormStart": "forbidden"},
                                              "flags": [],
                                              "rules": {"prefixIsNot": ["ん"], "suffixIsNot": [], "surfaceIsNot": [], "yieldToSurface": ["行く"], "requiredPrefix": []}}
                                             """);
      _readBack = serializer.DeserializeVocab(vocabFile.ToJsonString());
      _writtenAgain = JsonDocument.Parse(serializer.Serialize(_readBack)).RootElement.GetProperty("matching");
   }

   VocabNoteMatchingConfiguration Configuration => _readBack.MatchingConfiguration;

   [XF] public void the_rules_in_the_match_condition_become_all_of_their_conditions_in_the_order_matching_tested_them() =>
      Configuration.MatchCondition.Must().Be(ConditionGroup.AllOf(ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)),
                                                                  ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん")))),
                                                                  PrecedingToken.Meets(TokenConditions.IsAMasuStem)));

   [XF] public void dictionary_form_start_stays_a_require_forbid_rule() => Configuration.RequiresForbids.DictionaryFormStart.Choice.Must().Be(RequireForbidChoice.Forbidden);
   [XF] public void yield_to_surface_keeps_its_surfaces() => Configuration.YieldToSurface.Surfaces.Must().SequenceEqual(["行く"]);
   [XF] public void it_weighs_what_its_rules_did() => Configuration.CustomRequirementsWeight.Must().Be(50 + 1 + 2 + 1 + 2);

   [XF] public void written_again_it_has_no_rules_in_the_old_format() => _writtenAgain.TryGetProperty("rules", out _).Must().BeFalse();
   [XF] public void written_again_it_has_its_match_condition() => _writtenAgain.TryGetProperty("condition", out _).Must().BeTrue();
   [XF] public void written_again_it_keeps_only_dictionary_form_start_among_the_require_forbid_rules() =>
      _writtenAgain.GetProperty("requiredOrForbidden").EnumerateObject().Select(it => it.Name).Must().SequenceEqual(["dictionaryFormStart"]);
   [XF] public void written_again_its_yield_to_surface_is_in_the_matching_configuration_itself() =>
      _writtenAgain.GetProperty("yieldToSurface").EnumerateArray().Select(it => it.ToString()).Must().SequenceEqual(["行く"]);
}
