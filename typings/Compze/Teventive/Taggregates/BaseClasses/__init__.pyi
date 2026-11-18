import typing, clr, abc
import Compze.Teventive.Taggregates.Tevents
from System.Collections.Generic import IEnumerable_1, IReadOnlyList_1
from Compze.Teventive import ITeventRouted, ITaggregate_1, ITeventRoutedMember_1
from Compze.Abstractions import TaggregateId, EntityId, EntityId_1, VersionedEntity_1
from System import Guid
from Compze.Tessaging.TessageBus import PublisherTevent_1
from Compze.Teventive.Taggregates.Tevents import ITaggregateTevent_1

class IEntityCollectionManager_GenericClasses(abc.ABCMeta):
    Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntity = typing.TypeVar('Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntity')
    Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityId = typing.TypeVar('Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityId')
    Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityTevent = typing.TypeVar('Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityTevent')
    Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityTeventImplementation = typing.TypeVar('Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityTeventImplementation')
    Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityCreatedTevent = typing.TypeVar('Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityCreatedTevent')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntity], typing.Type[Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityId], typing.Type[Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityTevent], typing.Type[Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityTeventImplementation], typing.Type[Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityCreatedTevent]]) -> typing.Type[IEntityCollectionManager_5[Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntity, Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityId, Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityTevent, Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityTeventImplementation, Generic_IEntityCollectionManager_GenericClasses_IEntityCollectionManager_5_TEntityCreatedTevent]]: ...

IEntityCollectionManager : IEntityCollectionManager_GenericClasses

IEntityCollectionManager_5_TEntity = typing.TypeVar('IEntityCollectionManager_5_TEntity')
IEntityCollectionManager_5_TEntityId = typing.TypeVar('IEntityCollectionManager_5_TEntityId', contravariant=True)
IEntityCollectionManager_5_TEntityTevent = typing.TypeVar('IEntityCollectionManager_5_TEntityTevent', contravariant=True)
IEntityCollectionManager_5_TEntityTeventImplementation = typing.TypeVar('IEntityCollectionManager_5_TEntityTeventImplementation', contravariant=True)
IEntityCollectionManager_5_TEntityCreatedTevent = typing.TypeVar('IEntityCollectionManager_5_TEntityCreatedTevent', contravariant=True)
class IEntityCollectionManager_5(typing.Generic[IEntityCollectionManager_5_TEntity, IEntityCollectionManager_5_TEntityId, IEntityCollectionManager_5_TEntityTevent, IEntityCollectionManager_5_TEntityTeventImplementation, IEntityCollectionManager_5_TEntityCreatedTevent], typing.Protocol):
    @property
    def Entities(self) -> IReadOnlyEntityCollection_2[IEntityCollectionManager_5_TEntity, IEntityCollectionManager_5_TEntityId]: ...
    @abc.abstractmethod
    def AddRestored(self, id: IEntityCollectionManager_5_TEntityId) -> IEntityCollectionManager_5_TEntity: ...
    # Skipped AddByPublishing due to it being static, abstract and generic.

    AddByPublishing : AddByPublishing_MethodGroup[IEntityCollectionManager_5_TEntity, IEntityCollectionManager_5_TEntityId, IEntityCollectionManager_5_TEntityTevent, IEntityCollectionManager_5_TEntityTeventImplementation, IEntityCollectionManager_5_TEntityCreatedTevent]
    AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntity = typing.TypeVar('AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntity')
    AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityId = typing.TypeVar('AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityId', contravariant=True)
    AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTevent = typing.TypeVar('AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTevent', contravariant=True)
    AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTeventImplementation = typing.TypeVar('AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTeventImplementation', contravariant=True)
    AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityCreatedTevent = typing.TypeVar('AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityCreatedTevent', contravariant=True)
    class AddByPublishing_MethodGroup(typing.Generic[AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntity, AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityId, AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTevent, AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTeventImplementation, AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityCreatedTevent]):
        AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntity = IEntityCollectionManager_5.AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntity
        AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityId = IEntityCollectionManager_5.AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityId
        AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTevent = IEntityCollectionManager_5.AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTevent
        AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTeventImplementation = IEntityCollectionManager_5.AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTeventImplementation
        AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityCreatedTevent = IEntityCollectionManager_5.AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityCreatedTevent
        def __getitem__(self, t:typing.Type[AddByPublishing_1_T1]) -> AddByPublishing_1[AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntity, AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityId, AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTevent, AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityTeventImplementation, AddByPublishing_MethodGroup_IEntityCollectionManager_5_TEntityCreatedTevent, AddByPublishing_1_T1]: ...

        AddByPublishing_1_IEntityCollectionManager_5_TEntity = typing.TypeVar('AddByPublishing_1_IEntityCollectionManager_5_TEntity')
        AddByPublishing_1_IEntityCollectionManager_5_TEntityId = typing.TypeVar('AddByPublishing_1_IEntityCollectionManager_5_TEntityId', contravariant=True)
        AddByPublishing_1_IEntityCollectionManager_5_TEntityTevent = typing.TypeVar('AddByPublishing_1_IEntityCollectionManager_5_TEntityTevent', contravariant=True)
        AddByPublishing_1_IEntityCollectionManager_5_TEntityTeventImplementation = typing.TypeVar('AddByPublishing_1_IEntityCollectionManager_5_TEntityTeventImplementation', contravariant=True)
        AddByPublishing_1_IEntityCollectionManager_5_TEntityCreatedTevent = typing.TypeVar('AddByPublishing_1_IEntityCollectionManager_5_TEntityCreatedTevent', contravariant=True)
        AddByPublishing_1_T1 = typing.TypeVar('AddByPublishing_1_T1', bound=Union[AddByPublishing_1_IEntityCollectionManager_5_TEntityTeventImplementation, AddByPublishing_1_IEntityCollectionManager_5_TEntityCreatedTevent])
        class AddByPublishing_1(typing.Generic[AddByPublishing_1_IEntityCollectionManager_5_TEntity, AddByPublishing_1_IEntityCollectionManager_5_TEntityId, AddByPublishing_1_IEntityCollectionManager_5_TEntityTevent, AddByPublishing_1_IEntityCollectionManager_5_TEntityTeventImplementation, AddByPublishing_1_IEntityCollectionManager_5_TEntityCreatedTevent, AddByPublishing_1_T1]):
            AddByPublishing_1_IEntityCollectionManager_5_TEntity = IEntityCollectionManager_5.AddByPublishing_MethodGroup.AddByPublishing_1_IEntityCollectionManager_5_TEntity
            AddByPublishing_1_IEntityCollectionManager_5_TEntityId = IEntityCollectionManager_5.AddByPublishing_MethodGroup.AddByPublishing_1_IEntityCollectionManager_5_TEntityId
            AddByPublishing_1_IEntityCollectionManager_5_TEntityTevent = IEntityCollectionManager_5.AddByPublishing_MethodGroup.AddByPublishing_1_IEntityCollectionManager_5_TEntityTevent
            AddByPublishing_1_IEntityCollectionManager_5_TEntityTeventImplementation = IEntityCollectionManager_5.AddByPublishing_MethodGroup.AddByPublishing_1_IEntityCollectionManager_5_TEntityTeventImplementation
            AddByPublishing_1_IEntityCollectionManager_5_TEntityCreatedTevent = IEntityCollectionManager_5.AddByPublishing_MethodGroup.AddByPublishing_1_IEntityCollectionManager_5_TEntityCreatedTevent
            AddByPublishing_1_TCreationTevent = IEntityCollectionManager_5.AddByPublishing_MethodGroup.AddByPublishing_1_T1
            def __call__(self, creationTevent: AddByPublishing_1_TCreationTevent) -> AddByPublishing_1_IEntityCollectionManager_5_TEntity:...




class IReadOnlyEntityCollection_GenericClasses(abc.ABCMeta):
    Generic_IReadOnlyEntityCollection_GenericClasses_IReadOnlyEntityCollection_2_TEntity = typing.TypeVar('Generic_IReadOnlyEntityCollection_GenericClasses_IReadOnlyEntityCollection_2_TEntity')
    Generic_IReadOnlyEntityCollection_GenericClasses_IReadOnlyEntityCollection_2_TEntityId = typing.TypeVar('Generic_IReadOnlyEntityCollection_GenericClasses_IReadOnlyEntityCollection_2_TEntityId')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_IReadOnlyEntityCollection_GenericClasses_IReadOnlyEntityCollection_2_TEntity], typing.Type[Generic_IReadOnlyEntityCollection_GenericClasses_IReadOnlyEntityCollection_2_TEntityId]]) -> typing.Type[IReadOnlyEntityCollection_2[Generic_IReadOnlyEntityCollection_GenericClasses_IReadOnlyEntityCollection_2_TEntity, Generic_IReadOnlyEntityCollection_GenericClasses_IReadOnlyEntityCollection_2_TEntityId]]: ...

IReadOnlyEntityCollection : IReadOnlyEntityCollection_GenericClasses

IReadOnlyEntityCollection_2_TEntity = typing.TypeVar('IReadOnlyEntityCollection_2_TEntity')
IReadOnlyEntityCollection_2_TEntityId = typing.TypeVar('IReadOnlyEntityCollection_2_TEntityId', contravariant=True)
class IReadOnlyEntityCollection_2(typing.Generic[IReadOnlyEntityCollection_2_TEntity, IReadOnlyEntityCollection_2_TEntityId], IEnumerable_1[IReadOnlyEntityCollection_2_TEntity], typing.Protocol):
    @property
    def InCreationOrder(self) -> IReadOnlyList_1[IReadOnlyEntityCollection_2_TEntity]: ...
    @abc.abstractmethod
    def Contains(self, id: IReadOnlyEntityCollection_2_TEntityId) -> bool: ...
    @abc.abstractmethod
    def Get(self, id: IReadOnlyEntityCollection_2_TEntityId) -> IReadOnlyEntityCollection_2_TEntity: ...
    @abc.abstractmethod
    def TryGet(self, id: IReadOnlyEntityCollection_2_TEntityId, component: clr.Reference[IReadOnlyEntityCollection_2_TEntity]) -> bool: ...
    def __getitem__(self, id: IReadOnlyEntityCollection_2_TEntityId) -> IReadOnlyEntityCollection_2_TEntity: ...


