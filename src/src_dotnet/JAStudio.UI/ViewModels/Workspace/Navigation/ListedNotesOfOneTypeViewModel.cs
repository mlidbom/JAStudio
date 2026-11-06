using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// The notes of one note type in a <see cref="ListedNotesViewModel"/>, such as its vocab, which get a tab of their own when the list holds several types.<br/>
/// Enter or double-clicking opens the selected note as opening a note from outside the workspace does; middle-clicking opens a note in a background tab.
/// </summary>
partial class ListedNotesOfOneTypeViewModel : ObservableObject
{
   readonly WorkspaceViewModel _workspace;

   /// <param name="workspace">The workspace the notes open in.</param>
   /// <param name="noteType">The type's name, as its tab shows it, such as "Vocab".</param>
   public ListedNotesOfOneTypeViewModel(WorkspaceViewModel workspace, string noteType)
   {
      _workspace = workspace;
      NoteType = noteType;
   }

   public string NoteType { get; }

   public ObservableCollection<ListedNoteViewModel> Notes { get; } = [];

   [ObservableProperty] ListedNoteViewModel? _selectedNote;

   public bool HasNotes => Notes.Count > 0;

   /// <summary>The type and how many notes of it are listed, as its tab shows it, such as "Vocab (12)".</summary>
   public string Header => $"{NoteType} ({Notes.Count})";

   /// <summary>Lists <paramref name="notes"/> instead of the notes listed so far.</summary>
   public void Show(IEnumerable<JPNote> notes)
   {
      Notes.Clear();
      foreach(var note in notes)
         Notes.Add(new ListedNoteViewModel(note));
      OnPropertyChanged(nameof(HasNotes));
      OnPropertyChanged(nameof(Header));
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
