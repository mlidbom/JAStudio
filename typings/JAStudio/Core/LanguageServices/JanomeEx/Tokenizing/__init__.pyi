import typing, abc
from System.Collections.Generic import HashSet_1, List_1
from JAStudio.Core.Note.Collection import VocabCollection
from JAStudio.Core.LanguageServices.JamdictEx import DictLookup

class IAnalysisToken(abc.ABC):
    @property
    def BaseForm(self) -> str: ...
    @property
    def HasTeFormStem(self) -> bool: ...
    @property
    def IsAdverb(self) -> bool: ...
    @property
    def IsDictionaryVerbFormStem(self) -> bool: ...
    @property
    def IsDictionaryVerbInflection(self) -> bool: ...
    @property
    def IsEndOfStatement(self) -> bool: ...
    @property
    def IsGodanImperativeInflection(self) -> bool: ...
    @property
    def IsGodanImperativeStem(self) -> bool: ...
    @property
    def IsGodanPotentialInflection(self) -> bool: ...
    @property
    def IsGodanPotentialStem(self) -> bool: ...
    @property
    def IsGodanVerb(self) -> bool: ...
    @property
    def IsIchidanImperativeInflection(self) -> bool: ...
    @property
    def IsIchidanImperativeStem(self) -> bool: ...
    @property
    def IsIchidanVerb(self) -> bool: ...
    @property
    def IsInflectableWord(self) -> bool: ...
    @property
    def IsIrrealis(self) -> bool: ...
    @property
    def IsMasuStem(self) -> bool: ...
    @property
    def IsNonWordCharacter(self) -> bool: ...
    @property
    def IsPastTenseMarker(self) -> bool: ...
    @property
    def IsPastTenseStem(self) -> bool: ...
    @property
    def SourceToken(self) -> JNToken: ...
    @property
    def Surface(self) -> str: ...


class InflectionForm:
    def __init__(self, name: str, description: str) -> None: ...
    @property
    def Description(self) -> str: ...
    @property
    def Name(self) -> str: ...
    def Equals(self, obj: typing.Any) -> bool: ...
    def GetHashCode(self) -> int: ...
    def ToString(self) -> str: ...


class InflectionForms(abc.ABC):
    Unknown : InflectionForm
    @staticmethod
    def GetByName(name: str) -> InflectionForm: ...

    class Basic(abc.ABC):
        Classical : InflectionForm
        DictionaryForm : InflectionForm
        Euphonic : InflectionForm
        Gemination : InflectionForm


    class Continuative(abc.ABC):
        DeConnection : InflectionForm
        GozaiConnection : InflectionForm
        NiConnection : InflectionForm
        RenyoukeiMasuStem : InflectionForm
        TaConnection : InflectionForm
        TeConnection : InflectionForm
        TeConnectionForms : HashSet_1[InflectionForm]


    class Hypothetical(abc.ABC):
        Contraction1 : InflectionForm
        Contraction2 : InflectionForm
        GeneralHypotheticalKateikei : InflectionForm


    class ImperativeMeireikei(abc.ABC):
        E : InflectionForm
        GodanForms : HashSet_1[InflectionForm]
        I : InflectionForm
        IchidanForms : HashSet_1[InflectionForm]
        Ro : InflectionForm
        Yo : InflectionForm


    class Irrealis(abc.ABC):
        AllForms : HashSet_1[InflectionForm]
        GeneralIrrealisMizenkei : InflectionForm
        NuConnection : InflectionForm
        ReruConnection : InflectionForm
        SpecialIrrealis : InflectionForm
        UConnection : InflectionForm


    class Misc(abc.ABC):
        GaruConnection : InflectionForm


    class NounConnection(abc.ABC):
        GeneralNounConnection : InflectionForm
        Special1 : InflectionForm
        Special2 : InflectionForm



class InflectionType:
    def __init__(self, name: str, description: str, baseValue: str) -> None: ...
    @property
    def Base(self) -> str: ...
    @property
    def Description(self) -> str: ...
    @property
    def Name(self) -> str: ...
    def Equals(self, obj: typing.Any) -> bool: ...
    def GetHashCode(self) -> int: ...
    def ToString(self) -> str: ...


