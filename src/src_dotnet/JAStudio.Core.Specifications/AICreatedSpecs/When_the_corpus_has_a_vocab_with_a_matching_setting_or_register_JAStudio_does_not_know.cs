using System;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Storage;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A vocab in the corpus with a require/forbid rule, a matching flag, a fact in its match-condition or a register that JAStudio has no field for,<br/>
/// such as a misspelled one. Reading it must fail: the vocab would lose the setting the next time it's saved.
/// </summary>
public class When_the_corpus_has_a_vocab_with_a_matching_setting_or_register_JAStudio_does_not_know : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly NoteSerializer _serializer;
   readonly string _json;

   public When_the_corpus_has_a_vocab_with_a_matching_setting_or_register_JAStudio_does_not_know()
   {
      var vocab = CreateVocab("あれる", "get-_/is-_", "あれる");
      vocab.MatchingConfiguration.RequiresForbids.DictionaryFormStart.Choose(RequireForbidChoice.Required);
      vocab.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(MatchingRules.Irrealis.WhenRequired));
      vocab.MatchingConfiguration.Flags.IsInflectingWord.SetTo(true);
      vocab.Register.Humble.SetTo(true);
      _serializer = GetService<NoteSerializer>();
      _json = _serializer.Serialize(vocab);
   }

   void ReadWith(string knownName, string unknownName) => _serializer.DeserializeVocab(_json.Replace($"\"{knownName}\"", $"\"{unknownName}\""));

   [XF] public void reading_it_with_an_unknown_require_forbid_rule_fails() => Invoking(() => ReadWith("dictionaryFormStart", "dictionaryFormStrat")).Must().Throw<ArgumentAssertionFailedException>();
   [XF] public void reading_it_with_an_unknown_fact_in_its_match_condition_fails() => Invoking(() => ReadWith("irrealis", "irealis")).Must().Throw<FormatException>();
   [XF] public void reading_it_with_an_unknown_matching_flag_fails() => Invoking(() => ReadWith("isInflectingWord", "isInflecting")).Must().Throw<ArgumentAssertionFailedException>();
   [XF] public void reading_it_with_an_unknown_register_fails() => Invoking(() => ReadWith("humble", "humbled")).Must().Throw<ArgumentAssertionFailedException>();
}
