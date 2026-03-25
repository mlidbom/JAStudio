import abc
from System.Collections.Generic import HashSet_1, Dictionary_2, List_1
from JAStudio.Core.Note.Vocabulary import VocabNote
from System import Array_1

class Conjugator(abc.ABC):
    GodanImperativeVerbEndings : HashSet_1[str]
    GodanPotentialVerbEndingToDictionaryFormEndings : Dictionary_2[str, str]
    @staticmethod
    def ConstructRootVerbForPossiblyPotentialGodanVerbDictionaryForm(potentialVerbForm: str) -> str: ...
    @staticmethod
    def GetAStem(word: str, isIchidan: bool = ..., isGodan: bool = ...) -> str: ...
    @staticmethod
    def GetAStemVocab(vocab: VocabNote, form: str = ...) -> str: ...
    @staticmethod
    def GetEStem(word: str, isIchidan: bool = ..., isGodan: bool = ...) -> str: ...
    @staticmethod
    def GetEStemVocab(vocab: VocabNote, form: str = ...) -> str: ...
    @staticmethod
    def GetImperative(word: str, isIchidan: bool = ..., isGodan: bool = ...) -> str: ...
    @staticmethod
    def GetIStem(word: str, isIchidan: bool = ..., isGodan: bool = ...) -> str: ...
    @staticmethod
    def GetIStemVocab(vocab: VocabNote, form: str = ...) -> str: ...
    @staticmethod
    def GetTeStem(word: str, isIchidan: bool = ..., isGodan: bool = ...) -> str: ...
    @staticmethod
    def GetTeStemVocab(vocab: VocabNote, form: str = ...) -> str: ...
    @staticmethod
    def GetVocabStems(vocab: VocabNote) -> List_1[str]: ...
    @staticmethod
    def GetWordStems(word: str, isIchidanVerb: bool = ..., isGodan: bool = ...) -> List_1[str]: ...


class HepburnKanaToRomaji(abc.ABC):
    @staticmethod
    def Convert(text: str) -> str: ...


class HepburnRomajiToKana(abc.ABC):
    @staticmethod
    def ToHiragana(romaji: str) -> str: ...
    @staticmethod
    def ToKatakana(romaji: str) -> str: ...


class HiraganaChart(abc.ABC):
    ARow1 : Array_1[str]
    ARow2 : Array_1[str]
    ARow3 : Array_1[str]
    ERow1 : Array_1[str]
    ERow2 : Array_1[str]
    ERow3 : Array_1[str]
    HIndex : int
    IRow1 : Array_1[str]
    IRow2 : Array_1[str]
    IRow3 : Array_1[str]
    KIndex : int
    MIndex : int
    NIndex : int
    ORow1 : Array_1[str]
    ORow2 : Array_1[str]
    ORow3 : Array_1[str]
    RIndex : int
    SIndex : int
    TIndex : int
    URow1 : Array_1[str]
    URow2 : Array_1[str]
    URow3 : Array_1[str]
    WIndex : int
    YIndex : int


class KanaUtils(abc.ABC):
    @staticmethod
    def AnythingToHiragana(text: str) -> str: ...
    @staticmethod
    def CharacterIsHiragana(ch: str) -> bool: ...
    @staticmethod
    def CharacterIsKana(ch: str) -> bool: ...
    @staticmethod
    def CharacterIsKanji(ch: str) -> bool: ...
    @staticmethod
    def CharacterIsKatakana(ch: str) -> bool: ...
    @staticmethod
    def ExtractKanji(text: str) -> Array_1[str]: ...
    @staticmethod
    def HiraganaToKatakana(hiragana: str) -> str: ...
    @staticmethod
    def IsOnlyHiragana(text: str) -> bool: ...
    @staticmethod
    def IsOnlyKana(text: str) -> bool: ...
    @staticmethod
    def IsOnlyKatakana(text: str) -> bool: ...
    @staticmethod
    def KatakanaToHiragana(katakana: str) -> str: ...
    @staticmethod
    def PadToLength(value: str, targetLength: int) -> str: ...
    @staticmethod
    def RomajiToHiragana(romaji: str) -> str: ...
    @staticmethod
    def RomajiToKatakana(romaji: str) -> str: ...
    @staticmethod
    def Romanize(text: str) -> str: ...


class KatakanaChart(abc.ABC):
    ARow1 : Array_1[str]
    ARow2 : Array_1[str]
    ARow3 : Array_1[str]
    ERow1 : Array_1[str]
    ERow2 : Array_1[str]
    ERow3 : Array_1[str]
    HIndex : int
    IRow1 : Array_1[str]
    IRow2 : Array_1[str]
    IRow3 : Array_1[str]
    KIndex : int
    MIndex : int
    NIndex : int
    ORow1 : Array_1[str]
    ORow2 : Array_1[str]
    ORow3 : Array_1[str]
    RIndex : int
    SIndex : int
    TIndex : int
    URow1 : Array_1[str]
    URow2 : Array_1[str]
    URow3 : Array_1[str]
    WIndex : int
    YIndex : int

