import typing, abc
from System import Func_5

class AppendingPrerenderer_GenericClasses(abc.ABCMeta):
    Generic_AppendingPrerenderer_GenericClasses_AppendingPrerenderer_1_TNote = typing.TypeVar('Generic_AppendingPrerenderer_GenericClasses_AppendingPrerenderer_1_TNote')
    def __getitem__(self, types : typing.Type[Generic_AppendingPrerenderer_GenericClasses_AppendingPrerenderer_1_TNote]) -> typing.Type[AppendingPrerenderer_1[Generic_AppendingPrerenderer_GenericClasses_AppendingPrerenderer_1_TNote]]: ...

AppendingPrerenderer : AppendingPrerenderer_GenericClasses

AppendingPrerenderer_1_TNote = typing.TypeVar('AppendingPrerenderer_1_TNote')
class AppendingPrerenderer_1(typing.Generic[AppendingPrerenderer_1_TNote]):
    def __init__(self, renderIframe: Func_5[AppendingPrerenderer_1_TNote, str, str, str, str]) -> None: ...
    def Render(self, note: AppendingPrerenderer_1_TNote, html: str, typeOfDisplay: str, cardTemplateName: str) -> str: ...


class CardServerUrl(abc.ABC):
    @classmethod
    @property
    def BaseUrl(cls) -> str: ...
    @classmethod
    @BaseUrl.setter
    def BaseUrl(cls, value: str) -> str: ...
    @staticmethod
    def MediaUrl(fileName: str) -> str: ...

