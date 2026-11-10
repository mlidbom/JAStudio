from __future__ import annotations

from typing import TYPE_CHECKING

from jastudio.ankiutils import app

if TYPE_CHECKING:
    from anki.notes import Note, NoteId
    from JAStudio.Core.Note import JPNote

class JPNoteDataShim:
    @classmethod
    def write_study_note_fields(cls, jp_note: JPNote, note: Note) -> None:
        """Writes the fields JAStudio writes into the note's study-note. Its tags are the student's, which JAStudio never writes."""
        for key_value_pair in jp_note.StudyNoteFields():
            # str() converts pythonnet System.String to native Python str, which Anki's protobuf serialization requires.
            note[str(key_value_pair.Key)] = str(key_value_pair.Value)

    @classmethod
    def write_study_note(cls, jp_note: JPNote, anki_note_id: NoteId) -> None:
        """Writes the note's study-note into its Anki note."""
        anki_note = app.anki_collection().get_note(anki_note_id)
        cls.write_study_note_fields(jp_note, anki_note)
        app.anki_collection().update_note(anki_note)
