from __future__ import annotations

from typing import TYPE_CHECKING

from jastudio.anki_extentions.note_ex import NoteEx
from jastudio.ankiutils import app
from JAStudio.Core.Note import KanjiNote
from JAStudio.Core.Note.Sentences import SentenceNote
from JAStudio.Core.Note.Vocabulary import VocabNote
from jastudio.note.collection.anki_collection_sync_runner import AnkiCollectionSyncRunner
from jastudio.note.collection.anki_single_collection_syncer import AnkiSingleCollectionSyncer

if TYPE_CHECKING:
    from anki.notes import Note


class AnkiCollectionSynchronizer:
    def __init__(self) -> None:
        from jastudio.ui import dotnet_ui_root
        self._sync_runner: AnkiCollectionSyncRunner = AnkiCollectionSyncRunner(app.anki_collection())
        collection = dotnet_ui_root().Services.CoreApp.Collection
        self.Vocab: AnkiSingleCollectionSyncer[VocabNote] = AnkiSingleCollectionSyncer(VocabNote, collection.Vocab.ExternalSyncHandler, self._sync_runner)
        self.Sentences: AnkiSingleCollectionSyncer[SentenceNote] = AnkiSingleCollectionSyncer(SentenceNote, collection.Sentences.ExternalSyncHandler, self._sync_runner)
        self.Kanji: AnkiSingleCollectionSyncer[KanjiNote] = AnkiSingleCollectionSyncer(KanjiNote, collection.Kanji.ExternalSyncHandler, self._sync_runner)
        self._sync_runner.connect_note_added(_announce_the_added_note)

    def start(self) -> None:
        self._sync_runner.start()

    def stop(self) -> None:
        self._sync_runner.stop()

def _announce_the_added_note(note: Note) -> None:
    """Tells JAStudio that a note of any note type was added, once it's in Anki's database, so that the import profiles of its note type that run automatically can run."""
    from jastudio.ui import dotnet_ui_root
    dotnet_ui_root().ANoteWasAddedInAnki(NoteEx(note).note_type().name)