class ITeventiveInternals_GenericClasses(abc.ABCMeta):
    Generic_ITeventiveInternals_GenericClasses_ITeventiveInternals_2_TTevent = typing.TypeVar('Generic_ITeventiveInternals_GenericClasses_ITeventiveInternals_2_TTevent')
    Generic_ITeventiveInternals_GenericClasses_ITeventiveInternals_2_TTeventImplementation = typing.TypeVar('Generic_ITeventiveInternals_GenericClasses_ITeventiveInternals_2_TTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, ITeventiveInternals_GenericClasses_ITeventiveInternals_2_TTevent])
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_ITeventiveInternals_GenericClasses_ITeventiveInternals_2_TTevent], typing.Type[Generic_ITeventiveInternals_GenericClasses_ITeventiveInternals_2_TTeventImplementation]]) -> typing.Type[ITeventiveInternals_2[Generic_ITeventiveInternals_GenericClasses_ITeventiveInternals_2_TTevent, Generic_ITeventiveInternals_GenericClasses_ITeventiveInternals_2_TTeventImplementation]]: ...

ITeventiveInternals : ITeventiveInternals_GenericClasses

ITeventiveInternals_2_TTevent = typing.TypeVar('ITeventiveInternals_2_TTevent', contravariant=True)
ITeventiveInternals_2_TTeventImplementation = typing.TypeVar('ITeventiveInternals_2_TTeventImplementation', contravariant=True, bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, ITeventiveInternals_2_TTevent])
class ITeventiveInternals_2(typing.Generic[ITeventiveInternals_2_TTevent, ITeventiveInternals_2_TTeventImplementation], ITeventRouted, typing.Protocol):
    @abc.abstractmethod
    def PublishInternal(self, theTevent: ITeventiveInternals_2_TTeventImplementation) -> None: ...


class Taggregate_GenericClasses(abc.ABCMeta):
    Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregate = typing.TypeVar('Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregate')
    Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregateTevent = typing.TypeVar('Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregateTevent')
    Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregateTeventImplementation = typing.TypeVar('Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregateTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Taggregate_GenericClasses_Taggregate_5_TTaggregateTevent])
    Generic_Taggregate_GenericClasses_Taggregate_5_TWrapperTeventInterface = typing.TypeVar('Generic_Taggregate_GenericClasses_Taggregate_5_TWrapperTeventInterface')
    Generic_Taggregate_GenericClasses_Taggregate_5_TWrapperTeventImplementation = typing.TypeVar('Generic_Taggregate_GenericClasses_Taggregate_5_TWrapperTeventImplementation')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregate], typing.Type[Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregateTevent], typing.Type[Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregateTeventImplementation], typing.Type[Generic_Taggregate_GenericClasses_Taggregate_5_TWrapperTeventInterface], typing.Type[Generic_Taggregate_GenericClasses_Taggregate_5_TWrapperTeventImplementation]]) -> typing.Type[Taggregate_5[Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregate, Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregateTevent, Generic_Taggregate_GenericClasses_Taggregate_5_TTaggregateTeventImplementation, Generic_Taggregate_GenericClasses_Taggregate_5_TWrapperTeventInterface, Generic_Taggregate_GenericClasses_Taggregate_5_TWrapperTeventImplementation]]: ...

Taggregate : Taggregate_GenericClasses

