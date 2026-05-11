from __future__ import annotations

from typing import TYPE_CHECKING

from anki.notes import Note
from jastudio.ankiutils import app
from JAStudio.Core.Note import NoteTypes
from jastudio.note import studing_status_helper
from jastudio.note.jpnotedata_shim import JPNoteDataShim

if TYPE_CHECKING:
    from collections.abc import Callable

    from JAStudio.Core.Note import JPNote, KanjiNote
    from JAStudio.Core.Note.Sentences import SentenceNote
    from JAStudio.Core.Note.Vocabulary import VocabNote

# used from C#
# noinspection PyUnusedClass,PyUnusedFunction
class AnkiBackendNoteCreator:
    @classmethod
    def _create_note(cls, note: JPNote, note_type: str, callback: Callable[[], None]) -> None:
        backend_note = Note(app.anki_collection(), app.anki_collection().models.by_name(note_type))
        JPNoteDataShim.sync_note_to_anki_note(note, backend_note)
        app.anki_collection().addNote(backend_note)
        # Register the Anki ID → domain NoteId mapping before the background flush can
        # fire ExternalNoteAdded, which would otherwise create a duplicate note.
        note.Services.ExternalNoteIdMap.Register(int(backend_note.id), note.GetId())
        callback()
        note.UpdateInCache()
        studing_status_helper.update_note_in_studying_cache(backend_note)

    def create_kanji(self, note: KanjiNote, callback: Callable[[], None]) -> None:
        self._create_note(note, NoteTypes.Kanji, callback)

    def create_vocab(self, note: VocabNote, callback: Callable[[], None]) -> None:
        self._create_note(note, NoteTypes.Vocab, callback)

    def create_sentence(self, note: SentenceNote, callback: Callable[[], None]) -> None:
        self._create_note(note, NoteTypes.Sentence, callback)
