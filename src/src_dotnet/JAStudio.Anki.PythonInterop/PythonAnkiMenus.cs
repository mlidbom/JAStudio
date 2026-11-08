using System.Collections.Generic;
using JAStudio.PythonInterop;
using JAStudio.UI;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;

namespace JAStudio.Anki.PythonInterop;

/// <summary>
/// Python-facing wrapper around <see cref="AnkiMenus"/> that handles
/// dynamic → typed conversions at the Python interop boundary.
/// All methods that don't need conversion delegate directly.
/// </summary>
// ReSharper disable UnusedMember.Global used from python
public class PythonAnkiMenus
{
   readonly AnkiMenus _menus;

   internal PythonAnkiMenus(AnkiMenus menus) => _menus = menus;

   /// <summary>Called from Python to build the JA Studio menu in Anki's menu bar.</summary>
   public List<SpecMenuItem> BuildJAStudioMenuSpec() => _menus.BuildJAStudioMenuSpec();

   /// <summary>Called from Python to build right-click context menus.</summary>
   public NoteContextMenu CreateNoteContextMenu() => _menus.CreateNoteContextMenu();

   /// <summary>Show the context menu popup at the current cursor position.</summary>
   public void ShowContextMenuPopup(string? clipboardContent, string? selectionContent, int x, int y) =>
      _menus.ShowContextMenuPopup(clipboardContent, selectionContent, x, y);

   /// <summary>
   /// Build browser context menu specification.
   /// Accepts Python lists (dynamic) and converts them to typed .NET lists before delegating.
   /// </summary>
   public SpecMenuItem BuildBrowserMenuSpec(
      dynamic selectedCardIds,
      dynamic selectedNoteIds) =>
      _menus.BuildBrowserMenuSpec(PythonDotNetShim.LongList.ToDotNet(selectedCardIds),
                                  PythonDotNetShim.LongList.ToDotNet(selectedNoteIds));
}
