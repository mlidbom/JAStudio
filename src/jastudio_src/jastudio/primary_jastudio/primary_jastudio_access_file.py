"""The primary JAStudio's access file, as the Anki add-on reads it: the file in which Anki's primary JAStudio says how to reach it, as JSON:
{"port": ..., "processId": ...}. The port is its card server's. JAStudio writes it (PrimaryJAStudioAccessFile), whole or not at all, as the
JAStudio becomes the primary one, and removes it as it stops.

A JAStudio that ended without stopping, as one stopped in the debugger does, leaves its file behind, naming a port nothing listens on: a request
then fails with JAStudioDoesNotAnswer, until the next JAStudio to start replaces the file.
"""
from __future__ import annotations

import json
from typing import TYPE_CHECKING, cast

from jastudio.contract_with_jastudio import CONTRACT_VERSION
from jastudio.primary_jastudio.primary_jastudio import JAStudioSpeaksAnotherContract, PrimaryJAStudio, PrimaryJAStudioAccess
from jastudio.users_folder_for_jastudio import the_users_folder_for_jastudio

if TYPE_CHECKING:
    from pathlib import Path


def the_primary_jastudios_access_file_of_this_user() -> Path:
    """The access file every JAStudio of this user writes, wherever it was started from, where JAStudio names it too
    (PrimaryJAStudioAccessFile.OfThisUser)."""
    return the_users_folder_for_jastudio() / "primary-jastudio-access.json"


class PrimaryJAStudioAccessFile:
    def __init__(self, file: Path) -> None:
        self.file: Path = file
        self._the_one_last_found: PrimaryJAStudio | None = None

    def the_primary_jastudio_or_none(self) -> PrimaryJAStudio | None:
        """The primary JAStudio, found by the file as it is now, or None while no JAStudio runs: the file isn't there.

        A JAStudio found anew is first asked for its version of the contract, and one that speaks another is refused with
        JAStudioSpeaksAnotherContract. One that ended without stopping fails that request with JAStudioDoesNotAnswer."""
        access = self._the_access_or_none()
        if access is None:
            return None
        if self._the_one_last_found is None or self._the_one_last_found.access != access:
            found = PrimaryJAStudio(access)
            its_version = found.contract_version()
            if its_version != CONTRACT_VERSION:
                raise JAStudioSpeaksAnotherContract(its_version, CONTRACT_VERSION)
            self._the_one_last_found = found
        return self._the_one_last_found

    def _the_access_or_none(self) -> PrimaryJAStudioAccess | None:
        """Reads the file without first asking whether it's there: JAStudio may remove it at any time."""
        try:
            text = self.file.read_text(encoding="utf-8")
        except FileNotFoundError:
            return None
        access = cast(dict[str, int], json.loads(text))
        return PrimaryJAStudioAccess(port=access["port"], process_id=access["processId"])
