import typing, abc
from System import IEquatable_1
from System.Collections.Generic import IEnumerable_1, IReadOnlyList_1
from JAStudio.Core.LanguageServices.JanomeEx.WordExtraction import CandidateWordVariant
from System.Text.Json.Nodes import JsonObject
from JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping import AllOfGroup_1

class Condition_GenericClasses(abc.ABCMeta):
    Generic_Condition_GenericClasses_Condition_1_TSubject = typing.TypeVar('Generic_Condition_GenericClasses_Condition_1_TSubject')
    def __getitem__(self, types : typing.Type[Generic_Condition_GenericClasses_Condition_1_TSubject]) -> typing.Type[Condition_1[Generic_Condition_GenericClasses_Condition_1_TSubject]]: ...

Condition : Condition_GenericClasses

Condition_1_TSubject = typing.TypeVar('Condition_1_TSubject')
class Condition_1(typing.Generic[Condition_1_TSubject], IEquatable_1[Condition_1[Condition_1_TSubject]], abc.ABC):
    @property
    def IsEmpty(self) -> bool: ...
    @abc.abstractmethod
    def DescriptionLines(self) -> IEnumerable_1[str]: ...
    def GetHashCode(self) -> int: ...
    @abc.abstractmethod
    def IsMetBy(self, subject: Condition_1_TSubject) -> bool: ...
    def __eq__(self, left: Condition_1[Condition_1_TSubject], right: Condition_1[Condition_1_TSubject]) -> bool: ...
    def __ne__(self, left: Condition_1[Condition_1_TSubject], right: Condition_1[Condition_1_TSubject]) -> bool: ...
    def ToString(self) -> str: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup[Condition_1_TSubject]
    Equals_MethodGroup_Condition_1_TSubject = typing.TypeVar('Equals_MethodGroup_Condition_1_TSubject')
    class Equals_MethodGroup(typing.Generic[Equals_MethodGroup_Condition_1_TSubject]):
        Equals_MethodGroup_Condition_1_TSubject = Condition_1.Equals_MethodGroup_Condition_1_TSubject
        @typing.overload
        def __call__(self, other: Condition_1[Equals_MethodGroup_Condition_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class ConditionLevel_GenericClasses(abc.ABCMeta):
    Generic_ConditionLevel_GenericClasses_ConditionLevel_1_TSubject = typing.TypeVar('Generic_ConditionLevel_GenericClasses_ConditionLevel_1_TSubject')
    def __getitem__(self, types : typing.Type[Generic_ConditionLevel_GenericClasses_ConditionLevel_1_TSubject]) -> typing.Type[ConditionLevel_1[Generic_ConditionLevel_GenericClasses_ConditionLevel_1_TSubject]]: ...

ConditionLevel : ConditionLevel_GenericClasses

ConditionLevel_1_TSubject = typing.TypeVar('ConditionLevel_1_TSubject')
class ConditionLevel_1(typing.Generic[ConditionLevel_1_TSubject]):
    @property
    def Facts(self) -> IReadOnlyList_1[Fact_1[ConditionLevel_1_TSubject]]: ...


class ConditionSubject_GenericClasses(abc.ABCMeta):
    Generic_ConditionSubject_GenericClasses_ConditionSubject_2_TFrom = typing.TypeVar('Generic_ConditionSubject_GenericClasses_ConditionSubject_2_TFrom')
    Generic_ConditionSubject_GenericClasses_ConditionSubject_2_TSubject = typing.TypeVar('Generic_ConditionSubject_GenericClasses_ConditionSubject_2_TSubject')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_ConditionSubject_GenericClasses_ConditionSubject_2_TFrom], typing.Type[Generic_ConditionSubject_GenericClasses_ConditionSubject_2_TSubject]]) -> typing.Type[ConditionSubject_2[Generic_ConditionSubject_GenericClasses_ConditionSubject_2_TFrom, Generic_ConditionSubject_GenericClasses_ConditionSubject_2_TSubject]]: ...

ConditionSubject : ConditionSubject_GenericClasses

ConditionSubject_2_TFrom = typing.TypeVar('ConditionSubject_2_TFrom')
ConditionSubject_2_TSubject = typing.TypeVar('ConditionSubject_2_TSubject')
class ConditionSubject_2(typing.Generic[ConditionSubject_2_TFrom, ConditionSubject_2_TSubject]):
    @property
    def JsonKey(self) -> str: ...
    @property
    def Level(self) -> ConditionLevel_1[ConditionSubject_2_TSubject]: ...
    @property
    def Name(self) -> str: ...
    def Meets(self, condition: Condition_1[ConditionSubject_2_TSubject]) -> SubjectCondition_2[ConditionSubject_2_TFrom, ConditionSubject_2_TSubject]: ...
    def ToString(self) -> str: ...


class Fact_GenericClasses(abc.ABCMeta):
    Generic_Fact_GenericClasses_Fact_1_TSubject = typing.TypeVar('Generic_Fact_GenericClasses_Fact_1_TSubject')
    def __getitem__(self, types : typing.Type[Generic_Fact_GenericClasses_Fact_1_TSubject]) -> typing.Type[Fact_1[Generic_Fact_GenericClasses_Fact_1_TSubject]]: ...

Fact : Fact_GenericClasses

