import typing, abc

class ISnapshotable_GenericClasses(abc.ABCMeta):
    Generic_ISnapshotable_GenericClasses_ISnapshotable_2_TSelf = typing.TypeVar('Generic_ISnapshotable_GenericClasses_ISnapshotable_2_TSelf')
    Generic_ISnapshotable_GenericClasses_ISnapshotable_2_TSnapshot = typing.TypeVar('Generic_ISnapshotable_GenericClasses_ISnapshotable_2_TSnapshot')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_ISnapshotable_GenericClasses_ISnapshotable_2_TSelf], typing.Type[Generic_ISnapshotable_GenericClasses_ISnapshotable_2_TSnapshot]]) -> typing.Type[ISnapshotable_2[Generic_ISnapshotable_GenericClasses_ISnapshotable_2_TSelf, Generic_ISnapshotable_GenericClasses_ISnapshotable_2_TSnapshot]]: ...

ISnapshotable : ISnapshotable_GenericClasses

ISnapshotable_2_TSelf = typing.TypeVar('ISnapshotable_2_TSelf')
ISnapshotable_2_TSnapshot = typing.TypeVar('ISnapshotable_2_TSnapshot')
class ISnapshotable_2(typing.Generic[ISnapshotable_2_TSelf, ISnapshotable_2_TSnapshot], typing.Protocol):
    @abc.abstractmethod
    def RestoreFrom(self, snapshot: ISnapshotable_2_TSnapshot) -> None: ...
    @abc.abstractmethod
    def TakeSnapshot(self) -> ISnapshotable_2_TSnapshot: ...

