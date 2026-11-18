import typing, abc
from Compze.Abstractions import TaggregateId

class ITaggregateReference_GenericClasses(abc.ABCMeta):
    Generic_ITaggregateReference_GenericClasses_ITaggregateReference_1_TTaggregate = typing.TypeVar('Generic_ITaggregateReference_GenericClasses_ITaggregateReference_1_TTaggregate')
    def __getitem__(self, types : typing.Type[Generic_ITaggregateReference_GenericClasses_ITaggregateReference_1_TTaggregate]) -> typing.Type[ITaggregateReference_1[Generic_ITaggregateReference_GenericClasses_ITaggregateReference_1_TTaggregate]]: ...

ITaggregateReference : ITaggregateReference_GenericClasses

ITaggregateReference_1_TTaggregate = typing.TypeVar('ITaggregateReference_1_TTaggregate', covariant=True)
class ITaggregateReference_1(typing.Generic[ITaggregateReference_1_TTaggregate], typing.Protocol):
    @property
    def Current(self) -> ITaggregateReference_1_TTaggregate: ...
    @property
    def Id(self) -> TaggregateId: ...

