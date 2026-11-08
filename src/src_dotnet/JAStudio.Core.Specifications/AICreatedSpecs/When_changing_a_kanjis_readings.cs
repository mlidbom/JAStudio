using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.KanjiReadings;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Changing a kanji's readings (<see cref="KanjiNote.Readings"/>): adding and removing them, and making them primary or secondary.<br/>
/// On readings are kept in hiragana however they're typed, other readings as typed.
/// </summary>
public class When_changing_a_kanjis_readings : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly KanjiNote _kanji;

   protected When_changing_a_kanjis_readings() => _kanji = CreateKanji("頁", "page", "", "");

   protected KanjiReadingList On => _kanji.Readings.On;
   protected KanjiReadingList Kun => _kanji.Readings.Kun;

   public class after_adding_an_on_reading_in_katakana : When_changing_a_kanjis_readings
   {
      public after_adding_an_on_reading_in_katakana() => On.Add("ケツ");

      [XF] public void the_on_reading_is_kept_in_hiragana() => On.AllKana().Must().SequenceEqual(["けつ"]);
      [XF] public void it_is_secondary() => On.PrimaryKana().Must().BeEmpty();

      public class and_then_adding_it_in_hiragana : after_adding_an_on_reading_in_katakana
      {
         public and_then_adding_it_in_hiragana() => On.Add("けつ");

         [XF] public void the_kanji_has_the_reading_once() => On.AllKana().Must().SequenceEqual(["けつ"]);
      }

      public class and_then_making_it_primary_by_its_katakana : after_adding_an_on_reading_in_katakana
      {
         public and_then_making_it_primary_by_its_katakana() => On.MakePrimary("ケツ");

         [XF] public void it_is_primary() => On.PrimaryKana().Must().SequenceEqual(["けつ"]);
      }
   }

   public class after_adding_a_kun_reading_in_katakana : When_changing_a_kanjis_readings
   {
      public after_adding_a_kun_reading_in_katakana() => Kun.Add("ページ");

      [XF] public void the_kun_reading_is_kept_in_katakana() => Kun.AllKana().Must().SequenceEqual(["ページ"]);
   }

   public class after_adding_two_kun_readings_and_making_the_second_primary : When_changing_a_kanjis_readings
   {
      public after_adding_two_kun_readings_and_making_the_second_primary()
      {
         Kun.Add("かしら");
         Kun.Add("ページ");
         Kun.MakePrimary("ページ");
      }

      [XF] public void the_kanji_has_both_in_the_order_they_were_added() => Kun.AllKana().Must().SequenceEqual(["かしら", "ページ"]);
      [XF] public void the_second_is_the_kanjis_only_primary_reading() => _kanji.Readings.PrimaryKana().Must().SequenceEqual(["ページ"]);

      public class and_then_making_it_secondary : after_adding_two_kun_readings_and_making_the_second_primary
      {
         public and_then_making_it_secondary() => Kun.MakeSecondary("ページ");

         [XF] public void the_kanji_has_no_primary_reading() => _kanji.Readings.PrimaryKana().Must().BeEmpty();
         [XF] public void the_kanji_still_has_both_readings() => Kun.AllKana().Must().SequenceEqual(["かしら", "ページ"]);
      }

      public class and_then_removing_it : after_adding_two_kun_readings_and_making_the_second_primary
      {
         public and_then_removing_it() => Kun.Remove("ページ");

         [XF] public void only_the_first_is_left() => Kun.AllKana().Must().SequenceEqual(["かしら"]);
         [XF] public void the_kanji_has_no_primary_reading() => _kanji.Readings.PrimaryKana().Must().BeEmpty();
      }
   }

   public class making_primary_a_reading_the_kanji_does_not_have : When_changing_a_kanjis_readings
   {
      [XF] public void throws() => Invoking(() => Kun.MakePrimary("かしら")).Must().Throw<ArgumentAssertionFailedException>();
   }
}
