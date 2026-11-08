using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Note.Search;
using JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;
using JAStudio.UI.Views.Workspace.Navigation;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// The navigation pane on the left of a workspace (<see cref="NavigationPane"/>): ways to find notes to open in the workspace's tabs,<br/>
/// as tabs of their own: the <see cref="Search"/>, the <see cref="NoteList"/> the note menus' "Open" items show, and the <see cref="TagManagement"/>.
/// </summary>
partial class NavigationPaneViewModel : ObservableObject
{
   public NavigationPaneViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services)
   {
      Search = new NoteSearchViewModel(workspace, services);
      NoteList = new NoteListViewModel(workspace, services);
      TagManagement = new TagManagementViewModel(this, services);
   }

   public NoteSearchViewModel Search { get; }

   public NoteListViewModel NoteList { get; }

   public TagManagementViewModel TagManagement { get; }

   /// <summary>Whether the pane is open. Closing it gives its space back to the tabs.</summary>
   [ObservableProperty] bool _isOpen = true;

   /// <summary>The pane's width, as its edge was last dragged. Bound to the pane, so that it's kept between sessions.</summary>
   [ObservableProperty] double _width = 420;

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(IsSearchShown), nameof(IsNoteListShown), nameof(IsTagManagementShown))]
   NavigationPaneTab _shownTab = NavigationPaneTab.Search;

   /// <summary>The <see cref="TagManagement"/>'s tag tree is computed from the notes' tags each time its tab is shown, so it shows the tags as they are then.</summary>
   partial void OnShownTabChanged(NavigationPaneTab value)
   {
      if(value == NavigationPaneTab.TagManagement)
         TagManagement.ShowTheTagsAsTheyAreNow();
   }

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

   /// <summary>Whether <see cref="ShownTab"/> is the <see cref="TagManagement"/>. Bound to its tab, which sets it when clicked.</summary>
   public bool IsTagManagementShown
   {
      get => ShownTab == NavigationPaneTab.TagManagement;
      set
      {
         if(value) ShownTab = NavigationPaneTab.TagManagement;
      }
   }

   /// <summary>Opens the pane on its <see cref="Search"/>.</summary>
   public void ShowSearch()
   {
      IsOpen = true;
      ShownTab = NavigationPaneTab.Search;
   }

   /// <summary>
   /// Opens the pane on its <see cref="Search"/>, with the whole search replaced by one that finds only the notes <paramref name="condition"/> matches,<br/>
   /// as searching for a tag from the <see cref="TagManagement"/> does. Undo in the search goes back to the search before.
   /// </summary>
   public void SearchFor(NoteCondition condition)
   {
      Search.SearchFor(condition);
      ShowSearch();
   }

   /// <summary>Opens the pane on its <see cref="NoteList"/>, listing <paramref name="noteList"/>.</summary>
   public void ShowNoteList(NoteList noteList)
   {
      NoteList.Show(noteList);
      IsOpen = true;
      ShownTab = NavigationPaneTab.NoteList;
   }
}
