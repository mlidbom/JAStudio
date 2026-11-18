import typing, abc
from Compze.Abstractions import ITentity, TaggregateId
from System import IObservable_1, Action_1
from Compze.Teventive.Taggregates.Tevents import ITaggregateTevent_1, ITaggregateTevent
from System.Collections.Generic import IReadOnlyList_1, IEnumerable_1
from Compze.Tessaging.TessageTypes import IPublisherTevent_1, ITevent

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


class ITeventRoutedPart_GenericClasses(abc.ABCMeta):
    Generic_ITeventRoutedPart_GenericClasses_ITeventRoutedPart_1_TTevent = typing.TypeVar('Generic_ITeventRoutedPart_GenericClasses_ITeventRoutedPart_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_ITeventRoutedPart_GenericClasses_ITeventRoutedPart_1_TTevent]) -> typing.Type[ITeventRoutedPart_1[Generic_ITeventRoutedPart_GenericClasses_ITeventRoutedPart_1_TTevent]]: ...

ITeventRoutedPart : ITeventRoutedPart_GenericClasses

ITeventRoutedPart_1_TTevent = typing.TypeVar('ITeventRoutedPart_1_TTevent')
class ITeventRoutedPart_1(typing.Generic[ITeventRoutedPart_1_TTevent], typing.Protocol):
    @abc.abstractmethod
    def DispatchInternal(self, wrapped: IPublisherTevent_1[ITevent]) -> None: ...


class TeventRouting(abc.ABC):
    pass

