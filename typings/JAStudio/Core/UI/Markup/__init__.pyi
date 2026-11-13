import typing, clr
from System import IEquatable_1
from System.Collections.Generic import IReadOnlyList_1

class StyledRun(IEquatable_1[StyledRun]):
    def __init__(self, Text: str, Style: TextStyle) -> None: ...
    @property
    def Style(self) -> TextStyle: ...
    @Style.setter
    def Style(self, value: TextStyle) -> TextStyle: ...
    @property
    def Text(self) -> str: ...
    @Text.setter
    def Text(self, value: str) -> str: ...
    def Deconstruct(self, Text: clr.Reference[str], Style: clr.Reference[TextStyle]) -> None: ...
    def GetHashCode(self) -> int: ...
    def __eq__(self, left: StyledRun, right: StyledRun) -> bool: ...
    def __ne__(self, left: StyledRun, right: StyledRun) -> bool: ...
    def ToString(self) -> str: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup
    class Equals_MethodGroup:
        @typing.overload
        def __call__(self, other: StyledRun) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class StyledText:
    @property
    def Runs(self) -> IReadOnlyList_1[StyledRun]: ...
    @staticmethod
    def FromMarkup(markup: str) -> StyledText: ...
    def ToString(self) -> str: ...


class TextStyle(typing.SupportsInt):
    @typing.overload
    def __init__(self, value : int) -> None: ...
    @typing.overload
    def __init__(self, value : int, force_if_true: bool) -> None: ...
    def __int__(self) -> int: ...
    
    # Values:
    Plain : TextStyle # 0
    Bold : TextStyle # 1
    Italic : TextStyle # 2
    Underlined : TextStyle # 4
    StruckThrough : TextStyle # 8
    Japanese : TextStyle # 16
    Label : TextStyle # 32
    Kanji : TextStyle # 64
    Radical : TextStyle # 128
    Vocabulary : TextStyle # 256
    Reading : TextStyle # 512
    Primary : TextStyle # 1024
    ReadingsSeparator : TextStyle # 2048

