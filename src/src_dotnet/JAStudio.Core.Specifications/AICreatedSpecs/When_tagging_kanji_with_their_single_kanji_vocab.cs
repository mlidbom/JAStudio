using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Batches;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Tagging the kanji's metadata (<see cref="LocalNoteUpdater.TagKanjiMetadata"/>) marks a kanji whose single-kanji vocab, the vocab<br/>
/// that is the kanji alone, has a reading that isn't one of the kanji's primary readings.
/// </summary>
public class When_tagging_kanji_with_their_single_kanji_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected void TagKanjiMetadata() => GetService<LocalNoteUpdater>().TagKanjiMetadata();

   public class for_a_kanji_whose_single_kanji_vocab_has_a_reading_that_is_not_a_primary_reading : When_tagging_kanji_with_their_single_kanji_vocab
   {
      readonly KanjiNote _kanji;

      public for_a_kanji_whose_single_kanji_vocab_has_a_reading_that_is_not_a_primary_reading()
      {
         _kanji = CreateKanji("干", "dry", "<primary>カン</primary>", "ほ");
         CreateVocab("干", "dry", "ほし");
         TagKanjiMetadata();
      }

      [XF] public void it_is_tagged_as_having_a_single_kanji_vocab() => _kanji.Tags.Must().Contain(Tags.Kanji.WithSingleKanjiVocab);
      [XF] public void it_is_tagged_as_having_one_with_a_different_reading() => _kanji.Tags.Must().Contain(Tags.Kanji.WithSingleKanjiVocabWithDifferentReading);
   }

   public class for_a_kanji_tagged_as_having_one_with_a_different_reading_that_has_no_single_kanji_vocab : When_tagging_kanji_with_their_single_kanji_vocab
   {
      readonly KanjiNote _kanji;

      public for_a_kanji_tagged_as_having_one_with_a_different_reading_that_has_no_single_kanji_vocab()
      {
         _kanji = CreateKanji("莫", "not", "<primary>バク</primary>", "");
         _kanji.Tags.Set(Tags.Kanji.WithSingleKanjiVocabWithDifferentReading);
         TagKanjiMetadata();
      }

      [XF] public void the_tag_is_removed() => _kanji.Tags.Must().Satisfy(it => !it.Contains(Tags.Kanji.WithSingleKanjiVocabWithDifferentReading));
      [XF] public void it_is_not_tagged_as_having_a_single_kanji_vocab() => _kanji.Tags.Must().Satisfy(it => !it.Contains(Tags.Kanji.WithSingleKanjiVocab));
   }
}
