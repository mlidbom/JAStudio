from __future__ import annotations

from jaspythonutils.sysutils.typed import non_optional

from jastudio.ankiutils.app import main_window


def init() -> None:
    """Adds the JA Studio menu to Anki's menu bar: how to find JAStudio from Anki. C# builds it (AnkiMenus.BuildJAStudioMenuSpec)."""
    from jastudio.qt_adapters import qt_menu_adapter
    from jastudio.ui import dotnet_ui_root

    menu = non_optional(main_window().form.menubar.addMenu("&JA Studio"))
    qt_menu_adapter.add_to_qt_menu(menu, dotnet_ui_root().Menus.BuildJAStudioMenuSpec())
