using System;
using System.Collections.Generic;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;

namespace JAStudio.UI.Menus;

/// <summary>Converts UI-agnostic <see cref="SpecMenuItem"/> trees into Avalonia menu controls.</summary>
static class SpecMenuRenderer
{
   public static IEnumerable<Control> BuildMenuItems(IReadOnlyList<SpecMenuItem> specs)
   {
      foreach(var spec in specs)
      {
         if(!spec.IsVisible) continue;
         yield return ToControl(spec);
      }
   }

   static Control ToControl(SpecMenuItem spec) =>
      spec.Kind switch
      {
         SpecMenuItemKind.Separator => new Separator(),
         SpecMenuItemKind.Command   => BuildCommandItem(spec),
         SpecMenuItemKind.Submenu   => BuildSubmenuItem(spec),
         _                          => throw new ArgumentOutOfRangeException(nameof(spec), spec.Kind, null)
      };

   static MenuItem BuildCommandItem(SpecMenuItem spec) =>
      new()
      {
         Header    = spec.Name,
         Command   = new RelayCommand(spec.Action, () => spec.IsEnabled),
         IsEnabled = spec.IsEnabled
      };

   static MenuItem BuildSubmenuItem(SpecMenuItem spec)
   {
      var item = new MenuItem
      {
         Header    = spec.Name,
         IsEnabled = spec.IsEnabled
      };
      foreach(var child in spec.Children)
      {
         if(child.IsVisible)
            item.Items.Add(ToControl(child));
      }
      return item;
   }
}