class InflectionTypes(abc.ABC):
    Unknown : InflectionType
    @staticmethod
    def GetByName(name: str) -> InflectionType: ...

    class Adjective(abc.ABC):
        AuoEnding : InflectionType
        Base : str
        IEnding : InflectionType
        Ii : InflectionType


    class Bungo(abc.ABC):
        Base : str
        Beshi : InflectionType
        Gotoshi : InflectionType
        Keri : InflectionType
        Ki : InflectionType
        Nari : InflectionType
        Ri : InflectionType
        Ru : InflectionType


    class Godan(abc.ABC):
        Base : str
        Bu : InflectionType
        GuEnding : InflectionType
        KuGemination : InflectionType
        KuGeminationYuku : InflectionType
        KuISound : InflectionType
        Mu : InflectionType
        Nu : InflectionType
        Ru : InflectionType
        RuEndingAru : InflectionType
        RuSpecial : InflectionType
        Su : InflectionType
        Tsu : InflectionType
        UGemination : InflectionType
        UUSound : InflectionType


    class Ichidan(abc.ABC):
        Base : str
        Eru : InflectionType
        Kureru : InflectionType
        Regular : InflectionType


    class Kahen(abc.ABC):
        Base : str
        KuruKana : InflectionType
        KuruKanji : InflectionType


    class Nidan(abc.ABC):
        Base : str
        LowerDa : InflectionType
        LowerTa : InflectionType


    class Other(abc.ABC):
        Indeclinable : InflectionType


    class Ruhen(abc.ABC):
        Base : str
        RaHen : InflectionType


    class Sahen(abc.ABC):
        Base : str
        Suru : InflectionType
        SuruCompound : InflectionType
        Zuru : InflectionType


    class Special(abc.ABC):
        Base : str
        Da : InflectionType
        Desu : InflectionType
        Ja : InflectionType
        Masu : InflectionType
        Nai : InflectionType
        Nu : InflectionType
        Ta : InflectionType
        Tai : InflectionType
        Ya : InflectionType


    class Yodan(abc.ABC):
        BaEnding : InflectionType
        Base : str
        HaEnding : InflectionType



class JNPartsOfSpeech:
    @property
    def Level1(self) -> PartOfSpeechDescription: ...
    @property
    def Level2(self) -> PartOfSpeechDescription: ...
    @property
    def Level3(self) -> PartOfSpeechDescription: ...
    @property
    def Level4(self) -> PartOfSpeechDescription: ...
    @staticmethod
    def Fetch(unparsed: str) -> JNPartsOfSpeech: ...
    def IsNonWordCharacter(self) -> bool: ...
    def IsNoun(self) -> bool: ...
    def ToString(self) -> str: ...


