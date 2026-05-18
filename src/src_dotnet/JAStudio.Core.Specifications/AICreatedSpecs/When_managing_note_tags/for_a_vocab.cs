using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_managing_note_tags;

public class for_a_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_one_tagged_with_TTSAudio : for_a_vocab
   {
      readonly VocabNote _vocab;

      public given_one_tagged_with_TTSAudio()
      {
         _vocab = CreateVocab("食べる", "to eat", "たべる");
         _vocab.Tags.Set(Tags.TTSAudio);
      }

      [XF] public void the_note_contains_the_tag() => _vocab.Tags.Contains(Tags.TTSAudio).Must().BeTrue();

      public class after_unsetting_the_tag : given_one_tagged_with_TTSAudio
      {
         public after_unsetting_the_tag() => _vocab.Tags.Unset(Tags.TTSAudio);

         [XF] public void the_note_no_longer_contains_the_tag() =>
            _vocab.Tags.Contains(Tags.TTSAudio).Must().BeFalse();
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
         _tagged.Tags.Set(Tags.TTSAudio);
      }

      [XF] public void the_tagged_vocab_contains_the_tag() => _tagged.Tags.Contains(Tags.TTSAudio).Must().BeTrue();
      [XF] public void the_untagged_vocab_does_not_contain_the_tag() => _untagged.Tags.Contains(Tags.TTSAudio).Must().BeFalse();
   }
}
