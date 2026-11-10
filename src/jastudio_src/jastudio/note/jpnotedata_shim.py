from __future__ import annotations

from typing import TYPE_CHECKING

from jastudio.ankiutils import app

if TYPE_CHECKING:
    from anki.notes import Note, NoteId
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

    @classmethod
    def write_study_note(cls, jp_note: JPNote, anki_note_id: NoteId) -> None:
        """Writes the note's study-note into its Anki note: the fields JAStudio writes, and the tags."""
        anki_note = app.anki_collection().get_note(anki_note_id)
        cls.sync_note_to_anki_note(jp_note, anki_note)
        app.anki_collection().update_note(anki_note)
