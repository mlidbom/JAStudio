import typing, abc
from Compze.Tessaging.TessageTypes import ITevent

class IStringValueTevent(ITevent, typing.Protocol):

    class Set(IStringValueTevent, typing.Protocol):
        @property
        def Value(self) -> str: ...



class StringValueTevent(IStringValueTevent, abc.ABC):

    class Set(StringValueTevent, IStringValueTevent.Set):
        def __init__(self, value: str) -> None: ...
        @property
        def Value(self) -> str: ...


