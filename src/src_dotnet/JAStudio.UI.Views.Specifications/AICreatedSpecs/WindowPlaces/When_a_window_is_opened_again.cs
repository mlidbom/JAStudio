using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.UI.Views.Importing;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.WindowPlaces;

/// <summary>
/// The import profiles window, opened as from the workspace's main menu, left somewhere by closing it, then opened again.<br/>
/// The headless platform has one screen, 1920×1280 pixels.
/// </summary>
public class When_a_window_is_opened_again : SpecificationOpeningWindows, IAIGeneratedSpec
{
   /// <summary>Where Windows puts a window it minimizes: its place for minimized windows, off every screen.</summary>
   static readonly PixelPoint WindowsPlaceForMinimizedWindows = new(-32000, -32000);

   /// <summary>Where Windows puts a window it maximizes on the headless platform's screen: the screen's corner, less the window's 8-pixel border.</summary>
   static readonly PixelPoint WhereWindowsPutsAWindowItMaximizes = new(-8, -8);

   /// <summary>Opens the window, puts it at <paramref name="topLeft"/>, made 900×500, and returns it.</summary>
   private protected ImportProfilesWindow OpenItAt(PixelPoint topLeft)
   {
      var window = OnTheUIThread(() => ImportProfilesWindow.Open(Services));
      OnTheUIThread(() =>
      {
         window.Position = topLeft;
         window.Width = 900;
         window.Height = 500;
         Dispatcher.UIThread.RunJobs();
      });
      return window;
   }

   /// <summary>
   /// Minimizes <paramref name="window"/> as Windows does: it moves the window to <see cref="WindowsPlaceForMinimizedWindows"/>, and tells the window<br/>
   /// it moved before it tells it it's minimized.
   /// </summary>
   private protected static void MinimizeItAsWindowsDoes(Window window) => OnTheUIThread(() =>
   {
      window.Position = WindowsPlaceForMinimizedWindows;
      window.WindowState = WindowState.Minimized;
      Dispatcher.UIThread.RunJobs();
   });

   /// <summary>
   /// Maximizes <paramref name="window"/> as Windows does: it moves the window to <see cref="WhereWindowsPutsAWindowItMaximizes"/>, and tells the window<br/>
   /// it moved before it tells it it's maximized.
   /// </summary>
   private protected static void MaximizeItAsWindowsDoes(Window window) => OnTheUIThread(() =>
   {
      window.Position = WhereWindowsPutsAWindowItMaximizes;
      window.WindowState = WindowState.Maximized;
      Dispatcher.UIThread.RunJobs();
   });

   /// <summary>Closes <paramref name="window"/> where it is, and returns it opened again.</summary>
   private protected ImportProfilesWindow CloseItThenOpenItAgain(Window window)
   {
      OnTheUIThread(window.Close);
      return Open(() => ImportProfilesWindow.Open(Services));
   }

   public class after_being_left_on_the_screen : When_a_window_is_opened_again
   {
      readonly ImportProfilesWindow _openedAgain;

      public after_being_left_on_the_screen() => _openedAgain = CloseItThenOpenItAgain(OpenItAt(new PixelPoint(300, 200)));

      [XF] public void it_opens_where_it_was_left() => OnTheUIThread(() => _openedAgain.Position).Must().Be(new PixelPoint(300, 200));
      [XF] public void it_has_the_size_it_was_left_with() => OnTheUIThread(() => _openedAgain.ClientSize).Must().Be(new Size(900, 500));
   }

   public class after_being_left_minimized : When_a_window_is_opened_again
   {
      readonly ImportProfilesWindow _openedAgain;

      public after_being_left_minimized()
      {
         var window = OpenItAt(new PixelPoint(300, 200));
         MinimizeItAsWindowsDoes(window);
         _openedAgain = CloseItThenOpenItAgain(window);
      }

      [XF] public void it_opens_where_it_was_before_it_was_minimized() => OnTheUIThread(() => _openedAgain.Position).Must().Be(new PixelPoint(300, 200));
      [XF] public void it_has_the_size_it_had_before_it_was_minimized() => OnTheUIThread(() => _openedAgain.ClientSize).Must().Be(new Size(900, 500));
   }

   public class after_being_left_maximized : When_a_window_is_opened_again
   {
      readonly ImportProfilesWindow _openedAgain;

      public after_being_left_maximized()
      {
         var window = OpenItAt(new PixelPoint(300, 200));
         MaximizeItAsWindowsDoes(window);
         _openedAgain = CloseItThenOpenItAgain(window);
      }

      [XF] public void it_opens_maximized() => OnTheUIThread(() => _openedAgain.WindowState).Must().Be(WindowState.Maximized);

      public class and_is_then_no_longer_maximized : after_being_left_maximized
      {
         public and_is_then_no_longer_maximized() => OnTheUIThread(() => _openedAgain.WindowState = WindowState.Normal);

         [XF] public void it_is_where_it_was_before_it_was_maximized() => OnTheUIThread(() => _openedAgain.Position).Must().Be(new PixelPoint(300, 200));
      }
   }

   public class after_being_left_on_a_screen_that_is_no_longer_connected : When_a_window_is_opened_again
   {
      readonly ImportProfilesWindow _openedAgain;

      public after_being_left_on_a_screen_that_is_no_longer_connected() => _openedAgain = CloseItThenOpenItAgain(OpenItAt(new PixelPoint(5000, 200)));

      [XF] public void it_opens_on_the_connected_screen() => OnTheUIThread(() => _openedAgain.Screens.ScreenFromPoint(_openedAgain.Position)).Must().NotBeNull();
   }
}
