import typing, abc
from Compze.Tessaging.TessageTypes import IPublisherTevent_1

class PublisherTevent_GenericClasses(abc.ABCMeta):
    Generic_PublisherTevent_GenericClasses_PublisherTevent_1_TTevent = typing.TypeVar('Generic_PublisherTevent_GenericClasses_PublisherTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_PublisherTevent_GenericClasses_PublisherTevent_1_TTevent]) -> typing.Type[PublisherTevent_1[Generic_PublisherTevent_GenericClasses_PublisherTevent_1_TTevent]]: ...

PublisherTevent : PublisherTevent_GenericClasses

PublisherTevent_1_TTevent = typing.TypeVar('PublisherTevent_1_TTevent')
class PublisherTevent_1(typing.Generic[PublisherTevent_1_TTevent], IPublisherTevent_1[PublisherTevent_1_TTevent]):
    def __init__(self, tevent: PublisherTevent_1_TTevent) -> None: ...
    @property
    def Tevent(self) -> PublisherTevent_1_TTevent: ...

