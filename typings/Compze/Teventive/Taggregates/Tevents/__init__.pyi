import typing, abc
from Compze.Tessaging.TessageTypes import ITevent, IPublisherTevent_1
from Compze.Abstractions import TaggregateId
from System import DateTime

class ITaggregateCreatedTevent(ITaggregateTevent_0, typing.Protocol):
    pass


class ITaggregateDeletedTevent(ITaggregateTevent_0, typing.Protocol):
    pass


class ITaggregateTevent_GenericClasses(abc.ABCMeta):
    Generic_ITaggregateTevent_GenericClasses_ITaggregateTevent_1_TTeventInterface = typing.TypeVar('Generic_ITaggregateTevent_GenericClasses_ITaggregateTevent_1_TTeventInterface')
    def __getitem__(self, types : typing.Type[Generic_ITaggregateTevent_GenericClasses_ITaggregateTevent_1_TTeventInterface]) -> typing.Type[ITaggregateTevent_1[Generic_ITaggregateTevent_GenericClasses_ITaggregateTevent_1_TTeventInterface]]: ...

class ITaggregateTevent(ITaggregateTevent_0, metaclass =ITaggregateTevent_GenericClasses): ...

class ITaggregateTevent_0(ITevent, typing.Protocol):
    @property
    def TaggregateId(self) -> TaggregateId: ...
    @property
    def TaggregateVersion(self) -> int: ...
    @property
    def UtcTimeStamp(self) -> DateTime: ...


ITaggregateTevent_1_TTeventInterface = typing.TypeVar('ITaggregateTevent_1_TTeventInterface', covariant=True)
class ITaggregateTevent_1(typing.Generic[ITaggregateTevent_1_TTeventInterface], IPublisherTevent_1[ITaggregateTevent_1_TTeventInterface], typing.Protocol):
    pass


class TaggregateTevent(abc.ABC):
    @property
    def TaggregateId(self) -> TaggregateId: ...
    @TaggregateId.setter
    def TaggregateId(self, value: TaggregateId) -> TaggregateId: ...
    @property
    def TaggregateVersion(self) -> int: ...
    @TaggregateVersion.setter
    def TaggregateVersion(self, value: int) -> int: ...
    @property
    def UtcTimeStamp(self) -> DateTime: ...
    @UtcTimeStamp.setter
    def UtcTimeStamp(self, value: DateTime) -> DateTime: ...

