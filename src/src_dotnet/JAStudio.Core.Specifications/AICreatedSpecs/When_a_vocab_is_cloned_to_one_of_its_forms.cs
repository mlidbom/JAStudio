using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Cloning a vocab to one of its forms (<see cref="VocabCloner.CloneToForm"/>), such as それなり to 其れなり, keeps whether it's usually written in kana<br/>
/// (<see cref="VocabNoteUsuallyWrittenInKana"/>): the clone is the same word, written another way.
/// </summary>
public class When_a_vocab_is_cloned_to_one_of_its_forms : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly ITaggregateReference<VocabNote> _vocabReference;
   VocabNote _vocab => _vocabReference.Current;

   public When_a_vocab_is_cloned_to_one_of_its_forms() => _vocabReference = CreateVocab("それなり", "as-it-is", "それなり").Reference();

   public class that_is_decided_as_usually_written_in_kana : When_a_vocab_is_cloned_to_one_of_its_forms
   {
      readonly ITaggregateReference<VocabNote> _cloneReference;
      VocabNote _clone => _cloneReference.Current;

      public that_is_decided_as_usually_written_in_kana()
      {
         _vocab.Edit(it => it.UsuallyWrittenInKana.Decide(UsuallyWrittenInKanaDecision.Yes));
         _cloneReference = _vocab.Cloner.CloneToForm("其れなり").Vocab.Reference();
      }

      [XF] public void the_clone_is_decided_as_usually_written_in_kana() => _clone.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.Yes);
   }

   public class that_is_decided_as_not_usually_written_in_kana : When_a_vocab_is_cloned_to_one_of_its_forms
   {
      readonly ITaggregateReference<VocabNote> _cloneReference;
      VocabNote _clone => _cloneReference.Current;

      public that_is_decided_as_not_usually_written_in_kana()
      {
         _vocab.Edit(it => it.UsuallyWrittenInKana.Decide(UsuallyWrittenInKanaDecision.No));
         _cloneReference = _vocab.Cloner.CloneToForm("其れなり").Vocab.Reference();
      }

      [XF] public void the_clone_is_decided_as_not_usually_written_in_kana() => _clone.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.No);
   }
}
