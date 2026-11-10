import abc
from JAStudio.Core.Note.Vocabulary.Matching import RequireForbidChoice
from System.Collections.Generic import IEnumerable_1, IReadOnlySet_1, IReadOnlyList_1
from JAStudio.Core.Note.Vocabulary.Matching.Conditions import Condition_1
from JAStudio.Core.LanguageServices.JanomeEx.WordExtraction import CandidateWordVariant

class MatchConditionAsRules:
    @property
    def IsMadeOfRulesOnly(self) -> bool: ...
    def ChoiceOf(self, rule: RequireForbidMatchingRule) -> RequireForbidChoice: ...
    def Choose(self, rule: RequireForbidMatchingRule, choice: RequireForbidChoice) -> None: ...
    def SetTexts(self, rule: TextsMatchingRule, texts: IEnumerable_1[str]) -> None: ...
    def TextsOf(self, rule: TextsMatchingRule) -> IReadOnlySet_1[str]: ...


class MatchingRule(abc.ABC):
    @property
    def Name(self) -> str: ...
    def ToString(self) -> str: ...


class MatchingRules(abc.ABC):
    AdverbHead : RequireForbidMatchingRule
    All : IReadOnlyList_1[MatchingRule]
    DictionaryFormHead : RequireForbidMatchingRule
    Godan : RequireForbidMatchingRule
    GodanImperative : RequireForbidMatchingRule
    GodanImperativeHead : RequireForbidMatchingRule
    GodanPotential : RequireForbidMatchingRule
    Ichidan : RequireForbidMatchingRule
    IchidanImperative : RequireForbidMatchingRule
    Irrealis : RequireForbidMatchingRule
    MasuStem : RequireForbidMatchingRule
    PastTenseStem : RequireForbidMatchingRule
    PrefixIsNot : TextsMatchingRule
    RequiredPrefix : TextsMatchingRule
    SentenceEnd : RequireForbidMatchingRule
    SentenceStart : RequireForbidMatchingRule
    SingleToken : RequireForbidMatchingRule
    SuffixIsNot : TextsMatchingRule
    Surface : RequireForbidMatchingRule
    SurfaceIsNot : TextsMatchingRule
    TeFormStemHead : RequireForbidMatchingRule
    TeFormStemStart : RequireForbidMatchingRule
    @staticmethod
    def Named(name: str) -> MatchingRule: ...


class RequireForbidMatchingRule(MatchingRule):
    @property
    def Name(self) -> str: ...
    @property
    def WhenForbidden(self) -> Condition_1[CandidateWordVariant]: ...
    @property
    def WhenRequired(self) -> Condition_1[CandidateWordVariant]: ...


class TextsMatchingRule(MatchingRule):
    @property
    def Name(self) -> str: ...

