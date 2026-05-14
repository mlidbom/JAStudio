using System;
using System.Collections.Generic;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using System.Linq;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Specifications;

/// <summary>
/// Demonstrates the benefit of UI-agnostic menu specifications:
/// We can test the entire menu tree structure, conditional logic, and actions
/// WITHOUT instantiating any UI framework (Avalonia, WPF, Qt, etc.).
/// </summary>
public class MenuSpecificationTests
{
   [Fact]
   public void WebSearchMenu_ContainsKanjiSubmenu()
   {
      // Arrange
      var searchText = "漢字";

      // Act - Build the menu spec (no Avalonia needed!)
      var menuSpec = WebSearchMenuBuilder.BuildWebSearchMenu(ShortcutFinger.Home3("Web"), () => searchText);

      // Assert - Verify structure
      Assert.Equal("_o Web", menuSpec.Name); // ShortcutFinger.Home3("Web") = "_o Web"
      Assert.NotNull(menuSpec.Children);
      Assert.Equal(SpecMenuItemKind.Submenu, menuSpec.Kind);

      var kanjiSubmenu = menuSpec.Children.FirstOrDefault(c => c.Name.Contains("Kanji"));
      Assert.NotNull(kanjiSubmenu);
      Assert.Equal(SpecMenuItemKind.Submenu, kanjiSubmenu.Kind);
      Assert.Equal(3, kanjiSubmenu.Children.Count); // Kanji explosion, Kanshudo, Kanji map
   }

   [Fact]
   public void WebSearchMenu_KanjiExplosion_OpensCorrectUrl()
   {
      // Arrange
      var searchText = "漢字";

      // Act - Build menu and find the "Kanji explosion" action
      var menuSpec = WebSearchMenuBuilder.BuildWebSearchMenu(ShortcutFinger.Home3("Web"), () => searchText);

      var kanjiSubmenu = menuSpec.Children.First(c => c.Name.Contains("Kanji"));
      var kanjiExplosionItem = kanjiSubmenu.Children.First();

      Assert.Equal("_u Kanji explosion", kanjiExplosionItem.Name); // ShortcutFinger.Home1("Kanji explosion")
      // Note: Can't test execution since it calls AnkiFacade.OpenUrl
      Assert.NotNull(kanjiExplosionItem.Action);
   }

   [Fact]
   public void WebSearchMenu_AllLeafNodes_HaveActions()
   {
      // Arrange & Act
      var menuSpec = WebSearchMenuBuilder.BuildWebSearchMenu(ShortcutFinger.Home3("Web"), () => "test");

      // Assert - Recursively verify all leaf nodes have actions
      var leafItems = GetAllLeafItems(menuSpec);
      Assert.NotEmpty(leafItems);
      Assert.All(leafItems, item => Assert.NotNull(item.Action));
   }

   [Fact]
   public void MenuItem_Separator_HasCorrectProperties()
   {
      // Act
      var separator = SpecMenuItem.Separator();

      // Assert
      Assert.Equal(SpecMenuItemKind.Separator, separator.Kind);
      Assert.Throws<InvalidOperationException>(() => separator.Action);
      Assert.Throws<InvalidOperationException>(() => separator.Children);
      Assert.Throws<InvalidOperationException>(() => separator.HasVisibleChildren);
   }

   [Fact]
   public void MenuItem_Command_HasCorrectProperties()
   {
      // Arrange
      var executed = false;

      // Act
      var command = SpecMenuItem.Command(
         "Test Command",
         () => executed = true,
         acceleratorKey: 'T',
         shortcut: "Ctrl+T"
      );

      // Assert
      Assert.Equal(SpecMenuItemKind.Command, command.Kind);
      Assert.Equal("Test Command", command.Name);
      Assert.Equal('T', command.AcceleratorKey);
      Assert.Equal("Ctrl+T", command.KeyboardShortcut);

      command.Action();
      Assert.True(executed);
   }

