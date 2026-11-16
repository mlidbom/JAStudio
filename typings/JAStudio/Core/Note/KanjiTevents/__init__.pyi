import typing, abc
from JAStudio.Core.Note.Tevents import INoteTevent, INoteTevent_1, NoteTevent, NoteTevent_1
from Compze.Teventive.Taggregates.Tevents import ITaggregateCreatedTevent
from Compze.Abstractions import TaggregateId
from System import DateTime
from JAStudio.Core.Note import KanjiId

class IKanjiTevent_GenericClasses(abc.ABCMeta):
    Generic_IKanjiTevent_GenericClasses_IKanjiTevent_1_TTevent = typing.TypeVar('Generic_IKanjiTevent_GenericClasses_IKanjiTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_IKanjiTevent_GenericClasses_IKanjiTevent_1_TTevent]) -> typing.Type[IKanjiTevent_1[Generic_IKanjiTevent_GenericClasses_IKanjiTevent_1_TTevent]]: ...

class IKanjiTevent(IKanjiTevent_0, metaclass =IKanjiTevent_GenericClasses): ...

class IKanjiTevent_0(INoteTevent_0, typing.Protocol):

    class Created(IKanjiTevent_0, ITaggregateCreatedTevent, typing.Protocol):
        @property
        def Question(self) -> str: ...



IKanjiTevent_1_TTevent = typing.TypeVar('IKanjiTevent_1_TTevent', covariant=True)
class IKanjiTevent_1(typing.Generic[IKanjiTevent_1_TTevent], INoteTevent_1[IKanjiTevent_1_TTevent], typing.Protocol):
    pass


class KanjiTevent_GenericClasses(abc.ABCMeta):
    Generic_KanjiTevent_GenericClasses_KanjiTevent_1_TTevent = typing.TypeVar('Generic_KanjiTevent_GenericClasses_KanjiTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_KanjiTevent_GenericClasses_KanjiTevent_1_TTevent]) -> typing.Type[KanjiTevent_1[Generic_KanjiTevent_GenericClasses_KanjiTevent_1_TTevent]]: ...

class KanjiTevent(KanjiTevent_0, metaclass =KanjiTevent_GenericClasses): ...

class KanjiTevent_0(NoteTevent_0, IKanjiTevent_0):
    @property
    def TaggregateId(self) -> TaggregateId: ...
    @property
    def TaggregateVersion(self) -> int: ...
    @property
    def UtcTimeStamp(self) -> DateTime: ...

    class Created(KanjiTevent_0, IKanjiTevent.Created):
        def __init__(self, kanjiId: KanjiId, question: str) -> None: ...
        @property
        def Question(self) -> str: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...



KanjiTevent_1_TTevent = typing.TypeVar('KanjiTevent_1_TTevent')
class KanjiTevent_1(typing.Generic[KanjiTevent_1_TTevent], NoteTevent_1[KanjiTevent_1_TTevent], IKanjiTevent_1[KanjiTevent_1_TTevent]):
    def __init__(self, tevent: KanjiTevent_1_TTevent) -> None: ...
    @property
    def Tevent(self) -> KanjiTevent_1_TTevent: ...

