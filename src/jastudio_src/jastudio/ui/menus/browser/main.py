from __future__ import annotations

from typing import TYPE_CHECKING

from aqt import gui_hooks

from jastudio.ankiutils import app
from jastudio.qt_adapters import qt_menu_adapter
from jastudio.ui import dotnet_ui_root

if TYPE_CHECKING:
    from collections.abc import Sequence

    from anki.cards import Card, CardId
    from aqt.browser import Browser  # type: ignore[attr-defined]  # pyright: ignore[reportPrivateImportUsage]
    from JAStudio.UI.Menus.UIAgnosticMenuStructure import SpecMenuItem
    from PyQt6.QtWidgets import QMenu

def spread_due_dates(cards: Sequence[CardId], start_day: int, days: int) -> None:
    anki_col = app.anki_collection()
    scheduler = anki_col.sched
    for index, card_id in enumerate(cards):
        card: Card = anki_col.get_card(card_id)
        new_due = start_day + (index * days)
        scheduler.set_due_date([card.id], str(new_due))

    app.get_ui_utils().refresh()

def setup_browser_context_menu(browser: Browser, menu: QMenu) -> None:
    menu_spec: SpecMenuItem = dotnet_ui_root().Menus.BuildBrowserMenuSpec(browser.selected_cards(),
                                                                     browser.selectedNotes())
    qt_menu_adapter.add_to_qt_menu(menu, [menu_spec])


def init() -> None:
    gui_hooks.browser_will_show_context_menu.append(setup_browser_context_menu)
