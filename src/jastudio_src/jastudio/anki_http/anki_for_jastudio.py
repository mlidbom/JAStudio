"""Anki as the HTTP server for JAStudio reaches it: its open collection, whether JAStudio may use the collection right now, and its windows.

Inside Anki it's Anki with its main window (AnkiWithItsWindow). For the specifications it's a collection file opened with Anki's own library,
without Anki's window.
"""
from __future__ import annotations

from typing import TYPE_CHECKING, Protocol

if TYPE_CHECKING:
    from anki.collection import Collection, OpChanges, OpChangesWithCount


class AnkisWindows(Protocol):
    """Anki's own windows: its reviewer, browser and previewer. Each call returns at once: inside Anki the work is done on Anki's main thread
    afterwards."""

    def tell_of(self, changes: OpChanges | OpChangesWithCount) -> None:
        """Tells the windows of a change JAStudio made as an Anki operation, as Anki does after each of its own: its undo menu, its browser
        and its reviewer follow it."""
        ...

    def open_the_browser_on(self, search: str) -> None:
        """Opens Anki's browser, if it isn't open, and runs the search in it: a search in Anki's search syntax."""
        ...

    def open_the_browser_and_the_previewer_on(self, search: str) -> None:
        """As open_the_browser_on, and opens Anki's previewer on what the search finds."""
        ...

    def show_a_tooltip(self, message: str, milliseconds: int) -> None:
        """Shows the message in Anki's tooltip for that long."""
        ...

    def refresh(self) -> None:
        """Has the reviewer, the previewer and the browser draw what they show again."""
        ...


class AnkiForJAStudio(Protocol):
    def why_the_collection_cannot_be_used_now(self) -> str | None:
        """Why JAStudio may not use Anki's collection right now, in words for a person, or None when it may.

        It may not while no Anki profile is open, since Anki then has no collection open, and while Anki's own sync, with AnkiWeb, is under
        way: the sync holds the collection, and a full sync replaces it.
        """
        ...

    def collection(self) -> Collection:
        """The collection Anki has open. Only while JAStudio may use it."""
        ...

    def windows(self) -> AnkisWindows: ...
