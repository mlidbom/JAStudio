import abc
from JAStudio.Core.Note.KanjiReadings import KanjiReadingList
from System.Collections.Generic import List_1

class KanjiReadingsHtml(abc.ABC):
    @staticmethod
    def Listed(readings: KanjiReadingList) -> str: ...
    @staticmethod
    def Of(readings: KanjiReadingList) -> List_1[str]: ...

