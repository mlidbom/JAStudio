"""The functions JAStudio calls in Anki over HTTP that work on Anki's collection, by the names JAStudio calls them by.

Each takes its arguments by the names they have in the request's JSON, and returns what goes back as JSON. They are the collection's part of
jastudio.dotnet.anki_facade_backend, which .NET calls through pythonnet until JAStudio is a program of its own: see
dev_docs/moving-jastudio-out-of-anki.md. They need neither Anki's window nor .NET, so their specifications run on a collection file opened with
Anki's own library.

Each runs on the thread of the request that calls it: Anki's backend allows calls from any thread.
"""
from __future__ import annotations

import re
from typing import TYPE_CHECKING

from anki.collection import SearchNode
from anki.notes import NoteId
from jaspythonutils.sysutils.typed import non_optional, str_

if TYPE_CHECKING:
    from collections.abc import Callable, Mapping, Sequence

    from anki.collection import Collection

    from jastudio.anki_http.anki_for_jastudio import AnkiForJAStudio

type JsonValue = None | bool | int | float | str | Sequence[JsonValue] | Mapping[str, JsonValue]


class JsonText(str):
    """A JSON text a function answers with, which goes back as it is, not as a JSON string holding it: what a SELECT builds can be millions
    of characters."""
    __slots__: tuple[str, ...] = ()


class FunctionsOnTheCollection:
    def __init__(self, anki: AnkiForJAStudio) -> None:
        self._anki: AnkiForJAStudio = anki
        self.by_name: dict[str, Callable[..., JsonValue]] = {
            "notes_find": self.notes_find,
            "notes_add_tag": self.notes_add_tag,
            "notes_remove": self.notes_remove,
            "tags_add_to_notes": self.tags_add_to_notes,
            "tags_remove_from_notes": self.tags_remove_from_notes,
            "tags_rename": self.tags_rename,
            "tags_delete": self.tags_delete,
            "tags_number_of_notes_with_a_tag_in": self.tags_number_of_notes_with_a_tag_in,
            "study_note_write": self.study_note_write,
            "note_suspend_all_cards": self.note_suspend_all_cards,
            "note_unsuspend_all_cards": self.note_unsuspend_all_cards,
            "col_read_json": self.col_read_json,
            "media_add_file": self.media_add_file,
            "anki_media_dir": self.anki_media_dir,
        }
        """Each function by the name JAStudio calls it by."""

    @property
    def _collection(self) -> Collection: return self._anki.collection()

    # ── Notes of any note type ──

    def notes_find(self, search: str) -> list[int]:
        """The ids of the notes the Anki search finds."""
        return [int(note_id) for note_id in self._collection.find_notes(search)]

    def notes_add_tag(self, note_ids: list[int], tag: str) -> None:
        """Gives the notes the tag."""
        self._collection.tags.bulk_add(_note_ids(note_ids), tag)

    def notes_remove(self, note_ids: list[int]) -> None:
        """Permanently deletes the notes from Anki, with their cards and their review history."""
        self._collection.remove_notes(_note_ids(note_ids))

    # ── Tags ──
    # Each change runs as an Anki operation, as the browser runs its own: Anki's Edit > Undo undoes it, and Anki's windows are told of it.

    def tags_add_to_notes(self, note_ids: list[int], tag: str) -> None:
        self._anki.windows().tell_of(self._collection.tags.bulk_add(_note_ids(note_ids), tag))

    def tags_remove_from_notes(self, note_ids: list[int], tag: str) -> None:
        """Removes exactly the tag from the notes, ignoring case as Anki does, and leaves the tags below it, as JAStudio removes a note's own
        tag. Anki's bulk_remove would remove those too, so this is Anki's find and replace of the whole tag with nothing."""
        self._anki.windows().tell_of(self._collection.tags.find_and_replace(_note_ids(note_ids), f"^{re.escape(tag)}$", "", regex=True, match_case=False))

    def tags_rename(self, path: str, renamed_path: str) -> None:
        """Renames the tag and every tag below it, on every note, as the browser's sidebar does."""
        self._anki.windows().tell_of(self._collection.tags.rename(path, renamed_path))

    def tags_delete(self, path: str) -> None:
        """Removes the tag and every tag below it from every note, as the browser's sidebar does."""
        self._anki.windows().tell_of(self._collection.tags.remove(path))

    def tags_number_of_notes_with_a_tag_in(self, path: str) -> int:
        """The number of notes with the tag or a tag below it, as clicking the tag in the browser's sidebar finds them."""
        collection = self._collection
        return len(collection.find_notes(collection.build_search_string(SearchNode(tag=path))))

    # ── Study-notes: the Anki notes of JAStudio's note types ──

    def study_note_write(self, anki_note_id: int, fields: dict[str, str]) -> None:
        """Writes the fields, by name, into the Anki note: those JAStudio writes into a note's study-note. The note's other fields, and its
        tags, stay as they are."""
        collection = self._collection
        anki_note = collection.get_note(NoteId(anki_note_id))
        for name, value in fields.items():
            anki_note[name] = value
        collection.update_note(anki_note)

    def note_suspend_all_cards(self, note_id: int) -> None:
        collection = self._collection
        collection.sched.suspend_cards(collection.card_ids_of_note(NoteId(note_id)))

    def note_unsuspend_all_cards(self, note_id: int) -> None:
        collection = self._collection
        collection.sched.unsuspend_cards(collection.card_ids_of_note(NoteId(note_id)))

    # ── The collection's database ──

    def col_read_json(self, select: str) -> JsonText:
        """The one value the SELECT returns, a JSON text, through Anki's own connection to its collection, so it sees Anki's latest changes.
        Only a SELECT may run through it: Anki discards its undo history whenever anything else does."""
        return JsonText(str_(non_optional(self._collection.db).scalar(select)))  # pyright: ignore[reportAny]  # Anki types every database value as Any: str_ checks that it's the text

    # ── Media ──

    def media_add_file(self, file_path: str) -> str:
        """Copies the file into Anki's media folder, for Anki's playback and media sync, and returns the name Anki keeps it under."""
        return self._collection.media.add_file(file_path)

    def anki_media_dir(self) -> str:
        """Anki's media folder of the open collection."""
        return self._collection.media.dir()


def _note_ids(ids: list[int]) -> list[NoteId]: return [NoteId(note_id) for note_id in ids]
