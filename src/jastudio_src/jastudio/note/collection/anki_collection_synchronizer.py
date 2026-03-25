from __future__ import annotations

from jastudio.ankiutils import app
from JAStudio.Core.Note import KanjiNote, NoteTypes
from JAStudio.Core.Note.Sentences import SentenceNote
from JAStudio.Core.Note.Vocabulary import VocabNote
from jastudio.note.collection.anki_collection_sync_runner import AnkiCollectionSyncRunner
from jastudio.note.collection.anki_single_collection_syncer import AnkiSingleCollectionSyncer


class AnkiCollectionSynchronizer:
    def __init__(self) -> None:
        from jastudio.ui import dotnet_ui_root
        self._sync_runner: AnkiCollectionSyncRunner = AnkiCollectionSyncRunner(app.anki_collection())
        collection = dotnet_ui_root().Services.CoreApp.Collection
        self.Vocab: AnkiSingleCollectionSyncer[VocabNote] = AnkiSingleCollectionSyncer(VocabNote, collection.Vocab.ExternalSyncHandler, self._sync_runner, NoteTypes.Vocab)
        self.Sentences: AnkiSingleCollectionSyncer[SentenceNote] = AnkiSingleCollectionSyncer(SentenceNote, collection.Sentences.ExternalSyncHandler, self._sync_runner, NoteTypes.Sentence)
        self.Kanji: AnkiSingleCollectionSyncer[KanjiNote] = AnkiSingleCollectionSyncer(KanjiNote, collection.Kanji.ExternalSyncHandler, self._sync_runner, NoteTypes.Kanji)

    def start(self) -> None:
        self._sync_runner.start()

    def stop(self) -> None:
        self._sync_runner.stop()
