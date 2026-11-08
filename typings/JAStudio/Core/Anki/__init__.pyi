import abc

class AnkiFieldNames(abc.ABC):
    JasNoteId : str
    Question : str

    class Sentence(abc.ABC):
        Audio : str


    class Vocab(abc.ABC):
        AudioB : str
        AudioG : str
        AudioTTS : str


