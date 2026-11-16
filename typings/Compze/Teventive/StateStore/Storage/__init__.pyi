import typing, abc
from System.Collections.Generic import IReadOnlyList_1
from Compze.Abstractions import TaggregateId

class IPreparedWrite(typing.Protocol):
    @abc.abstractmethod
    def Commit(self) -> None: ...
    @abc.abstractmethod
    def Discard(self) -> None: ...


class ITaggregateStorage(typing.Protocol):
    @abc.abstractmethod
    def LoadAll(self) -> IReadOnlyList_1[TaggregateSnapshot]: ...
    @abc.abstractmethod
    def Prepare(self, toSave: IReadOnlyList_1[TaggregateSnapshot], toDelete: IReadOnlyList_1[TaggregateToDelete]) -> IPreparedWrite: ...


class TaggregateSnapshot:
    def __init__(self, taggregateClass: typing.Type[typing.Any], id: TaggregateId, snapshot: typing.Any) -> None: ...
    @property
    def Id(self) -> TaggregateId: ...
    @property
    def Snapshot(self) -> typing.Any: ...
    @property
    def TaggregateClass(self) -> typing.Type[typing.Any]: ...


class TaggregateToDelete:
    @property
    def Id(self) -> TaggregateId: ...
    @property
    def TaggregateClass(self) -> typing.Type[typing.Any]: ...

