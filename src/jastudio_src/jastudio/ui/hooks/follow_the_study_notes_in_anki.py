"""Tells JAStudio after every Anki operation that changed notes, so that it reads the study-notes from Anki again: their tags, which JAStudio shows
below anki::, the studying statuses of their cards, and which notes have one. Such as marking a note, tags edited in Anki's browser, or a review that
made a card a leech, which tags its note. Anki's undo and redo run as operations too, so they're followed as well, and so is a sync: once it finishes,
Anki fires this hook with every kind of change flagged. A profile opening isn't announced so: JAStudio catches up with it itself
(CatchingUpWithAnki). The operations JAStudio makes itself, such as changing Anki's tags, are left out: JAStudio reads the study-notes again after
each itself."""
from __future__ import annotations

from typing import TYPE_CHECKING

from aqt import gui_hooks

if TYPE_CHECKING:
    from anki.collection import OpChanges


class _JAStudiosOwnOperation:
    """What started an Anki operation JAStudio made itself, such as adding a tag to notes, after which JAStudio reads the study-notes again itself."""

JASTUDIOS_OWN_OPERATION = _JAStudiosOwnOperation()

def _have_jastudio_read_the_study_notes_again(changes: OpChanges, handler: object | None) -> None:
    if not changes.note or handler is JASTUDIOS_OWN_OPERATION: return
    from jastudio.ui import dotnet_ui_root
    dotnet_ui_root().AnkiChangedNotes()

def init() -> None:
    gui_hooks.operation_did_execute.append(_have_jastudio_read_the_study_notes_again)  # pyright: ignore[reportUnknownMemberType]  # Anki's generated hook names its parameter "anki.collection.OpChanges" in a module that never runs `import anki.collection`, so pyright can't resolve it
