using Avalonia;

namespace JAStudio.UI.WindowPlaces;

partial class RememberedWindowPlaces
{
   /// <summary>A <see cref="WindowPlace"/> as <c>window-places.json</c> holds it, in plain numbers.</summary>
   /// <param name="Left">The left edge of the window on the screen, in the screen's own pixels.</param>
   /// <param name="Top">The top edge of the window on the screen, in the screen's own pixels.</param>
   /// <param name="Width">The width of the window's content, in Avalonia's device-independent units.</param>
   /// <param name="Height">The height of the window's content, in Avalonia's device-independent units.</param>
   /// <param name="IsMaximized">Whether the window was maximized.</param>
   record SavedWindowPlace(int Left, int Top, double Width, double Height, bool IsMaximized)
   {
      public static SavedWindowPlace Of(WindowPlace place) =>
         new(place.TopLeft.X, place.TopLeft.Y, place.ClientSize.Width, place.ClientSize.Height, place.IsMaximized);

      public WindowPlace ToWindowPlace() => new(new PixelPoint(Left, Top), new Size(Width, Height), IsMaximized);
   }
}
