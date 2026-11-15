"""Anki with its main window, as the HTTP server for JAStudio reaches it inside Anki: what of it needs no real window. A stand-in is its main
window here, which keeps the work handed to Anki's main thread instead of running it."""
from __future__ import annotations

from typing import TYPE_CHECKING, cast

from jastudio.anki_http import inside_anki
from jastudio.anki_http.anki_with_its_window import AnkiWithItsWindow

if TYPE_CHECKING:
    from collections.abc import Callable

    from aqt.main import AnkiQt


class TaskManagerThatKeepsTheWork:
    def __init__(self) -> None:
        self.work_handed_to_ankis_main_thread: list[Callable[[], None]] = []

    def run_on_main(self, work: Callable[[], None]) -> None: self.work_handed_to_ankis_main_thread.append(work)


class StandInForAnkisMainWindow:
    def __init__(self, collection: object | None) -> None:
        self.col: object | None = collection
        self.taskman: TaskManagerThatKeepsTheWork = TaskManagerThatKeepsTheWork()


def anki_with(main_window: StandInForAnkisMainWindow) -> AnkiWithItsWindow: return AnkiWithItsWindow(cast("AnkiQt", cast("object", main_window)))


def test_with_a_profile_open_and_no_sync_under_way_the_collection_can_be_used() -> None:
    assert anki_with(StandInForAnkisMainWindow(collection=object())).why_the_collection_cannot_be_used_now() is None


def test_with_no_profile_open_the_collection_cannot_be_used_and_it_says_so() -> None:
    assert anki_with(StandInForAnkisMainWindow(collection=None)).why_the_collection_cannot_be_used_now() == "No Anki profile is open."


def test_while_ankis_own_sync_is_under_way_the_collection_cannot_be_used_and_once_it_has_finished_it_can() -> None:
    anki = anki_with(StandInForAnkisMainWindow(collection=object()))
    anki.its_own_sync_starts()
    assert anki.why_the_collection_cannot_be_used_now() == "Anki's own sync is under way."
    anki.its_own_sync_has_finished()
    assert anki.why_the_collection_cannot_be_used_now() is None


def test_what_its_windows_are_asked_for_is_handed_to_ankis_main_thread_and_not_run_by_the_caller() -> None:
    main_window = StandInForAnkisMainWindow(collection=object())
    windows = anki_with(main_window).windows()
    windows.open_the_browser_on("nid:215")
    windows.open_the_browser_and_the_previewer_on("nid:215")
    windows.show_a_tooltip("Reparsed 3 sentences", 3000)
    windows.refresh()
    assert len(main_window.taskman.work_handed_to_ankis_main_thread) == 4


def test_the_module_that_starts_the_serving_inside_anki_can_be_imported_without_anki_running() -> None:
    assert callable(inside_anki.init)
