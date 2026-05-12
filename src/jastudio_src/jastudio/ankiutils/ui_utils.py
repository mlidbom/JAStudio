from __future__ import annotations

from typing import TYPE_CHECKING, override

from autoslot import Slots
from typed_linq_collections.q_iterable import query

from jastudio.ui import dotnet_ui_root

if TYPE_CHECKING:

    from anki.notes import Note
    from JAStudio.Core.Note import JPNote

import aqt
from aqt import AnkiQt  # type: ignore[attr-defined]  # pyright: ignore[reportPrivateImportUsage]
from aqt.browser import Browser  # type: ignore[attr-defined]  # pyright: ignore[reportPrivateImportUsage]
from aqt.browser.previewer import Previewer
from aqt.clayout import CardLayout
from aqt.editcurrent import EditCurrent
from aqt.editor import Editor
from aqt.reviewer import RefreshNeeded
from aqt.webview import AnkiWebView, AnkiWebViewKind
from jaspythonutils.sysutils.typed import checked_cast, non_optional

from jastudio.ankiutils.audio_suppressor import audio_suppressor
from jastudio.ankiutils.ui_utils_interface import IUIUtils


def main_window() -> AnkiQt: return non_optional(aqt.mw)

def is_displaytype_displaying_review_question(display_type: str) -> bool:
    return display_type == "reviewQuestion"

def is_reviewer_display_type(display_type: str) -> bool:
    return display_type.startswith("review")

# noinspection PyTypeHints
def get_note_from_web_view(view: AnkiWebView) -> JPNote | None:
    inner_note: Note | None

    if view.kind == AnkiWebViewKind.MAIN:
        card = main_window().reviewer.card
        if card:
            inner_note = non_optional(main_window().reviewer.card).note()
        else:
            return None
    elif view.kind == AnkiWebViewKind.EDITOR:
        # noinspection PyProtectedMember
        editor = checked_cast(Editor, view._bridge_context)  # pyright: ignore[reportPrivateUsage]
        if not editor.card: return None
        card = non_optional(editor.card)
        inner_note = non_optional(card.note())
    elif view.kind == AnkiWebViewKind.PREVIEWER:
        inner_note = non_optional([window for window in main_window().app.topLevelWidgets() if isinstance(window, Previewer)][0].card()).note()
    elif view.kind == AnkiWebViewKind.CARD_LAYOUT:
        inner_note = non_optional([window for window in main_window().app.topLevelWidgets() if isinstance(window, CardLayout)][0].note)
    else:
        return None

    return dotnet_ui_root().Services.CoreApp.Collection.NoteFromExternalId(inner_note.id)

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

        audio_suppressor.suppress_for_seconds(.3)
        force_reviewer_rerender()
        force_previewer_rerender()

        if refresh_browser:
            force_browser_rerender()

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
    card = main_window().reviewer.card
    if not card: return None
    return dotnet_ui_root().Services.CoreApp.Collection.NoteFromExternalId(card.nid or card.note().id)
