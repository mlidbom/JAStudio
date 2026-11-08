using Avalonia;
using Avalonia.Controls;

namespace JAStudio.UI.WindowPlaces;

/// <summary>
/// Where a window was on the screen and how big, and whether it was maximized: the place it opens in again,<br/>
/// as <see cref="RememberedWindowPlaces"/> remembers it.
/// </summary>
/// <param name="TopLeft">The window's top-left corner on the screen, in the screen's own pixels.</param>
/// <param name="ClientSize">The size of the window's content, in Avalonia's device-independent units.</param>
/// <param name="IsMaximized">Whether it was maximized. <paramref name="TopLeft"/> and <paramref name="ClientSize"/> are then where it was before.</param>
record WindowPlace(PixelPoint TopLeft, Size ClientSize, bool IsMaximized)
{
   /// <summary>How far into the window's title bar a point must be on a screen for the window to be grabbed there.</summary>
   static readonly PixelVector IntoTheTitleBar = new(100, 10);

   /// <summary>
   /// Whether the left end of the window's title bar is on one of <paramref name="screens"/>, where it can be grabbed.<br/>
   /// It isn't when the place is on a monitor that's no longer connected.
   /// </summary>
   public bool IsOnOneOf(Screens screens) => screens.ScreenFromPoint(TopLeft + IntoTheTitleBar) != null;
}
