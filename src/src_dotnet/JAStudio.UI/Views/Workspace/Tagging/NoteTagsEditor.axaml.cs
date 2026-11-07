using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JAStudio.UI.ViewModels.Workspace.Tagging;

namespace JAStudio.UI.Views.Workspace.Tagging;

/// <summary>
/// The view of a <see cref="NoteTagsEditorViewModel"/>: a note's tags, each with a × that removes it, and a button that adds a tag,<br/>
/// picked in a <see cref="TagPickerDialog"/> that opens below the button.
/// </summary>
partial class NoteTagsEditor : UserControl
{
   public NoteTagsEditor() => InitializeComponent();

   /// <summary>Given by the editors it's part of, which bind its data context to their <see cref="NoteTagsEditorViewModel"/>.</summary>
   NoteTagsEditorViewModel ViewModel => DataContext as NoteTagsEditorViewModel
                                     ?? throw new InvalidOperationException("The tags editor is shown without its NoteTagsEditorViewModel.");

   Window OwnerWindow => TopLevel.GetTopLevel(this) as Window ?? throw new InvalidOperationException("The tags editor is only shown in a window.");

   void OnAddATagClicked(object? sender, RoutedEventArgs e) =>
      ViewModel.AddATag(dialog => TagPickerDialog.ShowAsync(OwnerWindow, dialog, AddATagButton.PointToScreen(new Point(0, AddATagButton.Bounds.Height))));
}
