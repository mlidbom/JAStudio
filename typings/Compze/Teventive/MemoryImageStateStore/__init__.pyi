import typing
from System import Func_2, Action_1, Func_1
from System.Collections.Generic import IEnumerable_1
from System.IO import DirectoryInfo
from System.Text.Json import JsonSerializerOptions
from Compze.Teventive import ITaggregate
from Compze.Teventive.Taggregates import ISnapshotable_2
from Compze.DependencyInjection import ComponentRegistration_1

class MemoryImageStateStoreBuilder:
    # Skipped Index due to it being static, abstract and generic.

    Index : Index_MethodGroup
    class Index_MethodGroup:
        def __getitem__(self, t:typing.Tuple[typing.Type[Index_2_T1], typing.Type[Index_2_T2]]) -> Index_2[Index_2_T1, Index_2_T2]: ...

        Index_2_T1 = typing.TypeVar('Index_2_T1')
        Index_2_T2 = typing.TypeVar('Index_2_T2')
        class Index_2(typing.Generic[Index_2_T1, Index_2_T2]):
            Index_2_TTaggregate = MemoryImageStateStoreBuilder.Index_MethodGroup.Index_2_T1
            Index_2_TKey = MemoryImageStateStoreBuilder.Index_MethodGroup.Index_2_T2
            def __call__(self, keysOf: Func_2[Index_2_TTaggregate, IEnumerable_1[Index_2_TKey]]) -> MemoryImageStateStoreBuilder:...


    # Skipped StoredInJsonFiles due to it being static, abstract and generic.

    StoredInJsonFiles : StoredInJsonFiles_MethodGroup
    class StoredInJsonFiles_MethodGroup:
        def __getitem__(self, t:typing.Type[StoredInJsonFiles_1_T1]) -> StoredInJsonFiles_1[StoredInJsonFiles_1_T1]: ...

        StoredInJsonFiles_1_T1 = typing.TypeVar('StoredInJsonFiles_1_T1')
        class StoredInJsonFiles_1(typing.Generic[StoredInJsonFiles_1_T1]):
            StoredInJsonFiles_1_TService = MemoryImageStateStoreBuilder.StoredInJsonFiles_MethodGroup.StoredInJsonFiles_1_T1
            def __call__(self, folderWith: Func_2[StoredInJsonFiles_1_TService, DirectoryInfo], options: JsonSerializerOptions, declare: Action_1[TaggregateFilesBuilder]) -> MemoryImageStateStoreBuilder:...

        def __call__(self, folder: DirectoryInfo, options: JsonSerializerOptions, declare: Action_1[TaggregateFilesBuilder]) -> MemoryImageStateStoreBuilder:...

    # Skipped Taggregate due to it being static, abstract and generic.

    Taggregate : Taggregate_MethodGroup
    class Taggregate_MethodGroup:
        @typing.overload
        def __getitem__(self, t:typing.Tuple[typing.Type[Taggregate_2_T1], typing.Type[Taggregate_2_T2]]) -> Taggregate_2[Taggregate_2_T1, Taggregate_2_T2]: ...

        Taggregate_2_T1 = typing.TypeVar('Taggregate_2_T1', bound=Union[ITaggregate, ISnapshotable_2[Taggregate_2_TTaggregate, Taggregate_2_TSnapshot]])
        Taggregate_2_T2 = typing.TypeVar('Taggregate_2_T2')
        class Taggregate_2(typing.Generic[Taggregate_2_T1, Taggregate_2_T2]):
            Taggregate_2_TTaggregate = MemoryImageStateStoreBuilder.Taggregate_MethodGroup.Taggregate_2_T1
            Taggregate_2_TSnapshot = MemoryImageStateStoreBuilder.Taggregate_MethodGroup.Taggregate_2_T2
            def __call__(self, createEmpty: Func_1[ISnapshotable_2[Taggregate_2_TTaggregate, Taggregate_2_TSnapshot]]) -> MemoryImageStateStoreBuilder:...

        @typing.overload
        def __getitem__(self, t:typing.Type[Taggregate_1_T1]) -> Taggregate_1[Taggregate_1_T1]: ...

        Taggregate_1_T1 = typing.TypeVar('Taggregate_1_T1')
        class Taggregate_1(typing.Generic[Taggregate_1_T1]):
            Taggregate_1_TTaggregate = MemoryImageStateStoreBuilder.Taggregate_MethodGroup.Taggregate_1_T1
            def __call__(self, registration: ComponentRegistration_1[Taggregate_1_TTaggregate]) -> MemoryImageStateStoreBuilder:...




class TaggregateFilesBuilder:
    def AcceptingNonAtomicCommitsWithADatabase(self) -> TaggregateFilesBuilder: ...
    # Skipped FolderOf due to it being static, abstract and generic.

    FolderOf : FolderOf_MethodGroup
    class FolderOf_MethodGroup:
        def __getitem__(self, t:typing.Type[FolderOf_1_T1]) -> FolderOf_1[FolderOf_1_T1]: ...

        FolderOf_1_T1 = typing.TypeVar('FolderOf_1_T1')
        class FolderOf_1(typing.Generic[FolderOf_1_T1]):
            FolderOf_1_TTaggregate = TaggregateFilesBuilder.FolderOf_MethodGroup.FolderOf_1_T1
            def __call__(self, folder: str) -> TaggregateFilesBuilder:...



