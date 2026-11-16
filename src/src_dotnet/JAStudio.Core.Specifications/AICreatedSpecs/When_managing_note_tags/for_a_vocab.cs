using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_managing_note_tags;

public class for_a_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_one_tagged_with_a_tag_JAStudio_maintains : for_a_vocab
   {
      readonly ITaggregateReference<VocabNote> _vocabReference;
      VocabNote _vocab => _vocabReference.Current;

      public given_one_tagged_with_a_tag_JAStudio_maintains()
      {
         _vocabReference = CreateVocab("食べる", "to eat", "たべる").Reference();
         _vocab.Edit(it => it.Tags.Set(Tags.Vocab.IsIchidanHidingGodanPotential));
      }

      [XF] public void the_note_contains_the_tag() => _vocab.Tags.Must().Contain(Tags.Vocab.IsIchidanHidingGodanPotential);

      public class after_unsetting_the_tag : given_one_tagged_with_a_tag_JAStudio_maintains
      {
         public after_unsetting_the_tag() => _vocab.Edit(it => it.Tags.Unset(Tags.Vocab.IsIchidanHidingGodanPotential));

         [XF] public void the_note_no_longer_contains_the_tag() =>
            _vocab.Tags.Must().Satisfy(it => !it.Contains(Tags.Vocab.IsIchidanHidingGodanPotential));
      }
   }

   public class given_two_where_only_one_is_tagged : for_a_vocab
   {
      readonly ITaggregateReference<VocabNote> _taggedReference;
      VocabNote _tagged => _taggedReference.Current;
      readonly ITaggregateReference<VocabNote> _untaggedReference;
      VocabNote _untagged => _untaggedReference.Current;

      public given_two_where_only_one_is_tagged()
      {
         _taggedReference = CreateVocab("食べる", "to eat", "たべる").Reference();
         _untaggedReference = CreateVocab("飲む", "to drink", "のむ").Reference();
         _tagged.Edit(it => it.Tags.Set(Tags.Vocab.IsIchidanHidingGodanPotential));
      }

      [XF] public void the_tagged_vocab_contains_the_tag() => _tagged.Tags.Must().Contain(Tags.Vocab.IsIchidanHidingGodanPotential);
      [XF] public void the_untagged_vocab_does_not_contain_the_tag() => _untagged.Tags.Must().Satisfy(it => !it.Contains(Tags.Vocab.IsIchidanHidingGodanPotential));
   }
}
