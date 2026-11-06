from __future__ import annotations

from typing import TYPE_CHECKING

from aqt import gui_hooks
from jaspythonutils.sysutils import typed
from PyQt6.QtCore import pyqtBoundSignal
from PyQt6.QtGui import QKeySequence, QShortcut

from jastudio.ankiutils import app, ui_utils
from jastudio.ui import dotnet_ui_root

if TYPE_CHECKING:
    from collections.abc import Callable

    from aqt.main import MainWindowState
    from JAStudio.UI.Shortcuts import NoteViewShortcut
    from PyQt6.QtWidgets import QWidget

# The shortcuts themselves are defined once, in C# (NoteViewShortcuts), and shared with JAStudio's workspace. This only binds Anki's keys to them.
def init() -> None:
    def run_on_reviewed_note(shortcut: NoteViewShortcut) -> Callable[[], None]:
        def run() -> None:
            reviewed_note = ui_utils.try_get_review_note()
            if reviewed_note:
                shortcut.Run(reviewed_note)
            else:
                shortcut.Run()
            app.get_ui_utils().refresh(refresh_browser=False)

        return run

    shortcuts: dict[str, Callable[[], None]] = {shortcut.KeyGesture: run_on_reviewed_note(shortcut) for shortcut in dotnet_ui_root().NoteViewShortcuts.All}

    # noinspection DuplicatedCode
    def set_shortcut(widget: QWidget, shortcut: str, callback: Callable[[], None]) -> None:
        typed.checked_cast(pyqtBoundSignal, QShortcut(QKeySequence(shortcut), widget).activated).connect(callback)  # pyright: ignore[reportUnknownMemberType]

    def inject_shortcuts_in_widget(widget: QWidget) -> None:
        for key, callback in shortcuts.items():
            set_shortcut(widget, key, callback)

    # noinspection Annotator
    def inject_shortcuts_in_reviewer(_state: MainWindowState, state_shortcuts: list[tuple[str, Callable[[], None]]]) -> None:  # pyright: ignore
        def remove_shortcut(string: str) -> None:
            for shortcut in state_shortcuts:
                if shortcut[0] == string:
                    state_shortcuts.remove(shortcut)
                    return

        for char in ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "o", "e", "u"]:
            remove_shortcut(char)

        for key, callback in shortcuts.items():
            state_shortcuts.append((key, callback))

    gui_hooks.previewer_did_init.append(inject_shortcuts_in_widget)  # pyright: ignore[reportUnknownMemberType]
    gui_hooks.state_shortcuts_will_change.append(inject_shortcuts_in_reviewer)  # pyright: ignore[reportUnknownMemberType]
