using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.WindowPlaces;

/// <summary>
/// What Windows does to a window when the user minimizes, maximizes or snaps it, done to a window on the headless platform, which does none of it itself.<br/>
/// Each is called on the UI thread, and lets the UI thread run what it was given to do, as it does before the user's next action.
/// </summary>
/// <remarks>
/// Windows tells a window it moved before it tells it it's minimized or maximized, so each moves the window first, then puts it in its new state,<br/>
/// in the same turn of the UI thread.
/// </remarks>
static class AsWindowsDoes
{
   /// <summary>Where Windows puts a window it minimizes: its place for minimized windows, off every screen.</summary>
   static readonly PixelPoint ThePlaceForMinimizedWindows = new(-32000, -32000);

   /// <summary>Where Windows puts a window it maximizes on the headless platform's screen: the screen's corner, less the window's 8-pixel border.</summary>
   static readonly PixelPoint WhereAMaximizedWindowIs = new(-8, -8);

   /// <summary>Where Windows puts a window snapped to the right half of the headless platform's screen, 1920×1280 pixels: in its normal state.</summary>
   internal static readonly WindowPlace SnappedToTheRightHalf = new(new PixelPoint(960, 0), new Size(960, 1240), IsMaximized: false);

   /// <summary>Minimizes <paramref name="window"/>: moves it to <see cref="ThePlaceForMinimizedWindows"/>, then minimizes it.</summary>
   internal static void Minimize(Window window)
   {
      window.Position = ThePlaceForMinimizedWindows;
      window.WindowState = WindowState.Minimized;
      Dispatcher.UIThread.RunJobs();
   }

   /// <summary>Maximizes <paramref name="window"/>: moves it to <see cref="WhereAMaximizedWindowIs"/>, then maximizes it.</summary>
   internal static void Maximize(Window window)
   {
      window.Position = WhereAMaximizedWindowIs;
      window.WindowState = WindowState.Maximized;
      Dispatcher.UIThread.RunJobs();
   }

   /// <summary>Snaps <paramref name="window"/> to the right half of the screen: puts it, in its normal state, in <see cref="SnappedToTheRightHalf"/>.</summary>
   internal static void SnapToTheRightHalf(Window window)
   {
      window.Position = SnappedToTheRightHalf.TopLeft;
      window.Width = SnappedToTheRightHalf.ClientSize.Width;
      window.Height = SnappedToTheRightHalf.ClientSize.Height;
      Dispatcher.UIThread.RunJobs();
   }
}
