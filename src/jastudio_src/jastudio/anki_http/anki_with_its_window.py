"""Anki as it runs, with its main window: what the HTTP server for JAStudio serves inside Anki (AnkiForJAStudio).

wip: what drives the windows is the add-on's code of today, which the functions .NET calls through pythonnet use too (anki_facade_backend), and
which still loads .NET. It's imported only when a window is driven. See dev_docs/moving-jastudio-out-of-anki.md.
"""
from __future__ import annotations

from typing import TYPE_CHECKING

from jaspythonutils.sysutils.typed import non_optional

if TYPE_CHECKING:
    from collections.abc import Callable

    from anki.collection import Collection, OpChanges, OpChangesWithCount
    from aqt.main import AnkiQt

    from jastudio.anki_http.anki_for_jastudio import AnkisWindows


class AnkiWithItsWindow:
    def __init__(self, main_window: AnkiQt) -> None:
        self._main_window: AnkiQt = main_window
        self._windows: _ItsWindows = _ItsWindows(main_window)
        self._its_own_sync_is_under_way: bool = False

    def its_own_sync_starts(self) -> None:
        """Anki's own sync, with AnkiWeb, starts: for Anki's hook sync_will_start."""
        self._its_own_sync_is_under_way = True

    def its_own_sync_has_finished(self) -> None:
        """Anki's own sync has finished: for Anki's hook sync_did_finish."""
        self._its_own_sync_is_under_way = False

    def why_the_collection_cannot_be_used_now(self) -> str | None:
        if self._main_window.col is None:
            return "No Anki profile is open."
        if self._its_own_sync_is_under_way:
            return "Anki's own sync is under way."
        return None

    def collection(self) -> Collection: return non_optional(self._main_window.col)

    def windows(self) -> AnkisWindows: return self._windows


class _ItsWindows:
    """Anki's own windows. Each call hands its work to Anki's main thread, and returns."""

    def __init__(self, main_window: AnkiQt) -> None:
        self._main_window: AnkiQt = main_window

    def tell_of(self, changes: OpChanges | OpChangesWithCount) -> None:
        """None of Anki's windows started the operation, so it has no initiator."""
        def tell_them() -> None:
            from aqt.operations import on_op_finished
            on_op_finished(self._main_window, changes, None)
        self._on_ankis_main_thread(tell_them)

    def open_the_browser_on(self, search: str) -> None:
        def open_it() -> None:
            from jastudio.ankiutils import search_executor
            search_executor.do_lookup(search)
        self._on_ankis_main_thread(open_it)

    def open_the_browser_and_the_previewer_on(self, search: str) -> None:
        def open_them() -> None:
            from jastudio.ankiutils import search_executor
            search_executor.do_lookup_and_show_previewer(search)
        self._on_ankis_main_thread(open_them)

    def show_a_tooltip(self, message: str, milliseconds: int) -> None:
        def show_it() -> None:
            from aqt.utils import tooltip
            tooltip(message, milliseconds)
        self._on_ankis_main_thread(show_it)

    def refresh(self) -> None:
        def refresh_them() -> None:
            from jastudio.ankiutils import app
            app.get_ui_utils().refresh()
        self._on_ankis_main_thread(refresh_them)

    def _on_ankis_main_thread(self, work: Callable[[], None]) -> None: self._main_window.taskman.run_on_main(work)
