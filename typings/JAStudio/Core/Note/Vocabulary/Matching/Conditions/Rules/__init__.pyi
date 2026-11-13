import abc
from System.Collections.Generic import IReadOnlyList_1
from JAStudio.Core.Note.Vocabulary.Matching.Conditions import Condition_1
from JAStudio.Core.LanguageServices.Parsing.WordExtraction import CandidateWordVariant

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

