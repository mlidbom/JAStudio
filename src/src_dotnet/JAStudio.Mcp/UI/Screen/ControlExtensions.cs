using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>What the screen-tools need to know of where a control is.</summary>
static class ControlExtensions
{
   extension(Control @this)
   {
      /// <summary>
      /// The middle of the part of this control in sight, in its own coordinates: where the screen-tools click it and turn the mouse wheel over it,<br/>
      /// and what has to be in sight for it to be. For a control larger than what shows it, such as a long page in a scrolled view, that's the middle<br/>
      /// of the part shown, as a person would point at it; for a control wholly out of sight, its middle.
      /// </summary>
      internal Point Middle
      {
         get
         {
            var partInSight = @this.GetVisualAncestors()
                                   .Where(it => it.ClipToBounds || it is TopLevel)
                                   .Aggregate(new Rect(@this.Bounds.Size), (inSight, clipping) => clipping.TranslatePoint(default, @this) is {} clippingsTopLeft
                                                                                                    ? inSight.Intersect(new Rect(clippingsTopLeft, clipping.Bounds.Size))
                                                                                                    : inSight);
            return partInSight is { Width: > 0, Height: > 0 } ? partInSight.Center : new Point(@this.Bounds.Width / 2, @this.Bounds.Height / 2);
         }
      }
   }
}
