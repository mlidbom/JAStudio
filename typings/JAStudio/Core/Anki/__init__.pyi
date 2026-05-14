import abc
from JAStudio.Core.Note import AnkiNoteData, NoteId
from System.Collections.Generic import List_1

class AnkiFieldNames(abc.ABC):
    Answer : str
    JasNoteId : str
    NoteId : str
    Question : str

    class ImmersionKit(abc.ABC):
        Answer : str
        Audio : str
        Id : str
        Question : str
        Reading : str
        Screenshot : str


    class Kanji(abc.ABC):
        ActiveAnswer : str
        Audio : str
        Image : str
        PrimaryReadingsTtsAudio : str
        Question : str
        ReadingKun : str
        ReadingOn : str
        SourceAnswer : str


    class Sentence(abc.ABC):
        Audio : str
        Id : str
        ParsingResult : str
        Reading : str
        Screenshot : str
        SourceAnswer : str
        SourceComments : str
        SourceQuestion : str


    class SentenceCard(abc.ABC):
        Listening : str
        Reading : str


    class Vocab(abc.ABC):
        ActiveAnswer : str
        AudioB : str
        AudioG : str
        AudioTTS : str
        Forms : str
        Image : str
        Question : str
        Reading : str
        UserImage : str


    class VocabCard(abc.ABC):
        Listening : str
        Reading : str



class AnkiKanjiNote:
    def __init__(self, data: AnkiNoteData) -> None: ...
    @property
    def ActiveAnswer(self) -> str: ...
    @property
    def Audio(self) -> str: ...
    @property
    def Id(self) -> NoteId: ...
    @property
    def Image(self) -> str: ...
    @property
    def PrimaryReadingsTtsAudio(self) -> str: ...
    @property
    def Question(self) -> str: ...
    @property
    def Raw(self) -> AnkiNoteData: ...
    @property
    def SourceAnswer(self) -> str: ...
    @property
    def Tags(self) -> List_1[str]: ...


class AnkiSentenceNote:
    def __init__(self, data: AnkiNoteData) -> None: ...
    @property
    def Audio(self) -> str: ...
    @property
    def ExternalId(self) -> str: ...
    @property
    def Id(self) -> NoteId: ...
    @property
    def Raw(self) -> AnkiNoteData: ...
    @property
    def Reading(self) -> str: ...
    @property
    def Screenshot(self) -> str: ...
    @property
    def SourceAnswer(self) -> str: ...
    @property
    def SourceComments(self) -> str: ...
    @property
    def SourceQuestion(self) -> str: ...
    @property
    def Tags(self) -> List_1[str]: ...


class AnkiVocabNote:
    def __init__(self, data: AnkiNoteData) -> None: ...
    @property
    def ActiveAnswer(self) -> str: ...
    @property
    def AudioB(self) -> str: ...
    @property
    def AudioG(self) -> str: ...
    @property
    def AudioTTS(self) -> str: ...
    @property
    def Id(self) -> NoteId: ...
    @property
    def Image(self) -> str: ...
    @property
    def Question(self) -> str: ...
    @property
    def Raw(self) -> AnkiNoteData: ...
    @property
    def Tags(self) -> List_1[str]: ...
    @property
    def UserImage(self) -> str: ...

