using System;
using Avalonia.Input;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>The mouse's buttons, by the names the screen-tools take them by: <c>left</c>, <c>right</c> and <c>middle</c>.</summary>
public static class MouseButtons
{
   /// <summary>The mouse button <paramref name="button"/> names.</summary>
   public static MouseButton Named(string button) => button switch
   {
      "left"   => MouseButton.Left,
      "right"  => MouseButton.Right,
      "middle" => MouseButton.Middle,
      _        => throw new ArgumentException($"""button is "left", "right" or "middle", not "{button}".""", nameof(button))
   };
}
