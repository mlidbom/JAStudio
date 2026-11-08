using Avalonia.Controls;
using JAStudio.Core;

namespace JAStudio.UI.WindowPlaces;

/// <summary>
/// Opens JAStudio's windows where they were last left, through a <see cref="WindowPlaceKeeper"/>. A window opened for the first time is centred<br/>
/// on the screen. It works on every platform, as opening at the mouse cursor didn't: see <c>dev_docs/multiplatform.md</c>.
/// </summary>
static class WindowPlaceExtensions
{
   extension<TWindow>(TWindow window) where TWindow : Window
   {
      /// <summary>Shows the window where it was last left, keeps its place from now on, and returns it.</summary>
      internal TWindow ShowWhereItWasLast(TemporaryServiceCollection services)
      {
         WindowPlaceKeeper.KeepThePlaceOf(window, services);
         window.Show();
         return window;
      }
   }
}
