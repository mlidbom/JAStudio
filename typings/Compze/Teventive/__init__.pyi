import typing, abc
from Compze.Teventive.Taggregates.Tevents import ITaggregateTevent, ITaggregateTevent_1
from Compze.Abstractions import TaggregateId, ITentity
from System import DateTime, IObservable_1, Action_1, IDisposable
from System.Collections.Generic import IReadOnlyList_1, IEnumerable_1
from Compze.Tessaging.TessageTypes import IPublisherTevent_1, ITevent

class IMutableTaggregateTevent(ITaggregateTevent_0, typing.Protocol):
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


class ITeventRouted(typing.Protocol):
    @property
    def TeventAppliers(self) -> TeventRouting: ...
    @property
    def TeventHandlers(self) -> TeventRouting: ...


class ITeventRoutedMember_GenericClasses(abc.ABCMeta):
    Generic_ITeventRoutedMember_GenericClasses_ITeventRoutedMember_1_TTevent = typing.TypeVar('Generic_ITeventRoutedMember_GenericClasses_ITeventRoutedMember_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_ITeventRoutedMember_GenericClasses_ITeventRoutedMember_1_TTevent]) -> typing.Type[ITeventRoutedMember_1[Generic_ITeventRoutedMember_GenericClasses_ITeventRoutedMember_1_TTevent]]: ...

ITeventRoutedMember : ITeventRoutedMember_GenericClasses

ITeventRoutedMember_1_TTevent = typing.TypeVar('ITeventRoutedMember_1_TTevent')
class ITeventRoutedMember_1(typing.Generic[ITeventRoutedMember_1_TTevent], typing.Protocol):
    @abc.abstractmethod
    def DispatchInternal(self, wrapped: IPublisherTevent_1[ITevent], toTeventHandlers: bool) -> None: ...


class ITeventSubscriber_GenericClasses(abc.ABCMeta):
    Generic_ITeventSubscriber_GenericClasses_ITeventSubscriber_1_TTevent = typing.TypeVar('Generic_ITeventSubscriber_GenericClasses_ITeventSubscriber_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_ITeventSubscriber_GenericClasses_ITeventSubscriber_1_TTevent]) -> typing.Type[ITeventSubscriber_1[Generic_ITeventSubscriber_GenericClasses_ITeventSubscriber_1_TTevent]]: ...

ITeventSubscriber : ITeventSubscriber_GenericClasses

ITeventSubscriber_1_TTevent = typing.TypeVar('ITeventSubscriber_1_TTevent', contravariant=True)
class ITeventSubscriber_1(typing.Generic[ITeventSubscriber_1_TTevent], IDisposable, typing.Protocol):
    # Skipped AfterHandlers due to it being static, abstract and generic.

    AfterHandlers : AfterHandlers_MethodGroup[ITeventSubscriber_1_TTevent]
    AfterHandlers_MethodGroup_ITeventSubscriber_1_TTevent = typing.TypeVar('AfterHandlers_MethodGroup_ITeventSubscriber_1_TTevent', contravariant=True)
    class AfterHandlers_MethodGroup(typing.Generic[AfterHandlers_MethodGroup_ITeventSubscriber_1_TTevent]):
        AfterHandlers_MethodGroup_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.AfterHandlers_MethodGroup_ITeventSubscriber_1_TTevent
        def __getitem__(self, t:typing.Type[AfterHandlers_1_T1]) -> AfterHandlers_1[AfterHandlers_MethodGroup_ITeventSubscriber_1_TTevent, AfterHandlers_1_T1]: ...

        AfterHandlers_1_ITeventSubscriber_1_TTevent = typing.TypeVar('AfterHandlers_1_ITeventSubscriber_1_TTevent', contravariant=True)
        AfterHandlers_1_T1 = typing.TypeVar('AfterHandlers_1_T1')
        class AfterHandlers_1(typing.Generic[AfterHandlers_1_ITeventSubscriber_1_TTevent, AfterHandlers_1_T1]):
            AfterHandlers_1_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.AfterHandlers_MethodGroup.AfterHandlers_1_ITeventSubscriber_1_TTevent
            AfterHandlers_1_THandledTevent = ITeventSubscriber_1.AfterHandlers_MethodGroup.AfterHandlers_1_T1
            def __call__(self, runAfterHandlers: Action_1[AfterHandlers_1_THandledTevent]) -> ITeventSubscriber_1[AfterHandlers_1_ITeventSubscriber_1_TTevent]:...


    # Skipped BeforeHandlers due to it being static, abstract and generic.

    BeforeHandlers : BeforeHandlers_MethodGroup[ITeventSubscriber_1_TTevent]
    BeforeHandlers_MethodGroup_ITeventSubscriber_1_TTevent = typing.TypeVar('BeforeHandlers_MethodGroup_ITeventSubscriber_1_TTevent', contravariant=True)
    class BeforeHandlers_MethodGroup(typing.Generic[BeforeHandlers_MethodGroup_ITeventSubscriber_1_TTevent]):
        BeforeHandlers_MethodGroup_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.BeforeHandlers_MethodGroup_ITeventSubscriber_1_TTevent
        def __getitem__(self, t:typing.Type[BeforeHandlers_1_T1]) -> BeforeHandlers_1[BeforeHandlers_MethodGroup_ITeventSubscriber_1_TTevent, BeforeHandlers_1_T1]: ...

        BeforeHandlers_1_ITeventSubscriber_1_TTevent = typing.TypeVar('BeforeHandlers_1_ITeventSubscriber_1_TTevent', contravariant=True)
        BeforeHandlers_1_T1 = typing.TypeVar('BeforeHandlers_1_T1')
        class BeforeHandlers_1(typing.Generic[BeforeHandlers_1_ITeventSubscriber_1_TTevent, BeforeHandlers_1_T1]):
            BeforeHandlers_1_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.BeforeHandlers_MethodGroup.BeforeHandlers_1_ITeventSubscriber_1_TTevent
            BeforeHandlers_1_THandledTevent = ITeventSubscriber_1.BeforeHandlers_MethodGroup.BeforeHandlers_1_T1
            def __call__(self, runBeforeHandlers: Action_1[BeforeHandlers_1_THandledTevent]) -> ITeventSubscriber_1[BeforeHandlers_1_ITeventSubscriber_1_TTevent]:...


    # Skipped For due to it being static, abstract and generic.

    For : For_MethodGroup[ITeventSubscriber_1_TTevent]
    For_MethodGroup_ITeventSubscriber_1_TTevent = typing.TypeVar('For_MethodGroup_ITeventSubscriber_1_TTevent', contravariant=True)
    class For_MethodGroup(typing.Generic[For_MethodGroup_ITeventSubscriber_1_TTevent]):
        For_MethodGroup_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.For_MethodGroup_ITeventSubscriber_1_TTevent
        def __getitem__(self, t:typing.Type[For_1_T1]) -> For_1[For_MethodGroup_ITeventSubscriber_1_TTevent, For_1_T1]: ...

        For_1_ITeventSubscriber_1_TTevent = typing.TypeVar('For_1_ITeventSubscriber_1_TTevent', contravariant=True)
        For_1_T1 = typing.TypeVar('For_1_T1')
        class For_1(typing.Generic[For_1_ITeventSubscriber_1_TTevent, For_1_T1]):
            For_1_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.For_MethodGroup.For_1_ITeventSubscriber_1_TTevent
            For_1_THandledTevent = ITeventSubscriber_1.For_MethodGroup.For_1_T1
            def __call__(self, handler: Action_1[For_1_THandledTevent]) -> ITeventSubscriber_1[For_1_ITeventSubscriber_1_TTevent]:...


    # Skipped ForGenericTevent due to it being static, abstract and generic.

    ForGenericTevent : ForGenericTevent_MethodGroup[ITeventSubscriber_1_TTevent]
    ForGenericTevent_MethodGroup_ITeventSubscriber_1_TTevent = typing.TypeVar('ForGenericTevent_MethodGroup_ITeventSubscriber_1_TTevent', contravariant=True)
    class ForGenericTevent_MethodGroup(typing.Generic[ForGenericTevent_MethodGroup_ITeventSubscriber_1_TTevent]):
        ForGenericTevent_MethodGroup_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.ForGenericTevent_MethodGroup_ITeventSubscriber_1_TTevent
        def __getitem__(self, t:typing.Type[ForGenericTevent_1_T1]) -> ForGenericTevent_1[ForGenericTevent_MethodGroup_ITeventSubscriber_1_TTevent, ForGenericTevent_1_T1]: ...

        ForGenericTevent_1_ITeventSubscriber_1_TTevent = typing.TypeVar('ForGenericTevent_1_ITeventSubscriber_1_TTevent', contravariant=True)
        ForGenericTevent_1_T1 = typing.TypeVar('ForGenericTevent_1_T1')
        class ForGenericTevent_1(typing.Generic[ForGenericTevent_1_ITeventSubscriber_1_TTevent, ForGenericTevent_1_T1]):
            ForGenericTevent_1_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.ForGenericTevent_MethodGroup.ForGenericTevent_1_ITeventSubscriber_1_TTevent
            ForGenericTevent_1_THandledTevent = ITeventSubscriber_1.ForGenericTevent_MethodGroup.ForGenericTevent_1_T1
            def __call__(self, handler: Action_1[ForGenericTevent_1_THandledTevent]) -> ITeventSubscriber_1[ForGenericTevent_1_ITeventSubscriber_1_TTevent]:...


    # Skipped ForWrapped due to it being static, abstract and generic.

    ForWrapped : ForWrapped_MethodGroup[ITeventSubscriber_1_TTevent]
    ForWrapped_MethodGroup_ITeventSubscriber_1_TTevent = typing.TypeVar('ForWrapped_MethodGroup_ITeventSubscriber_1_TTevent', contravariant=True)
    class ForWrapped_MethodGroup(typing.Generic[ForWrapped_MethodGroup_ITeventSubscriber_1_TTevent]):
        ForWrapped_MethodGroup_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.ForWrapped_MethodGroup_ITeventSubscriber_1_TTevent
        def __getitem__(self, t:typing.Type[ForWrapped_1_T1]) -> ForWrapped_1[ForWrapped_MethodGroup_ITeventSubscriber_1_TTevent, ForWrapped_1_T1]: ...

        ForWrapped_1_ITeventSubscriber_1_TTevent = typing.TypeVar('ForWrapped_1_ITeventSubscriber_1_TTevent', contravariant=True)
        ForWrapped_1_T1 = typing.TypeVar('ForWrapped_1_T1')
        class ForWrapped_1(typing.Generic[ForWrapped_1_ITeventSubscriber_1_TTevent, ForWrapped_1_T1]):
            ForWrapped_1_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.ForWrapped_MethodGroup.ForWrapped_1_ITeventSubscriber_1_TTevent
            ForWrapped_1_TWrapperTevent = ITeventSubscriber_1.ForWrapped_MethodGroup.ForWrapped_1_T1
            def __call__(self, handler: Action_1[ForWrapped_1_TWrapperTevent]) -> ITeventSubscriber_1[ForWrapped_1_ITeventSubscriber_1_TTevent]:...


    # Skipped ForWrappedGeneric due to it being static, abstract and generic.

    ForWrappedGeneric : ForWrappedGeneric_MethodGroup[ITeventSubscriber_1_TTevent]
    ForWrappedGeneric_MethodGroup_ITeventSubscriber_1_TTevent = typing.TypeVar('ForWrappedGeneric_MethodGroup_ITeventSubscriber_1_TTevent', contravariant=True)
    class ForWrappedGeneric_MethodGroup(typing.Generic[ForWrappedGeneric_MethodGroup_ITeventSubscriber_1_TTevent]):
        ForWrappedGeneric_MethodGroup_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.ForWrappedGeneric_MethodGroup_ITeventSubscriber_1_TTevent
        def __getitem__(self, t:typing.Type[ForWrappedGeneric_1_T1]) -> ForWrappedGeneric_1[ForWrappedGeneric_MethodGroup_ITeventSubscriber_1_TTevent, ForWrappedGeneric_1_T1]: ...

        ForWrappedGeneric_1_ITeventSubscriber_1_TTevent = typing.TypeVar('ForWrappedGeneric_1_ITeventSubscriber_1_TTevent', contravariant=True)
        ForWrappedGeneric_1_T1 = typing.TypeVar('ForWrappedGeneric_1_T1')
        class ForWrappedGeneric_1(typing.Generic[ForWrappedGeneric_1_ITeventSubscriber_1_TTevent, ForWrappedGeneric_1_T1]):
            ForWrappedGeneric_1_ITeventSubscriber_1_TTevent = ITeventSubscriber_1.ForWrappedGeneric_MethodGroup.ForWrappedGeneric_1_ITeventSubscriber_1_TTevent
            ForWrappedGeneric_1_TWrapperTevent = ITeventSubscriber_1.ForWrappedGeneric_MethodGroup.ForWrappedGeneric_1_T1
            def __call__(self, handler: Action_1[ForWrappedGeneric_1_TWrapperTevent]) -> ITeventSubscriber_1[ForWrappedGeneric_1_ITeventSubscriber_1_TTevent]:...




class TeventRouting(abc.ABC):
    pass

