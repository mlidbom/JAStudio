from __future__ import annotations

from typing import TYPE_CHECKING

import pyperclip
from aqt import gui_hooks
from jaspythonutils.sysutils import ex_str, typed
from jaspythonutils.sysutils.timeutil import StopWatch

from jastudio.anki_extentions.note_ex import NoteEx
from jastudio.ankiutils import app
from jastudio.ankiutils.app import get_ui_utils
from jastudio.sysutils import app_thread_pool
from jastudio.sysutils.collections.recent_items import RecentItems

if TYPE_CHECKING:
    from anki.cards import Card
    from anki.notes import Note

def copy_card_sort_field_to_clipboard(note: Note) -> None:
    with StopWatch.log_warning_if_slower_than(0.01):
        if app.config().YomitanIntegrationCopyAnswerToClipboard.Value:
            note_type = NoteEx(note).note_type()
            sort_value = typed.str_(note.fields[note_type.sortf])
            clean_string = ex_str.strip_html_and_bracket_markup_and_noise_characters(sort_value)
            app_thread_pool.pool.submit(lambda: pyperclip.copy(clean_string))

recent_review_answers = RecentItems[int](1)

def on_reviewer_show_answer(card: Card) -> None:
    note = card.note()
    if get_ui_utils().is_edit_current_open() or recent_review_answers.is_recent(note.id):
        return

    copy_card_sort_field_to_clipboard(note)

def init() -> None:
    gui_hooks.reviewer_did_show_answer.append(on_reviewer_show_answer)