class JNPOS(abc.ABC):
    BoundAuxiliary : JNPartsOfSpeech
    Conjunction : JNPartsOfSpeech
    Filler : JNPartsOfSpeech
    Interjection : JNPartsOfSpeech
    PreNounAdjectival : JNPartsOfSpeech

    class Adjective(abc.ABC):
        AllTypes : HashSet_1[JNPartsOfSpeech]
        Dependent : JNPartsOfSpeech
        Independent : JNPartsOfSpeech
        Suffix : JNPartsOfSpeech


    class Adverb(abc.ABC):
        General : JNPartsOfSpeech
        ParticleConnection : JNPartsOfSpeech


    class Noun(abc.ABC):
        Adverbial : JNPartsOfSpeech
        AuxiliaryVerb : JNPartsOfSpeech
        Conjunctive : JNPartsOfSpeech
        General : JNPartsOfSpeech
        NaAdjectiveStem : JNPartsOfSpeech
        NegativeAdjectiveStem : JNPartsOfSpeech
        Numeric : JNPartsOfSpeech
        QuotedCharacterString : JNPartsOfSpeech
        SuruVerb : JNPartsOfSpeech

        class Dependent(abc.ABC):
            Adverbial : JNPartsOfSpeech
            AuxiliaryVerbStem : JNPartsOfSpeech
            General : JNPartsOfSpeech
            NaAdjectiveStem : JNPartsOfSpeech


        class Pronoun(abc.ABC):
            Contracted : JNPartsOfSpeech
            General : JNPartsOfSpeech


        class ProperNoun(abc.ABC):
            General : JNPartsOfSpeech
            Organization : JNPartsOfSpeech

            class Location(abc.ABC):
                Country : JNPartsOfSpeech
                General : JNPartsOfSpeech


            class Person(abc.ABC):
                Firstname : JNPartsOfSpeech
                General : JNPartsOfSpeech
                Surname : JNPartsOfSpeech



        class Special(abc.ABC):
            AuxiliaryVerbStem : JNPartsOfSpeech


        class Suffix(abc.ABC):
            Adverbial : JNPartsOfSpeech
            AuxiliaryVerbStem : JNPartsOfSpeech
            Counter : JNPartsOfSpeech
            General : JNPartsOfSpeech
            NaAdjectiveStem : JNPartsOfSpeech
            PersonsName : JNPartsOfSpeech
            Region : JNPartsOfSpeech
            Special : JNPartsOfSpeech
            SuruVerbConnection : JNPartsOfSpeech



    class Other(abc.ABC):
        Interjection : JNPartsOfSpeech


    class Particle(abc.ABC):
        Adnominalization : JNPartsOfSpeech
        Adverbial : JNPartsOfSpeech
        AdverbialCoordinatingEnding : JNPartsOfSpeech
        Adverbialization : JNPartsOfSpeech
        Binding : JNPartsOfSpeech
        Conjunctive : JNPartsOfSpeech
        CoordinatingConjunction : JNPartsOfSpeech
        SentenceEnding : JNPartsOfSpeech
        Special : JNPartsOfSpeech

        class CaseMarking(abc.ABC):
            Compound : JNPartsOfSpeech
            General : JNPartsOfSpeech
            Quotation : JNPartsOfSpeech



    class Prefix(abc.ABC):
        Adjective : JNPartsOfSpeech
        Noun : JNPartsOfSpeech
        Number : JNPartsOfSpeech
        VerbConnective : JNPartsOfSpeech


    class Symbol(abc.ABC):
        Alphabet : JNPartsOfSpeech
        ClosingBracket : JNPartsOfSpeech
        Comma : JNPartsOfSpeech
        General : JNPartsOfSpeech
        OpeningBracket : JNPartsOfSpeech
        Period : JNPartsOfSpeech
        Space : JNPartsOfSpeech


    class Verb(abc.ABC):
        AllTypes : HashSet_1[JNPartsOfSpeech]
        Dependent : JNPartsOfSpeech
        Independent : JNPartsOfSpeech
        Suffix : JNPartsOfSpeech



class JNToken(IAnalysisToken):
    def __init__(self, partsOfSpeech: JNPartsOfSpeech, baseForm: str, surface: str, inflectionType: typing.Any = ..., inflectedForm: typing.Any = ..., reading: str = ..., phonetic: str = ..., nodeType: str = ...) -> None: ...
    SplitterTokenText : str
    @property
    def BaseForm(self) -> str: ...
    @property
    def HasTeFormStem(self) -> bool: ...
    @property
    def InflectedForm(self) -> InflectionForm: ...
    @property
    def InflectionType(self) -> InflectionType: ...
    @property
    def IsAdjective(self) -> bool: ...
    @property
    def IsAdverb(self) -> bool: ...
    @property
    def IsDictionaryVerbFormStem(self) -> bool: ...
    @property
    def IsDictionaryVerbInflection(self) -> bool: ...
    @property
    def IsEndOfStatement(self) -> bool: ...
    @property
    def IsGodanImperativeInflection(self) -> bool: ...
    @property
    def IsGodanImperativeStem(self) -> bool: ...
    @property
    def IsGodanPotentialInflection(self) -> bool: ...
    @property
    def IsGodanPotentialStem(self) -> bool: ...
    @property
    def IsGodanVerb(self) -> bool: ...
    @property
    def IsIchidanImperativeInflection(self) -> bool: ...
    @property
    def IsIchidanImperativeStem(self) -> bool: ...
    @property
    def IsIchidanVerb(self) -> bool: ...
    @property
    def IsInflectableWord(self) -> bool: ...
    @property
    def IsIrrealis(self) -> bool: ...
    @property
    def IsKuruVerb(self) -> bool: ...
    @property
    def IsMasuStem(self) -> bool: ...
    @property
    def IsNonWordCharacter(self) -> bool: ...
    @property
    def IsPastTenseMarker(self) -> bool: ...
    @property
    def IsPastTenseStem(self) -> bool: ...
    @property
    def IsSuruVerb(self) -> bool: ...
    @property
    def IsTeForm(self) -> bool: ...
    @property
    def IsVerb(self) -> bool: ...
    @property
    def Next(self) -> JNToken: ...
    @Next.setter
    def Next(self, value: JNToken) -> JNToken: ...
    @property
    def NodeType(self) -> str: ...
    @property
    def PartsOfSpeech(self) -> JNPartsOfSpeech: ...
    @property
    def Phonetic(self) -> str: ...
    @property
    def Previous(self) -> JNToken: ...
    @Previous.setter
    def Previous(self, value: JNToken) -> JNToken: ...
    @property
    def Reading(self) -> str: ...
    @property
    def SourceToken(self) -> JNToken: ...
    @property
    def Surface(self) -> str: ...
    def CannotFollowIchidanStem(self) -> bool: ...
    def Equals(self, obj: typing.Any) -> bool: ...
    def GetHashCode(self) -> int: ...
    def IsDictionaryForm(self) -> bool: ...
    def IsMoreLikelyToFollowImperativeThanIchidanStem(self) -> bool: ...
    def IsProgressiveForm(self) -> bool: ...
    def IsTFormMarker(self) -> bool: ...
    def IsValidGodanPotentialFormInflection(self) -> bool: ...
    def ToString(self) -> str: ...


