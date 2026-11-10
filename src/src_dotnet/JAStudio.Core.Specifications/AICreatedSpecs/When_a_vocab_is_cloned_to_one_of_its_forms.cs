using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Cloning a vocab to one of its forms (<see cref="VocabCloner.CloneToForm"/>), such as それなり to 其れなり, keeps whether it's usually written in kana<br/>
/// (<see cref="VocabNoteUsuallyWrittenInKana"/>): the clone is the same word, written another way.
/// </summary>
public class When_a_vocab_is_cloned_to_one_of_its_forms : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;

   public When_a_vocab_is_cloned_to_one_of_its_forms() => _vocab = CreateVocab("それなり", "as-it-is", "それなり");

   public class that_is_decided_as_usually_written_in_kana : When_a_vocab_is_cloned_to_one_of_its_forms
   {
      readonly VocabNote _clone;

      public that_is_decided_as_usually_written_in_kana()
      {
         _vocab.UsuallyWrittenInKana.Decide(UsuallyWrittenInKanaDecision.Yes);
         _clone = _vocab.Cloner.CloneToForm("其れなり").Vocab;
      }

      [XF] public void the_clone_is_decided_as_usually_written_in_kana() => _clone.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.Yes);
   }

   public class that_is_decided_as_not_usually_written_in_kana : When_a_vocab_is_cloned_to_one_of_its_forms
   {
      readonly VocabNote _clone;

      public that_is_decided_as_not_usually_written_in_kana()
      {
         _vocab.UsuallyWrittenInKana.Decide(UsuallyWrittenInKanaDecision.No);
         _clone = _vocab.Cloner.CloneToForm("其れなり").Vocab;
      }

      [XF] public void the_clone_is_decided_as_not_usually_written_in_kana() => _clone.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.No);
   }
}
