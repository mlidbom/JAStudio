import typing, abc
from JAStudio.Core.LanguageServices.JanomeEx.Tokenizing import IAnalysisToken, JNToken
from JAStudio.Core.Note.Vocabulary.Matching.Conditions import Condition_1, Condition, ConditionSubject_2, Fact_1, ConditionLevel_1, IConditionPath_2
from System import IEquatable_1, Array_1
from System.Collections.Generic import IReadOnlyList_1
from JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts import TextSet

class EmptyToken(IAnalysisToken):
    @property
    def BaseForm(self) -> str: ...
    @property
    def HasTeFormStem(self) -> bool: ...
    @classmethod
    @property
    def Instance(cls) -> EmptyToken: ...
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
    def IsTheTokenizersOwn(self) -> bool: ...
    @property
    def SourceToken(self) -> JNToken: ...
    @property
    def Surface(self) -> str: ...
    def ToString(self) -> str: ...


class InflectedFormIsIn(Condition_1[IAnalysisToken], IEquatable_1[InflectedFormIsIn]):
    @property
    def Children(self) -> IReadOnlyList_1[Condition]: ...
    @property
    def IsEmpty(self) -> bool: ...
    @property
    def Names(self) -> TextSet: ...
    def GetHashCode(self) -> int: ...
    def IsMetBy(self, subject: IAnalysisToken) -> bool: ...
    def __eq__(self, left: InflectedFormIsIn, right: InflectedFormIsIn) -> bool: ...
    def __ne__(self, left: InflectedFormIsIn, right: InflectedFormIsIn) -> bool: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup
    class Equals_MethodGroup:
        @typing.overload
        def __call__(self, other: InflectedFormIsIn) -> bool:...
        @typing.overload
        def __call__(self, other: Condition_1[IAnalysisToken]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class InflectedFormTest_GenericClasses(abc.ABCMeta):
    Generic_InflectedFormTest_GenericClasses_InflectedFormTest_1_TFrom = typing.TypeVar('Generic_InflectedFormTest_GenericClasses_InflectedFormTest_1_TFrom')
    def __getitem__(self, types : typing.Type[Generic_InflectedFormTest_GenericClasses_InflectedFormTest_1_TFrom]) -> typing.Type[InflectedFormTest_1[Generic_InflectedFormTest_GenericClasses_InflectedFormTest_1_TFrom]]: ...

InflectedFormTest : InflectedFormTest_GenericClasses

InflectedFormTest_1_TFrom = typing.TypeVar('InflectedFormTest_1_TFrom')
class InflectedFormTest_1(typing.Generic[InflectedFormTest_1_TFrom]):
    def In(self, names: Array_1[str]) -> Condition_1[InflectedFormTest_1_TFrom]: ...


class InflectionTypeIsIn(Condition_1[IAnalysisToken], IEquatable_1[InflectionTypeIsIn]):
    @property
    def Children(self) -> IReadOnlyList_1[Condition]: ...
    @property
    def IsEmpty(self) -> bool: ...
    @property
    def Names(self) -> TextSet: ...
    def GetHashCode(self) -> int: ...
    def IsMetBy(self, subject: IAnalysisToken) -> bool: ...
    def __eq__(self, left: InflectionTypeIsIn, right: InflectionTypeIsIn) -> bool: ...
    def __ne__(self, left: InflectionTypeIsIn, right: InflectionTypeIsIn) -> bool: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup
    class Equals_MethodGroup:
        @typing.overload
        def __call__(self, other: InflectionTypeIsIn) -> bool:...
        @typing.overload
        def __call__(self, other: Condition_1[IAnalysisToken]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class InflectionTypeTest_GenericClasses(abc.ABCMeta):
    Generic_InflectionTypeTest_GenericClasses_InflectionTypeTest_1_TFrom = typing.TypeVar('Generic_InflectionTypeTest_GenericClasses_InflectionTypeTest_1_TFrom')
    def __getitem__(self, types : typing.Type[Generic_InflectionTypeTest_GenericClasses_InflectionTypeTest_1_TFrom]) -> typing.Type[InflectionTypeTest_1[Generic_InflectionTypeTest_GenericClasses_InflectionTypeTest_1_TFrom]]: ...

InflectionTypeTest : InflectionTypeTest_GenericClasses

InflectionTypeTest_1_TFrom = typing.TypeVar('InflectionTypeTest_1_TFrom')
class InflectionTypeTest_1(typing.Generic[InflectionTypeTest_1_TFrom]):
    def In(self, names: Array_1[str]) -> Condition_1[InflectionTypeTest_1_TFrom]: ...


class PartOfSpeechIs(Condition_1[IAnalysisToken], IEquatable_1[PartOfSpeechIs]):
    @property
    def Children(self) -> IReadOnlyList_1[Condition]: ...
    @property
    def IsEmpty(self) -> bool: ...
    @property
    def JapaneseLevels(self) -> str: ...
    def GetHashCode(self) -> int: ...
    def IsMetBy(self, subject: IAnalysisToken) -> bool: ...
    def __eq__(self, left: PartOfSpeechIs, right: PartOfSpeechIs) -> bool: ...
    def __ne__(self, left: PartOfSpeechIs, right: PartOfSpeechIs) -> bool: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup
    class Equals_MethodGroup:
        @typing.overload
        def __call__(self, other: PartOfSpeechIs) -> bool:...
        @typing.overload
        def __call__(self, other: Condition_1[IAnalysisToken]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class PartOfSpeechTest_GenericClasses(abc.ABCMeta):
    Generic_PartOfSpeechTest_GenericClasses_PartOfSpeechTest_1_TFrom = typing.TypeVar('Generic_PartOfSpeechTest_GenericClasses_PartOfSpeechTest_1_TFrom')
    def __getitem__(self, types : typing.Type[Generic_PartOfSpeechTest_GenericClasses_PartOfSpeechTest_1_TFrom]) -> typing.Type[PartOfSpeechTest_1[Generic_PartOfSpeechTest_GenericClasses_PartOfSpeechTest_1_TFrom]]: ...

PartOfSpeechTest : PartOfSpeechTest_GenericClasses

PartOfSpeechTest_1_TFrom = typing.TypeVar('PartOfSpeechTest_1_TFrom')
class PartOfSpeechTest_1(typing.Generic[PartOfSpeechTest_1_TFrom]):
    def Is(self, japaneseLevels: Array_1[str]) -> Condition_1[PartOfSpeechTest_1_TFrom]: ...


class TokenConditions(abc.ABC):
    BaseForm : ConditionSubject_2[IAnalysisToken, str]
    EndsAStatement : Fact_1[IAnalysisToken]
    Exists : Fact_1[IAnalysisToken]
    HasATeFormStem : Fact_1[IAnalysisToken]
    IsADictionaryVerbFormStem : Fact_1[IAnalysisToken]
    IsADictionaryVerbInflection : Fact_1[IAnalysisToken]
    IsAGodanImperativeInflection : Fact_1[IAnalysisToken]
    IsAGodanImperativeStem : Fact_1[IAnalysisToken]
    IsAGodanPotentialInflection : Fact_1[IAnalysisToken]
    IsAGodanPotentialStem : Fact_1[IAnalysisToken]
    IsAGodanVerb : Fact_1[IAnalysisToken]
    IsAMasuStem : Fact_1[IAnalysisToken]
    IsAnAdverb : Fact_1[IAnalysisToken]
    IsAnIchidanImperativeInflection : Fact_1[IAnalysisToken]
    IsAnIchidanImperativeStem : Fact_1[IAnalysisToken]
    IsAnIchidanVerb : Fact_1[IAnalysisToken]
    IsAnInflectableWord : Fact_1[IAnalysisToken]
    IsANonWordCharacter : Fact_1[IAnalysisToken]
    IsAPastTenseMarker : Fact_1[IAnalysisToken]
    IsAPastTenseStem : Fact_1[IAnalysisToken]
    IsInAnIrrealisForm : Fact_1[IAnalysisToken]
    Level : ConditionLevel_1[IAnalysisToken]
    Reading : ConditionSubject_2[IAnalysisToken, str]
    Surface : ConditionSubject_2[IAnalysisToken, str]
    @classmethod
    @property
    def InflectedForm(cls) -> InflectedFormTest_1[IAnalysisToken]: ...
    @classmethod
    @property
    def InflectionType(cls) -> InflectionTypeTest_1[IAnalysisToken]: ...
    @classmethod
    @property
    def PartOfSpeech(cls) -> PartOfSpeechTest_1[IAnalysisToken]: ...


class TokenConditionsInCode(abc.ABC):
    # Skipped AllOf due to it being static, abstract and generic.

    AllOf : AllOf_MethodGroup
    class AllOf_MethodGroup:
        def __getitem__(self, t:typing.Type[AllOf_1_T1]) -> AllOf_1[AllOf_1_T1]: ...

        AllOf_1_T1 = typing.TypeVar('AllOf_1_T1')
        class AllOf_1(typing.Generic[AllOf_1_T1]):
            AllOf_1_TFrom = TokenConditionsInCode.AllOf_MethodGroup.AllOf_1_T1
            def __call__(self, token: IConditionPath_2[AllOf_1_TFrom, IAnalysisToken], conditions: Array_1[Condition_1[IAnalysisToken]]) -> Condition_1[AllOf_1_TFrom]:...


    # Skipped AnyOf due to it being static, abstract and generic.

    AnyOf : AnyOf_MethodGroup
    class AnyOf_MethodGroup:
        def __getitem__(self, t:typing.Type[AnyOf_1_T1]) -> AnyOf_1[AnyOf_1_T1]: ...

        AnyOf_1_T1 = typing.TypeVar('AnyOf_1_T1')
        class AnyOf_1(typing.Generic[AnyOf_1_T1]):
            AnyOf_1_TFrom = TokenConditionsInCode.AnyOf_MethodGroup.AnyOf_1_T1
            def __call__(self, token: IConditionPath_2[AnyOf_1_TFrom, IAnalysisToken], conditions: Array_1[Condition_1[IAnalysisToken]]) -> Condition_1[AnyOf_1_TFrom]:...


    # Skipped get_BaseForm due to it being static, abstract and generic.

    get_BaseForm : get_BaseForm_MethodGroup
    class get_BaseForm_MethodGroup:
        def __getitem__(self, t:typing.Type[get_BaseForm_1_T1]) -> get_BaseForm_1[get_BaseForm_1_T1]: ...

        get_BaseForm_1_T1 = typing.TypeVar('get_BaseForm_1_T1')
        class get_BaseForm_1(typing.Generic[get_BaseForm_1_T1]):
            get_BaseForm_1_TFrom = TokenConditionsInCode.get_BaseForm_MethodGroup.get_BaseForm_1_T1
            def __call__(self, token: IConditionPath_2[get_BaseForm_1_TFrom, IAnalysisToken]) -> IConditionPath_2[get_BaseForm_1_TFrom, str]:...


    # Skipped get_EndsAStatement due to it being static, abstract and generic.

    get_EndsAStatement : get_EndsAStatement_MethodGroup
    class get_EndsAStatement_MethodGroup:
        def __getitem__(self, t:typing.Type[get_EndsAStatement_1_T1]) -> get_EndsAStatement_1[get_EndsAStatement_1_T1]: ...

        get_EndsAStatement_1_T1 = typing.TypeVar('get_EndsAStatement_1_T1')
        class get_EndsAStatement_1(typing.Generic[get_EndsAStatement_1_T1]):
            get_EndsAStatement_1_TFrom = TokenConditionsInCode.get_EndsAStatement_MethodGroup.get_EndsAStatement_1_T1
            def __call__(self, token: IConditionPath_2[get_EndsAStatement_1_TFrom, IAnalysisToken]) -> Condition_1[get_EndsAStatement_1_TFrom]:...


    # Skipped get_Exists due to it being static, abstract and generic.

    get_Exists : get_Exists_MethodGroup
    class get_Exists_MethodGroup:
        def __getitem__(self, t:typing.Type[get_Exists_1_T1]) -> get_Exists_1[get_Exists_1_T1]: ...

        get_Exists_1_T1 = typing.TypeVar('get_Exists_1_T1')
        class get_Exists_1(typing.Generic[get_Exists_1_T1]):
            get_Exists_1_TFrom = TokenConditionsInCode.get_Exists_MethodGroup.get_Exists_1_T1
            def __call__(self, token: IConditionPath_2[get_Exists_1_TFrom, IAnalysisToken]) -> Condition_1[get_Exists_1_TFrom]:...


    # Skipped get_HasATeFormStem due to it being static, abstract and generic.

    get_HasATeFormStem : get_HasATeFormStem_MethodGroup
    class get_HasATeFormStem_MethodGroup:
        def __getitem__(self, t:typing.Type[get_HasATeFormStem_1_T1]) -> get_HasATeFormStem_1[get_HasATeFormStem_1_T1]: ...

        get_HasATeFormStem_1_T1 = typing.TypeVar('get_HasATeFormStem_1_T1')
        class get_HasATeFormStem_1(typing.Generic[get_HasATeFormStem_1_T1]):
            get_HasATeFormStem_1_TFrom = TokenConditionsInCode.get_HasATeFormStem_MethodGroup.get_HasATeFormStem_1_T1
            def __call__(self, token: IConditionPath_2[get_HasATeFormStem_1_TFrom, IAnalysisToken]) -> Condition_1[get_HasATeFormStem_1_TFrom]:...


    # Skipped get_InflectedForm due to it being static, abstract and generic.

    get_InflectedForm : get_InflectedForm_MethodGroup
    class get_InflectedForm_MethodGroup:
        def __getitem__(self, t:typing.Type[get_InflectedForm_1_T1]) -> get_InflectedForm_1[get_InflectedForm_1_T1]: ...

        get_InflectedForm_1_T1 = typing.TypeVar('get_InflectedForm_1_T1')
        class get_InflectedForm_1(typing.Generic[get_InflectedForm_1_T1]):
            get_InflectedForm_1_TFrom = TokenConditionsInCode.get_InflectedForm_MethodGroup.get_InflectedForm_1_T1
            def __call__(self, token: IConditionPath_2[get_InflectedForm_1_TFrom, IAnalysisToken]) -> InflectedFormTest_1[get_InflectedForm_1_TFrom]:...


    # Skipped get_InflectionType due to it being static, abstract and generic.

    get_InflectionType : get_InflectionType_MethodGroup
    class get_InflectionType_MethodGroup:
        def __getitem__(self, t:typing.Type[get_InflectionType_1_T1]) -> get_InflectionType_1[get_InflectionType_1_T1]: ...

        get_InflectionType_1_T1 = typing.TypeVar('get_InflectionType_1_T1')
        class get_InflectionType_1(typing.Generic[get_InflectionType_1_T1]):
            get_InflectionType_1_TFrom = TokenConditionsInCode.get_InflectionType_MethodGroup.get_InflectionType_1_T1
            def __call__(self, token: IConditionPath_2[get_InflectionType_1_TFrom, IAnalysisToken]) -> InflectionTypeTest_1[get_InflectionType_1_TFrom]:...


    # Skipped get_IsADictionaryVerbFormStem due to it being static, abstract and generic.

    get_IsADictionaryVerbFormStem : get_IsADictionaryVerbFormStem_MethodGroup
    class get_IsADictionaryVerbFormStem_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsADictionaryVerbFormStem_1_T1]) -> get_IsADictionaryVerbFormStem_1[get_IsADictionaryVerbFormStem_1_T1]: ...

        get_IsADictionaryVerbFormStem_1_T1 = typing.TypeVar('get_IsADictionaryVerbFormStem_1_T1')
        class get_IsADictionaryVerbFormStem_1(typing.Generic[get_IsADictionaryVerbFormStem_1_T1]):
            get_IsADictionaryVerbFormStem_1_TFrom = TokenConditionsInCode.get_IsADictionaryVerbFormStem_MethodGroup.get_IsADictionaryVerbFormStem_1_T1
            def __call__(self, token: IConditionPath_2[get_IsADictionaryVerbFormStem_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsADictionaryVerbFormStem_1_TFrom]:...


    # Skipped get_IsADictionaryVerbInflection due to it being static, abstract and generic.

    get_IsADictionaryVerbInflection : get_IsADictionaryVerbInflection_MethodGroup
    class get_IsADictionaryVerbInflection_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsADictionaryVerbInflection_1_T1]) -> get_IsADictionaryVerbInflection_1[get_IsADictionaryVerbInflection_1_T1]: ...

        get_IsADictionaryVerbInflection_1_T1 = typing.TypeVar('get_IsADictionaryVerbInflection_1_T1')
        class get_IsADictionaryVerbInflection_1(typing.Generic[get_IsADictionaryVerbInflection_1_T1]):
            get_IsADictionaryVerbInflection_1_TFrom = TokenConditionsInCode.get_IsADictionaryVerbInflection_MethodGroup.get_IsADictionaryVerbInflection_1_T1
            def __call__(self, token: IConditionPath_2[get_IsADictionaryVerbInflection_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsADictionaryVerbInflection_1_TFrom]:...


    # Skipped get_IsAGodanImperativeInflection due to it being static, abstract and generic.

    get_IsAGodanImperativeInflection : get_IsAGodanImperativeInflection_MethodGroup
    class get_IsAGodanImperativeInflection_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAGodanImperativeInflection_1_T1]) -> get_IsAGodanImperativeInflection_1[get_IsAGodanImperativeInflection_1_T1]: ...

        get_IsAGodanImperativeInflection_1_T1 = typing.TypeVar('get_IsAGodanImperativeInflection_1_T1')
        class get_IsAGodanImperativeInflection_1(typing.Generic[get_IsAGodanImperativeInflection_1_T1]):
            get_IsAGodanImperativeInflection_1_TFrom = TokenConditionsInCode.get_IsAGodanImperativeInflection_MethodGroup.get_IsAGodanImperativeInflection_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAGodanImperativeInflection_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAGodanImperativeInflection_1_TFrom]:...


    # Skipped get_IsAGodanImperativeStem due to it being static, abstract and generic.

    get_IsAGodanImperativeStem : get_IsAGodanImperativeStem_MethodGroup
    class get_IsAGodanImperativeStem_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAGodanImperativeStem_1_T1]) -> get_IsAGodanImperativeStem_1[get_IsAGodanImperativeStem_1_T1]: ...

        get_IsAGodanImperativeStem_1_T1 = typing.TypeVar('get_IsAGodanImperativeStem_1_T1')
        class get_IsAGodanImperativeStem_1(typing.Generic[get_IsAGodanImperativeStem_1_T1]):
            get_IsAGodanImperativeStem_1_TFrom = TokenConditionsInCode.get_IsAGodanImperativeStem_MethodGroup.get_IsAGodanImperativeStem_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAGodanImperativeStem_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAGodanImperativeStem_1_TFrom]:...


    # Skipped get_IsAGodanPotentialInflection due to it being static, abstract and generic.

    get_IsAGodanPotentialInflection : get_IsAGodanPotentialInflection_MethodGroup
    class get_IsAGodanPotentialInflection_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAGodanPotentialInflection_1_T1]) -> get_IsAGodanPotentialInflection_1[get_IsAGodanPotentialInflection_1_T1]: ...

        get_IsAGodanPotentialInflection_1_T1 = typing.TypeVar('get_IsAGodanPotentialInflection_1_T1')
        class get_IsAGodanPotentialInflection_1(typing.Generic[get_IsAGodanPotentialInflection_1_T1]):
            get_IsAGodanPotentialInflection_1_TFrom = TokenConditionsInCode.get_IsAGodanPotentialInflection_MethodGroup.get_IsAGodanPotentialInflection_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAGodanPotentialInflection_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAGodanPotentialInflection_1_TFrom]:...


    # Skipped get_IsAGodanPotentialStem due to it being static, abstract and generic.

    get_IsAGodanPotentialStem : get_IsAGodanPotentialStem_MethodGroup
    class get_IsAGodanPotentialStem_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAGodanPotentialStem_1_T1]) -> get_IsAGodanPotentialStem_1[get_IsAGodanPotentialStem_1_T1]: ...

        get_IsAGodanPotentialStem_1_T1 = typing.TypeVar('get_IsAGodanPotentialStem_1_T1')
        class get_IsAGodanPotentialStem_1(typing.Generic[get_IsAGodanPotentialStem_1_T1]):
            get_IsAGodanPotentialStem_1_TFrom = TokenConditionsInCode.get_IsAGodanPotentialStem_MethodGroup.get_IsAGodanPotentialStem_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAGodanPotentialStem_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAGodanPotentialStem_1_TFrom]:...


    # Skipped get_IsAGodanVerb due to it being static, abstract and generic.

    get_IsAGodanVerb : get_IsAGodanVerb_MethodGroup
    class get_IsAGodanVerb_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAGodanVerb_1_T1]) -> get_IsAGodanVerb_1[get_IsAGodanVerb_1_T1]: ...

        get_IsAGodanVerb_1_T1 = typing.TypeVar('get_IsAGodanVerb_1_T1')
        class get_IsAGodanVerb_1(typing.Generic[get_IsAGodanVerb_1_T1]):
            get_IsAGodanVerb_1_TFrom = TokenConditionsInCode.get_IsAGodanVerb_MethodGroup.get_IsAGodanVerb_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAGodanVerb_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAGodanVerb_1_TFrom]:...


    # Skipped get_IsAMasuStem due to it being static, abstract and generic.

    get_IsAMasuStem : get_IsAMasuStem_MethodGroup
    class get_IsAMasuStem_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAMasuStem_1_T1]) -> get_IsAMasuStem_1[get_IsAMasuStem_1_T1]: ...

        get_IsAMasuStem_1_T1 = typing.TypeVar('get_IsAMasuStem_1_T1')
        class get_IsAMasuStem_1(typing.Generic[get_IsAMasuStem_1_T1]):
            get_IsAMasuStem_1_TFrom = TokenConditionsInCode.get_IsAMasuStem_MethodGroup.get_IsAMasuStem_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAMasuStem_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAMasuStem_1_TFrom]:...


    # Skipped get_IsAnAdverb due to it being static, abstract and generic.

    get_IsAnAdverb : get_IsAnAdverb_MethodGroup
    class get_IsAnAdverb_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAnAdverb_1_T1]) -> get_IsAnAdverb_1[get_IsAnAdverb_1_T1]: ...

        get_IsAnAdverb_1_T1 = typing.TypeVar('get_IsAnAdverb_1_T1')
        class get_IsAnAdverb_1(typing.Generic[get_IsAnAdverb_1_T1]):
            get_IsAnAdverb_1_TFrom = TokenConditionsInCode.get_IsAnAdverb_MethodGroup.get_IsAnAdverb_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAnAdverb_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAnAdverb_1_TFrom]:...


    # Skipped get_IsAnIchidanImperativeInflection due to it being static, abstract and generic.

    get_IsAnIchidanImperativeInflection : get_IsAnIchidanImperativeInflection_MethodGroup
    class get_IsAnIchidanImperativeInflection_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAnIchidanImperativeInflection_1_T1]) -> get_IsAnIchidanImperativeInflection_1[get_IsAnIchidanImperativeInflection_1_T1]: ...

        get_IsAnIchidanImperativeInflection_1_T1 = typing.TypeVar('get_IsAnIchidanImperativeInflection_1_T1')
        class get_IsAnIchidanImperativeInflection_1(typing.Generic[get_IsAnIchidanImperativeInflection_1_T1]):
            get_IsAnIchidanImperativeInflection_1_TFrom = TokenConditionsInCode.get_IsAnIchidanImperativeInflection_MethodGroup.get_IsAnIchidanImperativeInflection_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAnIchidanImperativeInflection_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAnIchidanImperativeInflection_1_TFrom]:...


    # Skipped get_IsAnIchidanImperativeStem due to it being static, abstract and generic.

    get_IsAnIchidanImperativeStem : get_IsAnIchidanImperativeStem_MethodGroup
    class get_IsAnIchidanImperativeStem_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAnIchidanImperativeStem_1_T1]) -> get_IsAnIchidanImperativeStem_1[get_IsAnIchidanImperativeStem_1_T1]: ...

        get_IsAnIchidanImperativeStem_1_T1 = typing.TypeVar('get_IsAnIchidanImperativeStem_1_T1')
        class get_IsAnIchidanImperativeStem_1(typing.Generic[get_IsAnIchidanImperativeStem_1_T1]):
            get_IsAnIchidanImperativeStem_1_TFrom = TokenConditionsInCode.get_IsAnIchidanImperativeStem_MethodGroup.get_IsAnIchidanImperativeStem_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAnIchidanImperativeStem_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAnIchidanImperativeStem_1_TFrom]:...


    # Skipped get_IsAnIchidanVerb due to it being static, abstract and generic.

    get_IsAnIchidanVerb : get_IsAnIchidanVerb_MethodGroup
    class get_IsAnIchidanVerb_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAnIchidanVerb_1_T1]) -> get_IsAnIchidanVerb_1[get_IsAnIchidanVerb_1_T1]: ...

        get_IsAnIchidanVerb_1_T1 = typing.TypeVar('get_IsAnIchidanVerb_1_T1')
        class get_IsAnIchidanVerb_1(typing.Generic[get_IsAnIchidanVerb_1_T1]):
            get_IsAnIchidanVerb_1_TFrom = TokenConditionsInCode.get_IsAnIchidanVerb_MethodGroup.get_IsAnIchidanVerb_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAnIchidanVerb_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAnIchidanVerb_1_TFrom]:...


    # Skipped get_IsAnInflectableWord due to it being static, abstract and generic.

    get_IsAnInflectableWord : get_IsAnInflectableWord_MethodGroup
    class get_IsAnInflectableWord_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAnInflectableWord_1_T1]) -> get_IsAnInflectableWord_1[get_IsAnInflectableWord_1_T1]: ...

        get_IsAnInflectableWord_1_T1 = typing.TypeVar('get_IsAnInflectableWord_1_T1')
        class get_IsAnInflectableWord_1(typing.Generic[get_IsAnInflectableWord_1_T1]):
            get_IsAnInflectableWord_1_TFrom = TokenConditionsInCode.get_IsAnInflectableWord_MethodGroup.get_IsAnInflectableWord_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAnInflectableWord_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAnInflectableWord_1_TFrom]:...


    # Skipped get_IsANonWordCharacter due to it being static, abstract and generic.

    get_IsANonWordCharacter : get_IsANonWordCharacter_MethodGroup
    class get_IsANonWordCharacter_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsANonWordCharacter_1_T1]) -> get_IsANonWordCharacter_1[get_IsANonWordCharacter_1_T1]: ...

        get_IsANonWordCharacter_1_T1 = typing.TypeVar('get_IsANonWordCharacter_1_T1')
        class get_IsANonWordCharacter_1(typing.Generic[get_IsANonWordCharacter_1_T1]):
            get_IsANonWordCharacter_1_TFrom = TokenConditionsInCode.get_IsANonWordCharacter_MethodGroup.get_IsANonWordCharacter_1_T1
            def __call__(self, token: IConditionPath_2[get_IsANonWordCharacter_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsANonWordCharacter_1_TFrom]:...


    # Skipped get_IsAPastTenseMarker due to it being static, abstract and generic.

    get_IsAPastTenseMarker : get_IsAPastTenseMarker_MethodGroup
    class get_IsAPastTenseMarker_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAPastTenseMarker_1_T1]) -> get_IsAPastTenseMarker_1[get_IsAPastTenseMarker_1_T1]: ...

        get_IsAPastTenseMarker_1_T1 = typing.TypeVar('get_IsAPastTenseMarker_1_T1')
        class get_IsAPastTenseMarker_1(typing.Generic[get_IsAPastTenseMarker_1_T1]):
            get_IsAPastTenseMarker_1_TFrom = TokenConditionsInCode.get_IsAPastTenseMarker_MethodGroup.get_IsAPastTenseMarker_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAPastTenseMarker_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAPastTenseMarker_1_TFrom]:...


    # Skipped get_IsAPastTenseStem due to it being static, abstract and generic.

    get_IsAPastTenseStem : get_IsAPastTenseStem_MethodGroup
    class get_IsAPastTenseStem_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsAPastTenseStem_1_T1]) -> get_IsAPastTenseStem_1[get_IsAPastTenseStem_1_T1]: ...

        get_IsAPastTenseStem_1_T1 = typing.TypeVar('get_IsAPastTenseStem_1_T1')
        class get_IsAPastTenseStem_1(typing.Generic[get_IsAPastTenseStem_1_T1]):
            get_IsAPastTenseStem_1_TFrom = TokenConditionsInCode.get_IsAPastTenseStem_MethodGroup.get_IsAPastTenseStem_1_T1
            def __call__(self, token: IConditionPath_2[get_IsAPastTenseStem_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsAPastTenseStem_1_TFrom]:...


    # Skipped get_IsInAnIrrealisForm due to it being static, abstract and generic.

    get_IsInAnIrrealisForm : get_IsInAnIrrealisForm_MethodGroup
    class get_IsInAnIrrealisForm_MethodGroup:
        def __getitem__(self, t:typing.Type[get_IsInAnIrrealisForm_1_T1]) -> get_IsInAnIrrealisForm_1[get_IsInAnIrrealisForm_1_T1]: ...

        get_IsInAnIrrealisForm_1_T1 = typing.TypeVar('get_IsInAnIrrealisForm_1_T1')
        class get_IsInAnIrrealisForm_1(typing.Generic[get_IsInAnIrrealisForm_1_T1]):
            get_IsInAnIrrealisForm_1_TFrom = TokenConditionsInCode.get_IsInAnIrrealisForm_MethodGroup.get_IsInAnIrrealisForm_1_T1
            def __call__(self, token: IConditionPath_2[get_IsInAnIrrealisForm_1_TFrom, IAnalysisToken]) -> Condition_1[get_IsInAnIrrealisForm_1_TFrom]:...


    # Skipped get_PartOfSpeech due to it being static, abstract and generic.

    get_PartOfSpeech : get_PartOfSpeech_MethodGroup
    class get_PartOfSpeech_MethodGroup:
        def __getitem__(self, t:typing.Type[get_PartOfSpeech_1_T1]) -> get_PartOfSpeech_1[get_PartOfSpeech_1_T1]: ...

        get_PartOfSpeech_1_T1 = typing.TypeVar('get_PartOfSpeech_1_T1')
        class get_PartOfSpeech_1(typing.Generic[get_PartOfSpeech_1_T1]):
            get_PartOfSpeech_1_TFrom = TokenConditionsInCode.get_PartOfSpeech_MethodGroup.get_PartOfSpeech_1_T1
            def __call__(self, token: IConditionPath_2[get_PartOfSpeech_1_TFrom, IAnalysisToken]) -> PartOfSpeechTest_1[get_PartOfSpeech_1_TFrom]:...


    # Skipped get_Reading due to it being static, abstract and generic.

    get_Reading : get_Reading_MethodGroup
    class get_Reading_MethodGroup:
        def __getitem__(self, t:typing.Type[get_Reading_1_T1]) -> get_Reading_1[get_Reading_1_T1]: ...

        get_Reading_1_T1 = typing.TypeVar('get_Reading_1_T1')
        class get_Reading_1(typing.Generic[get_Reading_1_T1]):
            get_Reading_1_TFrom = TokenConditionsInCode.get_Reading_MethodGroup.get_Reading_1_T1
            def __call__(self, token: IConditionPath_2[get_Reading_1_TFrom, IAnalysisToken]) -> IConditionPath_2[get_Reading_1_TFrom, str]:...


    # Skipped get_Surface due to it being static, abstract and generic.

    get_Surface : get_Surface_MethodGroup
    class get_Surface_MethodGroup:
        def __getitem__(self, t:typing.Type[get_Surface_1_T1]) -> get_Surface_1[get_Surface_1_T1]: ...

        get_Surface_1_T1 = typing.TypeVar('get_Surface_1_T1')
        class get_Surface_1(typing.Generic[get_Surface_1_T1]):
            get_Surface_1_TFrom = TokenConditionsInCode.get_Surface_MethodGroup.get_Surface_1_T1
            def __call__(self, token: IConditionPath_2[get_Surface_1_TFrom, IAnalysisToken]) -> IConditionPath_2[get_Surface_1_TFrom, str]:...



    class <G>$4F22BD2C8527CC065115816539E78F82_GenericClasses(abc.ABCMeta):
        Generic_<G>$4F22BD2C8527CC065115816539E78F82_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0 = typing.TypeVar('Generic_<G>$4F22BD2C8527CC065115816539E78F82_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0')
        def __getitem__(self, types : typing.Type[Generic_<G>$4F22BD2C8527CC065115816539E78F82_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]) -> typing.Type[TokenConditionsInCode.<G>$4F22BD2C8527CC065115816539E78F82_1[Generic_<G>$4F22BD2C8527CC065115816539E78F82_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]]: ...

    <G>$4F22BD2C8527CC065115816539E78F82 : <G>$4F22BD2C8527CC065115816539E78F82_GenericClasses

    <G>$4F22BD2C8527CC065115816539E78F82_1_$T0 = typing.TypeVar('<G>$4F22BD2C8527CC065115816539E78F82_1_$T0')
    class <G>$4F22BD2C8527CC065115816539E78F82_1(typing.Generic[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]):
        <G>$4F22BD2C8527CC065115816539E78F82_1_$T0 = TokenConditionsInCode.<G>$4F22BD2C8527CC065115816539E78F82_1_$T0
        @property
        def BaseForm(self) -> IConditionPath_2[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0, str]: ...
        @property
        def EndsAStatement(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def Exists(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def HasATeFormStem(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def InflectedForm(self) -> InflectedFormTest_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def InflectionType(self) -> InflectionTypeTest_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsADictionaryVerbFormStem(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsADictionaryVerbInflection(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAGodanImperativeInflection(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAGodanImperativeStem(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAGodanPotentialInflection(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAGodanPotentialStem(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAGodanVerb(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAMasuStem(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAnAdverb(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAnIchidanImperativeInflection(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAnIchidanImperativeStem(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAnIchidanVerb(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAnInflectableWord(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsANonWordCharacter(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAPastTenseMarker(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsAPastTenseStem(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def IsInAnIrrealisForm(self) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def PartOfSpeech(self) -> PartOfSpeechTest_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        @property
        def Reading(self) -> IConditionPath_2[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0, str]: ...
        @property
        def Surface(self) -> IConditionPath_2[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0, str]: ...
        def AllOf(self, conditions: Array_1[Condition_1[IAnalysisToken]]) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...
        def AnyOf(self, conditions: Array_1[Condition_1[IAnalysisToken]]) -> Condition_1[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...

        <M>$B6ACC14E1073C003DA8462789C08CFC0_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0 = typing.TypeVar('<M>$B6ACC14E1073C003DA8462789C08CFC0_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0')
        class <M>$B6ACC14E1073C003DA8462789C08CFC0_GenericClasses(typing.Generic[<M>$B6ACC14E1073C003DA8462789C08CFC0_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0], abc.ABCMeta):
            <M>$B6ACC14E1073C003DA8462789C08CFC0_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0 = TokenConditionsInCode.<G>$4F22BD2C8527CC065115816539E78F82_1.<M>$B6ACC14E1073C003DA8462789C08CFC0_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0
            def __call__(self) -> TokenConditionsInCode.<G>$4F22BD2C8527CC065115816539E78F82_1.<M>$B6ACC14E1073C003DA8462789C08CFC0_1[<M>$B6ACC14E1073C003DA8462789C08CFC0_GenericClasses_<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]: ...

        <M>$B6ACC14E1073C003DA8462789C08CFC0 : <M>$B6ACC14E1073C003DA8462789C08CFC0_GenericClasses[<G>$4F22BD2C8527CC065115816539E78F82_1_$T0]

        <M>$B6ACC14E1073C003DA8462789C08CFC0_1_TFrom = typing.TypeVar('<M>$B6ACC14E1073C003DA8462789C08CFC0_1_TFrom')
        class <M>$B6ACC14E1073C003DA8462789C08CFC0_1(typing.Generic[<M>$B6ACC14E1073C003DA8462789C08CFC0_1_TFrom], abc.ABC):
            <M>$B6ACC14E1073C003DA8462789C08CFC0_1_TFrom = TokenConditionsInCode.<G>$4F22BD2C8527CC065115816539E78F82_1.<M>$B6ACC14E1073C003DA8462789C08CFC0_1_TFrom
            @staticmethod
            def <Extension>$(token: IConditionPath_2[<M>$B6ACC14E1073C003DA8462789C08CFC0_1_TFrom, IAnalysisToken]) -> None: ...



