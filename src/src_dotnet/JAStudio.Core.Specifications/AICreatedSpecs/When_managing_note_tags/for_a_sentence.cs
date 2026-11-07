using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_managing_note_tags;

public class for_a_sentence : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_one_with_tags_a_person_manages_and_a_jastudio_defined_tag : for_a_sentence
   {
      readonly SentenceNote _sentence;

      public given_one_with_tags_a_person_manages_and_a_jastudio_defined_tag()
      {
         _sentence = CreateTestSentence("これは本です。", "This is a book.");
         _sentence.Tags.Set(Tag.FromName("source::anime::natsume"));
         _sentence.Tags.Set(Tags.TTSAudio);
         _sentence.Tags.Set(Tag.FromName("-::counting"));
      }

      [XF] public void the_tags_not_defined_by_jastudio_are_those_a_person_manages_in_alphabetical_order() =>
         _sentence.Tags.NotDefinedByJAStudio().Select(it => it.Name).Must().SequenceEqual(["-::counting", "source::anime::natsume"]);
   }

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
