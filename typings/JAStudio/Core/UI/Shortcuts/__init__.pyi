import typing
from System import Action_1
from JAStudio.Core.Note import JPNote
from JAStudio.Core.Configuration import JapaneseConfig
from System.Collections.Generic import IReadOnlyList_1

class NoteViewShortcut:
    def __init__(self, keyGesture: str, action: Action_1[JPNote]) -> None: ...
    @property
    def KeyGesture(self) -> str: ...
    # Skipped Run due to it being static, abstract and generic.

    Run : Run_MethodGroup
    class Run_MethodGroup:
        @typing.overload
        def __call__(self) -> None:...
        @typing.overload
        def __call__(self, viewedNote: JPNote) -> None:...



class NoteViewShortcuts:
    def __init__(self, config: JapaneseConfig) -> None: ...
    @property
    def All(self) -> IReadOnlyList_1[NoteViewShortcut]: ...
    def BoundTo(self, keyGesture: str) -> NoteViewShortcut: ...

