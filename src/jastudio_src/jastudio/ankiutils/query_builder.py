from __future__ import annotations

from typing import TYPE_CHECKING

from JAStudio.Core.Note import Builtin, NoteTypes

if TYPE_CHECKING:
    from anki.cards import CardId


note_vocab = f"{Builtin.Note}:{NoteTypes.Vocab}"

note_vocab = note_vocab

def open_card_by_id(card_id: CardId) -> str:
    return f"cid:{card_id}"


