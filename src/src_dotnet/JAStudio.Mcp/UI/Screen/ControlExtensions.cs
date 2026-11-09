using Avalonia;
using Avalonia.Controls;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>What the screen-tools need to know of where a control is.</summary>
static class ControlExtensions
{
   extension(Control @this)
   {
      /// <summary>The middle of this control, in its own coordinates: where the screen-tools click it, and what has to be in sight for it to be.</summary>
      internal Point Middle => new(@this.Bounds.Width / 2, @this.Bounds.Height / 2);
   }
}
