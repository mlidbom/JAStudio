from __future__ import annotations

from typing import cast

from anki.decks import DeckId
from anki.models import NotetypeDict, NotetypeId
from autoslot import Slots
from jaspythonutils.sysutils import typed

from jastudio.anki_extentions.notetype_ex.note_type_field import NoteFieldEx
from jastudio.anki_extentions.notetype_ex.note_type_template import NoteTemplateEx


class NoteTypeEx(Slots):
    def __init__(self, name: str, fields: list[NoteFieldEx], templates: list[NoteTemplateEx]) -> None:
        self.name: str = name
        self.id: NotetypeId = NotetypeId(0)
        self.flds:list[NoteFieldEx] = fields
        self.tmpls:list[NoteTemplateEx] = templates
        self.type:int = 0
        self.mod:int = 0
        self.usn:int = 0
        self.sortf:int = 0
        self.deck_id:DeckId = DeckId(0)
        self.css:str = ""
        self.latexPre:str = ""
        self.latexPost:str = ""
        self.latexsvg:bool = False
        self.req: list[object] = []
        self.vers: list[object] = []
        self.tags: list[object] = []

        for index, field in enumerate(self.flds):
            field.ord = index

        for index, template in enumerate(self.tmpls):
            template.ord = index

    @classmethod
    def from_dict(cls, note_type_dict: NotetypeDict) -> NoteTypeEx:
        created = NoteTypeEx(note_type_dict["name"], [], [])  # pyright: ignore[reportAny]
        created.flds = [NoteFieldEx.from_dict(d) for d in note_type_dict["flds"]]  # pyright: ignore[reportAny]
        created.tmpls = [NoteTemplateEx.from_dict(t) for t in note_type_dict["tmpls"]]  # pyright: ignore[reportAny]

        created.id = cast(NotetypeId, typed.int_(note_type_dict["id"]))  # pyright: ignore[reportAny]
        created.type = typed.int_(note_type_dict["type"])  # pyright: ignore[reportAny]
        created.mod = typed.int_(note_type_dict["mod"])  # pyright: ignore[reportAny]
        created.usn = typed.int_(note_type_dict["usn"])  # pyright: ignore[reportAny]
        created.sortf = typed.int_(note_type_dict["sortf"])  # pyright: ignore[reportAny]
        created.deck_id = note_type_dict["did"]
        created.css = typed.str_(note_type_dict["css"])  # pyright: ignore[reportAny]
        created.latexPre = typed.str_(note_type_dict["latexPre"])  # pyright: ignore[reportAny]
        created.latexPost = typed.str_(note_type_dict["latexPost"])  # pyright: ignore[reportAny]
        created.latexsvg = typed.bool_(note_type_dict["latexsvg"])  # pyright: ignore[reportAny]
        created.req = note_type_dict["req"]
        # missing for som reason# created.vers = note_type_dict['vers']
        # missing for som reason# created.tags = note_type_dict['tags']

        return created
