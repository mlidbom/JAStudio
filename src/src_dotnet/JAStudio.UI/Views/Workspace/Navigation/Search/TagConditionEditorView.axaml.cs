using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JAStudio.UI.ViewModels.Workspace.Navigation.Search;

namespace JAStudio.UI.Views.Workspace.Navigation.Search;

/// <summary>
/// The view of a <see cref="TagConditionEditorViewModel"/>: a box for typing parts of a tag's path, and the matching tag paths to pick from.<br/>
/// The keyboard stays in the box: Up and Down pick the previous or next tag path in the list.
/// </summary>
partial class TagConditionEditorView : UserControl
{
   public TagConditionEditorView()
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
      if(step == 0 || MatchingTagPathsList.ItemCount == 0) return;

      MatchingTagPathsList.SelectedIndex = Math.Clamp(MatchingTagPathsList.SelectedIndex + step, 0, MatchingTagPathsList.ItemCount - 1);
      MatchingTagPathsList.ScrollIntoView(MatchingTagPathsList.SelectedIndex);
      e.Handled = true;
   }
}