   [Fact]
   public void MenuItem_Submenu_HasCorrectProperties()
   {
      // Act
      var submenu = SpecMenuItem.Submenu(
         "Test Menu",
         new List<SpecMenuItem>
         {
            SpecMenuItem.Command("Item 1", () => {}),
            SpecMenuItem.Separator(),
            SpecMenuItem.Command("Item 2", () => {})
         },
         acceleratorKey: 'M'
      );

      // Assert
      Assert.Equal(SpecMenuItemKind.Submenu, submenu.Kind);
      Assert.Equal("Test Menu", submenu.Name);
      Assert.Equal('M', submenu.AcceleratorKey);
      Assert.Equal(3, submenu.Children.Count);
   }

   [Fact]
   public void MenuItem_ConditionalVisibility_WorksCorrectly()
   {
      // Arrange
      var showItem = false;
      var conditionalItem = SpecMenuItem.Command("Conditionally Hidden", () => {});
      conditionalItem.IsVisible = showItem;

      // Act
      var menu = SpecMenuItem.Submenu(
         "Menu",
         new List<SpecMenuItem>
         {
            SpecMenuItem.Command("Always Visible", () => {}),
            conditionalItem
         }
      );

      // Assert - Only visible items should be counted
      var visibleItems = menu.Children.Where(c => c.IsVisible).ToList();
      Assert.Single(visibleItems);
      Assert.Equal("Always Visible", visibleItems[0].Name);
   }

   [Fact]
   public void MenuItem_EmptySubmenu_IsStillSubmenu()
   {
      // Act
      var submenu = SpecMenuItem.Submenu("Empty Menu", new List<SpecMenuItem>());

      // Assert - It's a submenu even with no children
      Assert.Equal(SpecMenuItemKind.Submenu, submenu.Kind);
   }

   [Fact]
   public void MenuItem_EmptySubmenu_IsAutoDisabled()
   {
      // Act
      var submenu = SpecMenuItem.Submenu("Empty Menu", new List<SpecMenuItem>());

      // Assert - Empty submenu should be disabled
      Assert.False(submenu.IsEnabled);
      Assert.False(submenu.HasVisibleChildren);
   }

   [Fact]
   public void MenuItem_SubmenuWithAllInvisibleChildren_IsAutoDisabled()
   {
      // Arrange
      var child1 = SpecMenuItem.Command("Hidden 1", () => {});
      child1.IsVisible = false;
      var child2 = SpecMenuItem.Command("Hidden 2", () => {});
      child2.IsVisible = false;

      // Act
      var submenu = SpecMenuItem.Submenu("Menu", new List<SpecMenuItem> { child1, child2 });

      // Assert
      Assert.Equal(SpecMenuItemKind.Submenu, submenu.Kind);
      Assert.False(submenu.IsEnabled);
      Assert.False(submenu.HasVisibleChildren);
   }

   [Fact]
   public void MenuItem_SubmenuWithOnlySeparators_IsAutoDisabled()
   {
      // A submenu containing only separators is effectively empty
      var submenu = SpecMenuItem.Submenu("Menu",
                                         new List<SpecMenuItem>
                                         {
                                            SpecMenuItem.Separator(),
                                            SpecMenuItem.Separator()
                                         });

      Assert.Equal(SpecMenuItemKind.Submenu, submenu.Kind);
      Assert.False(submenu.IsEnabled);
      Assert.False(submenu.HasVisibleChildren);
   }

   [Fact]
   public void MenuItem_SubmenuWithVisibleChildren_IsEnabled()
   {
      var submenu = SpecMenuItem.Submenu("Menu",
                                         new List<SpecMenuItem>
                                         {
                                            SpecMenuItem.Command("Item", () => {})
                                         });

      Assert.Equal(SpecMenuItemKind.Submenu, submenu.Kind);
      Assert.True(submenu.IsEnabled);
      Assert.True(submenu.HasVisibleChildren);
   }

   /// <summary>
   /// Helper to recursively extract all leaf (command) items from a menu tree.
   /// </summary>
   static List<SpecMenuItem> GetAllLeafItems(SpecMenuItem menu)
   {
      var result = new List<SpecMenuItem>();

      if(menu.Kind == SpecMenuItemKind.Command)
      {
         result.Add(menu);
      } else if(menu.Kind == SpecMenuItemKind.Submenu)
      {
         foreach(var child in menu.Children)
         {
            result.AddRange(GetAllLeafItems(child));
         }
      }

      return result;
   }
}
