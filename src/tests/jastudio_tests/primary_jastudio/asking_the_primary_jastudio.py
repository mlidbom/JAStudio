"""Asks a JAStudio what the Anki add-on asks it, through the add-on's own client (jastudio.primary_jastudio), for the .NET specifications of
those requests (JAStudio.Web.Specifications: When_the_Anki_add_on_asks_the_primary_JAStudio), which run this file as a script. The JAStudio is
the one an access file names, which the specifications have a JAStudio write as it becomes the primary one.

It reads requests from standard input, a JSON object a line, and answers each with a line of JSON on standard output:

    {"accessFile": <path>, "request": <a method of PrimaryJAStudio>, "arguments": {<its arguments by their Python names>}}
                         the primary JAStudio the access file names is asked: {"answer": <what it answered>}, or {"raised": <the exception's
                         class>, "why": <its message>}, or {"answer": null, "noPrimaryJAStudio": true} while the access file isn't there
    {"request": "access-file-of-this-user"}
                         the access file every JAStudio of this user writes, as the add-on names it: {"accessFile"}

It ends when standard input ends, as it does when the specifications' process ends.
"""
from __future__ import annotations

import json
import sys
from pathlib import Path
from typing import TYPE_CHECKING, TextIO, cast

if TYPE_CHECKING:
    from collections.abc import Callable

    from jastudio.primary_jastudio.primary_jastudio import PrimaryJAStudio


def answer_to(request: dict[str, object]) -> dict[str, object]:
    from jastudio.primary_jastudio.primary_jastudio_access_file import PrimaryJAStudioAccessFile, the_primary_jastudios_access_file_of_this_user

    if request["request"] == "access-file-of-this-user":
        return {"accessFile": str(the_primary_jastudios_access_file_of_this_user())}

    try:
        jastudio = PrimaryJAStudioAccessFile(Path(cast(str, request["accessFile"]))).the_primary_jastudio_or_none()
        if jastudio is None:
            return {"answer": None, "noPrimaryJAStudio": True}
        return {"answer": the_answer_of(jastudio, cast(str, request["request"]), cast(dict[str, object], request.get("arguments", {})))}
    except Exception as raised:  # the specifications are told of what the client raised, as Anki's hooks would be
        return {"raised": type(raised).__name__, "why": str(raised)}


def the_answer_of(jastudio: PrimaryJAStudio, request: str, arguments: dict[str, object]) -> object:
    def text(name: str) -> str: return cast(str, arguments[name])

    def text_or_none(name: str) -> str | None: return cast(str | None, arguments.get(name))

    requests: dict[str, Callable[[], object]] = {
        "contract_version": jastudio.contract_version,
        "the_address_of_a_cards_page": lambda: jastudio.the_address_of_a_cards_page(text("note_id"), text("card_type"), text("side"), text("display_context")),
        "the_address_of_the_stylesheet_for_ankis_page": jastudio.the_address_of_the_stylesheet_for_ankis_page,
        "toggle_the_workspace_window": jastudio.toggle_the_workspace_window,
        "toggle_the_note_search": jastudio.toggle_the_note_search,
        "show_the_note_in_the_workspace": lambda: jastudio.show_the_note_in_the_workspace(text("note_id")),
        "the_keys_of_the_note_view_shortcuts": jastudio.the_keys_of_the_note_view_shortcuts,
        "run_the_note_view_shortcut": lambda: jastudio.run_the_note_view_shortcut(text("key"), text_or_none("note_id")),
        "seconds_to_show_the_question": lambda: jastudio.seconds_to_show_the_question(text("note_id"), cast(float, arguments["seconds_the_deck_gives"]), cast(int, arguments["again_answers_in_a_row_today"])),
        "timebox_minutes": lambda: jastudio.timebox_minutes(text("note_type"), text("card_type")),
    }
    return requests[request]()


def make_the_addons_code_and_the_tests_importable() -> None:
    """Puts the folders pytest.ini names as its pythonpath first among those Python imports from: this file is run as a script, not by pytest."""
    source = Path(__file__).resolve().parents[3]
    sys.path[:0] = [str(source / "jastudio_src"), str(source / "jaspythonutils_src"), str(source / "tests")]


def answer_the_requests(requests: TextIO, answers: TextIO) -> None:
    for line in requests:
        print(json.dumps(answer_to(cast(dict[str, object], json.loads(line)))), file=answers, flush=True)


if __name__ == "__main__":
    make_the_addons_code_and_the_tests_importable()
    the_answers = sys.stdout
    sys.stdout = sys.stderr  # whatever else prints must not land among the answers
    answer_the_requests(sys.stdin, the_answers)
