using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_querying_collections;

public class for_kanji : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_two_kanji : for_kanji
   {
      readonly ITaggregateReference<KanjiNote> _kanji1Reference;
      KanjiNote _kanji1 => _kanji1Reference.Current;

      public given_two_kanji()
      {
         _kanji1Reference = CreateKanji("食", "eat", "ショク", "た").Reference();
         CreateKanji("本", "book", "ホン", "もと");
      }

      [XF] public void WithKanji_finds_the_matching_kanji() =>
         GetService<KanjiCollection>().WithKanji("食").Must().Be(_kanji1);

      [XF] public void WithKanji_returns_null_for_a_nonexistent_character() =>
         GetService<KanjiCollection>().WithKanji("存").Must().BeNull();
   }
}
