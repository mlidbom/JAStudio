"""Anki for the specifications of the HTTP server for JAStudio, and JAStudio as it calls the server."""
from __future__ import annotations

import http.client
import json
import shutil
from dataclasses import dataclass
from pathlib import Path
from typing import TYPE_CHECKING, cast
from urllib.parse import quote

from anki.collection import Collection

if TYPE_CHECKING:
    from anki.collection import OpChanges, OpChangesWithCount
    from jastudio.anki_http.http_server_for_jastudio import HttpServerForJAStudio

THE_TEST_COLLECTION = Path(__file__).parent.parent.parent / "collection.anki2"


class WindowsThatRecordWhatTheyAreAskedFor:
    """Anki's windows for the specifications, which have none: each call is recorded where Anki's windows would carry it out."""

    def __init__(self) -> None:
        self.changes_told_of: list[OpChanges | OpChangesWithCount] = []
        self.asked_for: list[str] = []
        """What they were asked for other than to follow a change, in order, each in words, such as "open the browser on: nid:215"."""

    def tell_of(self, changes: OpChanges | OpChangesWithCount) -> None: self.changes_told_of.append(changes)

    def open_the_browser_on(self, search: str) -> None: self.asked_for.append(f"open the browser on: {search}")

    def open_the_browser_and_the_previewer_on(self, search: str) -> None: self.asked_for.append(f"open the browser and the previewer on: {search}")

    def show_a_tooltip(self, message: str, milliseconds: int) -> None: self.asked_for.append(f"show a tooltip for {milliseconds} ms: {message}")

    def refresh(self) -> None: self.asked_for.append("refresh")


class AnkiWithoutItsWindow:
    """A copy of the test collection, opened with Anki's own library, and no window: what the HTTP server for JAStudio needs of Anki.

    The test collection has 1,217 notes, of JAStudio's three note types, and 1,749 cards, none suspended. Its vocab note 215 has the tags
    -::vocab::has-no-studying-sentences, legacy_scheduling and source::wani::level::04.
    """

    def __init__(self, folder: Path) -> None:
        self.collection_path: Path = folder / "collection.anki2"
        shutil.copy(THE_TEST_COLLECTION, self.collection_path)
        self._collection: Collection = Collection(str(self.collection_path))
        self.its_own_sync_is_under_way: bool = False
        self.its_windows: WindowsThatRecordWhatTheyAreAskedFor = WindowsThatRecordWhatTheyAreAskedFor()

    def why_the_collection_cannot_be_used_now(self) -> str | None: return "Anki is syncing." if self.its_own_sync_is_under_way else None

    def collection(self) -> Collection: return self._collection

    def windows(self) -> WindowsThatRecordWhatTheyAreAskedFor: return self.its_windows

    def close(self) -> None: self._collection.close()


@dataclass(frozen=True)
class Answered:
    """What the server answered a request with: its status, and its body, read as the JSON it is."""
    status: int
    body: dict[str, object]


class JAStudioCallingAnki:
    """JAStudio as it calls the server: with the server's token, naming the collection it works with."""

    def __init__(self, server: HttpServerForJAStudio, collection: Path) -> None:
        self._server: HttpServerForJAStudio = server
        self.collection: Path = collection

    def get(self, path: str) -> Answered: return self._answer_to("GET", path, self._carrying_the_token())

    def get_without_the_token(self, path: str) -> Answered: return self._answer_to("GET", path, {})

    def header_of_the_answer_to_get(self, path: str, header: str) -> str | None:
        """The value of the header in the server's answer to a GET of the path, or None when the answer has no such header."""
        connection = http.client.HTTPConnection("127.0.0.1", self._server.port)
        try:
            connection.request("GET", path, headers=self._carrying_the_token())
            response = connection.getresponse()
            _ = response.read()
            return response.getheader(header)
        finally:
            connection.close()

    def call(self, function: str, **arguments: object) -> Answered: return self.call_expecting(str(self.collection), function, **arguments)

    def call_expecting(self, collection: str | None, function: str, **arguments: object) -> Answered:
        """Calls the function, naming the collection JAStudio expects Anki to have open, or none."""
        naming_the_collection = "" if collection is None else f"?collection={quote(collection, safe='')}"
        return self._answer_to("POST", f"/call/{function}{naming_the_collection}", self._carrying_the_token(), json.dumps(arguments).encode("utf-8"))

    def result_of(self, function: str, **arguments: object) -> object:
        """What the function answers with, which must be a result."""
        answered = self.call(function, **arguments)
        assert answered.status == 200, answered
        return answered.body["result"]

    def _carrying_the_token(self) -> dict[str, str]: return {"Authorization": f"Bearer {self._server.token}"}

    def _answer_to(self, method: str, path: str, headers: dict[str, str], body: bytes | None = None) -> Answered:
        connection = http.client.HTTPConnection("127.0.0.1", self._server.port)
        try:
            connection.request(method, path, body=body, headers=headers)
            response = connection.getresponse()
            return Answered(response.status, cast("dict[str, object]", json.loads(response.read())))
        finally:
            connection.close()
