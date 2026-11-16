from __future__ import annotations

from jastudio.ui.hooks import (
    copy_sort_field_to_clipboard,
    custom_auto_advance_timings,
    custom_short_term_scheduling,
    custom_timebox_lengths,
    global_shortcuts,
    history_navigator,
    no_accidental_double_click,
    note_view_shortcuts,
    timebox_end_sound,
)


def init() -> None:
    copy_sort_field_to_clipboard.init()
    custom_auto_advance_timings.init()
    custom_short_term_scheduling.init()
    custom_timebox_lengths.init()
    history_navigator.init()
    no_accidental_double_click.init()
    timebox_end_sound.init()
    note_view_shortcuts.init()
    global_shortcuts.init()
