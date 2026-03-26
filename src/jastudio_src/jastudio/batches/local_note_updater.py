from __future__ import annotations


def convert_immersion_kit_sentences() -> None:
    pass
    # todo migration implement against new types
    # def convert_note(note_id: NoteId) -> None:
    #     immersion_kit_note = app.anki_collection().get_note(note_id)
    #     import jastudio.note.sentences.ankisentencenote
    #
    #     jastudio.note.sentences.ankisentencenote.AnkiSentenceNote.import_immersion_kit_sentence(immersion_kit_note)
    #     app.anki_collection().remove_notes([note_id])
    #
    # with TaskRunner.current("Converting immersion kit sentences") as runner:
    #     immersion_kit_sences = list(app.anki_collection().find_notes(query_builder.immersion_kit_sentences()))
    #     runner.process_with_progress(immersion_kit_sences, convert_note, "Converting immersion kit sentences")