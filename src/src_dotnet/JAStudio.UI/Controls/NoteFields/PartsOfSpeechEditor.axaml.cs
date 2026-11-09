using Avalonia.Controls;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>A vocab's parts of speech (<see cref="PartsOfSpeechEditorViewModel"/>): each as a chip, and a box that adds one of those JAStudio knows, filtered by what's typed.</summary>
partial class PartsOfSpeechEditor : UserControl
{
   public PartsOfSpeechEditor() => InitializeComponent();
}
