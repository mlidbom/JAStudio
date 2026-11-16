"""Anki's primary JAStudio, as the Anki add-on reaches it through one access: the requests the add-on makes of it, on its card server, under
/anki/ (JAStudio's EndpointsForTheAnkiAddOn).

Each request names a note by JAStudio's own id, which a study-note holds in its jas_note_id field, never by Anki's.
"""
from __future__ import annotations

import http.client
import json
import sys
from dataclasses import dataclass
from typing import cast
from urllib.parse import urlencode

_HOW_LONG_JASTUDIO_HAS_TO_TAKE_THE_CONNECTION = 1.0
"""Seconds. A JAStudio that runs, on this computer, takes it in under a millisecond. Windows otherwise keeps trying a port nothing listens on
for two seconds, as with a JAStudio that ended without stopping, and the request is made while a person waits for Anki."""


@dataclass(frozen=True)
class PrimaryJAStudioAccess:
    """How to reach the primary JAStudio: its card server's port, and its process's id, which may be let take the foreground."""
    port: int
    process_id: int


class JAStudioDoesNotAnswer(Exception):
    """Nothing listens on the port the access file names: the JAStudio ended without stopping, as one stopped in the debugger does."""


class JAStudioRefused(Exception):
    """JAStudio answered the request with an error, such as for a note it hasn't: with several JAStudios connected to one Anki, a JAStudio may
    lack a note another has."""
    def __init__(self, status: int, why: str) -> None:
        super().__init__(why)
        self.status: int = status
        self.why: str = why


class JAStudioSpeaksAnotherContract(Exception):
    """The JAStudio speaks another version of the contract than this add-on (jastudio.contract_with_jastudio)."""
    def __init__(self, its_version: int, this_add_ons_version: int) -> None:
        super().__init__(f"JAStudio speaks version {its_version} of the contract between JAStudio and its Anki add-on, and this add-on version {this_add_ons_version}: they were built from different checkouts. Use a JAStudio and an add-on of the same checkout.")


class PrimaryJAStudio:
    def __init__(self, access: PrimaryJAStudioAccess) -> None:
        self.access: PrimaryJAStudioAccess = access

    def contract_version(self) -> int:
        return cast(int, self._answer_to("GET", "/anki/contract")["version"])

    def the_address_of_a_cards_page(self, note_id: str, card_type: str, side: str, display_context: str) -> str:
        """The address of the page that shows a side of a card, as Anki's reviewer or previewer shows it, in an iframe: side is "front" or
        "back", display_context "review" or "preview". JAStudio sends it on to the card's page."""
        return f"{self._base_address}/anki/card/{side}?{urlencode({'noteId': note_id, 'cardType': card_type, 'displayContext': display_context})}"

    def the_address_of_the_stylesheet_for_ankis_page(self) -> str:
        """The stylesheet of the page in Anki that holds the card's iframe."""
        return f"{self._base_address}/css/jas_anki_card_template_styles.css"

    def toggle_the_workspace_window(self) -> None:
        self._let_it_take_the_foreground()
        self._answer_to("POST", "/anki/workspace/toggle")

    def toggle_the_note_search(self) -> None:
        self._let_it_take_the_foreground()
        self._answer_to("POST", "/anki/note-search/toggle")

    def show_the_note_in_the_workspace(self, note_id: str) -> None:
        self._let_it_take_the_foreground()
        self._answer_to("POST", "/anki/workspace/show-note", noteId=note_id)

    def the_keys_of_the_note_view_shortcuts(self) -> list[str]:
        """The key of each note view shortcut, written as Qt parses it, such as "a", "0" or "Ctrl+Shift+Alt+s"."""
        return cast(list[str], self._answer_to("GET", "/anki/note-view-shortcuts")["keys"])

    def run_the_note_view_shortcut(self, key: str, note_id: str | None) -> None:
        """Runs the note view shortcut bound to key, on the note with the id note_id, which Anki shows, or with no note while it shows none."""
        if note_id is None:
            self._answer_to("POST", "/anki/note-view-shortcuts/run", key=key)
        else:
            self._answer_to("POST", "/anki/note-view-shortcuts/run", key=key, noteId=note_id)

    def seconds_to_show_the_question(self, note_id: str, seconds_the_deck_gives: float, again_answers_in_a_row_today: int) -> float:
        """How long Anki's reviewer shows the question of a reading card of the note before it shows the answer by itself."""
        return float(cast(float, self._answer_to("GET", "/anki/seconds-to-show-the-question",
                                                 noteId=note_id,
                                                 secondsTheDeckGives=str(seconds_the_deck_gives),
                                                 againAnswersInARowToday=str(again_answers_in_a_row_today))["seconds"]))

    def timebox_minutes(self, note_type: str, card_type: str) -> int | None:
        """The minutes of a timebox for the cards of card_type, such as "Reading", of the note type named note_type, such as "_Vocab", or None
        for cards JAStudio has no timebox for."""
        return cast(int | None, self._answer_to("GET", "/anki/timebox-minutes", noteType=note_type, cardType=card_type)["minutes"])

    @property
    def _base_address(self) -> str: return f"http://127.0.0.1:{self.access.port}"

    def _let_it_take_the_foreground(self) -> None:
        """Before a request that brings one of JAStudio's windows to the front: Windows lets a process take the foreground only when the one that
        has it allows it. Anki has it, as a person has just pressed one of its keys. Not looked into on macOS and Linux."""
        if sys.platform == "win32":
            import ctypes
            _ = cast(int, ctypes.windll.user32.AllowSetForegroundWindow(self.access.process_id))

    def _answer_to(self, method: str, path: str, **query: str) -> dict[str, object]:
        """JAStudio's answer to the request, as a JSON object, or an empty one for an answer with no content. An error is raised."""
        connection = http.client.HTTPConnection("127.0.0.1", self.access.port, timeout=_HOW_LONG_JASTUDIO_HAS_TO_TAKE_THE_CONNECTION)
        try:
            try:
                connection.connect()
            except (ConnectionRefusedError, TimeoutError) as nothing_listens:
                raise JAStudioDoesNotAnswer(f"The JAStudio that was Anki's primary JAStudio no longer answers, on port {self.access.port}: it ended without stopping.") from nothing_listens
            # Once connected, a request waits for its answer for as long as JAStudio takes, as a call into JAStudio's own process does.
            connection.sock.settimeout(None)
            connection.request(method, f"{path}?{urlencode(query)}" if query else path)
            response = connection.getresponse()
            body = response.read()
        finally:
            connection.close()

        answer = cast(dict[str, object], json.loads(body)) if body and response.getheader("Content-Type", "").startswith("application/json") else {}
        if response.status >= 400:
            raise JAStudioRefused(response.status, str(answer.get("error", body.decode("utf-8", errors="replace"))))
        return answer
