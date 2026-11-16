import typing
from Microsoft.Extensions.DependencyInjection import IServiceCollection
from System import IServiceProvider
from Compze.DependencyInjection import DependencyInjectionContainer, ContainerBuilder
from Compze.DependencyInjection.Abstractions import IRootResolver, IScopeFactory, IScope, IComponentRegistrar, ContainerOptions
from System.Threading.Tasks import ValueTask

class IMicrosoftBuilderInternals(typing.Protocol):
    @property
    def ServiceCollection(self) -> IServiceCollection: ...


class IMicrosoftContainerInternals(typing.Protocol):
    @property
    def ServiceProvider(self) -> IServiceProvider: ...


class MicrosoftContainer(DependencyInjectionContainer, IRootResolver, IMicrosoftContainerInternals, IScopeFactory):
    def BeginScope(self) -> IScope: ...
    def CreateChildContainerBuilder(self) -> MicrosoftContainerBuilder: ...
    def Dispose(self) -> None: ...
    def DisposeAsync(self) -> ValueTask: ...


class MicrosoftContainerBuilder(ContainerBuilder, IMicrosoftBuilderInternals):
    def __init__(self, registrar: IComponentRegistrar = ...) -> None: ...
    @property
    def Registrar(self) -> IComponentRegistrar: ...
    def Build(self, options: ContainerOptions = ...) -> MicrosoftContainer: ...

