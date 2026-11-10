from __future__ import annotations

from jastudio.ui.hooks import (
    announce_finished_imports,
    clear_studying_cache_on_card_suspend_unsuspend,
    copy_sort_field_to_clipboard,
    custom_auto_advance_timings,
    custom_short_term_scheduling,
    custom_timebox_lengths,
    follow_the_study_notes_in_anki,
    global_shortcuts,
    history_navigator,
    no_accidental_double_click,
    note_view_shortcuts,
    timebox_end_sound,
)


def init() -> None:
    announce_finished_imports.init()
    clear_studying_cache_on_card_suspend_unsuspend.init()
    copy_sort_field_to_clipboard.init()
    custom_auto_advance_timings.init()
    custom_short_term_scheduling.init()
    custom_timebox_lengths.init()
    follow_the_study_notes_in_anki.init()
    history_navigator.init()
    no_accidental_double_click.init()
    timebox_end_sound.init()
    note_view_shortcuts.init()
    global_shortcuts.init()
