using System.Collections.Generic;
using Avalonia;
using Avalonia.Threading;
using JAStudio.Core;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;

namespace JAStudio.UI;

/// <summary>
/// Factory methods for building menus (context menus, toolbar menus, browser menus).
/// </summary>
public class AnkiMenus
{
   readonly TemporaryServiceCollection _services;

   public AnkiMenus(TemporaryServiceCollection services) => _services = services;

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
