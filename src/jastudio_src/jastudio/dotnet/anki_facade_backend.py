"""
Backend for JAStudio.Core.Anki.AnkiFacade - handles Qt thread marshaling.

All functions in this module ensure they execute on the Qt main thread,
making them safe to call from any .NET thread (including the Avalonia UI thread).
"""
from __future__ import annotations


from anki.cards import CardId
from anki.notes import NoteId
from aqt import mw
from jastudio.sysutils.app_thread_pool import run_on_ui_thread_fire_and_forget
from typed_linq_collections.q_iterable import query


# ── Browser ──
# noinspection PyUnusedFunction
def browser_execute_lookup(query: str) -> None:
    """Execute an Anki browser search query."""
    def do_it() -> None:
        from jastudio.ankiutils import search_executor
        search_executor.do_lookup(query)
    run_on_ui_thread_fire_and_forget(do_it)

# noinspection PyUnusedFunction
def browser_execute_lookup_and_show_previewer(query: str) -> None:
    """Execute an Anki browser search query and show the previewer."""
    def do_it() -> None:
        from jastudio.ankiutils import search_executor
        search_executor.do_lookup_and_show_previewer(query)
    run_on_ui_thread_fire_and_forget(do_it)

# noinspection PyUnusedFunction
def browser_prioritize_cards(card_ids: list[int]) -> None:
    from jastudio.note import queue_manager
    queue_manager.prioritize_selected_cards(query(card_ids).select(NoteId).to_list())

# noinspection PyUnusedFunction
def browser_spread_cards_over_days(card_ids: list[int], start_day: int, days_apart: int) -> None:
    """Spread selected cards over days (distributes due dates across time range)."""
    from jastudio.ui.menus.browser import main as browser_main
    browser_main.spread_due_dates(query(card_ids).select(NoteId).to_list(), start_day, days_apart)

# ── UIUtils ──

# noinspection PyUnusedFunction
def ui_show_tooltip(message: str, period_ms: int = 3000) -> None:
    """Show a tooltip message in Anki."""
    def do_it() -> None:
        from aqt.utils import tooltip
        tooltip(message, period_ms)
    run_on_ui_thread_fire_and_forget(do_it)

# noinspection PyUnusedFunction
def ui_refresh() -> None:
    """Refresh the currently displayed views in Anki."""
    def do_it() -> None:
        from jastudio.ankiutils import app
        app.get_ui_utils().refresh()
    run_on_ui_thread_fire_and_forget(do_it)

# ── Batches ──

# noinspection PyUnusedFunction
def batches_flush_anki_note(note_id: int) -> None:
    from jastudio.ankiutils import app
    col = app.anki_collection()
    note = col.get_note(NoteId(note_id))
    col.update_note(note)

# ── NoteEx ──
# noinspection PyUnusedFunction
def note_suspend_all_cards(note_id: int) -> None:
    from jastudio.anki_extentions.note_ex import NoteEx
    NoteEx.from_id(note_id).suspend_all_cards()

# noinspection PyUnusedFunction
def note_unsuspend_all_cards(note_id: int) -> None:
    from jastudio.anki_extentions.note_ex import NoteEx
    NoteEx.from_id(note_id).un_suspend_all_cards()

# ── Col ──
# noinspection PyUnusedFunction
def col_db_file_path() -> str | None:
    return mw.col.path if mw.col is not None else None

# ── Misc ──
# noinspection PyUnusedFunction
def get_note_id_from_card_id(card_id: int) -> int:
    from jastudio.ankiutils import app
    return int(app.anki_collection().get_card(CardId(card_id)).nid)

# noinspection PyUnusedFunction
def addon_root_dir() -> str:
    """Return the addon root directory (the directory containing __init__.py).

    Path: this file is at src/jastudio_src/jastudio/dotnet/anki_facade_backend.py
    Going up 4 levels: dotnet/ → jastudio/ → jastudio_src/ → src/
    """
    import os
    path = os.path.abspath(__file__)
    for _ in range(4):
        path = os.path.dirname(path)
    return path

# noinspection PyUnusedFunction
def anki_media_dir() -> str:
    """Return the Anki collection media directory."""
    assert mw.col is not None
    return mw.col.media.dir()
