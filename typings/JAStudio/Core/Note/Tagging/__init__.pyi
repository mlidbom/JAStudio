import typing, abc
from System.Collections.Generic import IEnumerable_1, List_1, IEnumerator_1, IReadOnlyList_1, IReadOnlyCollection_1
from JAStudio.Core.Note import JPNote

class NoteTags(IEnumerable_1[Tag]):
    def __init__(self, note: JPNote, tags: List_1[str] = ...) -> None: ...
    def Contains(self, value: Tag) -> bool: ...
    def GetEnumerator(self) -> IEnumerator_1[Tag]: ...
    def NotDefinedByJAStudio(self) -> IReadOnlyList_1[Tag]: ...
    def RemoveTheBranch(self, branch: TagBranch) -> None: ...
    def RenameTheBranch(self, branch: TagBranch, renamed: TagBranch) -> None: ...
    def Set(self, tag: Tag) -> None: ...
    def Toggle(self, tag: Tag, on: bool) -> None: ...
    def ToStringList(self) -> List_1[str]: ...
    def Unset(self, tag: Tag) -> None: ...


class Tag:
    LevelSeparator : str
    @property
    def Name(self) -> str: ...
    @property
    def PathsOfTheBranchesItIsIn(self) -> IReadOnlyList_1[str]: ...
    @staticmethod
    def FromName(name: str) -> Tag: ...
    @staticmethod
    def WhyItCannotBeTheNameOfATag(typedName: str) -> str: ...


class TagBranch:
    def __init__(self, path: str) -> None: ...
    @property
    def LastLevel(self) -> str: ...
    @property
    def Path(self) -> str: ...
    @property
    def PathsAboveIt(self) -> IReadOnlyList_1[str]: ...
    def ATagDefinedByJAStudioIn(self, tagsInUse: IEnumerable_1[Tag]) -> Tag: ...
    def TheNameRenamedTo(self, renamed: TagBranch, tagName: str) -> str: ...
    def WithTheLastLevel(self, lastLevel: str) -> TagBranch: ...
    # Skipped Contains due to it being static, abstract and generic.

    Contains : Contains_MethodGroup
    class Contains_MethodGroup:
        @typing.overload
        def __call__(self, tagName: str) -> bool:...
        @typing.overload
        def __call__(self, tag: Tag) -> bool:...



class TagBranchRename:
    @property
    def Branch(self) -> TagBranch: ...
    @property
    def Renamed(self) -> TagBranch: ...
    @property
    def TagsInUseItMergesWith(self) -> IReadOnlyList_1[str]: ...
    @property
    def WhyItCannotBeDone(self) -> str: ...
    def ApplyTo(self, note: JPNote) -> None: ...
    @staticmethod
    def OfTheLastLevel(branch: TagBranch, typedLastLevel: str, tagsInUse: IReadOnlyCollection_1[Tag]) -> TagBranchRename: ...
    @staticmethod
    def WithItsParents(branch: TagBranch, typedPath: str, tagsInUse: IReadOnlyCollection_1[Tag]) -> TagBranchRename: ...


class Tags(abc.ABC):
    DisableKanaOnly : Tag
    PriorityFolder : str
    TTSAudio : Tag
    UsuallyKanaOnly : Tag
    @staticmethod
    def IsDefinedByJAStudio(tagName: str) -> bool: ...

    class Kanji(abc.ABC):
        HasNonPrimaryOnReadingVocab : Tag
        HasNonPrimaryOnReadingVocabWithOnlyKnownKanji : Tag
        HasPrimaryReadingWithNoStudyingVocab : Tag
        HasStudyingVocabForEachPrimaryReading : Tag
        HasStudyingVocabWithNoMatchingPrimaryReading : Tag
        InAnyVocabForm : Tag
        InVocabMainForm : Tag
        IsRadical : Tag
        IsRadicalPurely : Tag
        IsRadicalSilent : Tag
        WithNoPrimaryOnReadings : Tag
        WithNoPrimaryReadings : Tag
        WithSingleKanjiVocab : Tag
        WithSingleKanjiVocabWithDifferentReading : Tag
        WithStudyingSingleKanjiVocabWithDifferentReading : Tag
        WithStudyingVocab : Tag
        WithStudyingVocabWithPrimaryOnReading : Tag
        WithVocabWithPrimaryOnReading : Tag


    class Sentence(abc.ABC):

        class Uses(abc.ABC):
            HiddenMatches : Tag
            IncorrectMatches : Tag



    class Source(abc.ABC):
        Folder : str
        ImmersionKit : Tag
        Jamdict : Tag


    class Vocab(abc.ABC):
        HasNoStudyingSentences : Tag
        IsIchidanHidingGodanPotential : Tag
        Root : str

        class Matching(abc.ABC):

            class Uses(abc.ABC):
                PrefixIsNot : Tag
                RequiredPrefix : Tag
                SuffixIsNot : Tag
                SurfaceIsNot : Tag





class TagTree:
    @property
    def TopLevel(self) -> IReadOnlyList_1[TagTreeNode]: ...
    def WithOnlyThePathsMatching(self, typedParts: TypedTagPathParts) -> TagTree: ...


class TagTreeNode:
    @property
    def Children(self) -> IReadOnlyList_1[TagTreeNode]: ...
    @property
    def LastLevel(self) -> str: ...
    @property
    def NumberOfNotesInTheBranch(self) -> int: ...
    @property
    def TagPath(self) -> str: ...
    def PathsInTheBranch(self) -> IEnumerable_1[str]: ...


class TypedTagPathParts:
    def __init__(self, typed: str) -> None: ...
    @property
    def AreNone(self) -> bool: ...
    def AreAllIn(self, tagPath: str) -> bool: ...
    def Matching(self, tagPaths: IEnumerable_1[str]) -> IReadOnlyList_1[str]: ...

