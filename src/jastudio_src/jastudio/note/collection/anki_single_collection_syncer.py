from __future__ import annotations

from typing import TYPE_CHECKING

from anki.notes import NoteId
from autoslot import Slots
from JAStudio.Anki.PythonInterop import NoteUpdatePythonAdapter
from jastudio.anki_extentions.note_ex import NoteEx
from jastudio.ankiutils import app
from JAStudio.Core.Note import JPNote
from jastudio.note.jpnotedata_shim import JPNoteDataShim

if TYPE_CHECKING:
    from collections.abc import Sequence

    from anki.notes import Note
    from JAStudio.Core.Note.Collection import IExternalNoteUpdateHandler
    from jastudio.note.collection.anki_collection_sync_runner import AnkiCollectionSyncRunner

class AnkiSingleCollectionSyncer[TNote: JPNote](Slots):
    def __init__(self, cached_note_type: type[TNote], anki_sync_handler: IExternalNoteUpdateHandler, cache_runner: AnkiCollectionSyncRunner, note_type_name: str) -> None:
        self._note_type: type[TNote] = cached_note_type
        self._anki_sync_handler: IExternalNoteUpdateHandler = anki_sync_handler
        self._note_type_name: str = note_type_name
        self._is_updating_anki_note: bool = False

        NoteUpdatePythonAdapter.Register(self._anki_sync_handler, self._update_anki_note)

        cache_runner.connect_will_remove(self._on_will_be_removed)
        cache_runner.connect_note_addded(self._on_added)
        cache_runner.connect_will_flush(self._on_will_flush)

    def _is_my_note_type(self, backend_note: Note) -> bool:
        return NoteEx(backend_note).note_type().name == self._note_type_name

    def _update_anki_note(self, note: TNote) -> None:
        self._is_updating_anki_note = True
        try:
            anki_id = self._anki_sync_handler.GetExternalNoteId(note.GetId())
            if anki_id:
                anki_note = app.anki_collection().get_note(NoteId(anki_id))
                JPNoteDataShim.sync_note_to_anki_note(note, anki_note)
                app.anki_collection().update_note(anki_note)
        finally:
            self._is_updating_anki_note = False

    def _on_will_flush(self, backend_note: Note) -> None:
        if self._is_updating_anki_note: return
        if not self._is_my_note_type(backend_note): return
        if not backend_note.id: return
        note_data = JPNoteDataShim.from_note(backend_note)
        self._anki_sync_handler.ExternalNoteWillFlush(int(backend_note.id), note_data)

    def _on_added(self, backend_note: Note) -> None:
        if not self._is_my_note_type(backend_note): return
        if not backend_note.id: return
        note_data = JPNoteDataShim.from_note(backend_note)
        self._anki_sync_handler.ExternalNoteAdded(int(backend_note.id), note_data)

    def _on_will_be_removed(self, note_ids: Sequence[NoteId]) -> None:
        for note_id in note_ids:
            self._anki_sync_handler.ExternalNoteRemoved(int(note_id))
