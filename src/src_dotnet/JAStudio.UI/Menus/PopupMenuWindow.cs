using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Menus;

/// <summary>
/// A menu shown at a point on the screen, rather than opened from a control in one of our windows.<br/>
/// It serves content Avalonia doesn't render itself, such as the web page in the
/// <see cref="WorkspaceWindow"/>, and places with no window at all.
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

   /// <summary>The item whose submenu is opened along with the menu, as if its key had been typed. Null to open just the menu.</summary>
   readonly SpecMenuItem? _submenuToOpen;

   /// <summary>
   /// Whether an item without a submenu was clicked or picked by its key, rather than the menu being closed without a pick.<br/>
   /// Set before the item's action runs, which may close the menu (<see cref="SpecMenuRenderer.BuildMenuItems(IReadOnlyList{SpecMenuItem}, Action)"/>).
   /// </summary>
   bool _anItemWasPicked;

   PopupMenuWindow(IReadOnlyList<SpecMenuItem> menuSpec, PixelPoint menuTopLeft, SpecMenuItem? submenuToOpen = null)
   {
      _submenuToOpen = submenuToOpen;
      _hiddenMenuBarItem = new MenuItem
                           {
                              ItemsSource = SpecMenuRenderer.BuildMenuItems(menuSpec, whenAnItemIsPicked: () => _anItemWasPicked = true).ToList(),
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

   /// <summary>
   /// Shows the menu as <see cref="ShowAt(PixelPoint, IReadOnlyList{SpecMenuItem})"/> does, and runs <paramref name="whenClosedWithoutAPick"/>
   /// if it closes without an item being picked, as with Esc.<br/>
   /// The window it came from can then put the keyboard back where it was. When an item is picked, what the item does decides where the keyboard goes,
   /// such as into a dialog it opens.
   /// </summary>
   public static void ShowAt(PixelPoint menuTopLeft, IReadOnlyList<SpecMenuItem> menuSpec, Action whenClosedWithoutAPick) =>
      new PopupMenuWindow(menuSpec, menuTopLeft).ShowAndRunIfClosedWithoutAPick(whenClosedWithoutAPick);

   /// <summary>
   /// Shows the menu as <see cref="ShowAt(PixelPoint, IReadOnlyList{SpecMenuItem}, Action)"/> does, with the submenu of <paramref name="submenuToOpen"/>,
   /// one of its items, open,<br/>
   /// as if that item's key had been typed in the menu. So it's clear what's happening, and another item is one key away.
   /// </summary>
   public static void ShowWithSubmenuOpen(PixelPoint menuTopLeft, IReadOnlyList<SpecMenuItem> menuSpec, SpecMenuItem submenuToOpen, Action whenClosedWithoutAPick) =>
      new PopupMenuWindow(menuSpec, menuTopLeft, submenuToOpen).ShowAndRunIfClosedWithoutAPick(whenClosedWithoutAPick);

   void ShowAndRunIfClosedWithoutAPick(Action whenClosedWithoutAPick)
   {
      Closed += (_, _) =>
      {
         if(!_anItemWasPicked)
            whenClosedWithoutAPick();
      };
      Show();
   }

   void OpenTheMenu()
   {
      _hiddenMenuBarItem.IsSubMenuOpen = true;
      Dispatcher.UIThread.Post(FocusTheFirstItem, DispatcherPriority.Loaded);
   }

   /// <summary>Access keys reach a menu as text typed into it, so the keyboard focus has to be inside the menu.</summary>
   void FocusTheFirstItem()
   {
      FirstEnabledItemIn(_hiddenMenuBarItem)?.Focus();
      if(_submenuToOpen is {} submenu)
         OpenTheSubmenuOf(submenu);
   }

   void OpenTheSubmenuOf(SpecMenuItem submenu)
   {
      var item = _hiddenMenuBarItem.Items.OfType<MenuItem>().Single(it => Equals(it.Header, submenu.Name));
      item.Focus();
      item.IsSubMenuOpen = true;
      Dispatcher.UIThread.Post(() => FirstEnabledItemIn(item)?.Focus(), DispatcherPriority.Loaded);
   }

   static MenuItem? FirstEnabledItemIn(MenuItem menu) => menu.Items.OfType<MenuItem>().FirstOrDefault(it => it.IsEnabled);
}
