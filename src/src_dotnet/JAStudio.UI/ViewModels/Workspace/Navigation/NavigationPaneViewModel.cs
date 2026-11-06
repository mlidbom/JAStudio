using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.UI.Views.Workspace.Navigation;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// The navigation pane on the left of a workspace (<see cref="NavigationPane"/>): ways to find notes to open in the workspace's tabs,<br/>
/// as tabs of their own. The <see cref="Search"/>, and the <see cref="NoteList"/> the note menus' "Open" items show.
/// </summary>
partial class NavigationPaneViewModel : ObservableObject
{
   public NavigationPaneViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services)
   {
      Search = new NoteSearchViewModel(workspace, services);
      NoteList = new NoteListViewModel(workspace, services);
   }

   public NoteSearchViewModel Search { get; }

   public NoteListViewModel NoteList { get; }

   /// <summary>Whether the pane is open. Closing it gives its space back to the tabs.</summary>
   [ObservableProperty] bool _isOpen = true;

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(IsSearchShown), nameof(IsNoteListShown))]
   NavigationPaneTab _shownTab = NavigationPaneTab.Search;

   /// <summary>Whether <see cref="ShownTab"/> is the <see cref="Search"/>. Bound to its tab, which sets it when clicked.</summary>
   public bool IsSearchShown
   {
      get => ShownTab == NavigationPaneTab.Search;
      set
      {
         if(value) ShownTab = NavigationPaneTab.Search;
      }
   }

   /// <summary>Whether <see cref="ShownTab"/> is the <see cref="NoteList"/>. Bound to its tab, which sets it when clicked.</summary>
   public bool IsNoteListShown
   {
      get => ShownTab == NavigationPaneTab.NoteList;
      set
      {
         if(value) ShownTab = NavigationPaneTab.NoteList;
      }
   }

   /// <summary>Opens the pane on its <see cref="Search"/>.</summary>
   public void ShowSearch()
   {
      IsOpen = true;
      ShownTab = NavigationPaneTab.Search;
   }

   /// <summary>Opens the pane on its <see cref="NoteList"/>, listing <paramref name="noteList"/>.</summary>
   public void ShowNoteList(NoteList noteList)
   {
      NoteList.Show(noteList);
      IsOpen = true;
      ShownTab = NavigationPaneTab.NoteList;
   }
}
