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
/// The small modal dialog that makes a shared-condition of a line of the tree editor, showing a <see cref="NewSharedConditionDialogViewModel"/>. It opens<br/>
/// with the keyboard in the box for the name. Enter or Make closes it with the shared-condition to make, once its name can be used; Esc or Cancel closes<br/>
/// it without one.
/// </summary>
partial class NewSharedConditionDialog : Window
{
   readonly NewSharedConditionDialogViewModel _dialog;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public NewSharedConditionDialog() => InitializeComponent();
#pragma warning restore CS8618

   NewSharedConditionDialog(NewSharedConditionDialogViewModel dialog)
   {
      _dialog = dialog;
      dialog.AcceptCommand = new RelayCommand(Accept);
      dialog.CancelCommand = new RelayCommand(() => Close(null));
      DataContext = dialog;
      InitializeComponent();
      Opened += (_, _) => Dispatcher.UIThread.Post(() => DialogInitialFocus.PutTheKeyboardInTheMainInputOf(this), DispatcherPriority.Loaded);
   }

   /// <summary>
   /// Shows <paramref name="dialog"/> modally over <paramref name="owner"/>, with its top left corner at <paramref name="topLeft"/>, and returns the<br/>
   /// shared-condition it closed with, or null when it was cancelled.
   /// </summary>
   internal static Task<NewSharedCondition?> ShowAsync(Window owner, NewSharedConditionDialogViewModel dialog, PixelPoint topLeft) =>
      new NewSharedConditionDialog(dialog) { Position = topLeft }.ShowDialog<NewSharedCondition?>(owner);

   void Accept()
   {
      if(_dialog.CanMake)
         Close(_dialog.ToMake);
   }
}
