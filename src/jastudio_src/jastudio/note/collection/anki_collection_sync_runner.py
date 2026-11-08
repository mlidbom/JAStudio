from __future__ import annotations

import threading
from typing import TYPE_CHECKING

from anki import hooks
from autoslot import Slots
from jaspythonutils.sysutils import ex_assert
from jastudio.anki_extentions.notetype_ex.note_type_ex import NoteTypeEx
from JAStudio.Core.Note import NoteTypes
from jastudio.sysutils import app_thread_pool, ex_thread
from typed_linq_collections.collections.q_list import QList

if TYPE_CHECKING:
    from collections.abc import Callable, Sequence

    from anki.collection import Collection
    from anki.decks import DeckId
    from anki.models import ModelManager
    from anki.notes import Note, NoteId

class AnkiCollectionSyncRunner(Slots):
    def __init__(self, anki_collection: Collection) -> None:
        self._added_subscribers: list[Callable[[Note], None]] = []
        self._will_remove_subscribers: list[Callable[[Sequence[NoteId]], None]] = []
        self._anki_collection: Collection = anki_collection
        self._running: bool = False
        self._lock: threading.RLock = threading.RLock()

        self._pending_add: QList[Note] = QList()

        model_manager: ModelManager = anki_collection.models
        all_note_types: list[NoteTypeEx] = [NoteTypeEx.from_dict(model) for model in model_manager.all()]
        self._note_types: list[NoteTypeEx] = [note_type for note_type in all_note_types if note_type.name in NoteTypes.AllList]
        ex_assert.equal(len(self._note_types), NoteTypes.AllList.Count)

        hooks.notes_will_be_deleted.append(self._on_will_be_removed)  # pyright: ignore[reportUnknownMemberType]
        hooks.note_will_be_added.append(self._on_will_be_added)  # pyright: ignore[reportUnknownMemberType]

    def start(self) -> None:
        with self._lock:
            ex_assert.that(not self._running)
            self._running = True
            app_thread_pool.pool.submit(self._run_periodic_flushes)

    def stop(self) -> None:
        with self._lock:
            self._running = False

    def _run_periodic_flushes(self) -> None:
        while self._running:
            self.flush_updates()
            ex_thread.sleep_thread_not_doing_the_current_work(0.1)

    def _internal_flush_updates(self) -> None:
        completely_added_list = self._pending_add.where(lambda it: it.id != 0).to_list()
        self._pending_add = self._pending_add.where(lambda it: it.id == 0).to_list()
        for note in completely_added_list:
            for subscriber in self._added_subscribers: subscriber(note)

    def flush_updates(self) -> None:
        with self._lock:
            self._internal_flush_updates()

    def _on_will_be_added(self, _collection: Collection, backend_note: Note, _deck_id: DeckId) -> None:  # pyright: ignore[reportUnusedParameter]
        if not self._running: return
        with self._lock:
            self._pending_add.append(backend_note)

    def _on_will_be_removed(self, _: Collection, note_ids: Sequence[NoteId]) -> None:
        if not self._running: return
        with self._lock:
            for callback in self._will_remove_subscribers: callback(note_ids)

    def connect_note_added(self, on_added: Callable[[Note], None]) -> None:
        with self._lock:
            self._added_subscribers.append(on_added)

    def connect_will_remove(self, on_will_remove: Callable[[Sequence[NoteId]], None]) -> None:
        with self._lock:
            self._will_remove_subscribers.append(on_will_remove)
