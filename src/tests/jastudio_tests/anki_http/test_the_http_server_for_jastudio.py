"""The HTTP server through which JAStudio calls Anki, serving a copy of the test collection opened with Anki's own library: see AnkiWithoutItsWindow."""
from __future__ import annotations

from typing import TYPE_CHECKING, cast

import pytest
from anki.notes import NoteId
from jastudio.anki_http.http_server_for_jastudio import HttpServerForJAStudio
from jastudio_tests.anki_http.anki_without_its_window import AnkiWithoutItsWindow, Answered, JAStudioCallingAnki

if TYPE_CHECKING:
    from collections.abc import Iterator
    from pathlib import Path

A_VOCABS_ANKI_NOTE_ID = 215


@pytest.fixture
def anki(tmp_path: Path) -> Iterator[AnkiWithoutItsWindow]:
    anki = AnkiWithoutItsWindow(tmp_path)
    yield anki
    anki.close()


@pytest.fixture
def jastudio(anki: AnkiWithoutItsWindow) -> Iterator[JAStudioCallingAnki]:
    server = HttpServerForJAStudio(anki)
    server.start()
    yield JAStudioCallingAnki(server, anki.collection_path)
    server.stop()


def tags_of_the_vocab(anki: AnkiWithoutItsWindow) -> list[str]: return sorted(anki.collection().get_note(NoteId(A_VOCABS_ANKI_NOTE_ID)).tags)


# ── What every request meets ──

def test_a_request_without_the_token_is_refused_as_unauthorized(jastudio: JAStudioCallingAnki) -> None:
    assert jastudio.get_without_the_token("/contract").status == 401


def test_the_contract_is_served_with_its_version(jastudio: JAStudioCallingAnki) -> None:
    assert jastudio.get("/contract") == Answered(200, {"result": {"version": 1}})


def test_the_collection_anki_has_open_is_served_by_its_path(jastudio: JAStudioCallingAnki) -> None:
    assert jastudio.get("/collection") == Answered(200, {"result": {"path": str(jastudio.collection)}})


def test_nothing_is_served_at_another_path(jastudio: JAStudioCallingAnki) -> None:
    assert jastudio.get("/notes").status == 404


def test_a_call_of_a_function_there_is_none_of_is_not_found_and_lists_the_functions(jastudio: JAStudioCallingAnki) -> None:
    answered = jastudio.call("notes_delete_everything")
    assert answered.status == 404
    assert "notes_find" in cast("str", answered.body["error"])


def test_a_call_whose_arguments_do_not_fit_the_function_is_a_bad_request_saying_how(jastudio: JAStudioCallingAnki) -> None:
    answered = jastudio.call("notes_find", query="note:_Vocab")
    assert answered.status == 400
    assert "notes_find(search: 'str')" in cast("str", answered.body["error"])


def test_a_call_that_names_no_collection_is_a_bad_request(jastudio: JAStudioCallingAnki) -> None:
    assert jastudio.call_expecting(None, "notes_find", search="note:_Vocab").status == 400


def test_a_call_that_expects_another_collection_is_refused_with_the_one_anki_has_open(jastudio: JAStudioCallingAnki) -> None:
    another_collection = str(jastudio.collection.parent / "another" / "collection.anki2")
    assert jastudio.call_expecting(another_collection, "notes_find", search="note:_Vocab") == Answered(
        409, {"refused": "Anki has another collection open than the one JAStudio works with.", "collectionOpen": str(jastudio.collection)})


def test_a_function_that_fails_answers_with_what_failed(jastudio: JAStudioCallingAnki) -> None:
    answered = jastudio.call("study_note_write", anki_note_id=A_VOCABS_ANKI_NOTE_ID, fields={"a field the note type has not": "x"})
    assert answered.status == 500
    assert "KeyError" in cast("str", answered.body["error"])


def test_while_ankis_own_sync_is_under_way_calls_and_the_collection_are_refused_saying_so_and_the_contract_is_served(anki: AnkiWithoutItsWindow, jastudio: JAStudioCallingAnki) -> None:
    anki.its_own_sync_is_under_way = True
    assert jastudio.call("notes_find", search="note:_Vocab") == Answered(409, {"refused": "Anki is syncing."})
    assert jastudio.get("/collection") == Answered(409, {"refused": "Anki is syncing."})
    assert jastudio.get("/contract").status == 200


# ── The functions ──

def test_notes_find_answers_with_the_ids_of_the_notes_the_search_finds(jastudio: JAStudioCallingAnki) -> None:
    ids = cast("list[int]", jastudio.result_of("notes_find", search="note:_Vocab"))
    assert len(ids) == 253
    assert A_VOCABS_ANKI_NOTE_ID in ids


def test_col_read_json_answers_with_the_json_the_select_builds_as_json(jastudio: JAStudioCallingAnki) -> None:
    select = f"select json_group_array(json_object('id', id, 'noteTypeId', mid)) from notes where id = {A_VOCABS_ANKI_NOTE_ID}"
    rows = cast("list[dict[str, int]]", jastudio.result_of("col_read_json", select=select))
    assert [row["id"] for row in rows] == [A_VOCABS_ANKI_NOTE_ID]


