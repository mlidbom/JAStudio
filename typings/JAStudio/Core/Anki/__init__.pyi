import typing, abc
from System.Collections.Generic import IReadOnlyList_1, IEnumerable_1
from JAStudio.Core.Note.Media import AudioReference, ImageReference

class AnkiAvailability:
    Available : AnkiAvailability
    Unavailable : AnkiAvailability
    @property
    def IsAvailable(self) -> bool: ...


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


class IAnkiWindows(typing.Protocol):
    @abc.abstractmethod
    def OpenTheBrowserOn(self, ankiSearch: str) -> None: ...
    @abc.abstractmethod
    def OpenThePreviewerOn(self, ankiSearch: str) -> None: ...
    @abc.abstractmethod
    def Refresh(self) -> None: ...
    @abc.abstractmethod
    def ShowAPassingMessage(self, message: str) -> None: ...


class NoAnkiWindows(IAnkiWindows):
    def __init__(self) -> None: ...
    def OpenTheBrowserOn(self, ankiSearch: str) -> None: ...
    def OpenThePreviewerOn(self, ankiSearch: str) -> None: ...
    def Refresh(self) -> None: ...
    def ShowAPassingMessage(self, message: str) -> None: ...

