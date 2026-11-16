using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>The copies <see cref="VocabCloner"/> makes of a vocab take its match-condition, matching flags and register with them.</summary>
public class When_a_vocab_with_matching_configuration_and_a_register_is_copied : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly ITaggregateReference<VocabNote> _vocabReference;
   VocabNote _vocab => _vocabReference.Current;

   public When_a_vocab_with_matching_configuration_and_a_register_is_copied()
   {
      _vocabReference = CreateVocab("あれる", "get-_/is-_", "あれる").Reference();
      _vocab.Edit(it =>
      {
         it.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(MatchingRules.Irrealis.WhenRequired, MatchingRules.GodanPotential.WhenForbidden));
         it.MatchingConfiguration.Flags.IsInflectingWord.SetTo(true);
         it.Register.Humble.SetTo(true);
      });
   }

   public class by_cloning_it_with_a_disambiguation : When_a_vocab_with_matching_configuration_and_a_register_is_copied
   {
      readonly ITaggregateReference<VocabNote> _cloneReference;
      VocabNote _clone => _cloneReference.Current;
      public by_cloning_it_with_a_disambiguation() => _cloneReference = _vocab.Cloner.CloneWithDisambiguation("clone").Vocab.Reference();

      [XF] public void the_clone_has_the_same_match_condition() => _clone.MatchingConfiguration.MatchCondition.Must().Be(_vocab.MatchingConfiguration.MatchCondition);
      [XF] public void the_clone_has_the_matching_flag_set() => _clone.MatchingConfiguration.Flags.IsInflectingWord.IsSet().Must().BeTrue();
      [XF] public void the_clone_is_in_the_register() => _clone.Register.Humble.IsSet().Must().BeTrue();
   }

   public class by_creating_a_suffix_version_of_it : When_a_vocab_with_matching_configuration_and_a_register_is_copied
   {
      readonly ITaggregateReference<VocabNote> _suffixVersionReference;
      VocabNote _suffixVersion => _suffixVersionReference.Current;
      public by_creating_a_suffix_version_of_it() => _suffixVersionReference = _vocab.Cloner.CreateSuffixVersion("ば").Vocab.Reference();

      [XF] public void the_suffix_version_has_the_same_match_condition() => _suffixVersion.MatchingConfiguration.MatchCondition.Must().Be(_vocab.MatchingConfiguration.MatchCondition);
      [XF] public void the_suffix_version_has_the_matching_flag_set() => _suffixVersion.MatchingConfiguration.Flags.IsInflectingWord.IsSet().Must().BeTrue();
      [XF] public void the_suffix_version_is_in_the_register() => _suffixVersion.Register.Humble.IsSet().Must().BeTrue();
   }
}
