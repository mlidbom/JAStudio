from __future__ import annotations

from jaspythonutils.sysutils.typed import non_optional

from jastudio.ankiutils import ui_utils
from jastudio.ankiutils.app import main_window


def init() -> None:
    """Adds the JA Studio menu to Anki's menu bar: how to find JAStudio from Anki. Its shortcuts are only shown here: global_shortcuts.py
    binds them, in Anki's browser and previewer too."""
    from jastudio.ui import dotnet_ui_root

    dialogs = dotnet_ui_root().Dialogs

    def open_the_reviewed_note_in_the_workspace() -> None:
        note = ui_utils.try_get_review_note()
        if note:
            dialogs.ShowTheNoteInTheWorkspace(note)

    menu = non_optional(main_window().form.menubar.addMenu("&JA Studio"))
    menu.addAction("&Workspace\tCtrl+Shift+O", dialogs.ToggleWorkspaceWindow)  # pyright: ignore[reportUnknownMemberType]
    menu.addAction("&Search notes\tCtrl+S", dialogs.ToggleNoteSearch)  # pyright: ignore[reportUnknownMemberType]
    menu.addAction("&Open the reviewed note in the workspace\tCtrl+O", open_the_reviewed_note_in_the_workspace)  # pyright: ignore[reportUnknownMemberType]
