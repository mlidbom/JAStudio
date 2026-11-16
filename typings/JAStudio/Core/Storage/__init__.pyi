import typing, abc
from System import IDisposable, Guid
from Compze.Teventive.StateStore.Storage import ITaggregateStorage, TaggregateSnapshot, IPreparedWrite, TaggregateToDelete
from System.Collections.Generic import IReadOnlyList_1, List_1
from System.Text.Json import JsonSerializerOptions
from JAStudio.Core.Note import NoteServices, KanjiNote, JPNote, NoteId
from JAStudio.Core.Note.Sentences import SentenceNote
from JAStudio.Core.Note.Vocabulary import VocabNote

class CorpusCheckoutLock(IDisposable):
    def Dispose(self) -> None: ...
    @staticmethod
    def On(corpusCheckoutFolder: str) -> CorpusCheckoutLock: ...


class CorpusNoteFiles(ITaggregateStorage):
    def __init__(self, corpusCheckoutFolder: str) -> None: ...
    def LoadAll(self) -> IReadOnlyList_1[TaggregateSnapshot]: ...
    def Prepare(self, toSave: IReadOnlyList_1[TaggregateSnapshot], toDelete: IReadOnlyList_1[TaggregateToDelete]) -> IPreparedWrite: ...


class IIdentifiableByGuid(typing.Protocol):
    @property
    def Id(self) -> Guid: ...


class JsonFilesystemObjectRepository_GenericClasses(abc.ABCMeta):
    Generic_JsonFilesystemObjectRepository_GenericClasses_JsonFilesystemObjectRepository_1_TData = typing.TypeVar('Generic_JsonFilesystemObjectRepository_GenericClasses_JsonFilesystemObjectRepository_1_TData')
    def __getitem__(self, types : typing.Type[Generic_JsonFilesystemObjectRepository_GenericClasses_JsonFilesystemObjectRepository_1_TData]) -> typing.Type[JsonFilesystemObjectRepository_1[Generic_JsonFilesystemObjectRepository_GenericClasses_JsonFilesystemObjectRepository_1_TData]]: ...

JsonFilesystemObjectRepository : JsonFilesystemObjectRepository_GenericClasses

JsonFilesystemObjectRepository_1_TData = typing.TypeVar('JsonFilesystemObjectRepository_1_TData')
class JsonFilesystemObjectRepository_1(typing.Generic[JsonFilesystemObjectRepository_1_TData]):
    def __init__(self, directory: str, jsonOptions: JsonSerializerOptions) -> None: ...
    def LoadAll(self) -> List_1[JsonFilesystemObjectRepository_1_TData]: ...
    def PrepareDeleting(self, id: Guid) -> IPreparedWrite: ...
    def PrepareSaving(self, data: JsonFilesystemObjectRepository_1_TData) -> IPreparedWrite: ...


class NoteSerializer:
    def __init__(self, noteServices: NoteServices) -> None: ...
    def DeserializeKanji(self, json: str) -> KanjiNote: ...
    def DeserializeSentence(self, json: str) -> SentenceNote: ...
    def DeserializeVocab(self, json: str) -> VocabNote: ...
    # Skipped Serialize due to it being static, abstract and generic.

    Serialize : Serialize_MethodGroup
    class Serialize_MethodGroup:
        @typing.overload
        def __call__(self, note: KanjiNote) -> str:...
        @typing.overload
        def __call__(self, note: VocabNote) -> str:...
        @typing.overload
        def __call__(self, note: SentenceNote) -> str:...
        @typing.overload
        def __call__(self, note: JPNote) -> str:...



class NotesStoredInMemory(ITaggregateStorage):
    def __init__(self) -> None: ...
    def LoadAll(self) -> IReadOnlyList_1[TaggregateSnapshot]: ...
    def Prepare(self, toSave: IReadOnlyList_1[TaggregateSnapshot], toDelete: IReadOnlyList_1[TaggregateToDelete]) -> IPreparedWrite: ...
    def Store(self, note: JPNote) -> None: ...
    # Skipped SnapshotOf due to it being static, abstract and generic.

    SnapshotOf : SnapshotOf_MethodGroup
    class SnapshotOf_MethodGroup:
        def __getitem__(self, t:typing.Type[SnapshotOf_1_T1]) -> SnapshotOf_1[SnapshotOf_1_T1]: ...

        SnapshotOf_1_T1 = typing.TypeVar('SnapshotOf_1_T1')
        class SnapshotOf_1(typing.Generic[SnapshotOf_1_T1]):
            SnapshotOf_1_TSnapshot = NotesStoredInMemory.SnapshotOf_MethodGroup.SnapshotOf_1_T1
            def __call__(self, noteId: NoteId) -> SnapshotOf_1_TSnapshot:...


    # Skipped Snapshots due to it being static, abstract and generic.

    Snapshots : Snapshots_MethodGroup
    class Snapshots_MethodGroup:
        def __getitem__(self, t:typing.Type[Snapshots_1_T1]) -> Snapshots_1[Snapshots_1_T1]: ...

        Snapshots_1_T1 = typing.TypeVar('Snapshots_1_T1')
        class Snapshots_1(typing.Generic[Snapshots_1_T1]):
            Snapshots_1_TSnapshot = NotesStoredInMemory.Snapshots_MethodGroup.Snapshots_1_T1
            def __call__(self) -> IReadOnlyList_1[Snapshots_1_TSnapshot]:...



