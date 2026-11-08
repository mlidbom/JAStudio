using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>The copies <see cref="VocabCloner"/> makes of a vocab take its require/forbid rules, matching flags and register with them.</summary>
public class When_a_vocab_with_matching_configuration_and_a_register_is_copied : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;

   public When_a_vocab_with_matching_configuration_and_a_register_is_copied()
   {
      _vocab = CreateVocab("あれる", "get-_/is-_", "あれる");
      _vocab.MatchingConfiguration.RequiresForbids.Irrealis.Choose(RequireForbidChoice.Required);
      _vocab.MatchingConfiguration.RequiresForbids.GodanPotential.Choose(RequireForbidChoice.Forbidden);
      _vocab.MatchingConfiguration.Flags.IsInflectingWord.SetTo(true);
      _vocab.Register.Humble.SetTo(true);
   }

   public class by_cloning_it : When_a_vocab_with_matching_configuration_and_a_register_is_copied
   {
      readonly VocabNote _clone;
      public by_cloning_it() => _clone = _vocab.Cloner.Clone();

      [XF] public void the_clone_requires_the_required_rule() => _clone.MatchingConfiguration.RequiresForbids.Irrealis.Choice.Must().Be(RequireForbidChoice.Required);
      [XF] public void the_clone_forbids_the_forbidden_rule() => _clone.MatchingConfiguration.RequiresForbids.GodanPotential.Choice.Must().Be(RequireForbidChoice.Forbidden);
      [XF] public void the_clone_has_the_matching_flag_set() => _clone.MatchingConfiguration.Flags.IsInflectingWord.IsSet().Must().BeTrue();
      [XF] public void the_clone_is_in_the_register() => _clone.Register.Humble.IsSet().Must().BeTrue();
   }

   public class by_creating_a_suffix_version_of_it : When_a_vocab_with_matching_configuration_and_a_register_is_copied
   {
      readonly VocabNote _suffixVersion;
      public by_creating_a_suffix_version_of_it() => _suffixVersion = _vocab.Cloner.CreateSuffixVersion("ば");

      [XF] public void the_suffix_version_requires_the_required_rule() => _suffixVersion.MatchingConfiguration.RequiresForbids.Irrealis.Choice.Must().Be(RequireForbidChoice.Required);
      [XF] public void the_suffix_version_forbids_the_forbidden_rule() => _suffixVersion.MatchingConfiguration.RequiresForbids.GodanPotential.Choice.Must().Be(RequireForbidChoice.Forbidden);
      [XF] public void the_suffix_version_has_the_matching_flag_set() => _suffixVersion.MatchingConfiguration.Flags.IsInflectingWord.IsSet().Must().BeTrue();
      [XF] public void the_suffix_version_is_in_the_register() => _suffixVersion.Register.Humble.IsSet().Must().BeTrue();
   }
}
