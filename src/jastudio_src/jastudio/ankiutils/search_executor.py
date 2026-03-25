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
    app.get_ui_utils().activate_preview()
