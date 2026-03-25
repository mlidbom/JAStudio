from __future__ import annotations

from typing import TYPE_CHECKING

from jastudio.anki_extentions.note_ex import NoteEx
from jastudio.ui import dotnet_ui_root

if TYPE_CHECKING:
    from anki.notes import Note
    from jastudio.anki_extentions.card_ex import CardEx

def update_note_in_studying_cache(note: Note) -> None:
    for card_ex in NoteEx(note).cards():
        update_card_in_studying_cache(card_ex)

def update_card_in_studying_cache(card_ex: CardEx) -> None:
    dotnet_ui_root().Services.CoreApp.Collection.UpdateCardStudyingStatus(card_ex.card.id)
