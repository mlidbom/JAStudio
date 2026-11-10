import abc
from JAStudio.Core.Note.Vocabulary.Matching.Conditions import ConditionSubject_2, Fact_1, ConditionLevel_1
from JAStudio.Core.LanguageServices.JanomeEx.WordExtraction import CandidateWordVariant
from JAStudio.Core.LanguageServices.JanomeEx.Tokenizing import IAnalysisToken

class WordConditions(abc.ABC):
    BaseForm : ConditionSubject_2[CandidateWordVariant, str]
    FirstToken : ConditionSubject_2[CandidateWordVariant, IAnalysisToken]
    FollowingText : ConditionSubject_2[CandidateWordVariant, str]
    FollowingToken : ConditionSubject_2[CandidateWordVariant, IAnalysisToken]
    IsASingleToken : Fact_1[CandidateWordVariant]
    IsMatchedByAnInflectedSurface : Fact_1[CandidateWordVariant]
    IsMatchedByItsSurface : Fact_1[CandidateWordVariant]
    LastToken : ConditionSubject_2[CandidateWordVariant, IAnalysisToken]
    Level : ConditionLevel_1[CandidateWordVariant]
    PrecedingText : ConditionSubject_2[CandidateWordVariant, str]
    PrecedingToken : ConditionSubject_2[CandidateWordVariant, IAnalysisToken]
    Surface : ConditionSubject_2[CandidateWordVariant, str]

