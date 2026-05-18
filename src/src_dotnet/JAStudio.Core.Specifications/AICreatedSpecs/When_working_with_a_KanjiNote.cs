using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

public class When_working_with_a_KanjiNote : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_a_newly_created_kanji : When_working_with_a_KanjiNote
   {
      readonly KanjiNote _kanji;

      public given_a_newly_created_kanji() => _kanji = CreateKanji("漢", "Chinese character", "カン、かん", "");

      [XF] public void the_question_is_set() => _kanji.GetQuestion().Must().Be("漢");
      [XF] public void the_answer_is_set() => _kanji.GetAnswer().Must().Be("Chinese character");
      [XF] public void it_has_an_id() => _kanji.GetId().Must().NotBeNull();
      [XF] public void ReadingsOn_includes_the_first_reading() => _kanji.ReadingsOn.Must().Contain("カン");
      [XF] public void ReadingsOn_includes_the_second_reading() => _kanji.ReadingsOn.Must().Contain("かん");
   }

   public class given_a_kanji_whose_radicals_include_itself : When_working_with_a_KanjiNote
   {
      readonly KanjiNote _kanji;

      public given_a_kanji_whose_radicals_include_itself()
      {
         _kanji = new KanjiNote(NoteServices);
         _kanji.SetQuestion("漢");
         _kanji.SetRadicals("漢, 水, 口");
      }

      [XF] public void Radicals_excludes_the_kanji_itself() => _kanji.Radicals.Contains("漢").Must().BeFalse();
      [XF] public void Radicals_includes_water() => _kanji.Radicals.Must().Contain("水");
      [XF] public void Radicals_includes_mouth() => _kanji.Radicals.Must().Contain("口");
   }

   public class given_a_kanji_with_a_marked_primary_on_reading : When_working_with_a_KanjiNote
   {
      readonly KanjiNote _kanji;

      public given_a_kanji_with_a_marked_primary_on_reading()
      {
         _kanji = new KanjiNote(NoteServices);
         _kanji.ReadingOnHtml.Set("<primary>カン</primary>, ケン");
      }

      [XF] public void PrimaryReadingsOn_has_one_reading() => _kanji.PrimaryReadingsOn.Count.Must().Be(1);
      [XF] public void PrimaryReadingsOn_contains_the_marked_reading() => _kanji.PrimaryReadingsOn[0].Must().Be("カン");
   }

   public class after_adding_a_primary_on_reading : When_working_with_a_KanjiNote
   {
      readonly KanjiNote _kanji;

      public after_adding_a_primary_on_reading()
      {
         _kanji = new KanjiNote(NoteServices);
         _kanji.ReadingOnHtml.Set("カン, ケン");
         _kanji.AddPrimaryOnReading("カン");
      }

      [XF] public void the_reading_is_wrapped_in_primary_tags() =>
         _kanji.ReadingOnHtml.Value.Must().Contain("<primary>カン</primary>");
   }

   public class after_removing_a_primary_on_reading : When_working_with_a_KanjiNote
   {
      readonly KanjiNote _kanji;

      public after_removing_a_primary_on_reading()
      {
         _kanji = new KanjiNote(NoteServices);
         _kanji.ReadingOnHtml.Set("<primary>カン</primary>, ケン");
         _kanji.RemovePrimaryOnReading("カン");
      }

      [XF] public void the_primary_tags_are_removed() =>
         _kanji.ReadingOnHtml.Value.Must().NotContain("<primary>カン</primary>");

      [XF] public void the_reading_text_is_preserved() =>
         _kanji.ReadingOnHtml.Value.Must().Contain("カン");
   }
}
