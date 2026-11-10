"""Tells JAStudio when Anki changed notes during a session, so that the study-notes' tags, which JAStudio shows below anki::, show a change at once
rather than at the next load: marking a note, tags edited in Anki's browser, or a review that made a card a leech, which tags its note. A sync loads
the collection again, which reads every study-note's tags, so it needs nothing here."""
from __future__ import annotations

import time
from typing import TYPE_CHECKING, cast

from aqt import gui_hooks

from jastudio.ankiutils import app

if TYPE_CHECKING:
    from anki.collection import OpChanges


class _NotesChangedSince:
    """The time from which the notes Anki changed are yet to be looked at: when they were last looked at. Anki stores a note's modification time in
    whole seconds, so the notes changed in that second are looked at again, which changes nothing."""
    seconds: int = int(time.time())


def _hand_over_the_tags_of_the_notes_changed(changes: OpChanges, _handler: object | None) -> None:
    if not changes.note: return
    looked_at = int(time.time())
    changed = cast("list[tuple[int, str]]", app.anki_db().all("select id, tags from notes where mod >= ?", _NotesChangedSince.seconds))  # the shape the query's columns give its rows
    _NotesChangedSince.seconds = looked_at
    from jastudio.ui import dotnet_ui_root
    collection = dotnet_ui_root().Services.CoreApp.Collection
    for anki_note_id, tags in changed:
        collection.UpdateTheAnkiTagsOfAStudyNote(anki_note_id, tags)

def init() -> None:
    gui_hooks.operation_did_execute.append(_hand_over_the_tags_of_the_notes_changed)  # pyright: ignore[reportUnknownMemberType]  # Anki's generated hook names its parameter "anki.collection.OpChanges" in a module that never runs `import anki.collection`, so pyright can't resolve it
