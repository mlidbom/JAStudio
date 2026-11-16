import typing, abc
from JAStudio.Core.Note.Tevents import INoteTevent, INoteTevent_1, NoteTevent, NoteTevent_1
from Compze.Teventive.Taggregates.Tevents import ITaggregateCreatedTevent
from Compze.Abstractions import TaggregateId
from System import DateTime
from JAStudio.Core.Note import SentenceId

class ISentenceTevent_GenericClasses(abc.ABCMeta):
    Generic_ISentenceTevent_GenericClasses_ISentenceTevent_1_TTevent = typing.TypeVar('Generic_ISentenceTevent_GenericClasses_ISentenceTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_ISentenceTevent_GenericClasses_ISentenceTevent_1_TTevent]) -> typing.Type[ISentenceTevent_1[Generic_ISentenceTevent_GenericClasses_ISentenceTevent_1_TTevent]]: ...

class ISentenceTevent(ISentenceTevent_0, metaclass =ISentenceTevent_GenericClasses): ...

class ISentenceTevent_0(INoteTevent_0, typing.Protocol):

    class Created(ISentenceTevent_0, ITaggregateCreatedTevent, typing.Protocol):
        pass



ISentenceTevent_1_TTevent = typing.TypeVar('ISentenceTevent_1_TTevent', covariant=True)
class ISentenceTevent_1(typing.Generic[ISentenceTevent_1_TTevent], INoteTevent_1[ISentenceTevent_1_TTevent], typing.Protocol):
    pass


class SentenceTevent_GenericClasses(abc.ABCMeta):
    Generic_SentenceTevent_GenericClasses_SentenceTevent_1_TTevent = typing.TypeVar('Generic_SentenceTevent_GenericClasses_SentenceTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_SentenceTevent_GenericClasses_SentenceTevent_1_TTevent]) -> typing.Type[SentenceTevent_1[Generic_SentenceTevent_GenericClasses_SentenceTevent_1_TTevent]]: ...

class SentenceTevent(SentenceTevent_0, metaclass =SentenceTevent_GenericClasses): ...

class SentenceTevent_0(NoteTevent_0, ISentenceTevent_0):
    @property
    def TaggregateId(self) -> TaggregateId: ...
    @property
    def TaggregateVersion(self) -> int: ...
    @property
    def UtcTimeStamp(self) -> DateTime: ...

    class Created(SentenceTevent_0, ISentenceTevent.Created):
        def __init__(self, sentenceId: SentenceId) -> None: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...



SentenceTevent_1_TTevent = typing.TypeVar('SentenceTevent_1_TTevent')
class SentenceTevent_1(typing.Generic[SentenceTevent_1_TTevent], NoteTevent_1[SentenceTevent_1_TTevent], ISentenceTevent_1[SentenceTevent_1_TTevent]):
    def __init__(self, tevent: SentenceTevent_1_TTevent) -> None: ...
    @property
    def Tevent(self) -> SentenceTevent_1_TTevent: ...

