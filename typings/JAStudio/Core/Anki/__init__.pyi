import abc
from System.Collections.Generic import IReadOnlyList_1, IEnumerable_1
from JAStudio.Core.Note.Media import AudioReference, ImageReference

class AnkiFieldNames(abc.ABC):
    JasNoteId : str
    Question : str
    @classmethod
    @property
    def AudioFields(cls) -> IReadOnlyList_1[str]: ...

    class Sentence(abc.ABC):
        Audio : str


    class Vocab(abc.ABC):
        AudioB : str
        AudioG : str
        AudioTTS : str



class AnkiMediaMarkup(abc.ABC):
    @staticmethod
    def AudioIn(field: str) -> IReadOnlyList_1[AudioReference]: ...
    @staticmethod
    def ImagesIn(field: str) -> IReadOnlyList_1[ImageReference]: ...
    @staticmethod
    def ImgTagsFor(images: IEnumerable_1[ImageReference]) -> str: ...
    @staticmethod
    def SoundTagsFor(audio: IEnumerable_1[AudioReference]) -> str: ...