Taggregate_5_TTaggregate = typing.TypeVar('Taggregate_5_TTaggregate')
Taggregate_5_TTaggregateTevent = typing.TypeVar('Taggregate_5_TTaggregateTevent')
Taggregate_5_TTaggregateTeventImplementation = typing.TypeVar('Taggregate_5_TTaggregateTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Taggregate_5_TTaggregateTevent])
Taggregate_5_TWrapperTeventInterface = typing.TypeVar('Taggregate_5_TWrapperTeventInterface')
Taggregate_5_TWrapperTeventImplementation = typing.TypeVar('Taggregate_5_TWrapperTeventImplementation')
class Taggregate_5(typing.Generic[Taggregate_5_TTaggregate, Taggregate_5_TTaggregateTevent, Taggregate_5_TTaggregateTeventImplementation, Taggregate_5_TWrapperTeventInterface, Taggregate_5_TWrapperTeventImplementation], TeventApplyingRoot_2[Taggregate_5_TTaggregate, Taggregate_5_TTaggregateTevent], ITaggregate_1[Taggregate_5_TTaggregateTevent], ITeventiveInternals_2[Taggregate_5_TTaggregateTevent, Taggregate_5_TTaggregateTeventImplementation]):
    @property
    def Id(self) -> TaggregateId: ...
    @property
    def Id(self) -> EntityId: ...
    @Id.setter
    def Id(self, value: EntityId) -> EntityId: ...
    @property
    def Id(self) -> EntityId_1[Guid]: ...
    @Id.setter
    def Id(self, value: EntityId_1[Guid]) -> EntityId_1[Guid]: ...
    @property
    def Version(self) -> int: ...
    @Version.setter
    def Version(self, value: int) -> int: ...

    Component_GenericClasses_Taggregate_5_TTaggregate = typing.TypeVar('Component_GenericClasses_Taggregate_5_TTaggregate')
    Component_GenericClasses_Taggregate_5_TTaggregateTevent = typing.TypeVar('Component_GenericClasses_Taggregate_5_TTaggregateTevent')
    Component_GenericClasses_Taggregate_5_TTaggregateTeventImplementation = typing.TypeVar('Component_GenericClasses_Taggregate_5_TTaggregateTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Component_GenericClasses_Taggregate_5_TTaggregateTevent])
    Component_GenericClasses_Taggregate_5_TWrapperTeventInterface = typing.TypeVar('Component_GenericClasses_Taggregate_5_TWrapperTeventInterface')
    Component_GenericClasses_Taggregate_5_TWrapperTeventImplementation = typing.TypeVar('Component_GenericClasses_Taggregate_5_TWrapperTeventImplementation')
    class Component_GenericClasses(typing.Generic[Component_GenericClasses_Taggregate_5_TTaggregate, Component_GenericClasses_Taggregate_5_TTaggregateTevent, Component_GenericClasses_Taggregate_5_TTaggregateTeventImplementation, Component_GenericClasses_Taggregate_5_TWrapperTeventInterface, Component_GenericClasses_Taggregate_5_TWrapperTeventImplementation], abc.ABCMeta):
        Component_GenericClasses_Taggregate_5_TTaggregate = Taggregate_5.Component_GenericClasses_Taggregate_5_TTaggregate
        Component_GenericClasses_Taggregate_5_TTaggregateTevent = Taggregate_5.Component_GenericClasses_Taggregate_5_TTaggregateTevent
        Component_GenericClasses_Taggregate_5_TTaggregateTeventImplementation = Taggregate_5.Component_GenericClasses_Taggregate_5_TTaggregateTeventImplementation
        Component_GenericClasses_Taggregate_5_TWrapperTeventInterface = Taggregate_5.Component_GenericClasses_Taggregate_5_TWrapperTeventInterface
        Component_GenericClasses_Taggregate_5_TWrapperTeventImplementation = Taggregate_5.Component_GenericClasses_Taggregate_5_TWrapperTeventImplementation
        Generic_Component_GenericClasses_Component_8_TComponent = typing.TypeVar('Generic_Component_GenericClasses_Component_8_TComponent')
        Generic_Component_GenericClasses_Component_8_TComponentTevent = typing.TypeVar('Generic_Component_GenericClasses_Component_8_TComponentTevent')
        Generic_Component_GenericClasses_Component_8_TComponentTeventImplementation = typing.TypeVar('Generic_Component_GenericClasses_Component_8_TComponentTeventImplementation', bound=Union[Component_GenericClasses_Component_8_TTaggregateTeventImplementation, Component_GenericClasses_Component_8_TComponentTevent])
        def __getitem__(self, types : typing.Tuple[typing.Type[Generic_Component_GenericClasses_Component_8_TComponent], typing.Type[Generic_Component_GenericClasses_Component_8_TComponentTevent], typing.Type[Generic_Component_GenericClasses_Component_8_TComponentTeventImplementation]]) -> typing.Type[Taggregate_5.Component_8[Component_GenericClasses_Taggregate_5_TTaggregate, Component_GenericClasses_Taggregate_5_TTaggregateTevent, Component_GenericClasses_Taggregate_5_TTaggregateTeventImplementation, Component_GenericClasses_Taggregate_5_TWrapperTeventInterface, Component_GenericClasses_Taggregate_5_TWrapperTeventImplementation, Generic_Component_GenericClasses_Component_8_TComponent, Generic_Component_GenericClasses_Component_8_TComponentTevent, Generic_Component_GenericClasses_Component_8_TComponentTeventImplementation]]: ...

    Component : Component_GenericClasses[Taggregate_5_TTaggregate, Taggregate_5_TTaggregateTevent, Taggregate_5_TTaggregateTeventImplementation, Taggregate_5_TWrapperTeventInterface, Taggregate_5_TWrapperTeventImplementation]

    Component_8_TTaggregate = typing.TypeVar('Component_8_TTaggregate')
    Component_8_TTaggregateTevent = typing.TypeVar('Component_8_TTaggregateTevent')
    Component_8_TTaggregateTeventImplementation = typing.TypeVar('Component_8_TTaggregateTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Component_8_TTaggregateTevent])
    Component_8_TWrapperTeventInterface = typing.TypeVar('Component_8_TWrapperTeventInterface')
    Component_8_TWrapperTeventImplementation = typing.TypeVar('Component_8_TWrapperTeventImplementation')
    Component_8_TComponent = typing.TypeVar('Component_8_TComponent')
    Component_8_TComponentTevent = typing.TypeVar('Component_8_TComponentTevent')
    Component_8_TComponentTeventImplementation = typing.TypeVar('Component_8_TComponentTeventImplementation', bound=Union[Component_8_TTaggregateTeventImplementation, Component_8_TComponentTevent])
    class Component_8(typing.Generic[Component_8_TTaggregate, Component_8_TTaggregateTevent, Component_8_TTaggregateTeventImplementation, Component_8_TWrapperTeventInterface, Component_8_TWrapperTeventImplementation, Component_8_TComponent, Component_8_TComponentTevent, Component_8_TComponentTeventImplementation], TeventiveComponent_6[Component_8_TTaggregate, Component_8_TTaggregateTevent, Component_8_TTaggregateTeventImplementation, Component_8_TComponent, Component_8_TComponentTevent, Component_8_TComponentTeventImplementation]):
        Component_8_TTaggregate = Taggregate_5.Component_8_TTaggregate
        Component_8_TTaggregateTevent = Taggregate_5.Component_8_TTaggregateTevent
        Component_8_TTaggregateTeventImplementation = Taggregate_5.Component_8_TTaggregateTeventImplementation
        Component_8_TWrapperTeventInterface = Taggregate_5.Component_8_TWrapperTeventInterface
        Component_8_TWrapperTeventImplementation = Taggregate_5.Component_8_TWrapperTeventImplementation
        Component_8_TComponent = Taggregate_5.Component_8_TComponent
        Component_8_TComponentTevent = Taggregate_5.Component_8_TComponentTevent
        Component_8_TComponentTeventImplementation = Taggregate_5.Component_8_TComponentTeventImplementation
        pass


    Entity_GenericClasses_Taggregate_5_TTaggregate = typing.TypeVar('Entity_GenericClasses_Taggregate_5_TTaggregate')
    Entity_GenericClasses_Taggregate_5_TTaggregateTevent = typing.TypeVar('Entity_GenericClasses_Taggregate_5_TTaggregateTevent')
    Entity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation = typing.TypeVar('Entity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Entity_GenericClasses_Taggregate_5_TTaggregateTevent])
    Entity_GenericClasses_Taggregate_5_TWrapperTeventInterface = typing.TypeVar('Entity_GenericClasses_Taggregate_5_TWrapperTeventInterface')
    Entity_GenericClasses_Taggregate_5_TWrapperTeventImplementation = typing.TypeVar('Entity_GenericClasses_Taggregate_5_TWrapperTeventImplementation')
    class Entity_GenericClasses(typing.Generic[Entity_GenericClasses_Taggregate_5_TTaggregate, Entity_GenericClasses_Taggregate_5_TTaggregateTevent, Entity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation, Entity_GenericClasses_Taggregate_5_TWrapperTeventInterface, Entity_GenericClasses_Taggregate_5_TWrapperTeventImplementation], abc.ABCMeta):
        Entity_GenericClasses_Taggregate_5_TTaggregate = Taggregate_5.Entity_GenericClasses_Taggregate_5_TTaggregate
        Entity_GenericClasses_Taggregate_5_TTaggregateTevent = Taggregate_5.Entity_GenericClasses_Taggregate_5_TTaggregateTevent
        Entity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation = Taggregate_5.Entity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation
        Entity_GenericClasses_Taggregate_5_TWrapperTeventInterface = Taggregate_5.Entity_GenericClasses_Taggregate_5_TWrapperTeventInterface
        Entity_GenericClasses_Taggregate_5_TWrapperTeventImplementation = Taggregate_5.Entity_GenericClasses_Taggregate_5_TWrapperTeventImplementation
        Generic_Entity_GenericClasses_Entity_11_TEntity = typing.TypeVar('Generic_Entity_GenericClasses_Entity_11_TEntity')
        Generic_Entity_GenericClasses_Entity_11_TEntityId = typing.TypeVar('Generic_Entity_GenericClasses_Entity_11_TEntityId')
        Generic_Entity_GenericClasses_Entity_11_TEntityTevent = typing.TypeVar('Generic_Entity_GenericClasses_Entity_11_TEntityTevent')
        Generic_Entity_GenericClasses_Entity_11_TEntityTeventImplementation = typing.TypeVar('Generic_Entity_GenericClasses_Entity_11_TEntityTeventImplementation', bound=Union[Entity_GenericClasses_Entity_11_TTaggregateTeventImplementation, Entity_GenericClasses_Entity_11_TEntityTevent])
        Generic_Entity_GenericClasses_Entity_11_TEntityCreatedTevent = typing.TypeVar('Generic_Entity_GenericClasses_Entity_11_TEntityCreatedTevent')
        Generic_Entity_GenericClasses_Entity_11_TEntityTeventIdGetterSetter = typing.TypeVar('Generic_Entity_GenericClasses_Entity_11_TEntityTeventIdGetterSetter')
        def __getitem__(self, types : typing.Tuple[typing.Type[Generic_Entity_GenericClasses_Entity_11_TEntity], typing.Type[Generic_Entity_GenericClasses_Entity_11_TEntityId], typing.Type[Generic_Entity_GenericClasses_Entity_11_TEntityTevent], typing.Type[Generic_Entity_GenericClasses_Entity_11_TEntityTeventImplementation], typing.Type[Generic_Entity_GenericClasses_Entity_11_TEntityCreatedTevent], typing.Type[Generic_Entity_GenericClasses_Entity_11_TEntityTeventIdGetterSetter]]) -> typing.Type[Taggregate_5.Entity_11[Entity_GenericClasses_Taggregate_5_TTaggregate, Entity_GenericClasses_Taggregate_5_TTaggregateTevent, Entity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation, Entity_GenericClasses_Taggregate_5_TWrapperTeventInterface, Entity_GenericClasses_Taggregate_5_TWrapperTeventImplementation, Generic_Entity_GenericClasses_Entity_11_TEntity, Generic_Entity_GenericClasses_Entity_11_TEntityId, Generic_Entity_GenericClasses_Entity_11_TEntityTevent, Generic_Entity_GenericClasses_Entity_11_TEntityTeventImplementation, Generic_Entity_GenericClasses_Entity_11_TEntityCreatedTevent, Generic_Entity_GenericClasses_Entity_11_TEntityTeventIdGetterSetter]]: ...

    Entity : Entity_GenericClasses[Taggregate_5_TTaggregate, Taggregate_5_TTaggregateTevent, Taggregate_5_TTaggregateTeventImplementation, Taggregate_5_TWrapperTeventInterface, Taggregate_5_TWrapperTeventImplementation]

    Entity_11_TTaggregate = typing.TypeVar('Entity_11_TTaggregate')
    Entity_11_TTaggregateTevent = typing.TypeVar('Entity_11_TTaggregateTevent')
    Entity_11_TTaggregateTeventImplementation = typing.TypeVar('Entity_11_TTaggregateTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Entity_11_TTaggregateTevent])
    Entity_11_TWrapperTeventInterface = typing.TypeVar('Entity_11_TWrapperTeventInterface')
    Entity_11_TWrapperTeventImplementation = typing.TypeVar('Entity_11_TWrapperTeventImplementation')
    Entity_11_TEntity = typing.TypeVar('Entity_11_TEntity')
    Entity_11_TEntityId = typing.TypeVar('Entity_11_TEntityId')
    Entity_11_TEntityTevent = typing.TypeVar('Entity_11_TEntityTevent')
    Entity_11_TEntityTeventImplementation = typing.TypeVar('Entity_11_TEntityTeventImplementation', bound=Union[Entity_11_TTaggregateTeventImplementation, Entity_11_TEntityTevent])
    Entity_11_TEntityCreatedTevent = typing.TypeVar('Entity_11_TEntityCreatedTevent')
    Entity_11_TEntityTeventIdGetterSetter = typing.TypeVar('Entity_11_TEntityTeventIdGetterSetter')
    class Entity_11(typing.Generic[Entity_11_TTaggregate, Entity_11_TTaggregateTevent, Entity_11_TTaggregateTeventImplementation, Entity_11_TWrapperTeventInterface, Entity_11_TWrapperTeventImplementation, Entity_11_TEntity, Entity_11_TEntityId, Entity_11_TEntityTevent, Entity_11_TEntityTeventImplementation, Entity_11_TEntityCreatedTevent, Entity_11_TEntityTeventIdGetterSetter], Tentity_9[Entity_11_TTaggregate, Entity_11_TTaggregateTevent, Entity_11_TTaggregateTeventImplementation, Entity_11_TEntity, Entity_11_TEntityId, Entity_11_TEntityTeventImplementation, Entity_11_TEntityTevent, Entity_11_TEntityCreatedTevent, Entity_11_TEntityTeventIdGetterSetter]):
        Entity_11_TTaggregate = Taggregate_5.Entity_11_TTaggregate
        Entity_11_TTaggregateTevent = Taggregate_5.Entity_11_TTaggregateTevent
        Entity_11_TTaggregateTeventImplementation = Taggregate_5.Entity_11_TTaggregateTeventImplementation
        Entity_11_TWrapperTeventInterface = Taggregate_5.Entity_11_TWrapperTeventInterface
        Entity_11_TWrapperTeventImplementation = Taggregate_5.Entity_11_TWrapperTeventImplementation
        Entity_11_TEntity = Taggregate_5.Entity_11_TEntity
        Entity_11_TEntityId = Taggregate_5.Entity_11_TEntityId
        Entity_11_TEntityTevent = Taggregate_5.Entity_11_TEntityTevent
        Entity_11_TEntityTeventImplementation = Taggregate_5.Entity_11_TEntityTeventImplementation
        Entity_11_TEntityCreatedTevent = Taggregate_5.Entity_11_TEntityCreatedTevent
        Entity_11_TEntityTeventIdGetterSetter = Taggregate_5.Entity_11_TEntityTeventIdGetterSetter
        @property
        def Id(self) -> Entity_11_TEntityId: ...


    RemovableEntity_GenericClasses_Taggregate_5_TTaggregate = typing.TypeVar('RemovableEntity_GenericClasses_Taggregate_5_TTaggregate')
    RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTevent = typing.TypeVar('RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTevent')
    RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation = typing.TypeVar('RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTevent])
    RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventInterface = typing.TypeVar('RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventInterface')
    RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventImplementation = typing.TypeVar('RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventImplementation')
    class RemovableEntity_GenericClasses(typing.Generic[RemovableEntity_GenericClasses_Taggregate_5_TTaggregate, RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTevent, RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation, RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventInterface, RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventImplementation], abc.ABCMeta):
        RemovableEntity_GenericClasses_Taggregate_5_TTaggregate = Taggregate_5.RemovableEntity_GenericClasses_Taggregate_5_TTaggregate
        RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTevent = Taggregate_5.RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTevent
        RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation = Taggregate_5.RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation
        RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventInterface = Taggregate_5.RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventInterface
        RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventImplementation = Taggregate_5.RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventImplementation
        Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntity = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntity')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityId = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityId')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTevent = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTevent')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTeventImplementation = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTeventImplementation', bound=Union[RemovableEntity_GenericClasses_RemovableEntity_12_TTaggregateTeventImplementation, RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTevent])
        Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityCreatedTevent = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityCreatedTevent')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityRemovedTevent = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityRemovedTevent')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTeventIdGetterSetter = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTeventIdGetterSetter')
        def __getitem__(self, types : typing.Tuple[typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntity], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityId], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTevent], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTeventImplementation], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityCreatedTevent], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityRemovedTevent], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTeventIdGetterSetter]]) -> typing.Type[Taggregate_5.RemovableEntity_12[RemovableEntity_GenericClasses_Taggregate_5_TTaggregate, RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTevent, RemovableEntity_GenericClasses_Taggregate_5_TTaggregateTeventImplementation, RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventInterface, RemovableEntity_GenericClasses_Taggregate_5_TWrapperTeventImplementation, Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntity, Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityId, Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTevent, Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTeventImplementation, Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityCreatedTevent, Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityRemovedTevent, Generic_RemovableEntity_GenericClasses_RemovableEntity_12_TEntityTeventIdGetterSetter]]: ...

    RemovableEntity : RemovableEntity_GenericClasses[Taggregate_5_TTaggregate, Taggregate_5_TTaggregateTevent, Taggregate_5_TTaggregateTeventImplementation, Taggregate_5_TWrapperTeventInterface, Taggregate_5_TWrapperTeventImplementation]

    RemovableEntity_12_TTaggregate = typing.TypeVar('RemovableEntity_12_TTaggregate')
    RemovableEntity_12_TTaggregateTevent = typing.TypeVar('RemovableEntity_12_TTaggregateTevent')
    RemovableEntity_12_TTaggregateTeventImplementation = typing.TypeVar('RemovableEntity_12_TTaggregateTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, RemovableEntity_12_TTaggregateTevent])
    RemovableEntity_12_TWrapperTeventInterface = typing.TypeVar('RemovableEntity_12_TWrapperTeventInterface')
    RemovableEntity_12_TWrapperTeventImplementation = typing.TypeVar('RemovableEntity_12_TWrapperTeventImplementation')
    RemovableEntity_12_TEntity = typing.TypeVar('RemovableEntity_12_TEntity')
    RemovableEntity_12_TEntityId = typing.TypeVar('RemovableEntity_12_TEntityId')
    RemovableEntity_12_TEntityTevent = typing.TypeVar('RemovableEntity_12_TEntityTevent')
    RemovableEntity_12_TEntityTeventImplementation = typing.TypeVar('RemovableEntity_12_TEntityTeventImplementation', bound=Union[RemovableEntity_12_TTaggregateTeventImplementation, RemovableEntity_12_TEntityTevent])
    RemovableEntity_12_TEntityCreatedTevent = typing.TypeVar('RemovableEntity_12_TEntityCreatedTevent')
    RemovableEntity_12_TEntityRemovedTevent = typing.TypeVar('RemovableEntity_12_TEntityRemovedTevent')
    RemovableEntity_12_TEntityTeventIdGetterSetter = typing.TypeVar('RemovableEntity_12_TEntityTeventIdGetterSetter')
    class RemovableEntity_12(typing.Generic[RemovableEntity_12_TTaggregate, RemovableEntity_12_TTaggregateTevent, RemovableEntity_12_TTaggregateTeventImplementation, RemovableEntity_12_TWrapperTeventInterface, RemovableEntity_12_TWrapperTeventImplementation, RemovableEntity_12_TEntity, RemovableEntity_12_TEntityId, RemovableEntity_12_TEntityTevent, RemovableEntity_12_TEntityTeventImplementation, RemovableEntity_12_TEntityCreatedTevent, RemovableEntity_12_TEntityRemovedTevent, RemovableEntity_12_TEntityTeventIdGetterSetter], TeventiveRemovableEntity_10[RemovableEntity_12_TTaggregate, RemovableEntity_12_TTaggregateTevent, RemovableEntity_12_TTaggregateTeventImplementation, RemovableEntity_12_TEntity, RemovableEntity_12_TEntityId, RemovableEntity_12_TEntityTevent, RemovableEntity_12_TEntityTeventImplementation, RemovableEntity_12_TEntityCreatedTevent, RemovableEntity_12_TEntityRemovedTevent, RemovableEntity_12_TEntityTeventIdGetterSetter]):
        RemovableEntity_12_TTaggregate = Taggregate_5.RemovableEntity_12_TTaggregate
        RemovableEntity_12_TTaggregateTevent = Taggregate_5.RemovableEntity_12_TTaggregateTevent
        RemovableEntity_12_TTaggregateTeventImplementation = Taggregate_5.RemovableEntity_12_TTaggregateTeventImplementation
        RemovableEntity_12_TWrapperTeventInterface = Taggregate_5.RemovableEntity_12_TWrapperTeventInterface
        RemovableEntity_12_TWrapperTeventImplementation = Taggregate_5.RemovableEntity_12_TWrapperTeventImplementation
        RemovableEntity_12_TEntity = Taggregate_5.RemovableEntity_12_TEntity
        RemovableEntity_12_TEntityId = Taggregate_5.RemovableEntity_12_TEntityId
        RemovableEntity_12_TEntityTevent = Taggregate_5.RemovableEntity_12_TEntityTevent
        RemovableEntity_12_TEntityTeventImplementation = Taggregate_5.RemovableEntity_12_TEntityTeventImplementation
        RemovableEntity_12_TEntityCreatedTevent = Taggregate_5.RemovableEntity_12_TEntityCreatedTevent
        RemovableEntity_12_TEntityRemovedTevent = Taggregate_5.RemovableEntity_12_TEntityRemovedTevent
        RemovableEntity_12_TEntityTeventIdGetterSetter = Taggregate_5.RemovableEntity_12_TEntityTeventIdGetterSetter
        @property
        def Id(self) -> RemovableEntity_12_TEntityId: ...



class TaggregateTevent_GenericClasses(abc.ABCMeta):
    Generic_TaggregateTevent_GenericClasses_TaggregateTevent_1_TBaseTeventInterface = typing.TypeVar('Generic_TaggregateTevent_GenericClasses_TaggregateTevent_1_TBaseTeventInterface')
    def __getitem__(self, types : typing.Type[Generic_TaggregateTevent_GenericClasses_TaggregateTevent_1_TBaseTeventInterface]) -> typing.Type[TaggregateTevent_1[Generic_TaggregateTevent_GenericClasses_TaggregateTevent_1_TBaseTeventInterface]]: ...

TaggregateTevent : TaggregateTevent_GenericClasses

TaggregateTevent_1_TBaseTeventInterface = typing.TypeVar('TaggregateTevent_1_TBaseTeventInterface')
class TaggregateTevent_1(typing.Generic[TaggregateTevent_1_TBaseTeventInterface], PublisherTevent_1[TaggregateTevent_1_TBaseTeventInterface], ITaggregateTevent_1[TaggregateTevent_1_TBaseTeventInterface]):
    def __init__(self, tevent: TaggregateTevent_1_TBaseTeventInterface) -> None: ...
    @property
    def Tevent(self) -> TaggregateTevent_1_TBaseTeventInterface: ...


class Tentity_GenericClasses(abc.ABCMeta):
    Generic_Tentity_GenericClasses_Tentity_9_TParent = typing.TypeVar('Generic_Tentity_GenericClasses_Tentity_9_TParent')
    Generic_Tentity_GenericClasses_Tentity_9_TParentTevent = typing.TypeVar('Generic_Tentity_GenericClasses_Tentity_9_TParentTevent')
    Generic_Tentity_GenericClasses_Tentity_9_TParentTeventImplementation = typing.TypeVar('Generic_Tentity_GenericClasses_Tentity_9_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Tentity_GenericClasses_Tentity_9_TParentTevent])
    Generic_Tentity_GenericClasses_Tentity_9_TTentity = typing.TypeVar('Generic_Tentity_GenericClasses_Tentity_9_TTentity')
    Generic_Tentity_GenericClasses_Tentity_9_TTentityId = typing.TypeVar('Generic_Tentity_GenericClasses_Tentity_9_TTentityId')
    Generic_Tentity_GenericClasses_Tentity_9_TTentityTeventImplementation = typing.TypeVar('Generic_Tentity_GenericClasses_Tentity_9_TTentityTeventImplementation', bound=Union[Tentity_GenericClasses_Tentity_9_TParentTeventImplementation, Tentity_GenericClasses_Tentity_9_TTentityTevent])
    Generic_Tentity_GenericClasses_Tentity_9_TTentityTevent = typing.TypeVar('Generic_Tentity_GenericClasses_Tentity_9_TTentityTevent')
    Generic_Tentity_GenericClasses_Tentity_9_TTentityCreatedTevent = typing.TypeVar('Generic_Tentity_GenericClasses_Tentity_9_TTentityCreatedTevent')
    Generic_Tentity_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter = typing.TypeVar('Generic_Tentity_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_Tentity_GenericClasses_Tentity_9_TParent], typing.Type[Generic_Tentity_GenericClasses_Tentity_9_TParentTevent], typing.Type[Generic_Tentity_GenericClasses_Tentity_9_TParentTeventImplementation], typing.Type[Generic_Tentity_GenericClasses_Tentity_9_TTentity], typing.Type[Generic_Tentity_GenericClasses_Tentity_9_TTentityId], typing.Type[Generic_Tentity_GenericClasses_Tentity_9_TTentityTeventImplementation], typing.Type[Generic_Tentity_GenericClasses_Tentity_9_TTentityTevent], typing.Type[Generic_Tentity_GenericClasses_Tentity_9_TTentityCreatedTevent], typing.Type[Generic_Tentity_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter]]) -> typing.Type[Tentity_9[Generic_Tentity_GenericClasses_Tentity_9_TParent, Generic_Tentity_GenericClasses_Tentity_9_TParentTevent, Generic_Tentity_GenericClasses_Tentity_9_TParentTeventImplementation, Generic_Tentity_GenericClasses_Tentity_9_TTentity, Generic_Tentity_GenericClasses_Tentity_9_TTentityId, Generic_Tentity_GenericClasses_Tentity_9_TTentityTeventImplementation, Generic_Tentity_GenericClasses_Tentity_9_TTentityTevent, Generic_Tentity_GenericClasses_Tentity_9_TTentityCreatedTevent, Generic_Tentity_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter]]: ...

Tentity : Tentity_GenericClasses

Tentity_9_TParent = typing.TypeVar('Tentity_9_TParent')
Tentity_9_TParentTevent = typing.TypeVar('Tentity_9_TParentTevent')
Tentity_9_TParentTeventImplementation = typing.TypeVar('Tentity_9_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Tentity_9_TParentTevent])
Tentity_9_TTentity = typing.TypeVar('Tentity_9_TTentity')
Tentity_9_TTentityId = typing.TypeVar('Tentity_9_TTentityId')
Tentity_9_TTentityTeventImplementation = typing.TypeVar('Tentity_9_TTentityTeventImplementation', bound=Union[Tentity_9_TParentTeventImplementation, Tentity_9_TTentityTevent])
Tentity_9_TTentityTevent = typing.TypeVar('Tentity_9_TTentityTevent')
Tentity_9_TTentityCreatedTevent = typing.TypeVar('Tentity_9_TTentityCreatedTevent')
Tentity_9_TTentityTeventIdGetterSetter = typing.TypeVar('Tentity_9_TTentityTeventIdGetterSetter')
class Tentity_9(typing.Generic[Tentity_9_TParent, Tentity_9_TParentTevent, Tentity_9_TParentTeventImplementation, Tentity_9_TTentity, Tentity_9_TTentityId, Tentity_9_TTentityTeventImplementation, Tentity_9_TTentityTevent, Tentity_9_TTentityCreatedTevent, Tentity_9_TTentityTeventIdGetterSetter], TeventiveComponent_6[Tentity_9_TParent, Tentity_9_TParentTevent, Tentity_9_TParentTeventImplementation, Tentity_9_TTentity, Tentity_9_TTentityTevent, Tentity_9_TTentityTeventImplementation]):
    @property
    def Id(self) -> Tentity_9_TTentityId: ...
    @staticmethod
    def CreateSelfManagingCollection(parent: Tentity_9_TParent) -> Tentity_9.ICollectionManager_9[Tentity_9_TParent, Tentity_9_TParentTevent, Tentity_9_TParentTeventImplementation, Tentity_9_TTentity, Tentity_9_TTentityId, Tentity_9_TTentityTeventImplementation, Tentity_9_TTentityTevent, Tentity_9_TTentityCreatedTevent, Tentity_9_TTentityTeventIdGetterSetter]: ...

    ICollectionManager_GenericClasses_Tentity_9_TParent = typing.TypeVar('ICollectionManager_GenericClasses_Tentity_9_TParent')
    ICollectionManager_GenericClasses_Tentity_9_TParentTevent = typing.TypeVar('ICollectionManager_GenericClasses_Tentity_9_TParentTevent')
    ICollectionManager_GenericClasses_Tentity_9_TParentTeventImplementation = typing.TypeVar('ICollectionManager_GenericClasses_Tentity_9_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, ICollectionManager_GenericClasses_Tentity_9_TParentTevent])
    ICollectionManager_GenericClasses_Tentity_9_TTentity = typing.TypeVar('ICollectionManager_GenericClasses_Tentity_9_TTentity')
    ICollectionManager_GenericClasses_Tentity_9_TTentityId = typing.TypeVar('ICollectionManager_GenericClasses_Tentity_9_TTentityId')
    ICollectionManager_GenericClasses_Tentity_9_TTentityTeventImplementation = typing.TypeVar('ICollectionManager_GenericClasses_Tentity_9_TTentityTeventImplementation', bound=Union[ICollectionManager_GenericClasses_Tentity_9_TParentTeventImplementation, ICollectionManager_GenericClasses_Tentity_9_TTentityTevent])
    ICollectionManager_GenericClasses_Tentity_9_TTentityTevent = typing.TypeVar('ICollectionManager_GenericClasses_Tentity_9_TTentityTevent')
    ICollectionManager_GenericClasses_Tentity_9_TTentityCreatedTevent = typing.TypeVar('ICollectionManager_GenericClasses_Tentity_9_TTentityCreatedTevent')
    ICollectionManager_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter = typing.TypeVar('ICollectionManager_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter')
    class ICollectionManager_GenericClasses(typing.Generic[ICollectionManager_GenericClasses_Tentity_9_TParent, ICollectionManager_GenericClasses_Tentity_9_TParentTevent, ICollectionManager_GenericClasses_Tentity_9_TParentTeventImplementation, ICollectionManager_GenericClasses_Tentity_9_TTentity, ICollectionManager_GenericClasses_Tentity_9_TTentityId, ICollectionManager_GenericClasses_Tentity_9_TTentityTeventImplementation, ICollectionManager_GenericClasses_Tentity_9_TTentityTevent, ICollectionManager_GenericClasses_Tentity_9_TTentityCreatedTevent, ICollectionManager_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter], abc.ABCMeta):
        ICollectionManager_GenericClasses_Tentity_9_TParent = Tentity_9.ICollectionManager_GenericClasses_Tentity_9_TParent
        ICollectionManager_GenericClasses_Tentity_9_TParentTevent = Tentity_9.ICollectionManager_GenericClasses_Tentity_9_TParentTevent
        ICollectionManager_GenericClasses_Tentity_9_TParentTeventImplementation = Tentity_9.ICollectionManager_GenericClasses_Tentity_9_TParentTeventImplementation
        ICollectionManager_GenericClasses_Tentity_9_TTentity = Tentity_9.ICollectionManager_GenericClasses_Tentity_9_TTentity
        ICollectionManager_GenericClasses_Tentity_9_TTentityId = Tentity_9.ICollectionManager_GenericClasses_Tentity_9_TTentityId
        ICollectionManager_GenericClasses_Tentity_9_TTentityTeventImplementation = Tentity_9.ICollectionManager_GenericClasses_Tentity_9_TTentityTeventImplementation
        ICollectionManager_GenericClasses_Tentity_9_TTentityTevent = Tentity_9.ICollectionManager_GenericClasses_Tentity_9_TTentityTevent
        ICollectionManager_GenericClasses_Tentity_9_TTentityCreatedTevent = Tentity_9.ICollectionManager_GenericClasses_Tentity_9_TTentityCreatedTevent
        ICollectionManager_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter = Tentity_9.ICollectionManager_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter
        def __call__(self) -> Tentity_9.ICollectionManager_9[ICollectionManager_GenericClasses_Tentity_9_TParent, ICollectionManager_GenericClasses_Tentity_9_TParentTevent, ICollectionManager_GenericClasses_Tentity_9_TParentTeventImplementation, ICollectionManager_GenericClasses_Tentity_9_TTentity, ICollectionManager_GenericClasses_Tentity_9_TTentityId, ICollectionManager_GenericClasses_Tentity_9_TTentityTeventImplementation, ICollectionManager_GenericClasses_Tentity_9_TTentityTevent, ICollectionManager_GenericClasses_Tentity_9_TTentityCreatedTevent, ICollectionManager_GenericClasses_Tentity_9_TTentityTeventIdGetterSetter]: ...

    ICollectionManager : ICollectionManager_GenericClasses[Tentity_9_TParent, Tentity_9_TParentTevent, Tentity_9_TParentTeventImplementation, Tentity_9_TTentity, Tentity_9_TTentityId, Tentity_9_TTentityTeventImplementation, Tentity_9_TTentityTevent, Tentity_9_TTentityCreatedTevent, Tentity_9_TTentityTeventIdGetterSetter]

    ICollectionManager_9_TParent = typing.TypeVar('ICollectionManager_9_TParent')
    ICollectionManager_9_TParentTevent = typing.TypeVar('ICollectionManager_9_TParentTevent')
    ICollectionManager_9_TParentTeventImplementation = typing.TypeVar('ICollectionManager_9_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, ICollectionManager_9_TParentTevent])
    ICollectionManager_9_TTentity = typing.TypeVar('ICollectionManager_9_TTentity')
    ICollectionManager_9_TTentityId = typing.TypeVar('ICollectionManager_9_TTentityId')
    ICollectionManager_9_TTentityTeventImplementation = typing.TypeVar('ICollectionManager_9_TTentityTeventImplementation', bound=Union[ICollectionManager_9_TParentTeventImplementation, ICollectionManager_9_TTentityTevent])
    ICollectionManager_9_TTentityTevent = typing.TypeVar('ICollectionManager_9_TTentityTevent')
    ICollectionManager_9_TTentityCreatedTevent = typing.TypeVar('ICollectionManager_9_TTentityCreatedTevent')
    ICollectionManager_9_TTentityTeventIdGetterSetter = typing.TypeVar('ICollectionManager_9_TTentityTeventIdGetterSetter')
    class ICollectionManager_9(typing.Generic[ICollectionManager_9_TParent, ICollectionManager_9_TParentTevent, ICollectionManager_9_TParentTeventImplementation, ICollectionManager_9_TTentity, ICollectionManager_9_TTentityId, ICollectionManager_9_TTentityTeventImplementation, ICollectionManager_9_TTentityTevent, ICollectionManager_9_TTentityCreatedTevent, ICollectionManager_9_TTentityTeventIdGetterSetter], ITeventRoutedMember_1[ICollectionManager_9_TTentityTevent], IEntityCollectionManager_5[ICollectionManager_9_TTentity, ICollectionManager_9_TTentityId, ICollectionManager_9_TTentityTevent, ICollectionManager_9_TTentityTeventImplementation, ICollectionManager_9_TTentityCreatedTevent], typing.Protocol):
        ICollectionManager_9_TParent = Tentity_9.ICollectionManager_9_TParent
        ICollectionManager_9_TParentTevent = Tentity_9.ICollectionManager_9_TParentTevent
        ICollectionManager_9_TParentTeventImplementation = Tentity_9.ICollectionManager_9_TParentTeventImplementation
        ICollectionManager_9_TTentity = Tentity_9.ICollectionManager_9_TTentity
        ICollectionManager_9_TTentityId = Tentity_9.ICollectionManager_9_TTentityId
        ICollectionManager_9_TTentityTeventImplementation = Tentity_9.ICollectionManager_9_TTentityTeventImplementation
        ICollectionManager_9_TTentityTevent = Tentity_9.ICollectionManager_9_TTentityTevent
        ICollectionManager_9_TTentityCreatedTevent = Tentity_9.ICollectionManager_9_TTentityCreatedTevent
        ICollectionManager_9_TTentityTeventIdGetterSetter = Tentity_9.ICollectionManager_9_TTentityTeventIdGetterSetter
        pass



class TeventApplyingComponent_GenericClasses(abc.ABCMeta):
    Generic_TeventApplyingComponent_GenericClasses_TeventApplyingComponent_2_TComponent = typing.TypeVar('Generic_TeventApplyingComponent_GenericClasses_TeventApplyingComponent_2_TComponent')
    Generic_TeventApplyingComponent_GenericClasses_TeventApplyingComponent_2_TComponentTevent = typing.TypeVar('Generic_TeventApplyingComponent_GenericClasses_TeventApplyingComponent_2_TComponentTevent')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_TeventApplyingComponent_GenericClasses_TeventApplyingComponent_2_TComponent], typing.Type[Generic_TeventApplyingComponent_GenericClasses_TeventApplyingComponent_2_TComponentTevent]]) -> typing.Type[TeventApplyingComponent_2[Generic_TeventApplyingComponent_GenericClasses_TeventApplyingComponent_2_TComponent, Generic_TeventApplyingComponent_GenericClasses_TeventApplyingComponent_2_TComponentTevent]]: ...

TeventApplyingComponent : TeventApplyingComponent_GenericClasses

TeventApplyingComponent_2_TComponent = typing.TypeVar('TeventApplyingComponent_2_TComponent')
TeventApplyingComponent_2_TComponentTevent = typing.TypeVar('TeventApplyingComponent_2_TComponentTevent')
class TeventApplyingComponent_2(typing.Generic[TeventApplyingComponent_2_TComponent, TeventApplyingComponent_2_TComponentTevent], ITeventRouted, ITeventRoutedMember_1[TeventApplyingComponent_2_TComponentTevent], abc.ABC):
    pass


class TeventApplyingRoot_GenericClasses(abc.ABCMeta):
    Generic_TeventApplyingRoot_GenericClasses_TeventApplyingRoot_2_TRoot = typing.TypeVar('Generic_TeventApplyingRoot_GenericClasses_TeventApplyingRoot_2_TRoot')
    Generic_TeventApplyingRoot_GenericClasses_TeventApplyingRoot_2_TTevent = typing.TypeVar('Generic_TeventApplyingRoot_GenericClasses_TeventApplyingRoot_2_TTevent')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_TeventApplyingRoot_GenericClasses_TeventApplyingRoot_2_TRoot], typing.Type[Generic_TeventApplyingRoot_GenericClasses_TeventApplyingRoot_2_TTevent]]) -> typing.Type[TeventApplyingRoot_2[Generic_TeventApplyingRoot_GenericClasses_TeventApplyingRoot_2_TRoot, Generic_TeventApplyingRoot_GenericClasses_TeventApplyingRoot_2_TTevent]]: ...

