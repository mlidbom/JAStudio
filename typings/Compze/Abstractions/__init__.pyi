import typing, abc
from System import Guid
from Compze.Abstractions.Infrastructure import ValueWrapper_1

class Entity_GenericClasses(abc.ABCMeta):
    Generic_Entity_GenericClasses_Entity_1_TEntity = typing.TypeVar('Generic_Entity_GenericClasses_Entity_1_TEntity')
    @typing.overload
    def __getitem__(self, types : typing.Type[Generic_Entity_GenericClasses_Entity_1_TEntity]) -> typing.Type[Entity_1[Generic_Entity_GenericClasses_Entity_1_TEntity]]: ...
    Generic_Entity_GenericClasses_Entity_2_TEntity = typing.TypeVar('Generic_Entity_GenericClasses_Entity_2_TEntity')
    Generic_Entity_GenericClasses_Entity_2_TKey = typing.TypeVar('Generic_Entity_GenericClasses_Entity_2_TKey')
    @typing.overload
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_Entity_GenericClasses_Entity_2_TEntity], typing.Type[Generic_Entity_GenericClasses_Entity_2_TKey]]) -> typing.Type[Entity_2[Generic_Entity_GenericClasses_Entity_2_TEntity, Generic_Entity_GenericClasses_Entity_2_TKey]]: ...

Entity : Entity_GenericClasses

Entity_1_TEntity = typing.TypeVar('Entity_1_TEntity')
class Entity_1(typing.Generic[Entity_1_TEntity], Entity_2[Entity_1_TEntity, Guid], IEntity_0):
    @property
    def Id(self) -> EntityId: ...
    @Id.setter
    def Id(self, value: EntityId) -> EntityId: ...
    @property
    def Id(self) -> EntityId_1[Guid]: ...
    @Id.setter
    def Id(self, value: EntityId_1[Guid]) -> EntityId_1[Guid]: ...


Entity_2_TEntity = typing.TypeVar('Entity_2_TEntity')
Entity_2_TKey = typing.TypeVar('Entity_2_TKey')
class Entity_2(typing.Generic[Entity_2_TEntity, Entity_2_TKey], IEntity_1[Entity_2_TKey]):
    @property
    def Id(self) -> EntityId_1[Entity_2_TKey]: ...
    @Id.setter
    def Id(self, value: EntityId_1[Entity_2_TKey]) -> EntityId_1[Entity_2_TKey]: ...
    def ToString(self) -> str: ...


class EntityId_GenericClasses(abc.ABCMeta):
    Generic_EntityId_GenericClasses_EntityId_1_TPrimitive = typing.TypeVar('Generic_EntityId_GenericClasses_EntityId_1_TPrimitive')
    def __getitem__(self, types : typing.Type[Generic_EntityId_GenericClasses_EntityId_1_TPrimitive]) -> typing.Type[EntityId_1[Generic_EntityId_GenericClasses_EntityId_1_TPrimitive]]: ...

class EntityId(EntityId_0, metaclass =EntityId_GenericClasses): ...

class EntityId_0(EntityId_1[Guid]):
    @typing.overload
    def __init__(self) -> None: ...
    @typing.overload
    def __init__(self, id: Guid) -> None: ...
    @property
    def Value(self) -> Guid: ...


EntityId_1_TPrimitive = typing.TypeVar('EntityId_1_TPrimitive')
class EntityId_1(typing.Generic[EntityId_1_TPrimitive], ValueWrapper_1[EntityId_1_TPrimitive]):
    def __init__(self, primitiveValue: EntityId_1_TPrimitive) -> None: ...
    @property
    def Value(self) -> EntityId_1_TPrimitive: ...


class IEntity_GenericClasses(abc.ABCMeta):
    Generic_IEntity_GenericClasses_IEntity_1_TKeyType = typing.TypeVar('Generic_IEntity_GenericClasses_IEntity_1_TKeyType')
    def __getitem__(self, types : typing.Type[Generic_IEntity_GenericClasses_IEntity_1_TKeyType]) -> typing.Type[IEntity_1[Generic_IEntity_GenericClasses_IEntity_1_TKeyType]]: ...

class IEntity(IEntity_0, metaclass =IEntity_GenericClasses): ...

class IEntity_0(IEntity_1[Guid], typing.Protocol):
    @property
    def Id(self) -> EntityId: ...


IEntity_1_TKeyType = typing.TypeVar('IEntity_1_TKeyType')
class IEntity_1(typing.Generic[IEntity_1_TKeyType], typing.Protocol):
    @property
    def Id(self) -> EntityId_1[IEntity_1_TKeyType]: ...


class ITentity(IEntity_0, typing.Protocol):
    pass


class TaggregateId(TentityId):
    @typing.overload
    def __init__(self) -> None: ...
    @typing.overload
    def __init__(self, id: Guid) -> None: ...
    @property
    def Value(self) -> Guid: ...


class TentityId(EntityId_0):
    @typing.overload
    def __init__(self) -> None: ...
    @typing.overload
    def __init__(self, id: Guid) -> None: ...
    @property
    def Value(self) -> Guid: ...


class VersionedEntity_GenericClasses(abc.ABCMeta):
    Generic_VersionedEntity_GenericClasses_VersionedEntity_1_T = typing.TypeVar('Generic_VersionedEntity_GenericClasses_VersionedEntity_1_T')
    def __getitem__(self, types : typing.Type[Generic_VersionedEntity_GenericClasses_VersionedEntity_1_T]) -> typing.Type[VersionedEntity_1[Generic_VersionedEntity_GenericClasses_VersionedEntity_1_T]]: ...

VersionedEntity : VersionedEntity_GenericClasses

VersionedEntity_1_T = typing.TypeVar('VersionedEntity_1_T')
class VersionedEntity_1(typing.Generic[VersionedEntity_1_T], Entity_1[VersionedEntity_1_T]):
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