def test_study_note_write_writes_the_fields_given_and_japanese_text_arrives_whole(anki: AnkiWithoutItsWindow, jastudio: JAStudioCallingAnki) -> None:
    a_field = anki.collection().get_note(NoteId(A_VOCABS_ANKI_NOTE_ID)).keys()[0]
    jastudio.result_of("study_note_write", anki_note_id=A_VOCABS_ANKI_NOTE_ID, fields={a_field: "勉強"})
    assert anki.collection().get_note(NoteId(A_VOCABS_ANKI_NOTE_ID))[a_field] == "勉強"
    rows = cast("list[dict[str, str]]", jastudio.result_of("col_read_json", select=f"select json_group_array(json_object('fields', flds)) from notes where id = {A_VOCABS_ANKI_NOTE_ID}"))
    assert rows[0]["fields"].startswith("勉強")


def test_tags_add_to_notes_adds_the_tag_and_tells_ankis_windows_of_the_operation(anki: AnkiWithoutItsWindow, jastudio: JAStudioCallingAnki) -> None:
    jastudio.result_of("tags_add_to_notes", note_ids=[A_VOCABS_ANKI_NOTE_ID], tag="flagged")
    assert "flagged" in tags_of_the_vocab(anki)
    assert len(anki.changes_its_windows_were_told_of) == 1


def test_tags_remove_from_notes_removes_exactly_the_tag(anki: AnkiWithoutItsWindow, jastudio: JAStudioCallingAnki) -> None:
    jastudio.result_of("tags_remove_from_notes", note_ids=[A_VOCABS_ANKI_NOTE_ID], tag="source::wani")
    jastudio.result_of("tags_remove_from_notes", note_ids=[A_VOCABS_ANKI_NOTE_ID], tag="legacy_scheduling")
    assert tags_of_the_vocab(anki) == ["-::vocab::has-no-studying-sentences", "source::wani::level::04"]


def test_tags_rename_renames_the_tag_and_every_tag_below_it(anki: AnkiWithoutItsWindow, jastudio: JAStudioCallingAnki) -> None:
    jastudio.result_of("tags_rename", path="source::wani", renamed_path="source::wanikani")
    assert "source::wanikani::level::04" in tags_of_the_vocab(anki)


def test_tags_delete_removes_the_tag_and_every_tag_below_it_from_every_note(jastudio: JAStudioCallingAnki) -> None:
    assert cast("int", jastudio.result_of("tags_number_of_notes_with_a_tag_in", path="source::wani")) > 0
    jastudio.result_of("tags_delete", path="source::wani")
    assert jastudio.result_of("tags_number_of_notes_with_a_tag_in", path="source::wani") == 0


def test_notes_add_tag_gives_the_notes_the_tag_without_telling_ankis_windows(anki: AnkiWithoutItsWindow, jastudio: JAStudioCallingAnki) -> None:
    jastudio.result_of("notes_add_tag", note_ids=[A_VOCABS_ANKI_NOTE_ID], tag="jastudio::imported")
    assert "jastudio::imported" in tags_of_the_vocab(anki)
    assert anki.changes_its_windows_were_told_of == []


def test_note_suspend_all_cards_suspends_them_and_note_unsuspend_all_cards_unsuspends_them(anki: AnkiWithoutItsWindow, jastudio: JAStudioCallingAnki) -> None:
    suspended_cards_of_the_vocab = f"nid:{A_VOCABS_ANKI_NOTE_ID} is:suspended"
    jastudio.result_of("note_suspend_all_cards", note_id=A_VOCABS_ANKI_NOTE_ID)
    assert len(anki.collection().find_cards(suspended_cards_of_the_vocab)) == len(anki.collection().card_ids_of_note(NoteId(A_VOCABS_ANKI_NOTE_ID))) > 0
    jastudio.result_of("note_unsuspend_all_cards", note_id=A_VOCABS_ANKI_NOTE_ID)
    assert len(anki.collection().find_cards(suspended_cards_of_the_vocab)) == 0


def test_note_remove_and_notes_remove_delete_the_notes(jastudio: JAStudioCallingAnki) -> None:
    two_other_vocab = [note_id for note_id in cast("list[int]", jastudio.result_of("notes_find", search="note:_Vocab")) if note_id != A_VOCABS_ANKI_NOTE_ID][:2]
    jastudio.result_of("note_remove", note_id=A_VOCABS_ANKI_NOTE_ID)
    jastudio.result_of("notes_remove", note_ids=two_other_vocab)
    assert len(cast("list[int]", jastudio.result_of("notes_find", search="note:_Vocab"))) == 250


def test_get_note_id_from_card_id_answers_with_the_id_of_the_cards_note(anki: AnkiWithoutItsWindow, jastudio: JAStudioCallingAnki) -> None:
    a_card_of_the_vocab = int(anki.collection().card_ids_of_note(NoteId(A_VOCABS_ANKI_NOTE_ID))[0])
    assert jastudio.result_of("get_note_id_from_card_id", card_id=a_card_of_the_vocab) == A_VOCABS_ANKI_NOTE_ID


def test_media_add_file_copies_the_file_into_ankis_media_folder_and_answers_with_its_name_there(tmp_path: Path, jastudio: JAStudioCallingAnki) -> None:
    a_file = tmp_path / "recording.mp3"
    a_file.write_bytes(b"ID3")
    name_in_anki = cast("str", jastudio.result_of("media_add_file", file_path=str(a_file)))
    media_folder = cast("str", jastudio.result_of("anki_media_dir"))
    assert name_in_anki == "recording.mp3"
    assert (a_file.parent / media_folder / name_in_anki).read_bytes() == b"ID3"
