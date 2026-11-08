using Avalonia.Controls;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>A vocab's forms besides its question (<see cref="EnteredFormsEditorViewModel"/>): each as a chip that says whether the vocab owns it, and a box for adding one.</summary>
partial class EnteredFormsEditor : UserControl
{
   public EnteredFormsEditor() => InitializeComponent();
}
