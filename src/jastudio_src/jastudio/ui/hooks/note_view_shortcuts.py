"""The keyboard shortcuts that act on the note shown in Anki's reviewer and in its previewer: JAStudio's note view shortcuts, defined once in C#
(NoteViewShortcuts) and shared with JAStudio's workspace. This only binds Anki's keys to them. A shortcut changes a setting or the note, and the
card's page, which is JAStudio's own, reloads itself on either (CardLayout).

Ctrl+O, which opens the shown note in JAStudio's workspace, is among the global shortcuts (global_shortcuts.py), since Anki's browser has it too.
"""
from __future__ import annotations

from typing import TYPE_CHECKING

from aqt import gui_hooks
from jaspythonutils.sysutils import typed
from PyQt6.QtCore import pyqtBoundSignal
from PyQt6.QtGui import QKeySequence, QShortcut

from jastudio.ankiutils import ui_utils
from jastudio.ui import dotnet_ui_root

if TYPE_CHECKING:
    from collections.abc import Callable

    from aqt.browser.previewer import Previewer
    from aqt.main import MainWindowState
    from JAStudio.Core.Note import JPNote
    from JAStudio.UI.Shortcuts import NoteViewShortcut
    from PyQt6.QtWidgets import QWidget


def init() -> None:
    def the_shortcuts_on(the_shown_note: Callable[[], JPNote | None]) -> dict[str, Callable[[], None]]:
        """Each shortcut by its key, acting on the note the_shown_note finds when the key is pressed."""
        def run(shortcut: NoteViewShortcut) -> Callable[[], None]:
            def run_it() -> None:
                note = the_shown_note()
                if note:
                    shortcut.Run(note)
                else:
                    shortcut.Run()

            return run_it

        return {shortcut.KeyGesture: run(shortcut) for shortcut in dotnet_ui_root().NoteViewShortcuts.All}

    # noinspection DuplicatedCode
    def set_shortcut(widget: QWidget, shortcut: str, callback: Callable[[], None]) -> None:
        typed.checked_cast(pyqtBoundSignal, QShortcut(QKeySequence(shortcut), widget).activated).connect(callback)  # pyright: ignore[reportUnknownMemberType]

    def inject_shortcuts_in_previewer(previewer: Previewer) -> None:
        for key, callback in the_shortcuts_on(lambda: ui_utils.try_get_note_of(previewer.card())).items():
            set_shortcut(previewer, key, callback)

    shortcuts_in_the_reviewer = the_shortcuts_on(ui_utils.try_get_review_note)

    # noinspection Annotator
    def inject_shortcuts_in_reviewer(_state: MainWindowState, state_shortcuts: list[tuple[str, Callable[[], None]]]) -> None:  # pyright: ignore
        def remove_shortcut(string: str) -> None:
            for shortcut in state_shortcuts:
                if shortcut[0] == string:
                    state_shortcuts.remove(shortcut)
                    return

        for char in ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "o", "e", "u"]:
            remove_shortcut(char)

        for key, callback in shortcuts_in_the_reviewer.items():
            state_shortcuts.append((key, callback))

    gui_hooks.previewer_did_init.append(inject_shortcuts_in_previewer)  # pyright: ignore[reportUnknownMemberType]
    gui_hooks.state_shortcuts_will_change.append(inject_shortcuts_in_reviewer)  # pyright: ignore[reportUnknownMemberType]
