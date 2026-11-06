using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace JAStudio.UI.Controls;

/// <summary>
/// The draggable edge of a pane docked at one side of a window, such as the workspace's note search pane:<br/>
/// dragging it changes the <see cref="ResizedPane"/>'s width.
/// </summary>
/// <remarks>
/// It's a <see cref="Thumb"/> with a template of its own, because a <see cref="Thumb"/> has no look unless a theme gives it one,<br/>
/// and a control that draws nothing can't be pointed at.
/// </remarks>
class PaneResizeEdge : Thumb
{
   /// <summary>The pane whose width the edge changes. Its <see cref="Layoutable.MinWidth"/> is respected.</summary>
   public static readonly StyledProperty<Control?> ResizedPaneProperty = AvaloniaProperty.Register<PaneResizeEdge, Control?>(nameof(ResizedPane));

   /// <summary>Which side of the window the <see cref="ResizedPane"/> is docked at, so which way of dragging widens it.</summary>
   public static readonly StyledProperty<PaneSide> ResizedPaneSideProperty = AvaloniaProperty.Register<PaneResizeEdge, PaneSide>(nameof(ResizedPaneSide));

   public PaneResizeEdge()
   {
      Width = 5;
      Cursor = new Cursor(StandardCursorType.SizeWestEast);
      Background = Brushes.LightGray;
      Template = new FuncControlTemplate<PaneResizeEdge>((edge, _) => new Border { [!Border.BackgroundProperty] = edge[!BackgroundProperty] });
      DragDelta += ResizePane;
   }

   public Control? ResizedPane
   {
      get => GetValue(ResizedPaneProperty);
      set => SetValue(ResizedPaneProperty, value);
   }

   public PaneSide ResizedPaneSide
   {
      get => GetValue(ResizedPaneSideProperty);
      set => SetValue(ResizedPaneSideProperty, value);
   }

   void ResizePane(object? sender, VectorEventArgs e)
   {
      var pane = ResizedPane ?? throw new InvalidOperationException("A pane resize edge needs its ResizedPane.");
      var widening = ResizedPaneSide == PaneSide.Left ? e.Vector.X : -e.Vector.X;
      pane.Width = Math.Max(pane.MinWidth, pane.Width + widening);
   }
}
