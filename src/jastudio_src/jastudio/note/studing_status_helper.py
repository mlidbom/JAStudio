from __future__ import annotations

from typing import TYPE_CHECKING

from JAStudio.Core.Note.Collection import CardStudyingStatus

from jastudio.anki_extentions.note_ex import NoteEx
from jastudio.ui import dotnet_ui_root

if TYPE_CHECKING:
    from anki.notes import Note
    from jastudio.anki_extentions.card_ex import CardEx

def update_note_in_studying_cache(note: Note) -> None:
    for card_ex in NoteEx(note).cards():
        update_card_in_studying_cache(card_ex)

def update_card_in_studying_cache(card_ex: CardEx) -> None:
    status = CardStudyingStatus(card_ex.card.nid, card_ex.type().name, card_ex.is_suspended(), card_ex.note_type().name)
    dotnet_ui_root().Services.CoreApp.Collection.UpdateCardStudyingStatus(status)
