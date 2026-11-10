using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace JAStudio.UI.Controls.NoteFields.RelatedKanji;

/// <summary>
/// One of a kanji's lists of other kanji (<see cref="KanjiListEditorViewModel"/>): each kanji as a chip, and a box that adds one, offering the corpus's<br/>
/// kanji found by what's typed (<see cref="PickableKanji.IsFoundBy"/>).
/// </summary>
partial class KanjiListEditor : UserControl
{
   /// <summary>A chip's opacity: whole for a kanji the corpus has a note for, dimmed for one it hasn't.</summary>
   public static readonly IValueConverter OpacityOfAChip = new FuncValueConverter<bool, double>(theCorpusHasANoteForIt => theCorpusHasANoteForIt ? 1.0 : 0.5);

   public KanjiListEditor()
   {
      InitializeComponent();
      KanjiBox.ItemFilter = (typed, item) => typed is not null && item is PickableKanji kanji && kanji.IsFoundBy(typed);
   }
}
