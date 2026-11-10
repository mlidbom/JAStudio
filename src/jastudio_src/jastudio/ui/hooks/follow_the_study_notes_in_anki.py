"""Tells JAStudio after every Anki operation that changed notes, so that it reads the study-notes from Anki again: their tags, which JAStudio shows
below anki::, the studying statuses of their cards, and which notes have one. Such as marking a note, tags edited in Anki's browser, or a review that
made a card a leech, which tags its note. Anki's undo and redo run as operations too, so they're followed as well. A sync loads the collection again,
which reads every study-note, so it needs nothing here."""
from __future__ import annotations

from typing import TYPE_CHECKING

from aqt import gui_hooks

if TYPE_CHECKING:
    from anki.collection import OpChanges


def _have_jastudio_read_the_study_notes_again(changes: OpChanges, _handler: object | None) -> None:
    if not changes.note: return
    from jastudio.ui import dotnet_ui_root
    dotnet_ui_root().AnkiChangedNotes()

def init() -> None:
    gui_hooks.operation_did_execute.append(_have_jastudio_read_the_study_notes_again)  # pyright: ignore[reportUnknownMemberType]  # Anki's generated hook names its parameter "anki.collection.OpChanges" in a module that never runs `import anki.collection`, so pyright can't resolve it
