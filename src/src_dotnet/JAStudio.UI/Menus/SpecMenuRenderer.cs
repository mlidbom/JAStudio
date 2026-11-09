using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;

namespace JAStudio.UI.Menus;

/// <summary>Converts UI-agnostic <see cref="SpecMenuItem"/> trees into Avalonia menu controls.</summary>
static class SpecMenuRenderer
{
   public static IEnumerable<Control> BuildMenuItems(IReadOnlyList<SpecMenuItem> specs) => BuildMenuItems(specs, whenAnItemIsPicked: () => {});

   /// <summary>
   /// The menu items <paramref name="specs"/> describe, which run <paramref name="whenAnItemIsPicked"/> as an item is picked, by a click or by its key,<br/>
   /// before the item's action runs.
   /// </summary>
   /// <remarks>
   /// Before, since the action can close the menu: an action that opens a window, such as a dialog, takes the activation from the menu's window,<br/>
   /// and on a desktop that can happen within the action. A <see cref="MenuItem.ClickEvent"/> handler on the menu would learn of the pick only after<br/>
   /// the action, as the click bubbles up from the item.
   /// </remarks>
   public static IEnumerable<Control> BuildMenuItems(IReadOnlyList<SpecMenuItem> specs, Action whenAnItemIsPicked)
   {
      foreach(var spec in specs)
      {
         if(!spec.IsVisible) continue;
         yield return ToControl(spec, whenAnItemIsPicked);
      }
   }

   static Control ToControl(SpecMenuItem spec, Action whenAnItemIsPicked) =>
      spec.Kind switch
      {
         SpecMenuItemKind.Separator => new Separator(),
         SpecMenuItemKind.Command   => BuildCommandItem(spec, whenAnItemIsPicked),
         SpecMenuItemKind.Submenu   => BuildSubmenuItem(spec, whenAnItemIsPicked),
         _                          => throw new ArgumentOutOfRangeException(nameof(spec), spec.Kind, null)
      };

   /// <summary>The item only shows its <see cref="SpecMenuItem.KeyboardShortcut"/>. Binding it is the job of the window that owns the menu.</summary>
   static MenuItem BuildCommandItem(SpecMenuItem spec, Action whenAnItemIsPicked)
   {
      return new MenuItem
             {
                Header       = spec.Name,
                Command      = new RelayCommand(PickIt, () => spec.IsEnabled),
                IsEnabled    = spec.IsEnabled,
                InputGesture = spec.KeyboardShortcut is {} keyGesture ? KeyGesture.Parse(keyGesture) : null
             };

      void PickIt()
      {
         whenAnItemIsPicked();
         spec.Action();
      }
   }

   static MenuItem BuildSubmenuItem(SpecMenuItem spec, Action whenAnItemIsPicked)
   {
      var item = new MenuItem
      {
         Header    = spec.Name,
         IsEnabled = spec.IsEnabled
      };
      foreach(var child in spec.Children)
      {
         if(child.IsVisible)
            item.Items.Add(ToControl(child, whenAnItemIsPicked));
      }
      return item;
   }
}
