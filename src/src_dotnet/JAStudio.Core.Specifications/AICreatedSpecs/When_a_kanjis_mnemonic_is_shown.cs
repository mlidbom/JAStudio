using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// The mnemonic a kanji's card shows (<see cref="KanjiNote.ShownMnemonic"/>): the corpus's own, or a default one. The source's mnemonic, WaniKani's<br/>
/// for most kanji, is never shown, since it isn't ours to show.
/// </summary>
public class When_a_kanjis_mnemonic_is_shown : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   const string SourceMnemonic = "A mnemonic from the source";
   readonly KanjiNote _kanji;

   public When_a_kanjis_mnemonic_is_shown()
   {
      _kanji = CreateKanji("川", "river", "セン", "かわ");
      _kanji.SourceData.MeaningMnemonic.Set(SourceMnemonic);
   }

   public class and_the_corpus_has_no_mnemonic_for_it : When_a_kanjis_mnemonic_is_shown
   {
      [XF] public void the_default_mnemonic_is_shown() => _kanji.ShownMnemonic.Must().StartWith("# ");
      [XF] public void the_source_mnemonic_is_not_shown() => _kanji.ShownMnemonic.Contains(SourceMnemonic).Must().BeFalse();
   }

   public class and_the_corpus_has_a_mnemonic_for_it : When_a_kanjis_mnemonic_is_shown
   {
      public and_the_corpus_has_a_mnemonic_for_it() => _kanji.Mnemonic.Set("The corpus's mnemonic");

      [XF] public void the_corpus_mnemonic_is_shown() => _kanji.ShownMnemonic.Must().Be("The corpus's mnemonic");
   }
}
