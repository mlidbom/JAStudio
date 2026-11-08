using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.UI.Controls.NoteFields;
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

   public class after_typing_radicals_followed_by_a_comma : When_editing_a_kanji_in_the_workspace
   {
      public after_typing_radicals_followed_by_a_comma() => _editor.RelatedFields.Single(it => it.Label == "Radicals").Text = "土, ";

      [XF] public void the_kanji_has_only_that_radical() => _kanji.AllRadicals.Must().SequenceEqual(["土"]);
      [XF] public void the_text_keeps_the_comma() => _editor.RelatedFields.Single(it => it.Label == "Radicals").Text.Must().Be("土, ");
   }
}
