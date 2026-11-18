import typing
from System import Func_2, Action_1, Func_1, Func_3
from System.Collections.Generic import IEnumerable_1
from System.IO import DirectoryInfo, FileInfo
from System.Text.Json import JsonSerializerOptions
from Compze.Teventive import ITaggregate
from Compze.Teventive.Taggregates import ISnapshotable_2
from Compze.DependencyInjection import ComponentRegistration_1

class StateStoreBuilder:
    # Skipped Index due to it being static, abstract and generic.

    Index : Index_MethodGroup
    class Index_MethodGroup:
        def __getitem__(self, t:typing.Tuple[typing.Type[Index_2_T1], typing.Type[Index_2_T2]]) -> Index_2[Index_2_T1, Index_2_T2]: ...

        Index_2_T1 = typing.TypeVar('Index_2_T1')
        Index_2_T2 = typing.TypeVar('Index_2_T2')
        class Index_2(typing.Generic[Index_2_T1, Index_2_T2]):
            Index_2_TTaggregate = StateStoreBuilder.Index_MethodGroup.Index_2_T1
            Index_2_TKey = StateStoreBuilder.Index_MethodGroup.Index_2_T2
            def __call__(self, keysOf: Func_2[Index_2_TTaggregate, IEnumerable_1[Index_2_TKey]]) -> StateStoreBuilder:...


    # Skipped StoredInFiles due to it being static, abstract and generic.

    StoredInFiles : StoredInFiles_MethodGroup
    class StoredInFiles_MethodGroup:
        def __getitem__(self, t:typing.Type[StoredInFiles_1_T1]) -> StoredInFiles_1[StoredInFiles_1_T1]: ...

        StoredInFiles_1_T1 = typing.TypeVar('StoredInFiles_1_T1')
        class StoredInFiles_1(typing.Generic[StoredInFiles_1_T1]):
            StoredInFiles_1_TService = StateStoreBuilder.StoredInFiles_MethodGroup.StoredInFiles_1_T1
            def __call__(self, folderWith: Func_2[StoredInFiles_1_TService, DirectoryInfo], options: JsonSerializerOptions, declare: Action_1[TaggregateFiles]) -> StateStoreBuilder:...

        def __call__(self, folder: DirectoryInfo, options: JsonSerializerOptions, declare: Action_1[TaggregateFiles]) -> StateStoreBuilder:...

    # Skipped Taggregate due to it being static, abstract and generic.

    Taggregate : Taggregate_MethodGroup
    class Taggregate_MethodGroup:
        @typing.overload
        def __getitem__(self, t:typing.Type[Taggregate_1_T1]) -> Taggregate_1[Taggregate_1_T1]: ...

        Taggregate_1_T1 = typing.TypeVar('Taggregate_1_T1')
        class Taggregate_1(typing.Generic[Taggregate_1_T1]):
            Taggregate_1_TTaggregate = StateStoreBuilder.Taggregate_MethodGroup.Taggregate_1_T1
            def __call__(self, registration: ComponentRegistration_1[Taggregate_1_TTaggregate]) -> StateStoreBuilder:...

        @typing.overload
        def __getitem__(self, t:typing.Tuple[typing.Type[Taggregate_2_T1], typing.Type[Taggregate_2_T2]]) -> Taggregate_2[Taggregate_2_T1, Taggregate_2_T2]: ...

        Taggregate_2_T1 = typing.TypeVar('Taggregate_2_T1', bound=Union[ITaggregate, ISnapshotable_2[Taggregate_2_TTaggregate, Taggregate_2_TSnapshot]])
        Taggregate_2_T2 = typing.TypeVar('Taggregate_2_T2')
        class Taggregate_2(typing.Generic[Taggregate_2_T1, Taggregate_2_T2]):
            Taggregate_2_TTaggregate = StateStoreBuilder.Taggregate_MethodGroup.Taggregate_2_T1
            Taggregate_2_TSnapshot = StateStoreBuilder.Taggregate_MethodGroup.Taggregate_2_T2
            @typing.overload
            def __call__(self, createEmpty: Func_1[ISnapshotable_2[Taggregate_2_TTaggregate, Taggregate_2_TSnapshot]], snapshotsAreEqual: Func_3[Taggregate_2_TSnapshot, Taggregate_2_TSnapshot, bool] = ...) -> StateStoreBuilder:...
            @typing.overload
            def __call__(self, registration: ComponentRegistration_1[Taggregate_2_TTaggregate], snapshotsAreEqual: Func_3[Taggregate_2_TSnapshot, Taggregate_2_TSnapshot, bool]) -> StateStoreBuilder:...




class TaggregateFiles:
    def AllowedInTransactionsThatAlsoWriteADatabase(self) -> TaggregateFiles: ...
    def KeptInMemory(self) -> TaggregateFiles: ...
    # Skipped Of due to it being static, abstract and generic.

    Of : Of_MethodGroup
    class Of_MethodGroup:
        def __getitem__(self, t:typing.Type[Of_1_T1]) -> Of_1[Of_1_T1]: ...

        Of_1_T1 = typing.TypeVar('Of_1_T1')
        class Of_1(typing.Generic[Of_1_T1]):
            Of_1_TTaggregate = TaggregateFiles.Of_MethodGroup.Of_1_T1
            def __call__(self, folder: str, fileOf: Func_3[Of_1_TTaggregate, DirectoryInfo, FileInfo] = ...) -> TaggregateFiles:...



