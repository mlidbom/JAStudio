using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_managing_note_tags;

public class for_a_kanji : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_one_with_multiple_tags : for_a_kanji
   {
      readonly ITaggregateReference<KanjiNote> _kanjiReference;
      KanjiNote _kanji => _kanjiReference.Current;

      public given_one_with_multiple_tags()
      {
         _kanjiReference = CreateKanji("食", "eat", "ショク", "た").Reference();
         _kanji.Edit(it =>
         {
            it.Tags.Set(Tags.Kanji.IsRadical);
            it.Tags.Set(Tags.Kanji.InVocabMainForm);
         });
      }

      [XF] public void the_IsRadical_tag_is_present() => _kanji.Tags.Must().Contain(Tags.Kanji.IsRadical);
      [XF] public void the_InVocabMainForm_tag_is_present() => _kanji.Tags.Must().Contain(Tags.Kanji.InVocabMainForm);
   }
}
