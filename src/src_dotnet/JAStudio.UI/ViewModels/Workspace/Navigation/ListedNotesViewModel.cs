using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note;
using JAStudio.UI.Views.Workspace.Navigation;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// A list of notes in the navigation pane that open in the workspace's tabs, such as the notes a search found (<see cref="ListedNotesView"/>).<br/>
/// Enter or double-clicking opens the selected note as opening a note from outside the workspace does; middle-clicking opens a note in a background tab.
/// </summary>
partial class ListedNotesViewModel : ObservableObject
{
   readonly WorkspaceViewModel _workspace;

   public ListedNotesViewModel(WorkspaceViewModel workspace) => _workspace = workspace;

   public ObservableCollection<ListedNoteViewModel> Notes { get; } = [];

   [ObservableProperty] ListedNoteViewModel? _selectedNote;

   /// <summary>Lists <paramref name="notes"/> instead of the notes listed so far.</summary>
   public void Show(IEnumerable<JPNote> notes)
   {
      Notes.Clear();
      foreach(var note in notes)
         Notes.Add(new ListedNoteViewModel(note));
   }

   /// <summary>Shows the selected note in the workspace, in the tab already showing it or else in a new tab.</summary>
   [RelayCommand]
   public void OpenSelectedNote()
   {
      if(SelectedNote != null)
         _workspace.OpenNote(SelectedNote.Note);
   }

   /// <summary>Opens <paramref name="listedNote"/> in a new tab in the background, as middle-clicking a link does.</summary>
   public void OpenInBackgroundTab(ListedNoteViewModel listedNote) => _workspace.OpenNoteInBackgroundTab(listedNote.Note);
}
