import typing, abc
from Compze.Teventive.Taggregates.Tevents import ITaggregateTevent, ITaggregateTevent_1
from Compze.Abstractions import TaggregateId, ITentity
from System import DateTime, IObservable_1, Action_1
from System.Collections.Generic import IReadOnlyList_1, IEnumerable_1
from Compze.Tessaging.TessageTypes import IPublisherTevent_1, ITevent

class IMutableTaggregateTevent(ITaggregateTevent, typing.Protocol):
    @abc.abstractmethod
    def SetTaggregateIdInternal(self, taggregateId: TaggregateId) -> None: ...
    @abc.abstractmethod
    def SetTaggregateVersionInternal(self, taggregateVersion: int) -> None: ...
    @abc.abstractmethod
    def SetUtcTimeStampInternal(self, utcTimeStamp: DateTime) -> None: ...


class ITaggregate_GenericClasses(abc.ABCMeta):
    Generic_ITaggregate_GenericClasses_ITaggregate_1_TTevent = typing.TypeVar('Generic_ITaggregate_GenericClasses_ITaggregate_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_ITaggregate_GenericClasses_ITaggregate_1_TTevent]) -> typing.Type[ITaggregate_1[Generic_ITaggregate_GenericClasses_ITaggregate_1_TTevent]]: ...

class ITaggregate(ITaggregate_0, metaclass =ITaggregate_GenericClasses): ...

class ITaggregate_0(ITentity, typing.Protocol):
    @property
    def Id(self) -> TaggregateId: ...
    @property
    def TeventStream(self) -> IObservable_1[ITaggregateTevent_1[ITaggregateTevent]]: ...
    @property
    def Version(self) -> int: ...
    @abc.abstractmethod
    def Commit(self, commitTevents: Action_1[IReadOnlyList_1[ITaggregateTevent_1[ITaggregateTevent]]]) -> None: ...
    @abc.abstractmethod
    def LoadFromHistory(self, history: IEnumerable_1[ITaggregateTevent_1[ITaggregateTevent]]) -> None: ...


ITaggregate_1_TTevent = typing.TypeVar('ITaggregate_1_TTevent', covariant=True)
class ITaggregate_1(typing.Generic[ITaggregate_1_TTevent], ITaggregate_0, typing.Protocol):
    @property
    def TeventStream(self) -> IObservable_1[ITaggregateTevent_1[ITaggregate_1_TTevent]]: ...


class ITeventRoutedMember_GenericClasses(abc.ABCMeta):
    Generic_ITeventRoutedMember_GenericClasses_ITeventRoutedMember_1_TTevent = typing.TypeVar('Generic_ITeventRoutedMember_GenericClasses_ITeventRoutedMember_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_ITeventRoutedMember_GenericClasses_ITeventRoutedMember_1_TTevent]) -> typing.Type[ITeventRoutedMember_1[Generic_ITeventRoutedMember_GenericClasses_ITeventRoutedMember_1_TTevent]]: ...

ITeventRoutedMember : ITeventRoutedMember_GenericClasses

ITeventRoutedMember_1_TTevent = typing.TypeVar('ITeventRoutedMember_1_TTevent')
class ITeventRoutedMember_1(typing.Generic[ITeventRoutedMember_1_TTevent], typing.Protocol):
    @abc.abstractmethod
    def DispatchInternal(self, wrapped: IPublisherTevent_1[ITevent]) -> None: ...


class TeventRouting(abc.ABC):
    pass