Fact_1_TSubject = typing.TypeVar('Fact_1_TSubject')
class Fact_1(typing.Generic[Fact_1_TSubject], Condition_1[Fact_1_TSubject], IEquatable_1[Fact_1[Fact_1_TSubject]]):
    @property
    def Description(self) -> str: ...
    @property
    def IsEmpty(self) -> bool: ...
    @property
    def JsonName(self) -> str: ...
    def DescriptionLines(self) -> IEnumerable_1[str]: ...
    def GetHashCode(self) -> int: ...
    def IsMetBy(self, subject: Fact_1_TSubject) -> bool: ...
    def __eq__(self, left: Fact_1[Fact_1_TSubject], right: Fact_1[Fact_1_TSubject]) -> bool: ...
    def __ne__(self, left: Fact_1[Fact_1_TSubject], right: Fact_1[Fact_1_TSubject]) -> bool: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup[Fact_1_TSubject]
    Equals_MethodGroup_Fact_1_TSubject = typing.TypeVar('Equals_MethodGroup_Fact_1_TSubject')
    class Equals_MethodGroup(typing.Generic[Equals_MethodGroup_Fact_1_TSubject]):
        Equals_MethodGroup_Fact_1_TSubject = Fact_1.Equals_MethodGroup_Fact_1_TSubject
        @typing.overload
        def __call__(self, other: Fact_1[Equals_MethodGroup_Fact_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, other: Condition_1[Equals_MethodGroup_Fact_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class MatchConditionJson(abc.ABC):
    @staticmethod
    def MatchConditionFrom(json: JsonObject) -> Condition_1[CandidateWordVariant]: ...
    @staticmethod
    def Of(matchCondition: Condition_1[CandidateWordVariant]) -> JsonObject: ...
    @staticmethod
    def VocabsMatchConditionFrom(json: JsonObject) -> AllOfGroup_1[CandidateWordVariant]: ...


class SubjectCondition_GenericClasses(abc.ABCMeta):
    Generic_SubjectCondition_GenericClasses_SubjectCondition_2_TFrom = typing.TypeVar('Generic_SubjectCondition_GenericClasses_SubjectCondition_2_TFrom')
    Generic_SubjectCondition_GenericClasses_SubjectCondition_2_TSubject = typing.TypeVar('Generic_SubjectCondition_GenericClasses_SubjectCondition_2_TSubject')
    def __getitem__(self, types : typing.Tuple[typing.Type[Generic_SubjectCondition_GenericClasses_SubjectCondition_2_TFrom], typing.Type[Generic_SubjectCondition_GenericClasses_SubjectCondition_2_TSubject]]) -> typing.Type[SubjectCondition_2[Generic_SubjectCondition_GenericClasses_SubjectCondition_2_TFrom, Generic_SubjectCondition_GenericClasses_SubjectCondition_2_TSubject]]: ...

SubjectCondition : SubjectCondition_GenericClasses

SubjectCondition_2_TFrom = typing.TypeVar('SubjectCondition_2_TFrom')
SubjectCondition_2_TSubject = typing.TypeVar('SubjectCondition_2_TSubject')
class SubjectCondition_2(typing.Generic[SubjectCondition_2_TFrom, SubjectCondition_2_TSubject], Condition_1[SubjectCondition_2_TFrom], IEquatable_1[SubjectCondition_2[SubjectCondition_2_TFrom, SubjectCondition_2_TSubject]]):
    @property
    def Condition(self) -> Condition_1[SubjectCondition_2_TSubject]: ...
    @property
    def IsEmpty(self) -> bool: ...
    @property
    def Subject(self) -> ConditionSubject_2[SubjectCondition_2_TFrom, SubjectCondition_2_TSubject]: ...
    def DescriptionLines(self) -> IEnumerable_1[str]: ...
    def GetHashCode(self) -> int: ...
    def IsMetBy(self, subject: SubjectCondition_2_TFrom) -> bool: ...
    def __eq__(self, left: SubjectCondition_2[SubjectCondition_2_TFrom, SubjectCondition_2_TSubject], right: SubjectCondition_2[SubjectCondition_2_TFrom, SubjectCondition_2_TSubject]) -> bool: ...
    def __ne__(self, left: SubjectCondition_2[SubjectCondition_2_TFrom, SubjectCondition_2_TSubject], right: SubjectCondition_2[SubjectCondition_2_TFrom, SubjectCondition_2_TSubject]) -> bool: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup[SubjectCondition_2_TFrom, SubjectCondition_2_TSubject]
    Equals_MethodGroup_SubjectCondition_2_TFrom = typing.TypeVar('Equals_MethodGroup_SubjectCondition_2_TFrom')
    Equals_MethodGroup_SubjectCondition_2_TSubject = typing.TypeVar('Equals_MethodGroup_SubjectCondition_2_TSubject')
    class Equals_MethodGroup(typing.Generic[Equals_MethodGroup_SubjectCondition_2_TFrom, Equals_MethodGroup_SubjectCondition_2_TSubject]):
        Equals_MethodGroup_SubjectCondition_2_TFrom = SubjectCondition_2.Equals_MethodGroup_SubjectCondition_2_TFrom
        Equals_MethodGroup_SubjectCondition_2_TSubject = SubjectCondition_2.Equals_MethodGroup_SubjectCondition_2_TSubject
        @typing.overload
        def __call__(self, other: SubjectCondition_2[Equals_MethodGroup_SubjectCondition_2_TFrom, Equals_MethodGroup_SubjectCondition_2_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, other: Condition_1[Equals_MethodGroup_SubjectCondition_2_TFrom]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...


