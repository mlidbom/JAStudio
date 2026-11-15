"""The menu the Anki add-on adds to the right-click menu of Anki's browser, on the cards selected there: Magnus's own scheduling of new cards.

Until 2026-11-15 C# described it, and it held commands on JAStudio's notes too, which are JAStudio's workspace's now: see
dev_docs/moving-jastudio-out-of-anki.md.
"""
from __future__ import annotations

from functools import partial
from typing import TYPE_CHECKING

from aqt import gui_hooks
from jaspythonutils.sysutils.typed import non_optional

from jastudio.ankiutils import app
from jastudio.note import queue_manager

if TYPE_CHECKING:
    from collections.abc import Sequence

    from anki.cards import CardId
    from aqt.browser import Browser  # type: ignore[attr-defined]  # pyright: ignore[reportPrivateImportUsage]
    from PyQt6.QtWidgets import QMenu


def spread_due_dates(cards: Sequence[CardId], start_day: int, days: int) -> None:
    anki_col = app.anki_collection()
    scheduler = anki_col.sched
    for index, card_id in enumerate(cards):
        new_due = start_day + (index * days)
        scheduler.set_due_date([card_id], str(new_due))

    app.get_ui_utils().refresh()


def setup_browser_context_menu(browser: Browser, menu: QMenu) -> None:
    selected_cards: Sequence[CardId] = browser.selected_cards()
    if not selected_cards:
        return

    magnus_menu = non_optional(menu.addMenu("&Magnus"))
    if len(selected_cards) == 1:
        magnus_menu.addAction("Prioritize selected cards", partial(queue_manager.prioritize_selected_cards, selected_cards))  # pyright: ignore[reportUnknownMemberType]

    spread_menu = non_optional(magnus_menu.addMenu("&Spread selected cards"))
    for start_day in range(10):
        first_card_menu = non_optional(spread_menu.addMenu(f"First card in {start_day} days"))
        for days_apart in range(1, 10):
            first_card_menu.addAction(f"{days_apart} days apart", partial(spread_due_dates, selected_cards, start_day, days_apart))  # pyright: ignore[reportUnknownMemberType]


def init() -> None:
    gui_hooks.browser_will_show_context_menu.append(setup_browser_context_menu)
