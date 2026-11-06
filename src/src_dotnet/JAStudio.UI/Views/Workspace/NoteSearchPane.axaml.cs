using System;
using Avalonia.Controls;
using Avalonia.Input;
using JAStudio.UI.ViewModels.Workspace;

namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// The workspace's note search pane (<see cref="NoteSearchViewModel"/>), on the left of the <see cref="WorkspaceWindow"/>:<br/>
/// a search box, and the notes found, which open in the workspace's tabs.
/// </summary>
public partial class NoteSearchPane : UserControl
{
   public NoteSearchPane() => InitializeComponent();

   /// <summary>Given by the <see cref="WorkspaceWindow"/>, which binds the pane's data context to <see cref="WorkspaceViewModel.NoteSearch"/>.</summary>
   NoteSearchViewModel ViewModel => DataContext as NoteSearchViewModel
                                 ?? throw new InvalidOperationException("The note search pane is shown without its NoteSearchViewModel.");

   /// <summary>Puts the keyboard in the search box, with its text selected so that typing replaces it.</summary>
   internal void FocusSearchBox()
   {
      SearchBox.Focus();
      SearchBox.SelectAll();
   }

   void OnResultDoubleTapped(object? sender, TappedEventArgs e) => ViewModel.OpenSelectedNote();

   /// <summary>Middle-clicking a result opens it in a background tab, as middle-clicking a link does.</summary>
   void OnResultPointerReleased(object? sender, PointerReleasedEventArgs e)
   {
      if(e.InitialPressMouseButton == MouseButton.Middle && sender is Control { DataContext: NoteSearchResultViewModel result })
         ViewModel.OpenInBackgroundTab(result);
   }
}
