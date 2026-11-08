using System.Collections.Generic;
using Avalonia;
using Avalonia.Threading;
using JAStudio.Core;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;

namespace JAStudio.UI;

/// <summary>
/// Factory methods for building menus (context menus, toolbar menus, browser menus).
/// </summary>
public class AnkiMenus
{
   readonly TemporaryServiceCollection _services;
   readonly AnkiDialogs _dialogs = new();

   public AnkiMenus(TemporaryServiceCollection services) => _services = services;

   /// <summary>
   /// The JA Studio menu in Anki's menu bar: how to find JAStudio from Anki. It opens the workspace, which everything else is reached from,<br/>
   /// or the workspace on its search. Its shortcuts are only shown: Anki's global shortcuts bind them (<c>global_shortcuts.py</c>),<br/>
   /// in Anki's browser and previewer too.
   /// </summary>
   public List<SpecMenuItem> BuildJAStudioMenuSpec() =>
   [
      SpecMenuItem.Command(ShortcutFinger.Home1("Workspace"), _dialogs.ToggleWorkspaceWindow, shortcut: "Ctrl+Shift+o"),
      SpecMenuItem.Command(ShortcutFinger.Home2("Search notes"), _dialogs.ToggleNoteSearch, shortcut: "Ctrl+o")
   ];

   /// <summary>Called from Python to build right-click context menus.</summary>
   public NoteContextMenu CreateNoteContextMenu() => new(_services);

   /// <summary>
   /// Shows the menu of actions on the clipboard text and the selected text at the screen point
   /// (<paramref name="x"/>, <paramref name="y"/>), in physical pixels, with no note involved.
   /// </summary>
   public void ShowContextMenuPopup(string? clipboardContent, string? selectionContent, int x, int y) =>
      Dispatcher.UIThread.Invoke(() => PopupMenuWindow.ShowAt(new PixelPoint(x, y),
                                                              new NoteContextMenu(_services).BuildGenericContextMenuSpec(selectionContent ?? "", clipboardContent ?? "")));

   /// <summary>
   /// Build browser context menu specification.
   /// Returns UI-agnostic menu specs that Python can convert to PyQt menus.
   /// </summary>
   public SpecMenuItem BuildBrowserMenuSpec(
      IReadOnlyList<long> selectedCardIds,
      IReadOnlyList<long> selectedNoteIds) =>
      new AnkiBrowserMenuBuilder(_services).BuildBrowserMenuSpec(selectedCardIds, selectedNoteIds);
}