TeventApplyingRoot : TeventApplyingRoot_GenericClasses

TeventApplyingRoot_2_TRoot = typing.TypeVar('TeventApplyingRoot_2_TRoot')
TeventApplyingRoot_2_TTevent = typing.TypeVar('TeventApplyingRoot_2_TTevent')
class TeventApplyingRoot_2(typing.Generic[TeventApplyingRoot_2_TRoot, TeventApplyingRoot_2_TTevent], VersionedEntity_1[TeventApplyingRoot_2_TRoot], ITeventRouted, abc.ABC):
    @property
    def Id(self) -> EntityId: ...
    @Id.setter
    def Id(self, value: EntityId) -> EntityId: ...
    @property
    def Id(self) -> EntityId_1[Guid]: ...
    @Id.setter
    def Id(self, value: EntityId_1[Guid]) -> EntityId_1[Guid]: ...
    @property
    def Version(self) -> int: ...
    @Version.setter
    def Version(self, value: int) -> int: ...


class TeventiveComponent_GenericClasses(abc.ABCMeta):
    Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParent = typing.TypeVar('Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParent')
    Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTevent = typing.TypeVar('Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTevent')
    Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTeventImplementation = typing.TypeVar('Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTevent])
    Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponent = typing.TypeVar('Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponent')
    Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponentTevent = typing.TypeVar('Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponentTevent')
    Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation = typing.TypeVar('Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation', bound=Union[TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponentTevent])
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParent], typing.Type[Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTevent], typing.Type[Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTeventImplementation], typing.Type[Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponent], typing.Type[Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponentTevent], typing.Type[Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation]]) -> typing.Type[TeventiveComponent_6[Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParent, Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTevent, Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponent, Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponentTevent, Generic_TeventiveComponent_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation]]: ...

