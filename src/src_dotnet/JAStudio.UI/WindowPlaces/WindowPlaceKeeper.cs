using Avalonia.Controls;
using JAStudio.Core;

namespace JAStudio.UI.WindowPlaces;

/// <summary>
/// Keeps one window's place: before the window is first shown, puts it where it was last left, as <see cref="RememberedWindowPlaces"/><br/>
/// remembers it, or else centres it on the screen. Then follows where it is while it's shown, and remembers that whenever it's left:<br/>
/// when another window is activated, or it's closed.
/// </summary>
/// <remarks>
/// It follows the place as the window moves, is resized, maximized or snapped, rather than reading it when the window is left: a window<br/>
/// being hidden or closed no longer tells. Avalonia then reports the state code last set, not a maximize the user made.
/// </remarks>
/// <remarks>
/// A remembered place that's on no connected screen is ignored. A minimized window's place says nothing about where to open it,<br/>
/// and a maximized window keeps the place it had before, to have it again when it's no longer maximized.
/// </remarks>
sealed class WindowPlaceKeeper
{
   readonly Window _window;
   readonly RememberedWindowPlaces _places;
   readonly string _windowName;

   /// <summary>Where the window is, as followed while it's shown, or null before it's been shown or placed.</summary>
   WindowPlace? _place;

   WindowPlaceKeeper(Window window, RememberedWindowPlaces places)
   {
      _window = window;
      _places = places;
      _windowName = window.GetType().Name;

      if(_places.Of(_windowName) is {} remembered && remembered.IsOnOneOf(window.Screens))
      {
         window.WindowStartupLocation = WindowStartupLocation.Manual;
         PutItIn(remembered);
      }
      else
         window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

      window.Opened += (_, _) => FollowThePlace();
      window.PositionChanged += (_, _) => FollowThePlace();
      window.PropertyChanged += (_, changed) =>
      {
         if(changed.Property == TopLevel.ClientSizeProperty || changed.Property == Window.WindowStateProperty) FollowThePlace();
      };
      window.Deactivated += (_, _) => RememberThePlace();
      window.Closing += (_, _) => RememberThePlace();
   }

   /// <summary>Keeps <paramref name="window"/>'s place from now on, which must be before it's first shown.</summary>
   internal static WindowPlaceKeeper KeepThePlaceOf(Window window, TemporaryServiceCollection services) =>
      new(window, new RememberedWindowPlaces(services.Resolve<IEnvironmentPaths>()));

   /// <summary>Shows the hidden window again where it was when it was hidden, maximized or snapped included.</summary>
   /// <remarks>
   /// Avalonia shows a hidden window in the state code last set, and Windows then undoes a maximize or a snap the user made.<br/>
   /// So a maximized window is maximized again before it's shown, and any other is moved back after: showing it moves it.
   /// </remarks>
   internal void ShowItWhereItWas()
   {
      var placeWhenHidden = _place;
      if(placeWhenHidden is { IsMaximized: true }) _window.WindowState = WindowState.Maximized;
      _window.Show();
      if(placeWhenHidden is { IsMaximized: false }) PutItIn(placeWhenHidden);
   }

   void PutItIn(WindowPlace place)
   {
      _window.Position = place.TopLeft;
      _window.Width = place.ClientSize.Width;
      _window.Height = place.ClientSize.Height;
      if(place.IsMaximized) _window.WindowState = WindowState.Maximized;
      _place = place;
   }

   void FollowThePlace() =>
      _place = _window.WindowState switch
      {
         WindowState.Normal => WhereItIs(),
         WindowState.Maximized => (_place ?? WhereItIs()) with { IsMaximized = true },
         _ => _place // Minimized or full screen: says nothing about where to open it.
      };

   WindowPlace WhereItIs() => new(_window.Position, _window.ClientSize, IsMaximized: false);

   void RememberThePlace()
   {
      if(_place is {} place) _places.Remember(_windowName, place);
   }
}
