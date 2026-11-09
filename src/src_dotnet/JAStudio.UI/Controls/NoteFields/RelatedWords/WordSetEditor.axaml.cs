using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace JAStudio.UI.Controls.NoteFields.RelatedWords;

/// <summary>
/// One of a vocab's relationships to other words (<see cref="WordSetEditorViewModel"/>): each word as a chip, and a box that adds one, offering the corpus's<br/>
/// vocab whose name, forms or readings hold what's typed (<see cref="PickableVocab.IsFoundBy"/>).
/// </summary>
partial class WordSetEditor : UserControl
{
   /// <summary>A chip's opacity: whole for a word that refers to a vocab, dimmed for one that refers to none.</summary>
   public static readonly IValueConverter OpacityOfAChip = new FuncValueConverter<bool, double>(refersToAVocab => refersToAVocab ? 1.0 : 0.5);

   public WordSetEditor()
   {
      InitializeComponent();
      WordBox.ItemFilter = (typed, item) => typed is not null && item is PickableVocab vocab && vocab.IsFoundBy(typed);
   }
}
