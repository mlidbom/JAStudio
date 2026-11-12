using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;

namespace JAStudio.UI.Views.Workspace.Editors.Matching;

/// <summary>
/// The small modal dialog that renames a shared-condition, showing a <see cref="SharedConditionRenameDialogViewModel"/>. It opens with the keyboard in the<br/>
/// box for the new name. Enter or Rename closes it with the new name, once it can be used; Esc or Cancel closes it without one.
/// </summary>
partial class SharedConditionRenameDialog : Window
{
   readonly SharedConditionRenameDialogViewModel _dialog;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public SharedConditionRenameDialog() => InitializeComponent();
#pragma warning restore CS8618

   SharedConditionRenameDialog(SharedConditionRenameDialogViewModel dialog)
   {
      _dialog = dialog;
      dialog.AcceptCommand = new RelayCommand(Accept);
      dialog.CancelCommand = new RelayCommand(() => Close(null));
      DataContext = dialog;
      InitializeComponent();
      Opened += (_, _) => Dispatcher.UIThread.Post(() => DialogInitialFocus.PutTheKeyboardInTheMainInputOf(this), DispatcherPriority.Loaded);
   }

   /// <summary>
   /// Shows <paramref name="dialog"/> modally over <paramref name="owner"/>, with its top left corner at <paramref name="topLeft"/>, and returns the new name it<br/>
   /// closed with, or null when it was cancelled.
   /// </summary>
   internal static Task<string?> ShowAsync(Window owner, SharedConditionRenameDialogViewModel dialog, PixelPoint topLeft) =>
      new SharedConditionRenameDialog(dialog) { Position = topLeft }.ShowDialog<string?>(owner);

   void Accept()
   {
      if(_dialog.CanRename)
         Close(_dialog.TypedName);
   }
}
