using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>The copies <see cref="VocabCloner"/> makes of a vocab take its match-condition, matching flags and register with them.</summary>
public class When_a_vocab_with_matching_configuration_and_a_register_is_copied : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;

   public When_a_vocab_with_matching_configuration_and_a_register_is_copied()
   {
      _vocab = CreateVocab("あれる", "get-_/is-_", "あれる");
      _vocab.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(MatchingRules.Irrealis.WhenRequired, MatchingRules.GodanPotential.WhenForbidden));
      _vocab.MatchingConfiguration.Flags.IsInflectingWord.SetTo(true);
      _vocab.Register.Humble.SetTo(true);
   }

   public class by_cloning_it_with_a_disambiguation : When_a_vocab_with_matching_configuration_and_a_register_is_copied
   {
      readonly VocabNote _clone;
      public by_cloning_it_with_a_disambiguation() => _clone = _vocab.Cloner.CloneWithDisambiguation("clone").Vocab;

      [XF] public void the_clone_has_the_same_match_condition() => _clone.MatchingConfiguration.MatchCondition.Must().Be(_vocab.MatchingConfiguration.MatchCondition);
      [XF] public void the_clone_has_the_matching_flag_set() => _clone.MatchingConfiguration.Flags.IsInflectingWord.IsSet().Must().BeTrue();
      [XF] public void the_clone_is_in_the_register() => _clone.Register.Humble.IsSet().Must().BeTrue();
   }

   public class by_creating_a_suffix_version_of_it : When_a_vocab_with_matching_configuration_and_a_register_is_copied
   {
      readonly VocabNote _suffixVersion;
      public by_creating_a_suffix_version_of_it() => _suffixVersion = _vocab.Cloner.CreateSuffixVersion("ば").Vocab;

      [XF] public void the_suffix_version_has_the_same_match_condition() => _suffixVersion.MatchingConfiguration.MatchCondition.Must().Be(_vocab.MatchingConfiguration.MatchCondition);
      [XF] public void the_suffix_version_has_the_matching_flag_set() => _suffixVersion.MatchingConfiguration.Flags.IsInflectingWord.IsSet().Must().BeTrue();
      [XF] public void the_suffix_version_is_in_the_register() => _suffixVersion.Register.Humble.IsSet().Must().BeTrue();
   }
}
