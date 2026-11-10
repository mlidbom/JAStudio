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

   public class given_a_kanji_whose_radicals_are_set_to_itself_water_and_mouth : When_working_with_a_KanjiNote
   {
      readonly KanjiNote _kanji;

      public given_a_kanji_whose_radicals_are_set_to_itself_water_and_mouth()
      {
         _kanji = new KanjiNote(NoteServices);
         _kanji.SetQuestion("漢");
         _kanji.Radicals.Set(["漢", "水", "口"]);
      }

      [XF] public void its_radicals_are_water_and_mouth_without_the_kanji_itself() => _kanji.Radicals.All.Must().SequenceEqual(["水", "口"]);
   }

   public class given_a_kanji_whose_primary_vocab_field_is_written_with_a_colon_alone_as_typing_a_disambiguated_name_does_on_the_way : When_working_with_a_KanjiNote
   {
      readonly KanjiNote _kanji;

      public given_a_kanji_whose_primary_vocab_field_is_written_with_a_colon_alone_as_typing_a_disambiguated_name_does_on_the_way()
      {
         _kanji = CreateKanji("掛", "hang", "カイ", "か");
         KanjiEditableFields.PrimaryVocab.Write(_kanji, [":"]);
      }

      [XF] public void the_kanji_has_no_primary_vocab_since_the_colon_names_no_word() => _kanji.PrimaryVocab.Words.Must().BeEmpty();
   }
}
