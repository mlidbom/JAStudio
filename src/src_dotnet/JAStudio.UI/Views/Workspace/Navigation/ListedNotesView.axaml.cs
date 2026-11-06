using System;
using Avalonia.Controls;
using Avalonia.Input;
using JAStudio.UI.ViewModels.Workspace.Navigation;

namespace JAStudio.UI.Views.Workspace.Navigation;

/// <summary>The view of a <see cref="ListedNotesViewModel"/>: a list of notes in the navigation pane, which open in the workspace's tabs.</summary>
public partial class ListedNotesView : UserControl
{
   public ListedNotesView() => InitializeComponent();

   /// <summary>Given by the view it's part of, which binds its data context to a <see cref="ListedNotesViewModel"/>.</summary>
   ListedNotesViewModel ViewModel => DataContext as ListedNotesViewModel
                                  ?? throw new InvalidOperationException("The listed notes are shown without their ListedNotesViewModel.");

   void OnListedNoteDoubleTapped(object? sender, TappedEventArgs e) => ViewModel.OpenSelectedNote();

   /// <summary>Middle-clicking a listed note opens it in a background tab, as middle-clicking a link does.</summary>
   void OnListedNotePointerReleased(object? sender, PointerReleasedEventArgs e)
   {
      if(e.InitialPressMouseButton == MouseButton.Middle && sender is Control { DataContext: ListedNoteViewModel listedNote })
         ViewModel.OpenInBackgroundTab(listedNote);
   }
}
