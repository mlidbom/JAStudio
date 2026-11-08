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

      public given_a_newly_created_kanji() => _kanji = CreateKanji("漢", "Chinese character", "カン", "");

      [XF] public void the_question_is_set() => _kanji.GetQuestion().Must().Be("漢");
      [XF] public void the_answer_is_set() => _kanji.GetAnswer().Must().Be("Chinese character");
      [XF] public void it_has_an_id() => _kanji.GetId().Must().NotBeNull();
   }

   public class given_a_kanji_whose_radicals_include_itself : When_working_with_a_KanjiNote
   {
      readonly KanjiNote _kanji;

      public given_a_kanji_whose_radicals_include_itself()
      {
         _kanji = new KanjiNote(NoteServices);
         _kanji.SetQuestion("漢");
         _kanji.RadicalsRaw = "漢, 水, 口";
      }

      [XF] public void Radicals_excludes_the_kanji_itself() => _kanji.Radicals.Contains("漢").Must().BeFalse();
      [XF] public void Radicals_includes_water() => _kanji.Radicals.Must().Contain("水");
      [XF] public void Radicals_includes_mouth() => _kanji.Radicals.Must().Contain("口");
   }
}
