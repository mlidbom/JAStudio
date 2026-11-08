using Avalonia;
using Avalonia.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.UI.Views.Importing;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.WindowPlaces;

/// <summary>
/// The import profiles window, opened as from the workspace's main menu, moved, made 900×500 and closed, then opened again.<br/>
/// The headless platform has one screen, 1920×1280 pixels.
/// </summary>
public class When_a_window_is_opened_again : SpecificationOpeningWindows, IAIGeneratedSpec
{
   /// <summary>Opens the window, leaves it at <paramref name="topLeft"/>, made 900×500, and returns it opened again.</summary>
   private protected ImportProfilesWindow LeaveItAtThenOpenItAgain(PixelPoint topLeft)
   {
      var window = OnTheUIThread(() => ImportProfilesWindow.Open(Services));
      OnTheUIThread(() =>
      {
         window.Position = topLeft;
         window.Width = 900;
         window.Height = 500;
         Dispatcher.UIThread.RunJobs();
         window.Close();
      });
      return Open(() => ImportProfilesWindow.Open(Services));
   }

   public class after_being_left_on_the_screen : When_a_window_is_opened_again
   {
      readonly ImportProfilesWindow _openedAgain;

      public after_being_left_on_the_screen() => _openedAgain = LeaveItAtThenOpenItAgain(new PixelPoint(300, 200));

      [XF] public void it_opens_where_it_was_left() => OnTheUIThread(() => _openedAgain.Position).Must().Be(new PixelPoint(300, 200));
      [XF] public void it_has_the_size_it_was_left_with() => OnTheUIThread(() => _openedAgain.ClientSize).Must().Be(new Size(900, 500));
   }

   public class after_being_left_on_a_screen_that_is_no_longer_connected : When_a_window_is_opened_again
   {
      readonly ImportProfilesWindow _openedAgain;

      public after_being_left_on_a_screen_that_is_no_longer_connected() => _openedAgain = LeaveItAtThenOpenItAgain(new PixelPoint(5000, 200));

      [XF] public void it_opens_on_the_connected_screen() => OnTheUIThread(() => _openedAgain.Screens.ScreenFromPoint(_openedAgain.Position)).Must().NotBeNull();
   }
}
