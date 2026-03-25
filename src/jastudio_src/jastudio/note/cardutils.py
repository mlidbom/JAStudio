# noinspection PyUnresolvedReferences
from __future__ import annotations

from typing import TYPE_CHECKING

from anki.consts import QUEUE_TYPE_NEW
from autoslot import Slots
from JAStudio.Core.Note import NoteTypes

from jastudio.anki_extentions.card_ex import CardEx

if TYPE_CHECKING:
    from anki.cards import Card


class CardUtils(Slots):
    @classmethod
    def is_new(cls, card: Card) -> bool:
        return card.queue == QUEUE_TYPE_NEW

    @classmethod
    def get_note_type_priority(cls, card: Card) -> int:
        note_type_name = CardEx(card).note_type().name
        if note_type_name == NoteTypes.Kanji: return 1
        if note_type_name == NoteTypes.Vocab: return 2

        return 4  # It's nice to use it for other note types too so default them to 4.

    @classmethod
    def prioritize(cls, card: Card) -> None:
        if CardUtils.is_new(card):
            card.due = cls.get_note_type_priority(card)
            card.flush()