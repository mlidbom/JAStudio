import typing, abc
from System import IEquatable_1
from System.Collections.Generic import IEnumerable_1
from JAStudio.Core.Note import JPNote
from JAStudio.Core.Note.Search.TextProperties import TextProperty
from JAStudio.Core.Note.Search.NoteTypeProperties import NoteTypeProperty
from JAStudio.Core.Note.Search.TrueOrFalseProperties import TrueOrFalseProperty
from JAStudio.Core.Note.Search.TagProperties import TagsProperty

class NoteCondition(IEquatable_1[NoteCondition], abc.ABC):
    @property
    def IsEmpty(self) -> bool: ...
    @abc.abstractmethod
    def DescriptionLines(self) -> IEnumerable_1[str]: ...
    def GetHashCode(self) -> int: ...
    @abc.abstractmethod
    def Matches(self, note: JPNote) -> bool: ...
    def __eq__(self, left: NoteCondition, right: NoteCondition) -> bool: ...
    def __ne__(self, left: NoteCondition, right: NoteCondition) -> bool: ...
    def ToString(self) -> str: ...
    # Skipped Equals due to it being static, abstract and generic.

    Equals : Equals_MethodGroup
    class Equals_MethodGroup:
        @typing.overload
        def __call__(self, other: NoteCondition) -> bool:...
        @typing.overload
        def __call__(self, obj: typing.Any) -> bool:...



class NoteProperties(abc.ABC):
    Answer : TextProperty
    NoteType : NoteTypeProperty
    Question : TextProperty
    StudyingListening : TrueOrFalseProperty
    StudyingReading : TrueOrFalseProperty
    Tags : TagsProperty


class NoteProperty(abc.ABC):
    @property
    def Name(self) -> str: ...
    def ToString(self) -> str: ...

