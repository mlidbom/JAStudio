using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Tagging;

namespace JAStudio.UI.Views.Workspace.Tagging;

/// <summary>
/// The small modal dialog that picks a tag to add to some notes or remove from them, showing a <see cref="TagPickerDialogViewModel"/>.<br/>
/// It opens with the keyboard in the box for typing parts of the tag's path. Enter, OK or double-clicking a tag closes it with the picked tag,<br/>
/// and Esc or Cancel closes it without one.
/// </summary>
partial class TagPickerDialog : Window
{
   readonly TagPickerDialogViewModel _dialog;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public TagPickerDialog() => InitializeComponent();
#pragma warning restore CS8618

   TagPickerDialog(TagPickerDialogViewModel dialog)
   {
      _dialog = dialog;
      dialog.AcceptCommand = new RelayCommand(Accept);
      dialog.CancelCommand = new RelayCommand(() => Close(null));
      DataContext = dialog;
      InitializeComponent();
      Opened += (_, _) => Dispatcher.UIThread.Post(() => DialogInitialFocus.PutTheKeyboardInTheMainInputOf(this), DispatcherPriority.Loaded);
      AddHandler(DoubleTappedEvent, OnDoubleTapped);
   }

   /// <summary>
   /// Shows <paramref name="dialog"/> modally over <paramref name="owner"/>, with its top left corner at <paramref name="topLeft"/>,<br/>
   /// and returns the tag path it closed with, or null when it was cancelled.
   /// </summary>
   internal static Task<string?> ShowAsync(Window owner, TagPickerDialogViewModel dialog, PixelPoint topLeft) =>
      new TagPickerDialog(dialog) { Position = topLeft }.ShowDialog<string?>(owner);

   /// <summary>Shows <paramref name="dialog"/> as <see cref="ShowAsync"/> does, centered over <paramref name="owner"/>, as for a key pressed with no place on screen to open at.</summary>
   internal static Task<string?> ShowCenteredOverAsync(Window owner, TagPickerDialogViewModel dialog) =>
      new TagPickerDialog(dialog) { WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog<string?>(owner);

   void Accept()
   {
      if(_dialog.PickedTagPath is {} tagPath)
         Close(tagPath);
   }

   void OnDoubleTapped(object? sender, TappedEventArgs e)
   {
      if((e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true) is not null)
         Accept();
   }
}
