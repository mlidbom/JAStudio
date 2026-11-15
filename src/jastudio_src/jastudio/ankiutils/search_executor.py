from __future__ import annotations

from typing import TYPE_CHECKING

import aqt
from jaspythonutils.sysutils.typed import non_optional

from jastudio.ankiutils import app

if TYPE_CHECKING:

    from aqt.browser import Browser  # type: ignore[attr-defined]  # pyright: ignore[reportPrivateImportUsage]


def do_lookup_and_show_previewer(text: str) -> None:
    do_lookup(text)
    app.get_ui_utils().activate_preview()

def do_lookup(text: str) -> None:
    browser: Browser = aqt.dialogs.open("Browser", aqt.mw)  # pyright: ignore[reportAny]
    non_optional(browser.form.searchEdit.lineEdit()).setText(text)
    browser.onSearchActivated()  # pyright: ignore[reportUnknownMemberType]
    # review: this opens the previewer too, so do_lookup_and_show_previewer, which calls this and then opens the previewer again, does nothing
    #   more than this does. JAStudio therefore can't open Anki's browser on a search without its previewer (IAnkisWindows.OpenTheBrowserOn says
    #   it can). Found by Claude, 2026-11-15, and left: changing it changes what "Show in Anki's browser" does.
    app.get_ui_utils().activate_preview()
