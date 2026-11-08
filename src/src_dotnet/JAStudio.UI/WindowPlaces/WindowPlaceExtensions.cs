using Avalonia.Controls;
using JAStudio.Core;

namespace JAStudio.UI.WindowPlaces;

/// <summary>
/// Opens JAStudio's windows where they were last left, as <see cref="RememberedWindowPlaces"/> remembers it, and keeps that up to date.<br/>
/// A window opened for the first time is centred on the screen, by Avalonia. It works on every platform, as opening at the mouse cursor didn't:<br/>
/// see <c>dev_docs/multiplatform.md</c>.
/// </summary>
static class WindowPlaceExtensions
{
   extension<TWindow>(TWindow window) where TWindow : Window
   {
      /// <summary>Shows the window as <see cref="PlacedWhereItWasLast"/> places it, and returns it.</summary>
      internal TWindow ShowWhereItWasLast(TemporaryServiceCollection services)
      {
         window.PlacedWhereItWasLast(services).Show();
         return window;
      }

      /// <summary>
      /// Gives the window, before it's first shown, the place it was last left in, or else centres it on the screen,<br/>
      /// and remembers its place whenever it's left: when another window is activated, or it's closed. Returns the window.
      /// </summary>
      /// <remarks>
      /// A remembered place that's on no connected screen is ignored. A minimized window's place says nothing about where to open it,<br/>
      /// and a maximized window keeps the place it had before, to have it again when it's no longer maximized.
      /// </remarks>
      internal TWindow PlacedWhereItWasLast(TemporaryServiceCollection services)
      {
         var places = new RememberedWindowPlaces(services.Resolve<IEnvironmentPaths>());
         var windowName = window.GetType().Name;
         if(places.Of(windowName) is {} place && place.IsOnOneOf(window.Screens))
            OpenIn(place);
         else
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

         window.Deactivated += (_, _) => RememberWhereItIs();
         window.Closing += (_, _) => RememberWhereItIs();
         return window;

         void OpenIn(WindowPlace placeToOpenIn)
         {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Position = placeToOpenIn.TopLeft;
            window.Width = placeToOpenIn.ClientSize.Width;
            window.Height = placeToOpenIn.ClientSize.Height;
            if(placeToOpenIn.IsMaximized) window.WindowState = WindowState.Maximized;
         }

         void RememberWhereItIs()
         {
            switch(window.WindowState)
            {
               case WindowState.Normal:
                  places.Remember(windowName, WhereItIs());
                  break;
               case WindowState.Maximized:
                  places.Remember(windowName, (places.Of(windowName) ?? WhereItIs()) with { IsMaximized = true });
                  break;
            }
         }

         WindowPlace WhereItIs() => new(window.Position, window.ClientSize, IsMaximized: false);
      }
   }
}
