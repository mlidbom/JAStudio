using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_managing_note_tags;

public class for_a_kanji : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_one_with_multiple_tags : for_a_kanji
   {
      readonly KanjiNote _kanji;

      public given_one_with_multiple_tags()
      {
         _kanji = CreateKanji("食", "eat", "ショク", "た");
         _kanji.Tags.Set(Tags.TTSAudio);
         _kanji.Tags.Set(Tags.Kanji.IsRadical);
         _kanji.Tags.Set(Tags.Kanji.InVocabMainForm);
      }

      [XF] public void the_TTSAudio_tag_is_present() => _kanji.Tags.Contains(Tags.TTSAudio).Must().BeTrue();
      [XF] public void the_IsRadical_tag_is_present() => _kanji.Tags.Contains(Tags.Kanji.IsRadical).Must().BeTrue();
      [XF] public void the_InVocabMainForm_tag_is_present() => _kanji.Tags.Contains(Tags.Kanji.InVocabMainForm).Must().BeTrue();
   }
}
