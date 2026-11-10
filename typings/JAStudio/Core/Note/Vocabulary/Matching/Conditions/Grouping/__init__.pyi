import typing, abc
from System import IEquatable_1, Array_1
from System.Collections.Generic import IReadOnlyList_1, IEnumerable_1
from JAStudio.Core.Note.Vocabulary.Matching.Conditions import Condition_1

class AllOfGroup_GenericClasses(abc.ABCMeta):
    Generic_AllOfGroup_GenericClasses_AllOfGroup_1_TSubject = typing.TypeVar('Generic_AllOfGroup_GenericClasses_AllOfGroup_1_TSubject')
    def __getitem__(self, types : typing.Type[Generic_AllOfGroup_GenericClasses_AllOfGroup_1_TSubject]) -> typing.Type[AllOfGroup_1[Generic_AllOfGroup_GenericClasses_AllOfGroup_1_TSubject]]: ...

AllOfGroup : AllOfGroup_GenericClasses

AllOfGroup_1_TSubject = typing.TypeVar('AllOfGroup_1_TSubject')
class AllOfGroup_1(typing.Generic[AllOfGroup_1_TSubject], ConditionGroup_1[AllOfGroup_1_TSubject], IEquatable_1[AllOfGroup_1[AllOfGroup_1_TSubject]]):
    @property
    def Conditions(self) -> IReadOnlyList_1[Condition_1[AllOfGroup_1_TSubject]]: ...
    @property
    def IsEmpty(self) -> bool: ...
    def GetHashCode(self) -> int: ...
    def IsMetBy(self, subject: AllOfGroup_1_TSubject) -> bool: ...
    def __eq__(self, left: AllOfGroup_1[AllOfGroup_1_TSubject], right: AllOfGroup_1[AllOfGroup_1_TSubject]) -> bool: ...
    def __ne__(self, left: AllOfGroup_1[AllOfGroup_1_TSubject], right: AllOfGroup_1[AllOfGroup_1_TSubject]) -> bool: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup[AllOfGroup_1_TSubject]
    Equals_MethodGroup_AllOfGroup_1_TSubject = typing.TypeVar('Equals_MethodGroup_AllOfGroup_1_TSubject')
    class Equals_MethodGroup(typing.Generic[Equals_MethodGroup_AllOfGroup_1_TSubject]):
        Equals_MethodGroup_AllOfGroup_1_TSubject = AllOfGroup_1.Equals_MethodGroup_AllOfGroup_1_TSubject
        @typing.overload
        def __call__(self, other: AllOfGroup_1[Equals_MethodGroup_AllOfGroup_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, other: ConditionGroup_1[Equals_MethodGroup_AllOfGroup_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class AnyOfGroup_GenericClasses(abc.ABCMeta):
    Generic_AnyOfGroup_GenericClasses_AnyOfGroup_1_TSubject = typing.TypeVar('Generic_AnyOfGroup_GenericClasses_AnyOfGroup_1_TSubject')
    def __getitem__(self, types : typing.Type[Generic_AnyOfGroup_GenericClasses_AnyOfGroup_1_TSubject]) -> typing.Type[AnyOfGroup_1[Generic_AnyOfGroup_GenericClasses_AnyOfGroup_1_TSubject]]: ...

AnyOfGroup : AnyOfGroup_GenericClasses

AnyOfGroup_1_TSubject = typing.TypeVar('AnyOfGroup_1_TSubject')
class AnyOfGroup_1(typing.Generic[AnyOfGroup_1_TSubject], ConditionGroup_1[AnyOfGroup_1_TSubject], IEquatable_1[AnyOfGroup_1[AnyOfGroup_1_TSubject]]):
    @property
    def Conditions(self) -> IReadOnlyList_1[Condition_1[AnyOfGroup_1_TSubject]]: ...
    @property
    def IsEmpty(self) -> bool: ...
    def GetHashCode(self) -> int: ...
    def IsMetBy(self, subject: AnyOfGroup_1_TSubject) -> bool: ...
    def __eq__(self, left: AnyOfGroup_1[AnyOfGroup_1_TSubject], right: AnyOfGroup_1[AnyOfGroup_1_TSubject]) -> bool: ...
    def __ne__(self, left: AnyOfGroup_1[AnyOfGroup_1_TSubject], right: AnyOfGroup_1[AnyOfGroup_1_TSubject]) -> bool: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup[AnyOfGroup_1_TSubject]
    Equals_MethodGroup_AnyOfGroup_1_TSubject = typing.TypeVar('Equals_MethodGroup_AnyOfGroup_1_TSubject')
    class Equals_MethodGroup(typing.Generic[Equals_MethodGroup_AnyOfGroup_1_TSubject]):
        Equals_MethodGroup_AnyOfGroup_1_TSubject = AnyOfGroup_1.Equals_MethodGroup_AnyOfGroup_1_TSubject
        @typing.overload
        def __call__(self, other: AnyOfGroup_1[Equals_MethodGroup_AnyOfGroup_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, other: ConditionGroup_1[Equals_MethodGroup_AnyOfGroup_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class ConditionGroup_GenericClasses(abc.ABCMeta):
    Generic_ConditionGroup_GenericClasses_ConditionGroup_1_TSubject = typing.TypeVar('Generic_ConditionGroup_GenericClasses_ConditionGroup_1_TSubject')
    def __getitem__(self, types : typing.Type[Generic_ConditionGroup_GenericClasses_ConditionGroup_1_TSubject]) -> typing.Type[ConditionGroup_1[Generic_ConditionGroup_GenericClasses_ConditionGroup_1_TSubject]]: ...

class ConditionGroup(ConditionGroup_0, metaclass =ConditionGroup_GenericClasses): ...

class ConditionGroup_0(abc.ABC):
    # Skipped AllOf due to it being static, abstract and generic.

    AllOf : AllOf_MethodGroup
    class AllOf_MethodGroup:
        def __getitem__(self, t:typing.Type[AllOf_1_T1]) -> AllOf_1[AllOf_1_T1]: ...

        AllOf_1_T1 = typing.TypeVar('AllOf_1_T1')
        class AllOf_1(typing.Generic[AllOf_1_T1]):
            AllOf_1_TSubject = ConditionGroup_0.AllOf_MethodGroup.AllOf_1_T1
            def __call__(self, conditions: Array_1[Condition_1[AllOf_1_TSubject]]) -> AllOfGroup_1[AllOf_1_TSubject]:...


    # Skipped AnyOf due to it being static, abstract and generic.

    AnyOf : AnyOf_MethodGroup
    class AnyOf_MethodGroup:
        def __getitem__(self, t:typing.Type[AnyOf_1_T1]) -> AnyOf_1[AnyOf_1_T1]: ...

        AnyOf_1_T1 = typing.TypeVar('AnyOf_1_T1')
        class AnyOf_1(typing.Generic[AnyOf_1_T1]):
            AnyOf_1_TSubject = ConditionGroup_0.AnyOf_MethodGroup.AnyOf_1_T1
            def __call__(self, conditions: Array_1[Condition_1[AnyOf_1_TSubject]]) -> AnyOfGroup_1[AnyOf_1_TSubject]:...


    # Skipped Not due to it being static, abstract and generic.

    Not : Not_MethodGroup
    class Not_MethodGroup:
        def __getitem__(self, t:typing.Type[Not_1_T1]) -> Not_1[Not_1_T1]: ...

        Not_1_T1 = typing.TypeVar('Not_1_T1')
        class Not_1(typing.Generic[Not_1_T1]):
            Not_1_TSubject = ConditionGroup_0.Not_MethodGroup.Not_1_T1
            def __call__(self, condition: Condition_1[Not_1_TSubject]) -> NegatedCondition_1[Not_1_TSubject]:...




ConditionGroup_1_TSubject = typing.TypeVar('ConditionGroup_1_TSubject')
class ConditionGroup_1(typing.Generic[ConditionGroup_1_TSubject], Condition_1[ConditionGroup_1_TSubject], IEquatable_1[ConditionGroup_1[ConditionGroup_1_TSubject]]):
    @property
    def Conditions(self) -> IReadOnlyList_1[Condition_1[ConditionGroup_1_TSubject]]: ...
    @property
    def IsEmpty(self) -> bool: ...
    def DescriptionLines(self) -> IEnumerable_1[str]: ...
    def GetHashCode(self) -> int: ...
    def __eq__(self, left: ConditionGroup_1[ConditionGroup_1_TSubject], right: ConditionGroup_1[ConditionGroup_1_TSubject]) -> bool: ...
    def __ne__(self, left: ConditionGroup_1[ConditionGroup_1_TSubject], right: ConditionGroup_1[ConditionGroup_1_TSubject]) -> bool: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup[ConditionGroup_1_TSubject]
    Equals_MethodGroup_ConditionGroup_1_TSubject = typing.TypeVar('Equals_MethodGroup_ConditionGroup_1_TSubject')
    class Equals_MethodGroup(typing.Generic[Equals_MethodGroup_ConditionGroup_1_TSubject]):
        Equals_MethodGroup_ConditionGroup_1_TSubject = ConditionGroup_1.Equals_MethodGroup_ConditionGroup_1_TSubject
        @typing.overload
        def __call__(self, other: ConditionGroup_1[Equals_MethodGroup_ConditionGroup_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, other: Condition_1[Equals_MethodGroup_ConditionGroup_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class NegatedCondition_GenericClasses(abc.ABCMeta):
    Generic_NegatedCondition_GenericClasses_NegatedCondition_1_TSubject = typing.TypeVar('Generic_NegatedCondition_GenericClasses_NegatedCondition_1_TSubject')
    def __getitem__(self, types : typing.Type[Generic_NegatedCondition_GenericClasses_NegatedCondition_1_TSubject]) -> typing.Type[NegatedCondition_1[Generic_NegatedCondition_GenericClasses_NegatedCondition_1_TSubject]]: ...

NegatedCondition : NegatedCondition_GenericClasses

NegatedCondition_1_TSubject = typing.TypeVar('NegatedCondition_1_TSubject')
class NegatedCondition_1(typing.Generic[NegatedCondition_1_TSubject], Condition_1[NegatedCondition_1_TSubject], IEquatable_1[NegatedCondition_1[NegatedCondition_1_TSubject]]):
    @property
    def Condition(self) -> Condition_1[NegatedCondition_1_TSubject]: ...
    @property
    def IsEmpty(self) -> bool: ...
    def DescriptionLines(self) -> IEnumerable_1[str]: ...
    def GetHashCode(self) -> int: ...
    def IsMetBy(self, subject: NegatedCondition_1_TSubject) -> bool: ...
    def __eq__(self, left: NegatedCondition_1[NegatedCondition_1_TSubject], right: NegatedCondition_1[NegatedCondition_1_TSubject]) -> bool: ...
    def __ne__(self, left: NegatedCondition_1[NegatedCondition_1_TSubject], right: NegatedCondition_1[NegatedCondition_1_TSubject]) -> bool: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup[NegatedCondition_1_TSubject]
    Equals_MethodGroup_NegatedCondition_1_TSubject = typing.TypeVar('Equals_MethodGroup_NegatedCondition_1_TSubject')
    class Equals_MethodGroup(typing.Generic[Equals_MethodGroup_NegatedCondition_1_TSubject]):
        Equals_MethodGroup_NegatedCondition_1_TSubject = NegatedCondition_1.Equals_MethodGroup_NegatedCondition_1_TSubject
        @typing.overload
        def __call__(self, other: NegatedCondition_1[Equals_MethodGroup_NegatedCondition_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, other: Condition_1[Equals_MethodGroup_NegatedCondition_1_TSubject]) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...