TeventiveComponent : TeventiveComponent_GenericClasses

TeventiveComponent_6_TParent = typing.TypeVar('TeventiveComponent_6_TParent')
TeventiveComponent_6_TParentTevent = typing.TypeVar('TeventiveComponent_6_TParentTevent')
TeventiveComponent_6_TParentTeventImplementation = typing.TypeVar('TeventiveComponent_6_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, TeventiveComponent_6_TParentTevent])
TeventiveComponent_6_TComponent = typing.TypeVar('TeventiveComponent_6_TComponent')
TeventiveComponent_6_TComponentTevent = typing.TypeVar('TeventiveComponent_6_TComponentTevent')
TeventiveComponent_6_TComponentTeventImplementation = typing.TypeVar('TeventiveComponent_6_TComponentTeventImplementation', bound=Union[TeventiveComponent_6_TParentTeventImplementation, TeventiveComponent_6_TComponentTevent])
class TeventiveComponent_6(typing.Generic[TeventiveComponent_6_TParent, TeventiveComponent_6_TParentTevent, TeventiveComponent_6_TParentTeventImplementation, TeventiveComponent_6_TComponent, TeventiveComponent_6_TComponentTevent, TeventiveComponent_6_TComponentTeventImplementation], TeventApplyingComponent_2[TeventiveComponent_6_TComponent, TeventiveComponent_6_TComponentTevent], ITeventiveInternals_2[TeventiveComponent_6_TComponentTevent, TeventiveComponent_6_TComponentTeventImplementation]):

    Component_GenericClasses_TeventiveComponent_6_TParent = typing.TypeVar('Component_GenericClasses_TeventiveComponent_6_TParent')
    Component_GenericClasses_TeventiveComponent_6_TParentTevent = typing.TypeVar('Component_GenericClasses_TeventiveComponent_6_TParentTevent')
    Component_GenericClasses_TeventiveComponent_6_TParentTeventImplementation = typing.TypeVar('Component_GenericClasses_TeventiveComponent_6_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Component_GenericClasses_TeventiveComponent_6_TParentTevent])
    Component_GenericClasses_TeventiveComponent_6_TComponent = typing.TypeVar('Component_GenericClasses_TeventiveComponent_6_TComponent')
    Component_GenericClasses_TeventiveComponent_6_TComponentTevent = typing.TypeVar('Component_GenericClasses_TeventiveComponent_6_TComponentTevent')
    Component_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation = typing.TypeVar('Component_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation', bound=Union[Component_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, Component_GenericClasses_TeventiveComponent_6_TComponentTevent])
    class Component_GenericClasses(typing.Generic[Component_GenericClasses_TeventiveComponent_6_TParent, Component_GenericClasses_TeventiveComponent_6_TParentTevent, Component_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, Component_GenericClasses_TeventiveComponent_6_TComponent, Component_GenericClasses_TeventiveComponent_6_TComponentTevent, Component_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation], abc.ABCMeta):
        Component_GenericClasses_TeventiveComponent_6_TParent = TeventiveComponent_6.Component_GenericClasses_TeventiveComponent_6_TParent
        Component_GenericClasses_TeventiveComponent_6_TParentTevent = TeventiveComponent_6.Component_GenericClasses_TeventiveComponent_6_TParentTevent
        Component_GenericClasses_TeventiveComponent_6_TParentTeventImplementation = TeventiveComponent_6.Component_GenericClasses_TeventiveComponent_6_TParentTeventImplementation
        Component_GenericClasses_TeventiveComponent_6_TComponent = TeventiveComponent_6.Component_GenericClasses_TeventiveComponent_6_TComponent
        Component_GenericClasses_TeventiveComponent_6_TComponentTevent = TeventiveComponent_6.Component_GenericClasses_TeventiveComponent_6_TComponentTevent
        Component_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation = TeventiveComponent_6.Component_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation
        Generic_Component_GenericClasses_Component_9_TEcComponent = typing.TypeVar('Generic_Component_GenericClasses_Component_9_TEcComponent')
        Generic_Component_GenericClasses_Component_9_TEcComponentTeventImplementation = typing.TypeVar('Generic_Component_GenericClasses_Component_9_TEcComponentTeventImplementation', bound=Union[Component_GenericClasses_Component_9_TComponentTeventImplementation, Component_GenericClasses_Component_9_TEcComponentTevent])
        Generic_Component_GenericClasses_Component_9_TEcComponentTevent = typing.TypeVar('Generic_Component_GenericClasses_Component_9_TEcComponentTevent')
        def __getitem__(self, types : typing.Tuple[typing.Type[Generic_Component_GenericClasses_Component_9_TEcComponent], typing.Type[Generic_Component_GenericClasses_Component_9_TEcComponentTeventImplementation], typing.Type[Generic_Component_GenericClasses_Component_9_TEcComponentTevent]]) -> typing.Type[TeventiveComponent_6.Component_9[Component_GenericClasses_TeventiveComponent_6_TParent, Component_GenericClasses_TeventiveComponent_6_TParentTevent, Component_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, Component_GenericClasses_TeventiveComponent_6_TComponent, Component_GenericClasses_TeventiveComponent_6_TComponentTevent, Component_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation, Generic_Component_GenericClasses_Component_9_TEcComponent, Generic_Component_GenericClasses_Component_9_TEcComponentTeventImplementation, Generic_Component_GenericClasses_Component_9_TEcComponentTevent]]: ...

    Component : Component_GenericClasses[TeventiveComponent_6_TParent, TeventiveComponent_6_TParentTevent, TeventiveComponent_6_TParentTeventImplementation, TeventiveComponent_6_TComponent, TeventiveComponent_6_TComponentTevent, TeventiveComponent_6_TComponentTeventImplementation]

    Component_9_TParent = typing.TypeVar('Component_9_TParent')
    Component_9_TParentTevent = typing.TypeVar('Component_9_TParentTevent')
    Component_9_TParentTeventImplementation = typing.TypeVar('Component_9_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Component_9_TParentTevent])
    Component_9_TComponent = typing.TypeVar('Component_9_TComponent')
    Component_9_TComponentTevent = typing.TypeVar('Component_9_TComponentTevent')
    Component_9_TComponentTeventImplementation = typing.TypeVar('Component_9_TComponentTeventImplementation', bound=Union[Component_9_TParentTeventImplementation, Component_9_TComponentTevent])
    Component_9_TEcComponent = typing.TypeVar('Component_9_TEcComponent')
    Component_9_TEcComponentTeventImplementation = typing.TypeVar('Component_9_TEcComponentTeventImplementation', bound=Union[Component_9_TComponentTeventImplementation, Component_9_TEcComponentTevent])
    Component_9_TEcComponentTevent = typing.TypeVar('Component_9_TEcComponentTevent')
    class Component_9(typing.Generic[Component_9_TParent, Component_9_TParentTevent, Component_9_TParentTeventImplementation, Component_9_TComponent, Component_9_TComponentTevent, Component_9_TComponentTeventImplementation, Component_9_TEcComponent, Component_9_TEcComponentTeventImplementation, Component_9_TEcComponentTevent], TeventiveComponent_6[Component_9_TComponent, Component_9_TComponentTevent, Component_9_TComponentTeventImplementation, Component_9_TEcComponent, Component_9_TEcComponentTevent, Component_9_TEcComponentTeventImplementation]):
        Component_9_TParent = TeventiveComponent_6.Component_9_TParent
        Component_9_TParentTevent = TeventiveComponent_6.Component_9_TParentTevent
        Component_9_TParentTeventImplementation = TeventiveComponent_6.Component_9_TParentTeventImplementation
        Component_9_TComponent = TeventiveComponent_6.Component_9_TComponent
        Component_9_TComponentTevent = TeventiveComponent_6.Component_9_TComponentTevent
        Component_9_TComponentTeventImplementation = TeventiveComponent_6.Component_9_TComponentTeventImplementation
        Component_9_TEcComponent = TeventiveComponent_6.Component_9_TEcComponent
        Component_9_TEcComponentTeventImplementation = TeventiveComponent_6.Component_9_TEcComponentTeventImplementation
        Component_9_TEcComponentTevent = TeventiveComponent_6.Component_9_TEcComponentTevent
        pass


    Entity_GenericClasses_TeventiveComponent_6_TParent = typing.TypeVar('Entity_GenericClasses_TeventiveComponent_6_TParent')
    Entity_GenericClasses_TeventiveComponent_6_TParentTevent = typing.TypeVar('Entity_GenericClasses_TeventiveComponent_6_TParentTevent')
    Entity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation = typing.TypeVar('Entity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Entity_GenericClasses_TeventiveComponent_6_TParentTevent])
    Entity_GenericClasses_TeventiveComponent_6_TComponent = typing.TypeVar('Entity_GenericClasses_TeventiveComponent_6_TComponent')
    Entity_GenericClasses_TeventiveComponent_6_TComponentTevent = typing.TypeVar('Entity_GenericClasses_TeventiveComponent_6_TComponentTevent')
    Entity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation = typing.TypeVar('Entity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation', bound=Union[Entity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, Entity_GenericClasses_TeventiveComponent_6_TComponentTevent])
    class Entity_GenericClasses(typing.Generic[Entity_GenericClasses_TeventiveComponent_6_TParent, Entity_GenericClasses_TeventiveComponent_6_TParentTevent, Entity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, Entity_GenericClasses_TeventiveComponent_6_TComponent, Entity_GenericClasses_TeventiveComponent_6_TComponentTevent, Entity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation], abc.ABCMeta):
        Entity_GenericClasses_TeventiveComponent_6_TParent = TeventiveComponent_6.Entity_GenericClasses_TeventiveComponent_6_TParent
        Entity_GenericClasses_TeventiveComponent_6_TParentTevent = TeventiveComponent_6.Entity_GenericClasses_TeventiveComponent_6_TParentTevent
        Entity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation = TeventiveComponent_6.Entity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation
        Entity_GenericClasses_TeventiveComponent_6_TComponent = TeventiveComponent_6.Entity_GenericClasses_TeventiveComponent_6_TComponent
        Entity_GenericClasses_TeventiveComponent_6_TComponentTevent = TeventiveComponent_6.Entity_GenericClasses_TeventiveComponent_6_TComponentTevent
        Entity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation = TeventiveComponent_6.Entity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation
        Generic_Entity_GenericClasses_Entity_12_TEntity = typing.TypeVar('Generic_Entity_GenericClasses_Entity_12_TEntity')
        Generic_Entity_GenericClasses_Entity_12_TEntityId = typing.TypeVar('Generic_Entity_GenericClasses_Entity_12_TEntityId')
        Generic_Entity_GenericClasses_Entity_12_TEntityTevent = typing.TypeVar('Generic_Entity_GenericClasses_Entity_12_TEntityTevent')
        Generic_Entity_GenericClasses_Entity_12_TEntityTeventImplementation = typing.TypeVar('Generic_Entity_GenericClasses_Entity_12_TEntityTeventImplementation', bound=Union[Entity_GenericClasses_Entity_12_TComponentTeventImplementation, Entity_GenericClasses_Entity_12_TEntityTevent])
        Generic_Entity_GenericClasses_Entity_12_TEntityCreatedTevent = typing.TypeVar('Generic_Entity_GenericClasses_Entity_12_TEntityCreatedTevent')
        Generic_Entity_GenericClasses_Entity_12_TEntityTeventIdGetterSetter = typing.TypeVar('Generic_Entity_GenericClasses_Entity_12_TEntityTeventIdGetterSetter')
        def __getitem__(self, types : typing.Tuple[typing.Type[Generic_Entity_GenericClasses_Entity_12_TEntity], typing.Type[Generic_Entity_GenericClasses_Entity_12_TEntityId], typing.Type[Generic_Entity_GenericClasses_Entity_12_TEntityTevent], typing.Type[Generic_Entity_GenericClasses_Entity_12_TEntityTeventImplementation], typing.Type[Generic_Entity_GenericClasses_Entity_12_TEntityCreatedTevent], typing.Type[Generic_Entity_GenericClasses_Entity_12_TEntityTeventIdGetterSetter]]) -> typing.Type[TeventiveComponent_6.Entity_12[Entity_GenericClasses_TeventiveComponent_6_TParent, Entity_GenericClasses_TeventiveComponent_6_TParentTevent, Entity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, Entity_GenericClasses_TeventiveComponent_6_TComponent, Entity_GenericClasses_TeventiveComponent_6_TComponentTevent, Entity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation, Generic_Entity_GenericClasses_Entity_12_TEntity, Generic_Entity_GenericClasses_Entity_12_TEntityId, Generic_Entity_GenericClasses_Entity_12_TEntityTevent, Generic_Entity_GenericClasses_Entity_12_TEntityTeventImplementation, Generic_Entity_GenericClasses_Entity_12_TEntityCreatedTevent, Generic_Entity_GenericClasses_Entity_12_TEntityTeventIdGetterSetter]]: ...

    Entity : Entity_GenericClasses[TeventiveComponent_6_TParent, TeventiveComponent_6_TParentTevent, TeventiveComponent_6_TParentTeventImplementation, TeventiveComponent_6_TComponent, TeventiveComponent_6_TComponentTevent, TeventiveComponent_6_TComponentTeventImplementation]

    Entity_12_TParent = typing.TypeVar('Entity_12_TParent')
    Entity_12_TParentTevent = typing.TypeVar('Entity_12_TParentTevent')
    Entity_12_TParentTeventImplementation = typing.TypeVar('Entity_12_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, Entity_12_TParentTevent])
    Entity_12_TComponent = typing.TypeVar('Entity_12_TComponent')
    Entity_12_TComponentTevent = typing.TypeVar('Entity_12_TComponentTevent')
    Entity_12_TComponentTeventImplementation = typing.TypeVar('Entity_12_TComponentTeventImplementation', bound=Union[Entity_12_TParentTeventImplementation, Entity_12_TComponentTevent])
    Entity_12_TEntity = typing.TypeVar('Entity_12_TEntity')
    Entity_12_TEntityId = typing.TypeVar('Entity_12_TEntityId')
    Entity_12_TEntityTevent = typing.TypeVar('Entity_12_TEntityTevent')
    Entity_12_TEntityTeventImplementation = typing.TypeVar('Entity_12_TEntityTeventImplementation', bound=Union[Entity_12_TComponentTeventImplementation, Entity_12_TEntityTevent])
    Entity_12_TEntityCreatedTevent = typing.TypeVar('Entity_12_TEntityCreatedTevent')
    Entity_12_TEntityTeventIdGetterSetter = typing.TypeVar('Entity_12_TEntityTeventIdGetterSetter')
    class Entity_12(typing.Generic[Entity_12_TParent, Entity_12_TParentTevent, Entity_12_TParentTeventImplementation, Entity_12_TComponent, Entity_12_TComponentTevent, Entity_12_TComponentTeventImplementation, Entity_12_TEntity, Entity_12_TEntityId, Entity_12_TEntityTevent, Entity_12_TEntityTeventImplementation, Entity_12_TEntityCreatedTevent, Entity_12_TEntityTeventIdGetterSetter], Tentity_9[Entity_12_TComponent, Entity_12_TComponentTevent, Entity_12_TComponentTeventImplementation, Entity_12_TEntity, Entity_12_TEntityId, Entity_12_TEntityTeventImplementation, Entity_12_TEntityTevent, Entity_12_TEntityCreatedTevent, Entity_12_TEntityTeventIdGetterSetter]):
        Entity_12_TParent = TeventiveComponent_6.Entity_12_TParent
        Entity_12_TParentTevent = TeventiveComponent_6.Entity_12_TParentTevent
        Entity_12_TParentTeventImplementation = TeventiveComponent_6.Entity_12_TParentTeventImplementation
        Entity_12_TComponent = TeventiveComponent_6.Entity_12_TComponent
        Entity_12_TComponentTevent = TeventiveComponent_6.Entity_12_TComponentTevent
        Entity_12_TComponentTeventImplementation = TeventiveComponent_6.Entity_12_TComponentTeventImplementation
        Entity_12_TEntity = TeventiveComponent_6.Entity_12_TEntity
        Entity_12_TEntityId = TeventiveComponent_6.Entity_12_TEntityId
        Entity_12_TEntityTevent = TeventiveComponent_6.Entity_12_TEntityTevent
        Entity_12_TEntityTeventImplementation = TeventiveComponent_6.Entity_12_TEntityTeventImplementation
        Entity_12_TEntityCreatedTevent = TeventiveComponent_6.Entity_12_TEntityCreatedTevent
        Entity_12_TEntityTeventIdGetterSetter = TeventiveComponent_6.Entity_12_TEntityTeventIdGetterSetter
        @property
        def Id(self) -> Entity_12_TEntityId: ...


    RemovableEntity_GenericClasses_TeventiveComponent_6_TParent = typing.TypeVar('RemovableEntity_GenericClasses_TeventiveComponent_6_TParent')
    RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTevent = typing.TypeVar('RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTevent')
    RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation = typing.TypeVar('RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTevent])
    RemovableEntity_GenericClasses_TeventiveComponent_6_TComponent = typing.TypeVar('RemovableEntity_GenericClasses_TeventiveComponent_6_TComponent')
    RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTevent = typing.TypeVar('RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTevent')
    RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation = typing.TypeVar('RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation', bound=Union[RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTevent])
    class RemovableEntity_GenericClasses(typing.Generic[RemovableEntity_GenericClasses_TeventiveComponent_6_TParent, RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTevent, RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, RemovableEntity_GenericClasses_TeventiveComponent_6_TComponent, RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTevent, RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation], abc.ABCMeta):
        RemovableEntity_GenericClasses_TeventiveComponent_6_TParent = TeventiveComponent_6.RemovableEntity_GenericClasses_TeventiveComponent_6_TParent
        RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTevent = TeventiveComponent_6.RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTevent
        RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation = TeventiveComponent_6.RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation
        RemovableEntity_GenericClasses_TeventiveComponent_6_TComponent = TeventiveComponent_6.RemovableEntity_GenericClasses_TeventiveComponent_6_TComponent
        RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTevent = TeventiveComponent_6.RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTevent
        RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation = TeventiveComponent_6.RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation
        Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntity = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntity')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityId = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityId')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTevent = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTevent')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTeventImplementation = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTeventImplementation', bound=Union[RemovableEntity_GenericClasses_RemovableEntity_13_TComponentTeventImplementation, RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTevent])
        Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityCreatedTevent = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityCreatedTevent')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityRemovedTevent = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityRemovedTevent')
        Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTeventIdGetterSetter = typing.TypeVar('Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTeventIdGetterSetter')
        def __getitem__(self, types : typing.Tuple[typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntity], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityId], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTevent], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTeventImplementation], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityCreatedTevent], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityRemovedTevent], typing.Type[Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTeventIdGetterSetter]]) -> typing.Type[TeventiveComponent_6.RemovableEntity_13[RemovableEntity_GenericClasses_TeventiveComponent_6_TParent, RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTevent, RemovableEntity_GenericClasses_TeventiveComponent_6_TParentTeventImplementation, RemovableEntity_GenericClasses_TeventiveComponent_6_TComponent, RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTevent, RemovableEntity_GenericClasses_TeventiveComponent_6_TComponentTeventImplementation, Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntity, Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityId, Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTevent, Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTeventImplementation, Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityCreatedTevent, Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityRemovedTevent, Generic_RemovableEntity_GenericClasses_RemovableEntity_13_TEntityTeventIdGetterSetter]]: ...

    RemovableEntity : RemovableEntity_GenericClasses[TeventiveComponent_6_TParent, TeventiveComponent_6_TParentTevent, TeventiveComponent_6_TParentTeventImplementation, TeventiveComponent_6_TComponent, TeventiveComponent_6_TComponentTevent, TeventiveComponent_6_TComponentTeventImplementation]

    RemovableEntity_13_TParent = typing.TypeVar('RemovableEntity_13_TParent')
    RemovableEntity_13_TParentTevent = typing.TypeVar('RemovableEntity_13_TParentTevent')
    RemovableEntity_13_TParentTeventImplementation = typing.TypeVar('RemovableEntity_13_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, RemovableEntity_13_TParentTevent])
    RemovableEntity_13_TComponent = typing.TypeVar('RemovableEntity_13_TComponent')
    RemovableEntity_13_TComponentTevent = typing.TypeVar('RemovableEntity_13_TComponentTevent')
    RemovableEntity_13_TComponentTeventImplementation = typing.TypeVar('RemovableEntity_13_TComponentTeventImplementation', bound=Union[RemovableEntity_13_TParentTeventImplementation, RemovableEntity_13_TComponentTevent])
    RemovableEntity_13_TEntity = typing.TypeVar('RemovableEntity_13_TEntity')
    RemovableEntity_13_TEntityId = typing.TypeVar('RemovableEntity_13_TEntityId')
    RemovableEntity_13_TEntityTevent = typing.TypeVar('RemovableEntity_13_TEntityTevent')
    RemovableEntity_13_TEntityTeventImplementation = typing.TypeVar('RemovableEntity_13_TEntityTeventImplementation', bound=Union[RemovableEntity_13_TComponentTeventImplementation, RemovableEntity_13_TEntityTevent])
    RemovableEntity_13_TEntityCreatedTevent = typing.TypeVar('RemovableEntity_13_TEntityCreatedTevent')
    RemovableEntity_13_TEntityRemovedTevent = typing.TypeVar('RemovableEntity_13_TEntityRemovedTevent')
    RemovableEntity_13_TEntityTeventIdGetterSetter = typing.TypeVar('RemovableEntity_13_TEntityTeventIdGetterSetter')
    class RemovableEntity_13(typing.Generic[RemovableEntity_13_TParent, RemovableEntity_13_TParentTevent, RemovableEntity_13_TParentTeventImplementation, RemovableEntity_13_TComponent, RemovableEntity_13_TComponentTevent, RemovableEntity_13_TComponentTeventImplementation, RemovableEntity_13_TEntity, RemovableEntity_13_TEntityId, RemovableEntity_13_TEntityTevent, RemovableEntity_13_TEntityTeventImplementation, RemovableEntity_13_TEntityCreatedTevent, RemovableEntity_13_TEntityRemovedTevent, RemovableEntity_13_TEntityTeventIdGetterSetter], TeventiveRemovableEntity_10[RemovableEntity_13_TComponent, RemovableEntity_13_TComponentTevent, RemovableEntity_13_TComponentTeventImplementation, RemovableEntity_13_TEntity, RemovableEntity_13_TEntityId, RemovableEntity_13_TEntityTevent, RemovableEntity_13_TEntityTeventImplementation, RemovableEntity_13_TEntityCreatedTevent, RemovableEntity_13_TEntityRemovedTevent, RemovableEntity_13_TEntityTeventIdGetterSetter]):
        RemovableEntity_13_TParent = TeventiveComponent_6.RemovableEntity_13_TParent
        RemovableEntity_13_TParentTevent = TeventiveComponent_6.RemovableEntity_13_TParentTevent
        RemovableEntity_13_TParentTeventImplementation = TeventiveComponent_6.RemovableEntity_13_TParentTeventImplementation
        RemovableEntity_13_TComponent = TeventiveComponent_6.RemovableEntity_13_TComponent
        RemovableEntity_13_TComponentTevent = TeventiveComponent_6.RemovableEntity_13_TComponentTevent
        RemovableEntity_13_TComponentTeventImplementation = TeventiveComponent_6.RemovableEntity_13_TComponentTeventImplementation
        RemovableEntity_13_TEntity = TeventiveComponent_6.RemovableEntity_13_TEntity
        RemovableEntity_13_TEntityId = TeventiveComponent_6.RemovableEntity_13_TEntityId
        RemovableEntity_13_TEntityTevent = TeventiveComponent_6.RemovableEntity_13_TEntityTevent
        RemovableEntity_13_TEntityTeventImplementation = TeventiveComponent_6.RemovableEntity_13_TEntityTeventImplementation
        RemovableEntity_13_TEntityCreatedTevent = TeventiveComponent_6.RemovableEntity_13_TEntityCreatedTevent
        RemovableEntity_13_TEntityRemovedTevent = TeventiveComponent_6.RemovableEntity_13_TEntityRemovedTevent
        RemovableEntity_13_TEntityTeventIdGetterSetter = TeventiveComponent_6.RemovableEntity_13_TEntityTeventIdGetterSetter
        @property
        def Id(self) -> RemovableEntity_13_TEntityId: ...



class TeventiveRemovableEntity_GenericClasses(abc.ABCMeta):
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParent = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParent')
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTevent = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTevent')
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTeventImplementation = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTevent])
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntity = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntity')
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityId = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityId')
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTevent = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTevent')
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTeventImplementation = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTeventImplementation', bound=Union[TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTeventImplementation, TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTevent])
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityCreatedTevent = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityCreatedTevent')
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityRemovedTevent = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityRemovedTevent')
    Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTeventIdGetterSetter = typing.TypeVar('Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTeventIdGetterSetter')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParent], typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTevent], typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTeventImplementation], typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntity], typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityId], typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTevent], typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTeventImplementation], typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityCreatedTevent], typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityRemovedTevent], typing.Type[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTeventIdGetterSetter]]) -> typing.Type[TeventiveRemovableEntity_10[Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParent, Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTevent, Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TParentTeventImplementation, Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntity, Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityId, Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTevent, Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTeventImplementation, Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityCreatedTevent, Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityRemovedTevent, Generic_TeventiveRemovableEntity_GenericClasses_TeventiveRemovableEntity_10_TEntityTeventIdGetterSetter]]: ...

