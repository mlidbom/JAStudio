from __future__ import annotations

from typing import TYPE_CHECKING, override

from autoslot import Slots
from typed_linq_collections.q_iterable import query

from jastudio.ui import dotnet_ui_root

if TYPE_CHECKING:

    from anki.cards import Card
    from JAStudio.Core.Note import JPNote

import aqt
from aqt import AnkiQt  # type: ignore[attr-defined]  # pyright: ignore[reportPrivateImportUsage]
from aqt.browser import Browser  # type: ignore[attr-defined]  # pyright: ignore[reportPrivateImportUsage]
from aqt.browser.previewer import Previewer
from aqt.editcurrent import EditCurrent
from aqt.reviewer import RefreshNeeded
from jaspythonutils.sysutils.typed import non_optional

from jastudio.ankiutils.audio_suppressor import audio_suppressor
from jastudio.ankiutils.ui_utils_interface import IUIUtils


def main_window() -> AnkiQt: return non_optional(aqt.mw)

def is_displaytype_displaying_review_question(display_type: str) -> bool:
    return display_type == "reviewQuestion"

def is_reviewer_display_type(display_type: str) -> bool:
    return display_type.startswith("review")

class UIUtils(IUIUtils, Slots):
    def __init__(self, mw: AnkiQt) -> None:
        self._mw: AnkiQt = mw

    @override
    def is_edit_current_open(self) -> bool:
        edit_current = [window for window in self._mw.app.topLevelWidgets() if isinstance(window, EditCurrent)]
        return len(edit_current) > 0

    @override
    def refresh(self, refresh_browser: bool = True) -> None:
        if not dotnet_ui_root().IsInitialized:
            return

        def force_previewer_rerender() -> None:
            previewers: list[Previewer] = [window for window in self._mw.app.topLevelWidgets() if isinstance(window, Previewer)]
            if len(previewers) > 0:
                previewer = previewers[0]
                # noinspection PyProtectedMember
                previewer._last_state = (previewer._state, non_optional(previewer.card()).id, 0)  # pyright: ignore[reportPrivateUsage]
                previewer.render_card()

        def force_reviewer_rerender() -> None:
            if self._mw.reviewer.card:
                self._mw.reviewer._refresh_needed = RefreshNeeded.NOTE_TEXT  # pyright: ignore[reportPrivateUsage]
                self._mw.reviewer.refresh_if_needed()

        def force_browser_rerender() -> None:
            browser = self._try_find_browser_window()
            if browser:
                browser.onSearchActivated()  # pyright: ignore[reportUnknownMemberType]

        if refresh_browser:
            force_browser_rerender()

        audio_suppressor.suppress_for_seconds(.3)
        force_reviewer_rerender()
        force_previewer_rerender()

    @override
    def activate_preview(self) -> None:
        browser: Browser = aqt.dialogs.open('Browser', self._mw)  # noqa  # pyright: ignore[reportAny]
        self._mw.app.processEvents()
        if browser._previewer is None:  # noqa  # pyright: ignore[reportPrivateUsage]
            browser.onTogglePreview()
        else:
            browser._previewer.activateWindow()  # noqa  # pyright: ignore[reportPrivateUsage]

    @override
    def is_preview_open(self) -> bool:
        browser = self._try_find_browser_window()
        return browser is not None and browser._previewer is not None  # pyright: ignore [reportPrivateUsage]

    def _try_find_browser_window(self) -> Browser | None:
        return query(self._mw.app.topLevelWidgets()).of_type(Browser).single_or_none()

# noinspection PyTypeHints
def try_get_review_note() -> JPNote | None:
    return try_get_note_of(main_window().reviewer.card)

# noinspection PyTypeHints
def try_get_note_of(card: Card | None) -> JPNote | None:
    """The note whose study-note holds the card, or None with no card, and when no note has that study-note."""
    if not card: return None
    return dotnet_ui_root().Services.CoreApp.Collection.NoteOfTheStudyNote(card.nid or card.note().id)
