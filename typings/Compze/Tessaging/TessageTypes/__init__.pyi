import typing, abc

class IPublisherTevent_GenericClasses(abc.ABCMeta):
    Generic_IPublisherTevent_GenericClasses_IPublisherTevent_1_TTevent = typing.TypeVar('Generic_IPublisherTevent_GenericClasses_IPublisherTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_IPublisherTevent_GenericClasses_IPublisherTevent_1_TTevent]) -> typing.Type[IPublisherTevent_1[Generic_IPublisherTevent_GenericClasses_IPublisherTevent_1_TTevent]]: ...

IPublisherTevent : IPublisherTevent_GenericClasses

IPublisherTevent_1_TTevent = typing.TypeVar('IPublisherTevent_1_TTevent', covariant=True)
class IPublisherTevent_1(typing.Generic[IPublisherTevent_1_TTevent], ITevent, typing.Protocol):
    @property
    def Tevent(self) -> IPublisherTevent_1_TTevent: ...


class ITessage(typing.Protocol):
    pass


class ITevent(ITessage, typing.Protocol):
    pass

