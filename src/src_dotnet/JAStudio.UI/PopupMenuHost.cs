using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Compze.Utilities.Logging;

namespace JAStudio.UI;

/// <summary>
/// Positions and displays an Avalonia Menu at arbitrary screen coordinates.
/// Uses a transparent window hack to show menus outside of normal UI hierarchy.
/// </summary>
public static class PopupMenuHost
{
   static readonly ILogger Log = CompzeLogger.For(typeof(PopupMenuHost));

   static PopupMenuHost() =>
      // Set menu delay to zero for snappy submenu opening
#pragma warning disable CS0618 // Type or member is obsolete
      DefaultMenuInteractionHandler.MenuShowDelay = TimeSpan.Zero;
#pragma warning restore CS0618 // Type or member is obsolete

   /// <summary>
   /// Show a complete Menu at the specified screen coordinates.
   /// </summary>
   /// <param name="menuItems">The menu items to display</param>
   /// <param name="x">X coordinate in physical (device) pixels</param>
   /// <param name="y">Y coordinate in physical (device) pixels</param>
   public static void ShowAt(IEnumerable<MenuItem> menuItems, int x, int y)
   {
      // Create a top-level menu item that contains our items as a submenu
      // We make this invisible to minimize visual presence
      var topMenuItem = new MenuItem
                        {
                           Header = "", // Empty header
                           ItemsSource = menuItems,
                           Height = 0,
                           MinHeight = 0,
                           Padding = new Thickness(0),
                           Margin = new Thickness(0)
                        };

      // Create the main menu (required to host the MenuItem)
      var menu = new Menu
                 {
                    ItemsSource = new List<MenuItem> { topMenuItem },
                    Height = 0,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0)
                 };

      // Create window with the Menu as content
      var hostWindow = new Window
                       {
                          Content = menu,
                          WindowStartupLocation = WindowStartupLocation.Manual,
                          CanResize = false,
                          ShowInTaskbar = false,
                          WindowDecorations = WindowDecorations.None,
                          Background = Avalonia.Media.Brushes.Transparent,
                          SizeToContent = SizeToContent.Manual,
                          Width = 1,
                          Height = 1,
                          Topmost = true
                       };

      hostWindow.Opened += (_, _) =>
      {
         // Get the actual height of the menu bar and adjust for DPI
         var menuHeight = (int)(hostWindow.Bounds.Height * 1.25); // Convert logical to physical pixels at 125% DPI

         // Offset the window upward by the menu height so the submenu appears at the correct position
         var adjustedY = y - menuHeight;

         hostWindow.Position = new PixelPoint(x, adjustedY);
         Log.Info($"Menu window opened at ({x}, {adjustedY}), original y: {y}");

         // Open the submenu immediately
         topMenuItem.IsSubMenuOpen = true;
      };

      // Close when the submenu closes or window loses focus
      topMenuItem.PropertyChanged += (_, e) =>
      {
         if(e.Property.Name == nameof(MenuItem.IsSubMenuOpen) && !topMenuItem.IsSubMenuOpen)
         {
            hostWindow.Close();
         }
      };

      hostWindow.Deactivated += (_, _) => hostWindow.Close();

      hostWindow.Show();
   }
}
