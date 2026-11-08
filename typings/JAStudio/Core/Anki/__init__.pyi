import abc

class AnkiFieldNames(abc.ABC):
    Answer : str
    JasNoteId : str
    NoteId : str
    Question : str

    class Kanji(abc.ABC):
        ActiveAnswer : str
        Question : str
        SourceAnswer : str


    class Sentence(abc.ABC):
        Audio : str
        Id : str
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
        UserImage : str


    class VocabCard(abc.ABC):
        Listening : str
        Reading : str


