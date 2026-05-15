from __future__ import annotations

from typing import TYPE_CHECKING

from aqt import gui_hooks, mw
from jaspythonutils.sysutils import ex_assert, typed
from jaspythonutils.sysutils.typed import checked_cast
from PyQt6.QtCore import pyqtBoundSignal
from PyQt6.QtGui import QKeySequence, QShortcut
from PyQt6.QtWidgets import QWidget

from jastudio.ankiutils import app
from jastudio.ui import dotnet_ui_root
from jastudio.ui.hooks import history_navigator

if TYPE_CHECKING:
    from collections.abc import Callable


def init() -> None:
    shortcuts: dict[str, Callable[[], None]] = {
        "Alt+Left": history_navigator.navigator.navigate_back,
        "Alt+Right": history_navigator.navigator.navigate_forward,
        "Ctrl+o": dotnet_ui_root().Dialogs.ToggleNoteSearchDialog,
        "Ctrl+Shift+o": dotnet_ui_root().Dialogs.ToggleMainWindow,
        "F5": lambda: app.get_ui_utils().refresh()
    }

    # noinspection DuplicatedCode
    def set_shortcut(widget: QWidget, shortcut: str, callback: Callable[[], None]) -> None:
        typed.checked_cast(pyqtBoundSignal, QShortcut(QKeySequence(shortcut), widget).activated).connect(callback)  # pyright: ignore[reportUnknownMemberType]

    def bind_universal_shortcuts(widget: QWidget) -> None:
        for key, callback in shortcuts.items():
            set_shortcut(widget, key, callback)

    def disable_escape(widget: QWidget) -> None:
        set_shortcut(widget, "Escape", lambda: None)

    ex_assert.not_none(history_navigator.navigator, "History navigator needs to be initialized before global shortcuts are bound")

    bind_universal_shortcuts(checked_cast(QWidget, mw))

    gui_hooks.previewer_did_init.append(bind_universal_shortcuts)  # pyright: ignore[reportUnknownMemberType]
    gui_hooks.previewer_did_init.append(disable_escape)  # pyright: ignore[reportUnknownMemberType]
    gui_hooks.browser_will_show.append(bind_universal_shortcuts)
    gui_hooks.browser_will_show.append(disable_escape)