class JNTokenizedText:
    def __init__(self, text: str, tokens: List_1[JNToken], rawTokens: typing.Any = ...) -> None: ...
    @property
    def RawTokens(self) -> typing.Any: ...
    @property
    def Text(self) -> str: ...
    @property
    def Tokens(self) -> List_1[JNToken]: ...
    def PreProcess(self, vocab: VocabCollection, dictLookup: DictLookup) -> List_1[IAnalysisToken]: ...


class JNTokenizer:
    Version : str
    @staticmethod
    def GetInstance() -> JNTokenizer: ...
    def Tokenize(self, text: str, cachedSerializedTokens: str = ...) -> JNTokenizeResult: ...


class JNTokenizeResult:
    def __init__(self, tokenizedText: JNTokenizedText, serializedTokens: str) -> None: ...
    @property
    def SerializedTokens(self) -> str: ...
    @property
    def TokenizedText(self) -> JNTokenizedText: ...


class PartOfSpeechDescription:
    def __init__(self, japanese: str, english: str, explanation: str) -> None: ...
    @property
    def English(self) -> str: ...
    @property
    def Explanation(self) -> str: ...
    @property
    def Japanese(self) -> str: ...
    def ToString(self) -> str: ...


class SplitTokenBase(IAnalysisToken):
    @property
    def BaseForm(self) -> str: ...
    @property
    def HasTeFormStem(self) -> bool: ...
    @property
    def IsAdverb(self) -> bool: ...
    @property
    def IsDictionaryVerbFormStem(self) -> bool: ...
    @property
    def IsDictionaryVerbInflection(self) -> bool: ...
    @property
    def IsEndOfStatement(self) -> bool: ...
    @property
    def IsGodanImperativeInflection(self) -> bool: ...
    @property
    def IsGodanImperativeStem(self) -> bool: ...
    @property
    def IsGodanPotentialInflection(self) -> bool: ...
    @property
    def IsGodanPotentialStem(self) -> bool: ...
    @property
    def IsGodanVerb(self) -> bool: ...
    @property
    def IsIchidanImperativeInflection(self) -> bool: ...
    @property
    def IsIchidanImperativeStem(self) -> bool: ...
    @property
    def IsIchidanVerb(self) -> bool: ...
    @property
    def IsInflectableWord(self) -> bool: ...
    @property
    def IsIrrealis(self) -> bool: ...
    @property
    def IsMasuStem(self) -> bool: ...
    @property
    def IsNonWordCharacter(self) -> bool: ...
    @property
    def IsPastTenseMarker(self) -> bool: ...
    @property
    def IsPastTenseStem(self) -> bool: ...
    @property
    def SourceToken(self) -> JNToken: ...
    @property
    def Surface(self) -> str: ...
    def ToString(self) -> str: ...

