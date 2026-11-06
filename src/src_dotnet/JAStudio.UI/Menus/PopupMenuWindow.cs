using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;

namespace JAStudio.UI.Menus;

/// <summary>
/// A menu shown at a point on the screen, rather than opened from a control in one of our windows.<br/>
/// It serves content Avalonia doesn't render itself, such as the web page in the
/// <see cref="Views.NoteViewerWindow"/>, and places with no window at all.
/// </summary>
/// <remarks>
/// Avalonia's <see cref="ContextMenu"/> can't do this with working access keys, for two reasons:<br/>
/// - Typed access keys work only in the submenu of a <see cref="MenuItem"/>, which has its own access key handler.<br/>
///   A context menu's own first level of items has none.<br/>
/// - Over a native control, such as a web view, the keyboard stays with that control.
/// </remarks>
/// <remarks>
/// So this is a tiny, borderless, topmost window that takes activation, and with it the keyboard.<br/>
/// The menu's items are the submenu of a hidden item in a menu bar, opened at once.<br/>
/// The window closes when that submenu closes. Losing activation closes the submenu.
/// </remarks>
sealed class PopupMenuWindow : Window
{
   readonly MenuItem _hiddenMenuBarItem;

   PopupMenuWindow(IReadOnlyList<SpecMenuItem> menuSpec, PixelPoint menuTopLeft)
   {
      _hiddenMenuBarItem = new MenuItem
                           {
                              ItemsSource = SpecMenuRenderer.BuildMenuItems(menuSpec).ToList(),
                              Height = 0,
                              MinHeight = 0,
                              Padding = new Thickness(0),
                              Margin = new Thickness(0)
                           };

      Content = new Menu
                {
                   ItemsSource = new List<MenuItem> { _hiddenMenuBarItem },
                   Height = 0,
                   Padding = new Thickness(0),
                   Margin = new Thickness(0)
                };

      WindowStartupLocation = WindowStartupLocation.Manual;
      Position = menuTopLeft;
      SizeToContent = SizeToContent.Manual;
      Width = 1;
      Height = 1;
      CanResize = false;
      ShowInTaskbar = false;
      WindowDecorations = WindowDecorations.None;
      Background = Brushes.Transparent;
      Topmost = true;

      Opened += (_, _) => OpenTheMenu();
      Deactivated += (_, _) => _hiddenMenuBarItem.IsSubMenuOpen = false;
      _hiddenMenuBarItem.PropertyChanged += (_, e) =>
      {
         if(e.Property == MenuItem.IsSubMenuOpenProperty && !_hiddenMenuBarItem.IsSubMenuOpen)
            Close();
      };
   }

   /// <summary>
   /// Shows the menu that <paramref name="menuSpec"/> describes, with its top left corner at <paramref name="menuTopLeft"/>,
   /// in physical screen pixels.<br/>
   /// Call on the UI thread.
   /// </summary>
   public static void ShowAt(PixelPoint menuTopLeft, IReadOnlyList<SpecMenuItem> menuSpec) =>
      new PopupMenuWindow(menuSpec, menuTopLeft).Show();

   void OpenTheMenu()
   {
      _hiddenMenuBarItem.IsSubMenuOpen = true;
      Dispatcher.UIThread.Post(FocusTheFirstItem, DispatcherPriority.Loaded);
   }

   /// <summary>Access keys reach a menu as text typed into it, so the keyboard focus has to be inside the menu.</summary>
   void FocusTheFirstItem() => _hiddenMenuBarItem.Items.OfType<MenuItem>().FirstOrDefault(it => it.IsEnabled)?.Focus();
}
