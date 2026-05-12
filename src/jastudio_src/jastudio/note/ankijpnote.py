from __future__ import annotations

from typing import TYPE_CHECKING

from autoslot import Slots
from jastudio.ui import dotnet_ui_root

if TYPE_CHECKING:
    from anki.cards import Card
    from JAStudio.Core.Note import JPNote

class AnkiJPNote(Slots):
    @classmethod
    def note_from_card(cls, card: Card) -> JPNote:
        return dotnet_ui_root().Services.CoreApp.Collection.NoteFromExternalId(card.nid or card.note().id)