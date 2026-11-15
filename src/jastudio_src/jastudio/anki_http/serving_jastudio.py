"""The Anki add-on's serving of JAStudio, for as long as it lasts: its HTTP server, and the access file, which tells a JAStudio how to reach the
server. See dev_docs/moving-jastudio-out-of-anki.md.

The access file holds the server's port and token, as JSON: {"port": ..., "token": ...}. The system picks the port, and the token is new, each
time the serving starts, so a JAStudio reads the file whenever it looks for Anki. The file is there only while the add-on serves.
"""
from __future__ import annotations

import json
import os
import sys
from pathlib import Path
from typing import TYPE_CHECKING

from jaspythonutils.sysutils.typed import non_optional

from jastudio.anki_http.http_server_for_jastudio import HttpServerForJAStudio

if TYPE_CHECKING:
    from jastudio.anki_http.anki_for_jastudio import AnkiForJAStudio


def the_access_file_of_this_user() -> Path:
    """The access file every JAStudio of this user reads, wherever that JAStudio was started from: in the user's own folder for JAStudio's data.

    That folder is where .NET has it on each platform (Environment.SpecialFolder.LocalApplicationData), since JAStudio names the file by it
    (AnkisHttpServerAccessFile.OfThisUser).
    """
    if sys.platform == "win32":
        the_users_local_data = Path(os.environ["LOCALAPPDATA"])
    elif sys.platform == "darwin":
        the_users_local_data = Path.home() / "Library" / "Application Support"
    else:
        the_users_local_data = Path(os.environ.get("XDG_DATA_HOME") or Path.home() / ".local" / "share")
    return the_users_local_data / "JAStudio" / "anki-http-server-access.json"


class ServingJAStudio:
    def __init__(self, anki: AnkiForJAStudio, access_file: Path) -> None:
        self._anki: AnkiForJAStudio = anki
        self._access_file: Path = access_file
        self._server: HttpServerForJAStudio | None = None

    def start(self) -> None:
        """Starts the server, then writes the access file: a JAStudio that finds the file finds the server serving."""
        server = HttpServerForJAStudio(self._anki)
        server.start()
        self._server = server
        _write_for_the_user_alone(self._access_file, json.dumps({"port": server.port, "token": server.token}))

    def stop(self) -> None:
        """Removes the access file, then stops the server."""
        self._access_file.unlink()
        non_optional(self._server).stop()
        self._server = None


def _write_for_the_user_alone(file: Path, text: str) -> None:
    """Writes the file so that only the user may read it, and so that a reader finds either the whole of it or none of it.

    On Windows the folder decides who may read the file: the user's own folder for local data is the user's alone.
    """
    file.parent.mkdir(parents=True, exist_ok=True)
    being_written = file.with_name(f"{file.name}.being-written")
    with os.fdopen(os.open(being_written, os.O_WRONLY | os.O_CREAT | os.O_TRUNC, 0o600), "w", encoding="utf-8") as opened:
        _ = opened.write(text)
    os.replace(being_written, file)
