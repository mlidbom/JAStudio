using System;
using Avalonia;
using Avalonia.Controls;

namespace JAStudio.UI.Controls;

/// <summary>
/// A panel of two children: the first, and the second beside it, in the width left, as long as that's at least <see cref="MinimumWidthBeside"/>; on a<br/>
/// narrower panel the second goes below the first, at the panel's whole width. As a sentence breakdown's word and its answer: the answer beside the<br/>
/// word on a wide page, and below it on a narrow one, rather than squeezed into a sliver beside it.
/// </summary>
public class BesideOrBelowPanel : Panel
{
   public static readonly StyledProperty<double> MinimumWidthBesideProperty = AvaloniaProperty.Register<BesideOrBelowPanel, double>(nameof(MinimumWidthBeside), 250);

   static BesideOrBelowPanel() => AffectsMeasure<BesideOrBelowPanel>(MinimumWidthBesideProperty);

   /// <summary>The least width the second child is shown beside the first in, in pixels.</summary>
   public double MinimumWidthBeside
   {
      get => GetValue(MinimumWidthBesideProperty);
      set => SetValue(MinimumWidthBesideProperty, value);
   }

   protected override Size MeasureOverride(Size availableSize)
   {
      var (first, second) = TheTwoChildren();
      first.Measure(availableSize);
      var widthLeft = availableSize.Width - first.DesiredSize.Width;
      if(FitsBeside(widthLeft))
      {
         second.Measure(availableSize.WithWidth(widthLeft));
         return new Size(first.DesiredSize.Width + second.DesiredSize.Width, Math.Max(first.DesiredSize.Height, second.DesiredSize.Height));
      }

      second.Measure(availableSize);
      return new Size(Math.Max(first.DesiredSize.Width, second.DesiredSize.Width), first.DesiredSize.Height + second.DesiredSize.Height);
   }

   protected override Size ArrangeOverride(Size finalSize)
   {
      var (first, second) = TheTwoChildren();
      var widthLeft = finalSize.Width - first.DesiredSize.Width;
      if(FitsBeside(widthLeft))
      {
         first.Arrange(new Rect(0, 0, first.DesiredSize.Width, finalSize.Height));
         second.Arrange(new Rect(first.DesiredSize.Width, 0, widthLeft, finalSize.Height));
      }
      else
      {
         first.Arrange(new Rect(0, 0, finalSize.Width, first.DesiredSize.Height));
         second.Arrange(new Rect(0, first.DesiredSize.Height, finalSize.Width, Math.Max(0, finalSize.Height - first.DesiredSize.Height)));
      }

      return finalSize;
   }

   bool FitsBeside(double widthLeft) => double.IsPositiveInfinity(widthLeft) || widthLeft >= MinimumWidthBeside;

   (Control First, Control Second) TheTwoChildren() =>
      Children.Count == 2 ? (Children[0], Children[1]) : throw new InvalidOperationException($"A {nameof(BesideOrBelowPanel)} has two children, not {Children.Count}.");
}
