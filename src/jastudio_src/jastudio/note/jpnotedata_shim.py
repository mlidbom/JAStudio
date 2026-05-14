from __future__ import annotations

from typing import TYPE_CHECKING

from JAStudio.Anki.PythonInterop import NoteDataPythonAdapter

if TYPE_CHECKING:
    from anki.notes import Note
    from JAStudio.Core.Note import AnkiNoteData, JPNote

class JPNoteDataShim:
    @classmethod
    def from_note(cls, note: Note) -> AnkiNoteData:
        fields: dict[str, str] = {}
        for name in note._fmap:  # pyright: ignore [reportPrivateUsage]
            fields[name] = note[name]

        jp_note_data = JPNoteData(note.id, fields, note.tags)

        return NoteDataPythonAdapter.FromPython(jp_note_data)

    @classmethod
    def sync_note_to_anki_note(cls, jp_note: JPNote, note: Note) -> None:
        data = jp_note.GetData()
        # str() converts pythonnet System.String to native Python str, which Anki's
        # protobuf serialization requires.
        note.tags = [str(tag) for tag in data.Tags]
        for key_value_pair in data.Fields:
            note[str(key_value_pair.Key)] = str(key_value_pair.Value)
        # Persist the domain NoteId into the Anki field so it survives across sessions.
        note["jas_note_id"] = str(jp_note.GetId())

class JPNoteData:
    def __init__(self, id: int, fields: dict[str, str], tags: list[str]) -> None:
        self.id: int = id
        self.fields: dict[str, str] = fields
        self.tags: list[str] = tags