TeventiveRemovableEntity : TeventiveRemovableEntity_GenericClasses

TeventiveRemovableEntity_10_TParent = typing.TypeVar('TeventiveRemovableEntity_10_TParent')
TeventiveRemovableEntity_10_TParentTevent = typing.TypeVar('TeventiveRemovableEntity_10_TParentTevent')
TeventiveRemovableEntity_10_TParentTeventImplementation = typing.TypeVar('TeventiveRemovableEntity_10_TParentTeventImplementation', bound=Union[Compze.Teventive.Taggregates.Tevents.TaggregateTevent, TeventiveRemovableEntity_10_TParentTevent])
TeventiveRemovableEntity_10_TEntity = typing.TypeVar('TeventiveRemovableEntity_10_TEntity')
TeventiveRemovableEntity_10_TEntityId = typing.TypeVar('TeventiveRemovableEntity_10_TEntityId')
TeventiveRemovableEntity_10_TEntityTevent = typing.TypeVar('TeventiveRemovableEntity_10_TEntityTevent')
TeventiveRemovableEntity_10_TEntityTeventImplementation = typing.TypeVar('TeventiveRemovableEntity_10_TEntityTeventImplementation', bound=Union[TeventiveRemovableEntity_10_TParentTeventImplementation, TeventiveRemovableEntity_10_TEntityTevent])
TeventiveRemovableEntity_10_TEntityCreatedTevent = typing.TypeVar('TeventiveRemovableEntity_10_TEntityCreatedTevent')
TeventiveRemovableEntity_10_TEntityRemovedTevent = typing.TypeVar('TeventiveRemovableEntity_10_TEntityRemovedTevent')
TeventiveRemovableEntity_10_TEntityTeventIdGetterSetter = typing.TypeVar('TeventiveRemovableEntity_10_TEntityTeventIdGetterSetter')
class TeventiveRemovableEntity_10(typing.Generic[TeventiveRemovableEntity_10_TParent, TeventiveRemovableEntity_10_TParentTevent, TeventiveRemovableEntity_10_TParentTeventImplementation, TeventiveRemovableEntity_10_TEntity, TeventiveRemovableEntity_10_TEntityId, TeventiveRemovableEntity_10_TEntityTevent, TeventiveRemovableEntity_10_TEntityTeventImplementation, TeventiveRemovableEntity_10_TEntityCreatedTevent, TeventiveRemovableEntity_10_TEntityRemovedTevent, TeventiveRemovableEntity_10_TEntityTeventIdGetterSetter], Tentity_9[TeventiveRemovableEntity_10_TParent, TeventiveRemovableEntity_10_TParentTevent, TeventiveRemovableEntity_10_TParentTeventImplementation, TeventiveRemovableEntity_10_TEntity, TeventiveRemovableEntity_10_TEntityId, TeventiveRemovableEntity_10_TEntityTeventImplementation, TeventiveRemovableEntity_10_TEntityTevent, TeventiveRemovableEntity_10_TEntityCreatedTevent, TeventiveRemovableEntity_10_TEntityTeventIdGetterSetter]):
    @property
    def Id(self) -> TeventiveRemovableEntity_10_TEntityId: ...
    @staticmethod
    def CreateSelfManagingCollection(parent: TeventiveRemovableEntity_10_TParent) -> Tentity_9.ICollectionManager_9[TeventiveRemovableEntity_10_TParent, TeventiveRemovableEntity_10_TParentTevent, TeventiveRemovableEntity_10_TParentTeventImplementation, TeventiveRemovableEntity_10_TEntity, TeventiveRemovableEntity_10_TEntityId, TeventiveRemovableEntity_10_TEntityTeventImplementation, TeventiveRemovableEntity_10_TEntityTevent, TeventiveRemovableEntity_10_TEntityCreatedTevent, TeventiveRemovableEntity_10_TEntityTeventIdGetterSetter]: ...

