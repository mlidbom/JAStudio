using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;

namespace JAStudio.UI.Views.Workspace.Navigation.TagManagement;

/// <summary>
/// The small modal dialog that renames a branch of the tag tree, showing a <see cref="TagRenameDialogViewModel"/>.<br/>
/// It opens with the keyboard in the box for the new name. Enter or Rename closes it with the rename, unless the rename merges tags:<br/>
/// then only the Merge button does. Esc or Cancel closes it without one.
/// </summary>
partial class TagRenameDialog : Window
{
   readonly TagRenameDialogViewModel _dialog;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public TagRenameDialog() => InitializeComponent();
#pragma warning restore CS8618

   TagRenameDialog(TagRenameDialogViewModel dialog)
   {
      _dialog = dialog;
      dialog.AcceptCommand = new RelayCommand(Accept);
      dialog.CancelCommand = new RelayCommand(() => Close(null));
      DataContext = dialog;
      InitializeComponent();
      Opened += (_, _) => Dispatcher.UIThread.Post(() => DialogInitialFocus.PutTheKeyboardInTheMainInputOf(this), DispatcherPriority.Loaded);
   }

   /// <summary>
   /// Shows <paramref name="dialog"/> modally over <paramref name="owner"/>, with its top left corner at <paramref name="topLeft"/>,<br/>
   /// and returns the rename it closed with, or null when it was cancelled.
   /// </summary>
   internal static Task<TagBranchRename?> ShowAsync(Window owner, TagRenameDialogViewModel dialog, PixelPoint topLeft) =>
      new TagRenameDialog(dialog) { Position = topLeft }.ShowDialog<TagBranchRename?>(owner);

   void Accept()
   {
      if(_dialog.CanAccept)
         Close(_dialog.Rename);
   }
}
