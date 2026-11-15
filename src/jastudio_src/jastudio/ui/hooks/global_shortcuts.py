from __future__ import annotations

from typing import TYPE_CHECKING

from aqt import gui_hooks, mw
from jaspythonutils.sysutils import ex_assert, typed
from jaspythonutils.sysutils.typed import checked_cast
from PyQt6.QtCore import pyqtBoundSignal
from PyQt6.QtGui import QKeySequence, QShortcut
from PyQt6.QtWidgets import QWidget

from jastudio.ankiutils import app, ui_utils
from jastudio.ui import dotnet_ui_root
from jastudio.ui.hooks import history_navigator

if TYPE_CHECKING:
    from collections.abc import Callable

    from aqt.browser import Browser  # type: ignore[attr-defined]  # pyright: ignore[reportPrivateImportUsage]
    from aqt.browser.previewer import Previewer
    from JAStudio.Core.Note import JPNote


def init() -> None:
    dialogs = dotnet_ui_root().Dialogs

    def the_shortcuts_on(the_shown_note: Callable[[], JPNote | None]) -> dict[str, Callable[[], None]]:
        """Each shortcut by its key, in a window whose shown note the_shown_note finds when a key is pressed."""
        def open_the_shown_note_in_the_workspace() -> None:
            note = the_shown_note()
            if note:
                dialogs.ShowTheNoteInTheWorkspace(note)

        return {
            "Alt+Left": history_navigator.navigator.navigate_back,
            "Alt+Right": history_navigator.navigator.navigate_forward,
            # The JA Studio menu shows these three (jastudio_menu.py) without binding them.
            "Ctrl+s": dialogs.ToggleNoteSearch,
            "Ctrl+Shift+o": dialogs.ToggleWorkspaceWindow,
            "Ctrl+o": open_the_shown_note_in_the_workspace,
            "F5": lambda: app.get_ui_utils().refresh()
        }

    # noinspection DuplicatedCode
    def set_shortcut(widget: QWidget, shortcut: str, callback: Callable[[], None]) -> None:
        typed.checked_cast(pyqtBoundSignal, QShortcut(QKeySequence(shortcut), widget).activated).connect(callback)  # pyright: ignore[reportUnknownMemberType]

    def bind(widget: QWidget, shortcuts: dict[str, Callable[[], None]]) -> None:
        for key, callback in shortcuts.items():
            set_shortcut(widget, key, callback)

    def disable_escape(widget: QWidget) -> None:
        set_shortcut(widget, "Escape", lambda: None)

    def bind_in_the_previewer(previewer: Previewer) -> None:
        bind(previewer, the_shortcuts_on(lambda: ui_utils.try_get_note_of(previewer.card())))
        disable_escape(previewer)

    def bind_in_the_browser(browser: Browser) -> None:
        bind(browser, the_shortcuts_on(lambda: ui_utils.try_get_note_of(browser.card)))
        disable_escape(browser)

    ex_assert.not_none(history_navigator.navigator, "History navigator needs to be initialized before global shortcuts are bound")

    bind(checked_cast(QWidget, mw), the_shortcuts_on(ui_utils.try_get_review_note))

    gui_hooks.previewer_did_init.append(bind_in_the_previewer)  # pyright: ignore[reportUnknownMemberType]
    gui_hooks.browser_will_show.append(bind_in_the_browser)
