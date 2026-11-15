"""Has the Anki add-on serve JAStudio over HTTP for as long as an Anki profile is open: see dev_docs/moving-jastudio-out-of-anki.md.

wip: nothing calls the server yet but what is tried from outside Anki. JAStudio inside Anki calls Anki through pythonnet until the add-on stops
loading .NET.
"""
from __future__ import annotations

import traceback
from typing import TYPE_CHECKING

import aqt
from aqt import gui_hooks
from jaspythonutils.sysutils.typed import non_optional

from jastudio import mylog
from jastudio.anki_http.anki_with_its_window import AnkiWithItsWindow
from jastudio.anki_http.serving_jastudio import ServingJAStudio, the_access_file_of_this_user

if TYPE_CHECKING:
    from collections.abc import Callable


def init() -> None:
    """Called once, as the add-on loads."""
    anki = AnkiWithItsWindow(non_optional(aqt.mw))
    serving = ServingJAStudio(anki, the_access_file_of_this_user())

    def stop_if_it_serves() -> None:
        if serving.is_serving:
            serving.stop()

    gui_hooks.sync_will_start.append(anki.its_own_sync_starts)
    gui_hooks.sync_did_finish.append(anki.its_own_sync_has_finished)
    gui_hooks.profile_did_open.append(_reporting_what_fails(serving.start, "start serving JAStudio over HTTP"))
    gui_hooks.profile_will_close.append(_reporting_what_fails(stop_if_it_serves, "stop serving JAStudio over HTTP"))


def _reporting_what_fails(work: Callable[[], None], what_it_does: str) -> Callable[[], None]:
    """The work as one of Anki's hooks runs it: a failure is logged, and Anki opens or closes its profile all the same. An exception from a hook
    would stop the hooks after it, the add-on's own among them."""
    def run() -> None:
        try:
            work()
        except Exception:  # the add-on's entry point from Anki: the failure is reported here, where nothing else waits for the result
            mylog.error(f"The Anki add-on failed to {what_it_does}:\n{traceback.format_exc()}")

    return run
