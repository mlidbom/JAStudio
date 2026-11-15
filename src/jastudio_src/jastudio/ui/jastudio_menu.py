from __future__ import annotations

from jaspythonutils.sysutils.typed import non_optional

from jastudio.ankiutils.app import main_window


def init() -> None:
    """Adds the JA Studio menu to Anki's menu bar: how to find JAStudio from Anki. Its shortcuts are only shown here: global_shortcuts.py
    binds them, in Anki's browser and previewer too."""
    from jastudio.ui import dotnet_ui_root

    dialogs = dotnet_ui_root().Dialogs
    menu = non_optional(main_window().form.menubar.addMenu("&JA Studio"))
    menu.addAction("&Workspace\tCtrl+Shift+O", dialogs.ToggleWorkspaceWindow)  # pyright: ignore[reportUnknownMemberType]
    menu.addAction("&Search notes\tCtrl+O", dialogs.ToggleNoteSearch)  # pyright: ignore[reportUnknownMemberType]
