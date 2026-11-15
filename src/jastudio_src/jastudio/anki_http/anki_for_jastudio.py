"""Anki as the HTTP server for JAStudio reaches it: its open collection, whether JAStudio may use the collection right now, and its windows.

Inside Anki it's Anki's main window. For the specifications it's a collection file opened with Anki's own library, without Anki's window.
"""
from __future__ import annotations

from typing import TYPE_CHECKING, Protocol

if TYPE_CHECKING:
    from anki.collection import Collection, OpChanges, OpChangesWithCount


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

    def tell_the_windows_of(self, changes: OpChanges | OpChangesWithCount) -> None:
        """Tells Anki's windows of a change JAStudio made as an Anki operation, as Anki does after each of its own: its undo menu, its
        browser and its reviewer follow it."""
        ...
