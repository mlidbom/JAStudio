"""The Anki add-on's serving of JAStudio: its HTTP server with the access file that tells a JAStudio how to reach it. On a copy of the test
collection opened with Anki's own library: see AnkiWithoutItsWindow."""
from __future__ import annotations

import http.client
import json
from typing import TYPE_CHECKING, cast

import pytest
from jastudio.anki_http.serving_jastudio import ServingJAStudio, the_access_file_of_this_user
from jastudio_tests.anki_http.anki_without_its_window import AnkiWithoutItsWindow

if TYPE_CHECKING:
    from collections.abc import Iterator
    from pathlib import Path


@pytest.fixture
def anki(tmp_path: Path) -> Iterator[AnkiWithoutItsWindow]:
    collection_folder = tmp_path / "collection"
    collection_folder.mkdir()
    anki = AnkiWithoutItsWindow(collection_folder)
    yield anki
    anki.close()


@pytest.fixture
def access_file(tmp_path: Path) -> Path: return tmp_path / "a folder that isn't there yet" / "anki-http-server-access.json"


def status_of_the_answer_to_a_request_for_the_contract(port: int, token: str) -> int:
    connection = http.client.HTTPConnection("127.0.0.1", port)
    try:
        connection.request("GET", "/contract", headers={"Authorization": f"Bearer {token}"})
        return connection.getresponse().status
    finally:
        connection.close()


def test_while_it_serves_the_access_file_holds_the_port_and_the_token_the_server_answers_to(anki: AnkiWithoutItsWindow, access_file: Path) -> None:
    serving = ServingJAStudio(anki, access_file)
    serving.start()
    try:
        access = cast("dict[str, object]", json.loads(access_file.read_text(encoding="utf-8")))
        assert sorted(access) == ["port", "token"]
        assert status_of_the_answer_to_a_request_for_the_contract(cast("int", access["port"]), cast("str", access["token"])) == 200
    finally:
        serving.stop()


def test_once_it_has_stopped_the_access_file_is_gone_and_the_server_answers_no_more(anki: AnkiWithoutItsWindow, access_file: Path) -> None:
    serving = ServingJAStudio(anki, access_file)
    serving.start()
    access = cast("dict[str, object]", json.loads(access_file.read_text(encoding="utf-8")))
    serving.stop()

    assert not access_file.exists()
    with pytest.raises(ConnectionRefusedError):
        _ = status_of_the_answer_to_a_request_for_the_contract(cast("int", access["port"]), cast("str", access["token"]))


def test_each_start_gives_a_new_token(anki: AnkiWithoutItsWindow, access_file: Path) -> None:
    serving = ServingJAStudio(anki, access_file)
    serving.start()
    first_token = cast("dict[str, object]", json.loads(access_file.read_text(encoding="utf-8")))["token"]
    serving.stop()
    serving.start()
    try:
        assert cast("dict[str, object]", json.loads(access_file.read_text(encoding="utf-8")))["token"] != first_token
    finally:
        serving.stop()


def test_the_access_file_of_this_user_is_in_a_folder_of_jastudios_own() -> None:
    assert the_access_file_of_this_user().parent.name == "JAStudio"
    assert the_access_file_of_this_user().name == "anki-http-server-access.json"
