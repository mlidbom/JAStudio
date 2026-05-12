using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Compze.Utilities.Logging;

namespace JAStudio.UI.Views;

partial class ContextMenuPopup : UserControl
{
   readonly string _clipboardContent;
   readonly string _selectionContent;
   Window? _hostWindow;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
   [Obsolete("Parameterless constructor is only for XAML designer support and should not be used directly.")]
   public ContextMenuPopup() {}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

   public ContextMenuPopup(string clipboardContent, string selectionContent)
   {
      _clipboardContent = clipboardContent;
      _selectionContent = selectionContent;

      InitializeComponent();
   }

   public ContextMenu CreateContextMenu()
   {
      var clipboardItem = new MenuItem
                          {
                             Header = string.IsNullOrEmpty(_clipboardContent)
                                         ? "Clipboard: (empty)"
                                         : $"Clipboard: {TruncateText(_clipboardContent, 30)}"
                          };
      clipboardItem.Click += OnClipboardItemClick;

      var selectionItem = new MenuItem
                          {
                             Header = string.IsNullOrEmpty(_selectionContent)
                                         ? "Selection: (empty)"
                                         : $"Selection: {TruncateText(_selectionContent, 30)}"
                          };
      selectionItem.Click += OnSelectionItemClick;

      var contextMenu = new ContextMenu
                        {
                           ItemsSource = new List<MenuItem> { clipboardItem, selectionItem },
                           Placement = PlacementMode.Pointer
                        };

      // Close the host window when the context menu closes
      contextMenu.Closed += (_, _) => _hostWindow?.Close();

      return contextMenu;
   }

   public void ShowAt(int x, int y)
   {
      this.Log().Info($"ShowAt called with: ({x}, {y})");

      // Create sub-menu items
      var menuItems = new List<MenuItem>();

      var clipboardItem = new MenuItem
                          {
                             Header = string.IsNullOrEmpty(_clipboardContent)
                                         ? "Clipboard: (empty)"
                                         : $"Clipboard: {TruncateText(_clipboardContent, 30)}"
                          };
      clipboardItem.Click += OnClipboardItemClick;
      menuItems.Add(clipboardItem);

      var selectionItem = new MenuItem
                          {
                             Header = string.IsNullOrEmpty(_selectionContent)
                                         ? "Selection: (empty)"
                                         : $"Selection: {TruncateText(_selectionContent, 30)}"
                          };
      selectionItem.Click += OnSelectionItemClick;
      menuItems.Add(selectionItem);

      // Add 10 dummy entries
      for(var i = 1; i <= 10; i++)
      {
         var dummyItem = new MenuItem
                         {
                            Header = $"Dummy Entry {i}"
                         };
         dummyItem.Click += (s, _) => this.Log().Info($"Dummy item clicked: {((MenuItem)s!).Header}");
         menuItems.Add(dummyItem);
      }

      // Create a top-level menu item that contains our items as a submenu
      var topMenuItem = new MenuItem
                        {
                           Header = "", // Empty to minimize visual presence
                           ItemsSource = menuItems,
                           // Try to make it invisible
                           Height = 0,
                           MinHeight = 0,
                           Padding = new Thickness(0),
                           Margin = new Thickness(0)
                        };

      // Create the main menu
      var menu = new Menu
                 {
                    ItemsSource = new List<MenuItem> { topMenuItem },
                    Height = 0,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0)
                 };

      // Create window with the Menu as content
      _hostWindow = new Window
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

      _hostWindow.Opened += (_, _) =>
      {
         // Get the actual height of the menu bar
         var menuHeight = (int)(_hostWindow.Bounds.Height * 1.25); // Convert logical to physical pixels at 125% DPI

         // Offset the window upward by the menu height so the submenu appears at the correct position
         var adjustedY = y - menuHeight;

         _hostWindow.Position = new PixelPoint(x, adjustedY);
         this.Log().Info($"Window opened at ({x}, {adjustedY}), original y: {y}, menu height: {menuHeight}, bounds: {_hostWindow.Bounds}");

         // Open the submenu immediately
         topMenuItem.IsSubMenuOpen = true;
         this.Log().Info("Submenu opened");
      };

      // Close when the submenu closes or window loses focus
      topMenuItem.PropertyChanged += (_, e) =>
      {
         if(e.Property.Name == nameof(MenuItem.IsSubMenuOpen) && !topMenuItem.IsSubMenuOpen)
         {
            _hostWindow?.Close();
         }
      };

      _hostWindow.Deactivated += (_, _) => _hostWindow.Close();

      _hostWindow.Show();
   }

   static string TruncateText(string text, int maxLength)
   {
      if(text.Length <= maxLength)
         return text;
      return text.Substring(0, maxLength) + "...";
   }

   void OnClipboardItemClick(object? sender, RoutedEventArgs e)
   {
      this.Log().Info($"Clipboard item clicked: {_clipboardContent}");
   }

   void OnSelectionItemClick(object? sender, RoutedEventArgs e)
   {
      this.Log().Info($"Selection item clicked: {_selectionContent}");
   }
}
