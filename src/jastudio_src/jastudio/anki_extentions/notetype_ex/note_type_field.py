from __future__ import annotations

from autoslot import Slots
from jaspythonutils.sysutils import typed


class NoteFieldEx(Slots):
    def __init__(self, name: str) -> None:
        self.name: str = name
        self.ord: int = 0
        self.sticky: bool = False
        self.rtl: bool = False
        self.font: str = "Arial"
        self.size: int = 20
        self.description: str = ""
        self.plainText: bool = False
        self.collapsed: bool = False
        self.excludeFromSearch: bool = False
        self.media: list[object] = []

    @classmethod
    def from_dict(cls, d: dict[str, object]) -> NoteFieldEx:
        instance = NoteFieldEx(typed.str_(d["name"]))

        instance.ord = typed.int_(d["ord"])
        instance.sticky = typed.bool_(d["sticky"])
        instance.rtl = typed.bool_(d["rtl"])
        instance.font = typed.str_(d["font"])
        instance.size = typed.int_(d["size"])
        instance.description = typed.str_(d["description"])
        instance.plainText = typed.bool_(d["plainText"])
        instance.collapsed = typed.bool_(d["collapsed"])
        instance.excludeFromSearch = typed.bool_(d["excludeFromSearch"])
        # missing for som reason# instance.media = d['media']

        return instance
