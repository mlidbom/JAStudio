using Avalonia.Controls;
using JAStudio.UI.ViewModels.Workspace.Navigation;

namespace JAStudio.UI.Views.Workspace.Navigation;

/// <summary>
/// The view of a <see cref="NavigationPaneViewModel"/>: the navigation pane on the left of the <see cref="WorkspaceWindow"/>,<br/>
/// with a tab for the note search and one for the list a note menu's "Open" item showed.
/// </summary>
public partial class NavigationPane : UserControl
{
   public NavigationPane() => InitializeComponent();

   /// <summary>Puts the keyboard in the search tab's search box. The search tab must be the one shown.</summary>
   internal void FocusSearchBox() => NoteSearch.FocusSearchBox();
}
