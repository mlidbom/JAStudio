using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.Dialogs;

/// <summary>
/// A scope-level panel bound to a <see cref="JAStudio.Core.TaskRunners.TaskProgressScopeViewModel"/>.
/// Displays a heading with elapsed time and hosts child progress panels
/// via an <c>ItemsControl</c> bound to the view model's <c>Children</c> collection.
/// Nested scopes are structurally nested via recursive <c>DataTemplate</c>s, so
/// indentation comes from the tree structure itself. Only background color varies by depth.
/// </summary>
partial class TaskProgressScopePanel : UserControl
{
   static readonly string[] DepthBackgrounds = ["#18607890", "#14508070", "#10406860", "#0C305050"];

   /// <summary>
   /// Parameterless constructor used by DataTemplate instantiation.
   /// Reads depth from the DataContext once it is set.
   /// </summary>
   public TaskProgressScopePanel()
   {
      InitializeComponent();
      DataContextChanged += (_, _) =>
      {
         if(DataContext is TaskProgressScopeViewModel vm)
            ApplyDepthStyling(vm.Depth);
      };
   }

   void InitializeComponent()
   {
      AvaloniaXamlLoader.Load(this);
   }

   void ApplyDepthStyling(int depth)
   {
      var border = this.FindControl<Border>("ScopeBorder");
      if(border != null)
      {
         var bgIndex = Math.Min(depth - 1, DepthBackgrounds.Length - 1);
         border.Background = Avalonia.Media.Brush.Parse(DepthBackgrounds[Math.Max(0, bgIndex)]);
      }
   }
}
