import typing, abc
from JAStudio.Core.Note.EditableFields import EditableField_2, EditableTextListField_1, EditableTextField_1, EditableSetField_2, EditableFlagField_1, EditableTagsField_1, EditableTextSetField_1, IEditableField_1
from JAStudio.Core.Note.Vocabulary import VocabNote, EnteredForm, UsuallyWrittenInKanaDecision, WordReference
from System.Collections.Generic import IReadOnlyList_1
from JAStudio.Core.Note.Vocabulary.Matching import RequireForbidChoice
from System import Func_2, Action_2

class EditableFormsField(EditableField_2[VocabNote, IReadOnlyList_1[EnteredForm]]):
    def __init__(self) -> None: ...
    @property
    def ChangesHowSentencesParse(self) -> bool: ...
    @property
    def Label(self) -> str: ...
    @property
    def Name(self) -> str: ...
    @property
    def ValueType(self) -> typing.Type[typing.Any]: ...
    def Read(self, note: VocabNote) -> IReadOnlyList_1[EnteredForm]: ...
    def Write(self, note: VocabNote, value: IReadOnlyList_1[EnteredForm]) -> None: ...


class EditablePartsOfSpeechField(EditableField_2[VocabNote, IReadOnlyList_1[str]]):
    def __init__(self) -> None: ...
    @property
    def ChangesHowSentencesParse(self) -> bool: ...
    @property
    def Label(self) -> str: ...
    @property
    def Name(self) -> str: ...
    @property
    def ValueType(self) -> typing.Type[typing.Any]: ...
    def Read(self, note: VocabNote) -> IReadOnlyList_1[str]: ...
    def Write(self, note: VocabNote, value: IReadOnlyList_1[str]) -> None: ...


class EditableRequireForbidField(EditableField_2[VocabNote, RequireForbidChoice]):
    def __init__(self, name: str, label: str, read: Func_2[VocabNote, RequireForbidChoice], write: Action_2[VocabNote, RequireForbidChoice]) -> None: ...
    @property
    def ChangesHowSentencesParse(self) -> bool: ...
    @property
    def Label(self) -> str: ...
    @property
    def Name(self) -> str: ...
    @property
    def ValueType(self) -> typing.Type[typing.Any]: ...
    def Read(self, note: VocabNote) -> RequireForbidChoice: ...
    def Write(self, note: VocabNote, value: RequireForbidChoice) -> None: ...


class EditableUsuallyWrittenInKanaField(EditableField_2[VocabNote, UsuallyWrittenInKanaDecision]):
    def __init__(self) -> None: ...
    @property
    def ChangesHowSentencesParse(self) -> bool: ...
    @property
    def Label(self) -> str: ...
    @property
    def Name(self) -> str: ...
    @property
    def ValueType(self) -> typing.Type[typing.Any]: ...
    def Read(self, note: VocabNote) -> UsuallyWrittenInKanaDecision: ...
    def Write(self, note: VocabNote, value: UsuallyWrittenInKanaDecision) -> None: ...


class VocabEditableFields(abc.ABC):
    AlternativeCompoundParts : EditableTextListField_1[VocabNote]
    Answer : EditableTextField_1[VocabNote]
    Antonyms : EditableSetField_2[VocabNote, WordReference]
    Archaic : EditableFlagField_1[VocabNote]
    Childish : EditableFlagField_1[VocabNote]
    CompoundParts : EditableTextListField_1[VocabNote]
    ConfusedWith : EditableSetField_2[VocabNote, WordReference]
    DerivedFrom : EditableTextField_1[VocabNote]
    Derogatory : EditableFlagField_1[VocabNote]
    DictionaryFormStart : EditableRequireForbidField
    Disambiguation : EditableTextField_1[VocabNote]
    ErgativeTwins : EditableSetField_2[VocabNote, WordReference]
    Explanation : EditableTextField_1[VocabNote]
    Formal : EditableFlagField_1[VocabNote]
    Forms : EditableFormsField
    Honorific : EditableFlagField_1[VocabNote]
    Humble : EditableFlagField_1[VocabNote]
    Informal : EditableFlagField_1[VocabNote]
    IsCompositionallyTransparentCompound : EditableFlagField_1[VocabNote]
    IsInflectingWord : EditableFlagField_1[VocabNote]
    IsPoisonWord : EditableFlagField_1[VocabNote]
    Literary : EditableFlagField_1[VocabNote]
    LongExplanation : EditableTextField_1[VocabNote]
    MatchCondition : EditableTextField_1[VocabNote]
    MatchWithPrecedingVowel : EditableFlagField_1[VocabNote]
    Mnemonic : EditableTextField_1[VocabNote]
    PartsOfSpeech : EditablePartsOfSpeechField
    PerfectSynonyms : EditableSetField_2[VocabNote, WordReference]
    Polite : EditableFlagField_1[VocabNote]
    Question : EditableTextField_1[VocabNote]
    QuestionOverridesForm : EditableFlagField_1[VocabNote]
    Readings : EditableTextListField_1[VocabNote]
    RoughMasculine : EditableFlagField_1[VocabNote]
    SeeAlso : EditableSetField_2[VocabNote, WordReference]
    Sensitive : EditableFlagField_1[VocabNote]
    Slang : EditableFlagField_1[VocabNote]
    SoftFeminine : EditableFlagField_1[VocabNote]
    SourceAnswer : EditableTextField_1[VocabNote]
    SourceMnemonic : EditableTextField_1[VocabNote]
    SourceReadingMnemonic : EditableTextField_1[VocabNote]
    Synonyms : EditableSetField_2[VocabNote, WordReference]
    Tags : EditableTagsField_1[VocabNote]
    TechnicalNotes : EditableTextField_1[VocabNote]
    UsuallyWrittenInKana : EditableUsuallyWrittenInKanaField
    Vulgar : EditableFlagField_1[VocabNote]
    YieldLastToken : EditableRequireForbidField
    YieldToSurface : EditableTextSetField_1[VocabNote]
    @classmethod
    @property
    def All(cls) -> IReadOnlyList_1[IEditableField_1[VocabNote]]: ...

