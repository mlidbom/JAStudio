import typing, abc
from System import IDisposable, Action_1, Func_3, Action_2
from System.Collections.Generic import IReadOnlyList_1
from Compze.DependencyInjection.Abstractions import IScopeResolver
from System.Threading.Tasks import Task
from Compze.Tessaging.TessageTypes import IPublisherTevent_1

class ITeventObservation(typing.Protocol):
    @abc.abstractmethod
    def ObserveTevents(self, observe: Action_1[TeventObservationRegistrar]) -> IDisposable: ...


class ITeventObservationRegistrar(typing.Protocol):
    # Skipped ForEachUnitOfWork due to it being static, abstract and generic.

    ForEachUnitOfWork : ForEachUnitOfWork_MethodGroup
    class ForEachUnitOfWork_MethodGroup:
        def __getitem__(self, t:typing.Type[ForEachUnitOfWork_1_T1]) -> ForEachUnitOfWork_1[ForEachUnitOfWork_1_T1]: ...

        ForEachUnitOfWork_1_T1 = typing.TypeVar('ForEachUnitOfWork_1_T1')
        class ForEachUnitOfWork_1(typing.Generic[ForEachUnitOfWork_1_T1]):
            ForEachUnitOfWork_1_TTevent = ITeventObservationRegistrar.ForEachUnitOfWork_MethodGroup.ForEachUnitOfWork_1_T1
            @typing.overload
            def __call__(self, observer: Func_3[IReadOnlyList_1[ForEachUnitOfWork_1_TTevent], IScopeResolver, Task]) -> ITeventObservationRegistrar:...
            @typing.overload
            def __call__(self, observer: Action_2[IReadOnlyList_1[ForEachUnitOfWork_1_TTevent], IScopeResolver]) -> ITeventObservationRegistrar:...


    # Skipped ForTevent due to it being static, abstract and generic.

    ForTevent : ForTevent_MethodGroup
    class ForTevent_MethodGroup:
        def __getitem__(self, t:typing.Type[ForTevent_1_T1]) -> ForTevent_1[ForTevent_1_T1]: ...

        ForTevent_1_T1 = typing.TypeVar('ForTevent_1_T1')
        class ForTevent_1(typing.Generic[ForTevent_1_T1]):
            ForTevent_1_TTevent = ITeventObservationRegistrar.ForTevent_MethodGroup.ForTevent_1_T1
            @typing.overload
            def __call__(self, observer: Func_3[ForTevent_1_TTevent, IScopeResolver, Task]) -> ITeventObservationRegistrar:...
            @typing.overload
            def __call__(self, observer: Action_2[ForTevent_1_TTevent, IScopeResolver]) -> ITeventObservationRegistrar:...




class PublisherTevent_GenericClasses(abc.ABCMeta):
    Generic_PublisherTevent_GenericClasses_PublisherTevent_1_TTevent = typing.TypeVar('Generic_PublisherTevent_GenericClasses_PublisherTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_PublisherTevent_GenericClasses_PublisherTevent_1_TTevent]) -> typing.Type[PublisherTevent_1[Generic_PublisherTevent_GenericClasses_PublisherTevent_1_TTevent]]: ...

PublisherTevent : PublisherTevent_GenericClasses

PublisherTevent_1_TTevent = typing.TypeVar('PublisherTevent_1_TTevent')
class PublisherTevent_1(typing.Generic[PublisherTevent_1_TTevent], IPublisherTevent_1[PublisherTevent_1_TTevent]):
    def __init__(self, tevent: PublisherTevent_1_TTevent) -> None: ...
    @property
    def Tevent(self) -> PublisherTevent_1_TTevent: ...


class TeventObservationRegistrar(ITeventObservationRegistrar):
    # Skipped ForEachUnitOfWork due to it being static, abstract and generic.

    ForEachUnitOfWork : ForEachUnitOfWork_MethodGroup
    class ForEachUnitOfWork_MethodGroup:
        def __getitem__(self, t:typing.Type[ForEachUnitOfWork_1_T1]) -> ForEachUnitOfWork_1[ForEachUnitOfWork_1_T1]: ...

        ForEachUnitOfWork_1_T1 = typing.TypeVar('ForEachUnitOfWork_1_T1')
        class ForEachUnitOfWork_1(typing.Generic[ForEachUnitOfWork_1_T1]):
            ForEachUnitOfWork_1_TTevent = TeventObservationRegistrar.ForEachUnitOfWork_MethodGroup.ForEachUnitOfWork_1_T1
            @typing.overload
            def __call__(self, observer: Func_3[IReadOnlyList_1[ForEachUnitOfWork_1_TTevent], IScopeResolver, Task]) -> TeventObservationRegistrar:...
            @typing.overload
            def __call__(self, observer: Action_2[IReadOnlyList_1[ForEachUnitOfWork_1_TTevent], IScopeResolver]) -> TeventObservationRegistrar:...


    # Skipped ForTevent due to it being static, abstract and generic.

    ForTevent : ForTevent_MethodGroup
    class ForTevent_MethodGroup:
        def __getitem__(self, t:typing.Type[ForTevent_1_T1]) -> ForTevent_1[ForTevent_1_T1]: ...

        ForTevent_1_T1 = typing.TypeVar('ForTevent_1_T1')
        class ForTevent_1(typing.Generic[ForTevent_1_T1]):
            ForTevent_1_TTevent = TeventObservationRegistrar.ForTevent_MethodGroup.ForTevent_1_T1
            @typing.overload
            def __call__(self, observer: Func_3[ForTevent_1_TTevent, IScopeResolver, Task]) -> TeventObservationRegistrar:...
            @typing.overload
            def __call__(self, observer: Action_2[ForTevent_1_TTevent, IScopeResolver]) -> TeventObservationRegistrar:...



