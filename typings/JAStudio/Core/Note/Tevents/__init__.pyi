import typing, abc
from Compze.Teventive.Taggregates.Tevents import ITaggregateTevent, ITaggregateTevent_1, TaggregateTevent
from System.Collections.Generic import IReadOnlySet_1
from JAStudio.Core.Note.Tagging import Tag
from Compze.Abstractions import TaggregateId
from System import DateTime
from Compze.Teventive.Taggregates.BaseClasses import TaggregateTevent_1

class INoteTevent_GenericClasses(abc.ABCMeta):
    Generic_INoteTevent_GenericClasses_INoteTevent_1_TTevent = typing.TypeVar('Generic_INoteTevent_GenericClasses_INoteTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_INoteTevent_GenericClasses_INoteTevent_1_TTevent]) -> typing.Type[INoteTevent_1[Generic_INoteTevent_GenericClasses_INoteTevent_1_TTevent]]: ...

class INoteTevent(INoteTevent_0, metaclass =INoteTevent_GenericClasses): ...

class INoteTevent_0(ITaggregateTevent, typing.Protocol):

    class PropertyUpdated(abc.ABC):

        class Tags(INoteTevent_0, typing.Protocol):
            @property
            def Tags(self) -> IReadOnlySet_1[Tag]: ...




INoteTevent_1_TTevent = typing.TypeVar('INoteTevent_1_TTevent', covariant=True)
class INoteTevent_1(typing.Generic[INoteTevent_1_TTevent], ITaggregateTevent_1[INoteTevent_1_TTevent], typing.Protocol):
    pass


class NoteTevent_GenericClasses(abc.ABCMeta):
    Generic_NoteTevent_GenericClasses_NoteTevent_1_TTevent = typing.TypeVar('Generic_NoteTevent_GenericClasses_NoteTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_NoteTevent_GenericClasses_NoteTevent_1_TTevent]) -> typing.Type[NoteTevent_1[Generic_NoteTevent_GenericClasses_NoteTevent_1_TTevent]]: ...

class NoteTevent(NoteTevent_0, metaclass =NoteTevent_GenericClasses): ...

class NoteTevent_0(TaggregateTevent, INoteTevent_0):
    @property
    def TaggregateId(self) -> TaggregateId: ...
    @property
    def TaggregateVersion(self) -> int: ...
    @property
    def UtcTimeStamp(self) -> DateTime: ...

    class TagsUpdated(NoteTevent_0, INoteTevent.PropertyUpdated.Tags):
        def __init__(self, tags: IReadOnlySet_1[Tag]) -> None: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def Tags(self) -> IReadOnlySet_1[Tag]: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...



NoteTevent_1_TTevent = typing.TypeVar('NoteTevent_1_TTevent')
class NoteTevent_1(typing.Generic[NoteTevent_1_TTevent], TaggregateTevent_1[NoteTevent_1_TTevent], INoteTevent_1[NoteTevent_1_TTevent]):
    def __init__(self, tevent: NoteTevent_1_TTevent) -> None: ...
    @property
    def Tevent(self) -> NoteTevent_1_TTevent: ...

