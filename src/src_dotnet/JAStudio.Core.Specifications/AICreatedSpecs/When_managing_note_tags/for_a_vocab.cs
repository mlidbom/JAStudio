using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_managing_note_tags;

public class for_a_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_one_tagged_with_a_tag_JAStudio_maintains : for_a_vocab
   {
      readonly VocabNote _vocab;

      public given_one_tagged_with_a_tag_JAStudio_maintains()
      {
         _vocab = CreateVocab("食べる", "to eat", "たべる");
         _vocab.Tags.Set(Tags.Vocab.IsIchidanHidingGodanPotential);
      }

      [XF] public void the_note_contains_the_tag() => _vocab.Tags.Contains(Tags.Vocab.IsIchidanHidingGodanPotential).Must().BeTrue();

      public class after_unsetting_the_tag : given_one_tagged_with_a_tag_JAStudio_maintains
      {
         public after_unsetting_the_tag() => _vocab.Tags.Unset(Tags.Vocab.IsIchidanHidingGodanPotential);

         [XF] public void the_note_no_longer_contains_the_tag() =>
            _vocab.Tags.Contains(Tags.Vocab.IsIchidanHidingGodanPotential).Must().BeFalse();
      }
   }

   public class given_two_where_only_one_is_tagged : for_a_vocab
   {
      readonly VocabNote _tagged;
      readonly VocabNote _untagged;

      public given_two_where_only_one_is_tagged()
      {
         _tagged = CreateVocab("食べる", "to eat", "たべる");
         _untagged = CreateVocab("飲む", "to drink", "のむ");
         _tagged.Tags.Set(Tags.Vocab.IsIchidanHidingGodanPotential);
      }

      [XF] public void the_tagged_vocab_contains_the_tag() => _tagged.Tags.Contains(Tags.Vocab.IsIchidanHidingGodanPotential).Must().BeTrue();
      [XF] public void the_untagged_vocab_does_not_contain_the_tag() => _untagged.Tags.Contains(Tags.Vocab.IsIchidanHidingGodanPotential).Must().BeFalse();
   }
}
