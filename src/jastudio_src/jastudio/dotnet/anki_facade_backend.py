"""
Backend for JAStudio.Anki.AnkiFacade: the Python side of each call .NET makes into Anki.

The functions that drive Anki's windows, such as its browser, run their work on the Qt main thread themselves, so they're safe to call from any .NET
thread, the Avalonia UI thread included. The others call Anki's collection on the calling thread, which Anki's backend allows from any thread.
"""
from __future__ import annotations

from typing import TYPE_CHECKING

from anki.cards import CardId
from anki.notes import NoteId
from aqt import mw
from jaspythonutils.sysutils.typed import non_optional, str_
from jastudio.sysutils.app_thread_pool import run_on_ui_thread_fire_and_forget
from typed_linq_collections.q_iterable import query

if TYPE_CHECKING:
    from anki.collection import OpChangesWithCount
    from JAStudio.Core.Note import JPNote


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

# ── Media ──

# noinspection PyUnusedFunction
def media_add_file(file_path: str) -> str:
    """Copies the file into Anki's media folder, for Anki's playback and media sync, and returns the name Anki keeps it under."""
    from jastudio.ankiutils import app
    return app.anki_collection().media.add_file(file_path)

# ── Notes ──

# noinspection PyUnusedFunction
def notes_find(search: str) -> list[int]:
    """The ids of the notes the Anki search finds."""
    from jastudio.ankiutils import app
    return [int(note_id) for note_id in app.anki_collection().find_notes(search)]

# noinspection PyUnusedFunction
def notes_add_tag(note_ids: list[int], tag: str) -> None:
    from jastudio.ankiutils import app
    app.anki_collection().tags.bulk_add([NoteId(note_id) for note_id in note_ids], tag)

# noinspection PyUnusedFunction
def notes_remove(note_ids: list[int]) -> None:
    from jastudio.ankiutils import app
    app.anki_collection().remove_notes([NoteId(note_id) for note_id in note_ids])

# ── Tags ──
# Each change runs as an Anki operation, as the browser runs its own: Anki's Edit > Undo undoes it, and Anki's windows are told of it.
# noinspection PyUnusedFunction
def tags_add_to_notes(note_ids: list[int], tag: str) -> None:
    from jastudio.ankiutils import app
    _tell_anki_of_jastudios_own_operation(app.anki_collection().tags.bulk_add([NoteId(note_id) for note_id in note_ids], tag))

# noinspection PyUnusedFunction
def tags_remove_from_notes(note_ids: list[int], tag: str) -> None:
    """Removes exactly the tag from the notes, ignoring case as Anki does, and leaves the tags below it, as JAStudio removes a note's own tag.
    Anki's bulk_remove would remove those too, so this is Anki's find and replace of the whole tag with nothing."""
    import re

    from jastudio.ankiutils import app
    _tell_anki_of_jastudios_own_operation(app.anki_collection().tags.find_and_replace([NoteId(note_id) for note_id in note_ids], f"^{re.escape(tag)}$", "", regex=True, match_case=False))

# noinspection PyUnusedFunction
def tags_rename(path: str, renamed_path: str) -> None:
    """Renames the tag and every tag below it, on every note, as the browser's sidebar does."""
    from jastudio.ankiutils import app
    _tell_anki_of_jastudios_own_operation(app.anki_collection().tags.rename(path, renamed_path))

# noinspection PyUnusedFunction
def tags_delete(path: str) -> None:
    """Removes the tag and every tag below it from every note, as the browser's sidebar does."""
    from jastudio.ankiutils import app
    _tell_anki_of_jastudios_own_operation(app.anki_collection().tags.remove(path))

# noinspection PyUnusedFunction
def tags_number_of_notes_with_a_tag_in(path: str) -> int:
    """The number of notes with the tag or a tag below it, as clicking the tag in the browser's sidebar finds them."""
    from anki.collection import SearchNode
    from jastudio.ankiutils import app
    collection = app.anki_collection()
    return len(collection.find_notes(collection.build_search_string(SearchNode(tag=path))))

def _tell_anki_of_jastudios_own_operation(changes: OpChangesWithCount) -> None:
    """Tells Anki's windows of the change, as Anki does after each operation, on its main thread: its undo menu, its browser and its reviewer
    follow it. As JAStudio's own operation, which JAStudio reads the study-notes again after itself."""
    from aqt.operations import on_op_finished
    from jastudio.ui.hooks.follow_the_study_notes_in_anki import JASTUDIOS_OWN_OPERATION
    mw.taskman.run_on_main(lambda: on_op_finished(mw, changes, JASTUDIOS_OWN_OPERATION))

# ── Study-notes ──
# noinspection PyUnusedFunction
def study_note_write(anki_note_id: int, jp_note: JPNote) -> None:
    """Writes the note's study-note into the Anki note, as JAStudio writes it whenever it updates the note."""
    from jastudio.note.jpnotedata_shim import JPNoteDataShim
    JPNoteDataShim.write_study_note(jp_note, NoteId(anki_note_id))

# ── NoteEx ──
# noinspection PyUnusedFunction
def note_suspend_all_cards(note_id: int) -> None:
    from jastudio.anki_extentions.note_ex import NoteEx
    NoteEx.from_id(note_id).suspend_all_cards()

# noinspection PyUnusedFunction
def note_unsuspend_all_cards(note_id: int) -> None:
    from jastudio.anki_extentions.note_ex import NoteEx
    NoteEx.from_id(note_id).un_suspend_all_cards()

# noinspection PyUnusedFunction
def note_remove(note_id: int) -> None:
    from jastudio.ankiutils import app
    app.anki_collection().remove_notes([NoteId(note_id)])

# ── Col ──
# noinspection PyUnusedFunction
def col_read_json(select: str) -> str:
    """The one value the SELECT returns, a JSON text, through Anki's own connection to its collection, so it sees Anki's latest changes. Only a
    SELECT may run through it: Anki discards its undo history whenever anything else does."""
    from jastudio.ankiutils import app
    return str_(non_optional(app.anki_collection().db).scalar(select))  # pyright: ignore[reportAny]  # Anki types every database value as Any: str_ checks that it's the text

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
