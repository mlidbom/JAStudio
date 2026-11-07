import abc
from System.Collections.Generic import IEnumerable_1, List_1, IEnumerator_1, IReadOnlyList_1
from JAStudio.Core.Note import JPNote

class NoteTags(IEnumerable_1[Tag]):
    def __init__(self, note: JPNote, tags: List_1[str] = ...) -> None: ...
    def Contains(self, value: Tag) -> bool: ...
    def GetEnumerator(self) -> IEnumerator_1[Tag]: ...
    def NotDefinedByJAStudio(self) -> IReadOnlyList_1[Tag]: ...
    def Set(self, tag: Tag) -> None: ...
    def Toggle(self, tag: Tag, on: bool) -> None: ...
    def ToStringList(self) -> List_1[str]: ...
    def Unset(self, tag: Tag) -> None: ...


class Tag:
    @property
    def Name(self) -> str: ...
    @staticmethod
    def FromName(name: str) -> Tag: ...
    @staticmethod
    def WhyItCannotBeTheNameOfATag(typedName: str) -> str: ...


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
        IsCompositionallyTransparentCompound : Tag
        IsIchidanHidingGodanPotential : Tag
        QuestionOverridesForm : Tag
        Root : str

        class Matching(abc.ABC):
            IsInflectingWord : Tag
            IsPoisonWord : Tag
            YieldLastTokenToOverlappingCompound : Tag

            class Forbids(abc.ABC):
                AdverbHead : Tag
                AutoYielding : Tag
                DictionaryFormHead : Tag
                DictionaryFormStart : Tag
                Godan : Tag
                GodanImperative : Tag
                GodanImperativeHead : Tag
                GodanPotential : Tag
                Ichidan : Tag
                IchidanImperative : Tag
                Irrealis : Tag
                MasuStem : Tag
                PastTenseStem : Tag
                SentenceEnd : Tag
                SentenceStart : Tag
                Surface : Tag
                TeFormStemHead : Tag
                TeFormStemStart : Tag


            class Requires(abc.ABC):
                AdverbHead : Tag
                Compound : Tag
                DictionaryFormHead : Tag
                DictionaryFormStart : Tag
                FolderName : str
                Godan : Tag
                GodanImperative : Tag
                GodanImperativeHead : Tag
                GodanPotential : Tag
                Ichidan : Tag
                IchidanImperative : Tag
                Irrealis : Tag
                MasuStem : Tag
                PastTenseStem : Tag
                SentenceEnd : Tag
                SentenceStart : Tag
                SingleToken : Tag
                Surface : Tag
                TeFormStemHead : Tag
                TeFormStemStart : Tag


            class Todo(abc.ABC):
                WithPrecedingVowel : Tag


            class Uses(abc.ABC):
                PrefixIsNot : Tag
                RequiredPrefix : Tag
                SuffixIsNot : Tag
                SurfaceIsNot : Tag



        class Register(abc.ABC):
            Archaic : Tag
            Childish : Tag
            Derogatory : Tag
            Formal : Tag
            Honorific : Tag
            Humble : Tag
            Informal : Tag
            Literary : Tag
            Polite : Tag
            RoughMasculine : Tag
            Sensitive : Tag
            Slang : Tag
            SoftFeminine : Tag
            Vulgar : Tag




class TypedTagPathParts:
    def __init__(self, typed: str) -> None: ...
    def AreAllIn(self, tagPath: str) -> bool: ...
    def Matching(self, tagPaths: IEnumerable_1[str]) -> IReadOnlyList_1[str]: ...

