import typing, abc
from System import Array_1, Action_1, IAsyncDisposable, IDisposable
from Compze.DependencyInjection import ComponentRegistration
from System.Collections.Generic import IEnumerable_1

class ContainerOptions:
    def __init__(self) -> None: ...
    @property
    def AllowScopedResolutionFromRoot(self) -> bool: ...
    @AllowScopedResolutionFromRoot.setter
    def AllowScopedResolutionFromRoot(self, value: bool) -> bool: ...
    @classmethod
    @property
    def Default(cls) -> ContainerOptions: ...


class IComponentRegistrar(typing.Protocol):
    @property
    def IsClone(self) -> bool: ...
    @abc.abstractmethod
    def Clone(self) -> IComponentRegistrar: ...
    # Skipped IsRegistered due to it being static, abstract and generic.

    IsRegistered : IsRegistered_MethodGroup
    class IsRegistered_MethodGroup:
        def __getitem__(self, t:typing.Type[IsRegistered_1_T1]) -> IsRegistered_1[IsRegistered_1_T1]: ...

        IsRegistered_1_T1 = typing.TypeVar('IsRegistered_1_T1')
        class IsRegistered_1(typing.Generic[IsRegistered_1_T1]):
            IsRegistered_1_TComponent = IComponentRegistrar.IsRegistered_MethodGroup.IsRegistered_1_T1
            def __call__(self) -> bool:...


    # Skipped Register due to it being static, abstract and generic.

    Register : Register_MethodGroup
    class Register_MethodGroup:
        @typing.overload
        def __call__(self, registrationMethods: Array_1[Action_1[IComponentRegistrar]]) -> IComponentRegistrar:...
        @typing.overload
        def __call__(self, registrations: Array_1[ComponentRegistration]) -> IComponentRegistrar:...

    # Skipped TryGetTestingRegistrar due to it being static, abstract and generic.

    TryGetTestingRegistrar : TryGetTestingRegistrar_MethodGroup
    class TryGetTestingRegistrar_MethodGroup:
        def __getitem__(self, t:typing.Type[TryGetTestingRegistrar_1_T1]) -> TryGetTestingRegistrar_1[TryGetTestingRegistrar_1_T1]: ...

        TryGetTestingRegistrar_1_T1 = typing.TypeVar('TryGetTestingRegistrar_1_T1')
        class TryGetTestingRegistrar_1(typing.Generic[TryGetTestingRegistrar_1_T1]):
            TryGetTestingRegistrar_1_TTestingRegistrar = IComponentRegistrar.TryGetTestingRegistrar_MethodGroup.TryGetTestingRegistrar_1_T1
            def __call__(self) -> TryGetTestingRegistrar_1_TTestingRegistrar:...




class IContainerBuilder(typing.Protocol):
    @property
    def Registrar(self) -> IComponentRegistrar: ...
    @abc.abstractmethod
    def Build(self, options: ContainerOptions = ...) -> IDependencyInjectionContainer: ...


class IDependencyInjectionContainer(IAsyncDisposable, IDisposable, typing.Protocol):
    @property
    def RootResolver(self) -> IRootResolver: ...
    @property
    def ScopeFactory(self) -> IScopeFactory: ...
    @abc.abstractmethod
    def CreateChildContainerBuilder(self) -> IContainerBuilder: ...
    @abc.abstractmethod
    def CreateCloneContainerBuilder(self) -> IContainerBuilder: ...


class IRootResolver(IServiceResolver, typing.Protocol):
    pass


class IScope(IDisposable, typing.Protocol):
    @property
    def Resolver(self) -> IScopeResolver: ...


class IScopeFactory(typing.Protocol):
    @abc.abstractmethod
    def BeginScope(self) -> IScope: ...


class IScopeResolver(IServiceResolver, typing.Protocol):
    pass


class IServiceResolver(typing.Protocol):
    @abc.abstractmethod
    def Resolve(self, serviceType: typing.Type[typing.Any]) -> typing.Any: ...
    @abc.abstractmethod
    def ResolveSet(self, serviceType: typing.Type[typing.Any]) -> IEnumerable_1[typing.Any]: ...


class Lifestyle(typing.SupportsInt):
    @typing.overload
    def __init__(self, value : int) -> None: ...
    @typing.overload
    def __init__(self, value : int, force_if_true: bool) -> None: ...
    def __int__(self) -> int: ...
    
    # Values:
    Singleton : Lifestyle # 0
    Scoped : Lifestyle # 1
    TrackedTransient : Lifestyle # 2

