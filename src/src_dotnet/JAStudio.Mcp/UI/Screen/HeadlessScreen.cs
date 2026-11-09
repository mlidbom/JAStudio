using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using JAStudio.Core.UI;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>
/// The screen of the headless JAStudio: the windows it has open, and their popups, such as an open menu or a drop-down's list, where they are,<br/>
/// one over another, as a person would see them on a screen. The screen-tools (<see cref="JAStudio.Mcp.Tools.Screen.ScreenTools"/>) show it to Claude, and act on it.
/// </summary>
/// <remarks>
/// On Avalonia's headless platform each window and each popup is a <see cref="TopLevel"/> of its own, drawn on its own and placed in screen<br/>
/// coordinates, as on a real screen, but nothing draws them together, nor keeps track of which is in front. This class draws them together, and<br/>
/// its <see cref="WindowManager"/> keeps track of which window is in front.
/// </remarks>
/// <remarks>
/// Used on the UI thread only. Popups are in front of every window, in the order they opened: on a real screen a popup closes as its window loses<br/>
/// the keyboard, so that's where it would be.
/// </remarks>
public sealed class HeadlessScreen
{
   /// <summary>What the screen shows where no window is.</summary>
   static readonly IBrush NoWindowThere = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30));

   /// <summary>The outline drawn around each window, which the headless platform draws with no frame or title bar to tell one window from another.</summary>
   static readonly IPen WindowOutline = new Pen(Brushes.DimGray);

   /// <summary>Every popup open, in the order they opened.</summary>
   readonly List<Popup> _openPopups = [];

   HeadlessScreen(HeadlessWindowManager windowManager)
   {
      WindowManager = windowManager;
      Refs = new ControlRefs();
   }

   /// <summary>Which window is in front, which is active, and which has the keyboard, as a desktop's window manager would have it.</summary>
   public HeadlessWindowManager WindowManager { get; }

   /// <summary>The refs the screen-tools name its controls by.</summary>
   public ControlRefs Refs { get; }

   /// <summary>Starts following the windows and popups of the process's UI as they open, close and are activated. Called once, on the UI thread, before any window opens.</summary>
   internal static HeadlessScreen FollowingTheWindowsFromNowOn()
   {
      var screen = new HeadlessScreen(HeadlessWindowManager.ManagingTheWindowsFromNowOn());
      Popup.IsOpenProperty.Changed.AddClassHandler<Popup>((popup, change) =>
      {
         screen._openPopups.Remove(popup);
         if(!change.GetNewValue<bool>()) return;

         screen._openPopups.Add(popup);
         HeadlessWindowManager.KeepFromBeingActivated(popup);
      });
      return screen;
   }

   /// <summary>Everything the screen shows, windows and popups, each a <see cref="TopLevel"/>, from the one at the back to the one in front.</summary>
   public IReadOnlyList<TopLevel> TopLevelsFromTheBack => [.. WindowManager.WindowsFromTheBack, .. PopupsShown];

   /// <summary>Everything the screen shows, windows and popups, each a <see cref="TopLevel"/>, from the one in front to the one at the back.</summary>
   public IReadOnlyList<TopLevel> TopLevelsFromTheFront => [.. TopLevelsFromTheBack.Reverse()];

   /// <summary>Whether <paramref name="topLevel"/> is on the screen: a window shown, or a popup open.</summary>
   public bool Shows(TopLevel topLevel) => TopLevelsFromTheBack.Contains(topLevel);

   /// <summary>The window or popup on the screen that <paramref name="topLevelRef"/> names, or an exception saying it names none.</summary>
   public TopLevel TopLevelWithRef(ControlRef topLevelRef) =>
      Refs.ControlWithRef(topLevelRef) is TopLevel topLevel && Shows(topLevel)
         ? topLevel
         : throw new ArgumentException($"{topLevelRef} is no window or popup on the screen: render_the_screen lists them.", nameof(topLevelRef));

   /// <summary>Where on the screen <paramref name="topLevel"/> is, in the screen's pixels.</summary>
   public static PixelRect PlaceOf(TopLevel topLevel) => new(topLevel.PointToScreen(default), PixelSize.FromSize(topLevel.ClientSize, topLevel.RenderScaling));

   /// <summary>The window or popup in front at <paramref name="point"/>, which a click there would reach, or null where the screen shows nothing.</summary>
   public TopLevel? InFrontAt(PixelPoint point) => TopLevelsFromTheBack.LastOrDefault(it => PlaceOf(it).Contains(point));

   /// <summary>The part of the screen the windows and popups cover, in the screen's pixels, or null when nothing is open.</summary>
   public PixelRect? AreaShown => TopLevelsFromTheBack.Select(PlaceOf).Aggregate((PixelRect?)null, (area, place) => area?.Union(place) ?? place);

   /// <summary>
   /// The part of the screen the windows and popups cover, <paramref name="area"/>, as an image: each drawn where it is, the ones in front over<br/>
   /// the ones behind, each window with an outline.
   /// </summary>
   public PngImage Render(PixelRect area)
   {
      using var screen = new RenderTargetBitmap(area.Size);
      using(var drawing = screen.CreateDrawingContext())
      {
         drawing.FillRectangle(NoWindowThere, new Rect(area.Size.ToSize(1)));
         foreach(var topLevel in TopLevelsFromTheBack)
         {
            var placeInTheImage = PlaceOf(topLevel).Translate(new PixelVector(-area.X, -area.Y)).ToRect(1);
            using var frame = topLevel.CaptureRenderedFrame() ?? throw new InvalidOperationException($"{topLevel.GetType().Name} drew no frame.");
            drawing.DrawImage(frame, placeInTheImage);
            if(topLevel is Window) drawing.DrawRectangle(null, WindowOutline, placeInTheImage);
         }
      }
      return screen.ToPngImage();
   }

   /// <summary>Closes every window shown, and with them their popups: for an empty screen to start from.</summary>
   public void CloseEverything()
   {
      foreach(var window in WindowManager.WindowsFromTheFront)
         window.Close();
   }

   /// <summary>
   /// The popups open in top levels of their own, in the order they opened. A popup drawn in its window's overlay layer instead is part of the<br/>
   /// window's own image, and a popup whose content isn't laid out in a top level yet isn't shown yet.
   /// </summary>
   IEnumerable<TopLevel> PopupsShown =>
      _openPopups.Select(it => TopLevel.GetTopLevel(it.Child))
                 .OfType<PopupRoot>()
                 .Distinct();
}
