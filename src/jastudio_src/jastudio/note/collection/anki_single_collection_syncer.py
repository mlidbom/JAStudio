from __future__ import annotations

from typing import TYPE_CHECKING

from anki.notes import NoteId
from autoslot import Slots
from JAStudio.Anki.PythonInterop import NoteUpdatePythonAdapter
from jastudio.ankiutils import app
from JAStudio.Core.Note import JPNote
from jastudio.note.jpnotedata_shim import JPNoteDataShim

if TYPE_CHECKING:
    from collections.abc import Sequence

    from JAStudio.Core.Note.Collection import IExternalNoteUpdateHandler
    from jastudio.note.collection.anki_collection_sync_runner import AnkiCollectionSyncRunner

class AnkiSingleCollectionSyncer[TNote: JPNote](Slots):
    def __init__(self, cached_note_type: type[TNote], anki_sync_handler: IExternalNoteUpdateHandler, cache_runner: AnkiCollectionSyncRunner) -> None:
        self._note_type: type[TNote] = cached_note_type
        self._anki_sync_handler: IExternalNoteUpdateHandler = anki_sync_handler

        NoteUpdatePythonAdapter.Register(self._anki_sync_handler, self._update_anki_note)

        cache_runner.connect_will_remove(self._on_will_be_removed)

    def _update_anki_note(self, note: TNote) -> None:
        anki_id = self._anki_sync_handler.GetExternalNoteId(note.GetId())
        if anki_id:
            anki_note = app.anki_collection().get_note(NoteId(anki_id))
            JPNoteDataShim.sync_note_to_anki_note(note, anki_note)
            app.anki_collection().update_note(anki_note)

    def _on_will_be_removed(self, note_ids: Sequence[NoteId]) -> None:
        for note_id in note_ids:
            self._anki_sync_handler.ExternalNoteRemoved(int(note_id))
