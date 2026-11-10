using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Tagging;

namespace JAStudio.UI.Views.Workspace.Tagging;

/// <summary>
/// The view of a <see cref="NoteTagsEditorViewModel"/>: a note's tags, each with a × that removes it, those JAStudio maintains dimmed and without one,<br/>
/// and a button that adds a tag, picked in a <see cref="TagPickerDialog"/> that opens below the button.
/// </summary>
partial class NoteTagsEditor : UserControl
{
   /// <summary>The opacity of a tag's chip: half for a tag JAStudio maintains (<see cref="TagOnTheNoteViewModel.IsMaintainedByJAStudio"/>), so it reads as not editable.</summary>
   public static readonly IValueConverter OpacityOfAChip = new FuncValueConverter<bool, double>(isMaintainedByJAStudio => isMaintainedByJAStudio ? 0.5 : 1.0);

   public NoteTagsEditor() => InitializeComponent();

   /// <summary>Given by the editors it's part of, which bind its data context to their <see cref="NoteTagsEditorViewModel"/>.</summary>
   NoteTagsEditorViewModel ViewModel => DataContext as NoteTagsEditorViewModel
                                     ?? throw new InvalidOperationException("The tags editor is shown without its NoteTagsEditorViewModel.");

   Window OwnerWindow => TopLevel.GetTopLevel(this) as Window ?? throw new InvalidOperationException("The tags editor is only shown in a window.");

   void OnAddATagClicked(object? sender, RoutedEventArgs e) =>
      ViewModel.AddATag(dialog => TagPickerDialog.ShowAsync(OwnerWindow, dialog, AddATagButton.PointToScreen(new Point(0, AddATagButton.Bounds.Height))));

   /// <summary>A tag's ×, whose data context is the tag's <see cref="TagOnTheNoteViewModel"/>.</summary>
   void OnRemoveClicked(object? sender, RoutedEventArgs e) =>
      ViewModel.Remove((sender as Control)?.DataContext as TagOnTheNoteViewModel ?? throw new InvalidOperationException("A tag's × is clicked without its tag."),
                       ConfirmInADialog);

   Task<bool> ConfirmInADialog(string question) => ConfirmDialog.ShowOverAsync(OwnerWindow, "Remove a source tag", question);
}
