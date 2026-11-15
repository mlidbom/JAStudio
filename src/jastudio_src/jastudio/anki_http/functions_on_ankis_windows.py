"""The functions JAStudio calls in Anki over HTTP that drive Anki's own windows, by the names JAStudio calls them by.

They are the windows' part of jastudio.dotnet.anki_facade_backend, which .NET calls through pythonnet until JAStudio is a program of its own: see
dev_docs/moving-jastudio-out-of-anki.md. Each returns at once: Anki does the work on its main thread afterwards (AnkisWindows).
"""
from __future__ import annotations

from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from collections.abc import Callable

    from jastudio.anki_http.anki_for_jastudio import AnkisWindows
    from jastudio.anki_http.functions_on_the_collection import JsonValue


class FunctionsOnAnkisWindows:
    def __init__(self, windows: AnkisWindows) -> None:
        self._windows: AnkisWindows = windows
        self.by_name: dict[str, Callable[..., JsonValue]] = {
            "browser_execute_lookup": self.browser_execute_lookup,
            "browser_execute_lookup_and_show_previewer": self.browser_execute_lookup_and_show_previewer,
            "ui_show_tooltip": self.ui_show_tooltip,
            "ui_refresh": self.ui_refresh,
        }
        """Each function by the name JAStudio calls it by."""

    def browser_execute_lookup(self, query: str) -> None:
        """Opens Anki's browser, if it isn't open, and runs the query in it: a search in Anki's search syntax."""
        self._windows.open_the_browser_on(query)

    def browser_execute_lookup_and_show_previewer(self, query: str) -> None:
        """As browser_execute_lookup, and opens Anki's previewer on what the query finds."""
        self._windows.open_the_browser_and_the_previewer_on(query)

    def ui_show_tooltip(self, message: str, period_ms: int) -> None:
        """Shows the message in Anki's tooltip for that many milliseconds."""
        self._windows.show_a_tooltip(message, period_ms)

    def ui_refresh(self) -> None:
        """Has Anki's reviewer, previewer and browser draw what they show again."""
        self._windows.refresh()
