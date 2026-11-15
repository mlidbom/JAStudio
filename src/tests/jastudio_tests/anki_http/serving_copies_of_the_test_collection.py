"""Serves copies of the test collection over HTTP, each as the Anki add-on serves JAStudio the collection Anki has open, to the .NET
specifications of JAStudio's calls to Anki over HTTP (JAStudio.Core.Specifications: AnkisHttpServersForSpecifications), which run this file as a
script.

It reads commands from standard input, a line each, and answers each with a line of JSON on standard output:

    start                             starts serving a new copy of the test collection: {"id", "accessFile", "collection"}
    stop <id>                         stops that serving, and removes its copy of the collection and its access file's folder
    ankis-own-sync-starts <id>        has that server refuse calls, as while Anki's own sync is under way
    ankis-own-sync-finishes <id>      has it serve them again
    access-file-of-this-user          the access file a JAStudio of this user reads, as the add-on names it: {"accessFile"}

It ends when standard input ends, as it does when the specifications' process ends.
"""
from __future__ import annotations

import json
import shutil
import sys
import tempfile
from dataclasses import dataclass
from pathlib import Path
from typing import TYPE_CHECKING, TextIO

if TYPE_CHECKING:
    from jastudio.anki_http.serving_jastudio import ServingJAStudio
    from jastudio_tests.anki_http.anki_without_its_window import AnkiWithoutItsWindow


@dataclass(frozen=True)
class ServedCopy:
    """A copy of the test collection in a folder of its own, and the add-on's serving of it, whose access file is in another folder of its own."""
    folder: Path
    access_file: Path
    anki: AnkiWithoutItsWindow
    serving: ServingJAStudio

    @staticmethod
    def start() -> ServedCopy:
        from jastudio.anki_http.serving_jastudio import ServingJAStudio
        from jastudio_tests.anki_http.anki_without_its_window import AnkiWithoutItsWindow

        folder = Path(tempfile.mkdtemp(prefix="jastudio-anki-http-"))
        access_file = Path(tempfile.mkdtemp(prefix="jastudio-anki-http-access-")) / "anki-http-server-access.json"
        anki = AnkiWithoutItsWindow(folder)
        serving = ServingJAStudio(anki, access_file)
        serving.start()
        return ServedCopy(folder, access_file, anki, serving)

    def stop(self) -> None:
        self.serving.stop()
        self.anki.close()
        shutil.rmtree(self.folder, ignore_errors=True)
        shutil.rmtree(self.access_file.parent, ignore_errors=True)


class ServedCopies:
    """The copies being served, each by the id its start answered with."""

    def __init__(self) -> None:
        self._by_id: dict[int, ServedCopy] = {}
        self._last_id: int = 0

    def answer_to(self, command: str, argument: str) -> dict[str, object]:
        if command == "start":
            self._last_id += 1
            copy = ServedCopy.start()
            self._by_id[self._last_id] = copy
            return {"id": self._last_id, "accessFile": str(copy.access_file), "collection": str(copy.anki.collection_path)}
        if command == "stop":
            self._by_id.pop(int(argument)).stop()
            return {"stopped": int(argument)}
        if command in ("ankis-own-sync-starts", "ankis-own-sync-finishes"):
            self._by_id[int(argument)].anki.its_own_sync_is_under_way = command == "ankis-own-sync-starts"
            return {"ankisOwnSyncIsUnderWay": command == "ankis-own-sync-starts"}
        if command == "access-file-of-this-user":
            from jastudio.anki_http.serving_jastudio import the_access_file_of_this_user
            return {"accessFile": str(the_access_file_of_this_user())}
        raise ValueError(f"There is no command {command}.")

    def stop_all(self) -> None:
        for copy in self._by_id.values():
            copy.stop()
        self._by_id.clear()


def make_the_addons_code_and_the_tests_importable() -> None:
    """Puts the folders pytest.ini names as its pythonpath first among those Python imports from: this file is run as a script, not by pytest."""
    source = Path(__file__).resolve().parents[3]
    sys.path[:0] = [str(source / "jastudio_src"), str(source / "jaspythonutils_src"), str(source / "tests")]


def answer_the_commands(commands: TextIO, answers: TextIO) -> None:
    served = ServedCopies()
    try:
        for line in commands:
            command, _, argument = line.strip().partition(" ")
            print(json.dumps(served.answer_to(command, argument)), file=answers, flush=True)
    finally:
        served.stop_all()


if __name__ == "__main__":
    make_the_addons_code_and_the_tests_importable()
    the_answers = sys.stdout
    sys.stdout = sys.stderr  # whatever else prints must not land among the answers
    answer_the_commands(sys.stdin, the_answers)
