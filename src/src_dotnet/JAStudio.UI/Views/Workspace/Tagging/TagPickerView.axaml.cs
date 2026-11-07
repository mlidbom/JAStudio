using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JAStudio.UI.ViewModels.Workspace.Tagging;

namespace JAStudio.UI.Views.Workspace.Tagging;

/// <summary>
/// The view of a <see cref="TagPickerViewModel"/>: a box for typing parts of a tag's path, and the matching tag paths to pick from.<br/>
/// The keyboard stays in the box: Up and Down pick the previous or next tag path in the list.
/// </summary>
partial class TagPickerView : UserControl
{
   public TagPickerView()
   {
      InitializeComponent();
      TypedPartsBox.AddHandler(KeyDownEvent, OnKeyDownInTheBox, RoutingStrategies.Tunnel);
   }

   void OnKeyDownInTheBox(object? sender, KeyEventArgs e)
   {
      var step = e.Key switch
      {
         Key.Down => 1,
         Key.Up => -1,
         _ => 0
      };
      if(step == 0 || ChoicesList.ItemCount == 0) return;

      ChoicesList.SelectedIndex = Math.Clamp(ChoicesList.SelectedIndex + step, 0, ChoicesList.ItemCount - 1);
      ChoicesList.ScrollIntoView(ChoicesList.SelectedIndex);
      e.Handled = true;
   }
}
