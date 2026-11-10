using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Controls.NoteFields.RelatedKanji;
using JAStudio.UI.ViewModels.Workspace.Editors;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

public class When_editing_a_kanji_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly KanjiNote _kanji;
   readonly KanjiEditorSectionViewModel _editor;

   protected When_editing_a_kanji_in_the_workspace()
   {
      _kanji = CreateKanji("走", "run", "", "はし");
      _editor = Edit<KanjiEditorSectionViewModel>(_kanji);
   }

   KanjiReadingListEditorViewModel ReadingsEditor(string label) => _editor.Readings.Single(it => it.Label == label);

   public class after_adding_an_on_reading_typed_in_katakana : When_editing_a_kanji_in_the_workspace
   {
      public after_adding_an_on_reading_typed_in_katakana()
      {
         ReadingsEditor("On").NewReading = "ソウ";
         ReadingsEditor("On").AddCommand.Execute(null);
      }

      [XF] public void the_kanji_stores_it_in_hiragana() => _kanji.Readings.On.AllKana().Must().SequenceEqual(["そう"]);
      [XF] public void the_kanji_stores_it_as_secondary() => _kanji.Readings.On.PrimaryKana().Must().BeEmpty();
      [XF] public void the_editor_shows_it_in_hiragana() => ReadingsEditor("On").Items.Select(it => it.Kana).Must().SequenceEqual(["そう"]);
      [XF] public void the_box_for_adding_a_reading_is_emptied() => ReadingsEditor("On").NewReading.Must().Be("");

      public class and_then_checking_it_as_primary : after_adding_an_on_reading_typed_in_katakana
      {
         public and_then_checking_it_as_primary() => ReadingsEditor("On").Items.Single().IsPrimary = true;

         [XF] public void the_kanji_stores_it_as_primary() => _kanji.Readings.On.PrimaryKana().Must().SequenceEqual(["そう"]);
      }
   }

   public class after_removing_its_kun_reading : When_editing_a_kanji_in_the_workspace
   {
      public after_removing_its_kun_reading() => ReadingsEditor("Kun").Items.Single().RemoveCommand.Execute(null);

      [XF] public void the_kanji_has_no_kun_readings() => _kanji.Readings.Kun.AllKana().Must().BeEmpty();
      [XF] public void the_editor_shows_no_kun_readings() => ReadingsEditor("Kun").Items.Must().BeEmpty();
   }

   KanjiListEditorViewModel ListEditor(string title) => _editor.RelatedKanji.Single(it => it.Title == title);

   public class after_typing_土_in_the_radicals_box : When_editing_a_kanji_in_the_workspace
   {
      public after_typing_土_in_the_radicals_box() => ListEditor("Radicals").NewKanji = "土";

      [XF] public void it_can_be_added() => ListEditor("Radicals").AddCommand.CanExecute(null).Must().BeTrue();

      public class and_adding_it : after_typing_土_in_the_radicals_box
      {
         public and_adding_it() => ListEditor("Radicals").AddCommand.Execute(null);

         [XF] public void the_kanji_has_the_radical_土() => _kanji.Radicals.All.Must().SequenceEqual(["土"]);
         [XF] public void the_editor_shows_土_as_a_chip() => ListEditor("Radicals").Items.Select(it => it.Kanji).Must().SequenceEqual(["土"]);
         [XF] public void the_box_for_adding_a_kanji_is_emptied() => ListEditor("Radicals").NewKanji.Must().Be("");

         public class and_typing_土_again : and_adding_it
         {
            public and_typing_土_again() => ListEditor("Radicals").NewKanji = "土";

            [XF] public void it_can_be_added_again_since_a_kanji_may_have_a_radical_more_than_once() => ListEditor("Radicals").AddCommand.CanExecute(null).Must().BeTrue();
         }

         public class and_removing_its_chip : and_adding_it
         {
            public and_removing_its_chip() => ListEditor("Radicals").Items.Single().RemoveCommand.Execute(null);

            [XF] public void the_kanji_has_no_radicals() => _kanji.Radicals.All.Must().BeEmpty();
            [XF] public void the_editor_shows_no_chips() => ListEditor("Radicals").Items.Must().BeEmpty();
         }
      }
   }

   public class after_typing_two_characters_in_the_radicals_box : When_editing_a_kanji_in_the_workspace
   {
      public after_typing_two_characters_in_the_radicals_box() => ListEditor("Radicals").NewKanji = "土人";

      [XF] public void they_cannot_be_added_since_a_kanji_is_one_character() => ListEditor("Radicals").AddCommand.CanExecute(null).Must().BeFalse();
   }

   public class after_typing_the_kanji_itself_in_the_similar_meaning_box : When_editing_a_kanji_in_the_workspace
   {
      public after_typing_the_kanji_itself_in_the_similar_meaning_box() => ListEditor("Similar meaning").NewKanji = "走";

      [XF] public void it_cannot_be_added() => ListEditor("Similar meaning").AddCommand.CanExecute(null).Must().BeFalse();
   }

   public class after_adding_歩_in_the_similar_meaning_box : When_editing_a_kanji_in_the_workspace
   {
      readonly KanjiNote _walk;

      public after_adding_歩_in_the_similar_meaning_box()
      {
         _walk = CreateKanji("歩", "walk", "ホ", "ある");
         ListEditor("Similar meaning").NewKanji = "歩";
         ListEditor("Similar meaning").AddCommand.Execute(null);
      }

      [XF] public void the_kanji_has_歩_as_a_kanji_with_a_similar_meaning() => _kanji.SimilarMeaning.Kanji.Must().SequenceEqual(["歩"]);
      [XF] public void 歩_has_the_kanji_as_a_kanji_with_a_similar_meaning() => _walk.SimilarMeaning.Kanji.Must().SequenceEqual(["走"]);

      public class and_typing_歩_again : after_adding_歩_in_the_similar_meaning_box
      {
         public and_typing_歩_again() => ListEditor("Similar meaning").NewKanji = "歩";

         [XF] public void it_cannot_be_added_since_the_kanji_has_it_already() => ListEditor("Similar meaning").AddCommand.CanExecute(null).Must().BeFalse();
      }
   }
}
