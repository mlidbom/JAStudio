using Avalonia.Controls;
using JAStudio.UI.ViewModels.Workspace.Navigation;

namespace JAStudio.UI.Views.Workspace.Navigation;

/// <summary>The view of a <see cref="NoteSearchViewModel"/>: the "Search" tab of the <see cref="NavigationPane"/>, a search box and the notes found.</summary>
public partial class NoteSearchView : UserControl
{
   public NoteSearchView() => InitializeComponent();

   /// <summary>Puts the keyboard in the search box, with its text selected so that typing replaces it.</summary>
   internal void FocusSearchBox()
   {
      SearchBox.Focus();
      SearchBox.SelectAll();
   }
}
