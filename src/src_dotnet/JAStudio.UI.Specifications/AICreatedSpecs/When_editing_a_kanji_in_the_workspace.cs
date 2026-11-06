using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
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

   public class after_typing_an_on_reading_in_katakana : When_editing_a_kanji_in_the_workspace
   {
      public after_typing_an_on_reading_in_katakana() => _editor.ReadingFields.Single(it => it.Label == "On").Text = "ソウ";

      [XF] public void the_kanji_stores_it_in_hiragana() => _kanji.ReadingOnHtml.Value.Must().Be("そう");
   }

   public class after_typing_radicals_followed_by_a_comma : When_editing_a_kanji_in_the_workspace
   {
      public after_typing_radicals_followed_by_a_comma() => _editor.RelatedFields.Single(it => it.Label == "Radicals").Text = "土, ";

      [XF] public void the_kanji_has_only_that_radical() => _kanji.AllRadicals.Must().SequenceEqual(["土"]);
      [XF] public void the_text_keeps_the_comma() => _editor.RelatedFields.Single(it => it.Label == "Radicals").Text.Must().Be("土, ");
   }
}
