import abc
from JAStudio.Core.Note.Vocabulary.Matching.Conditions import ConditionSubject_2, Fact_1, ConditionLevel_1
from JAStudio.Core.LanguageServices.Parsing.WordExtraction import CandidateWordVariant
from JAStudio.Core.LanguageServices.Parsing.Tokenizing import AnalysisToken

class WordConditions(abc.ABC):
    BaseForm : ConditionSubject_2[CandidateWordVariant, str]
    FirstToken : ConditionSubject_2[CandidateWordVariant, AnalysisToken]
    FollowingText : ConditionSubject_2[CandidateWordVariant, str]
    FollowingToken : ConditionSubject_2[CandidateWordVariant, AnalysisToken]
    IsASingleToken : Fact_1[CandidateWordVariant]
    IsMatchedByAnInflectedSurface : Fact_1[CandidateWordVariant]
    IsMatchedByItsSurface : Fact_1[CandidateWordVariant]
    LastToken : ConditionSubject_2[CandidateWordVariant, AnalysisToken]
    Level : ConditionLevel_1[CandidateWordVariant]
    PrecedingText : ConditionSubject_2[CandidateWordVariant, str]
    PrecedingToken : ConditionSubject_2[CandidateWordVariant, AnalysisToken]
    Surface : ConditionSubject_2[CandidateWordVariant, str]

