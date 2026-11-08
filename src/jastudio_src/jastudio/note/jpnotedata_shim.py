from __future__ import annotations

from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from anki.notes import Note
    from JAStudio.Core.Note import JPNote

class JPNoteDataShim:
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
