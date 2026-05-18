using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_managing_note_tags;

public class for_a_sentence : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_one_tagged_with_TTSAudio : for_a_sentence
   {
      readonly SentenceNote _sentence;

      public given_one_tagged_with_TTSAudio()
      {
         _sentence = CreateTestSentence("これは本です。", "This is a book.");
         _sentence.Tags.Set(Tags.TTSAudio);
      }

      [XF] public void the_tag_is_present() => _sentence.Tags.Contains(Tags.TTSAudio).Must().BeTrue();

      public class after_unsetting_the_tag : given_one_tagged_with_TTSAudio
      {
         public after_unsetting_the_tag() => _sentence.Tags.Unset(Tags.TTSAudio);

         [XF] public void the_tag_is_not_present() => _sentence.Tags.Contains(Tags.TTSAudio).Must().BeFalse();

         public class after_setting_it_again : after_unsetting_the_tag
         {
            public after_setting_it_again() => _sentence.Tags.Set(Tags.TTSAudio);

            [XF] public void the_tag_is_present_again() => _sentence.Tags.Contains(Tags.TTSAudio).Must().BeTrue();
         }
      }
   }
}
