using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_updating_cached_note_data;

public class for_a_kanji : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class after_changing_its_question : for_a_kanji
   {
      readonly KanjiNote _kanji;

      public after_changing_its_question()
      {
         _kanji = CreateKanji("食", "eat", "ショク", "た");
         _kanji.SetQuestion("飲");
         _kanji.UpdateGeneratedData();
      }

      [XF] public void the_old_character_no_longer_finds_it() =>
         GetService<KanjiCollection>().WithKanji("食").Must().BeNull();

      [XF] public void the_new_character_finds_it() =>
         GetService<KanjiCollection>().WithKanji("飲").Must().Be(_kanji);
   }
}
