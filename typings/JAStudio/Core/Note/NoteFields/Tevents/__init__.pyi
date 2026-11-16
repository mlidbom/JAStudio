import typing, abc
from Compze.Tessaging.TessageTypes import ITevent

class IStringValueTevent(ITevent, typing.Protocol):

    class PropertyUpdated(abc.ABC):

        class Value(IStringValueTevent, typing.Protocol):
            @property
            def Value(self) -> str: ...




class StringValueTevent(IStringValueTevent, abc.ABC):

    class ValueUpdated(StringValueTevent, IStringValueTevent.PropertyUpdated.Value):
        def __init__(self, value: str) -> None: ...
        @property
        def Value(self) -> str: ...


