import typing, abc
from JAStudio.Core.Note.Tevents import INoteTevent, INoteTevent_1, NoteTevent, NoteTevent_1
from Compze.Tessaging.TessageTypes import IPublisherTevent_1
from Compze.Teventive.Taggregates.Tevents import ITaggregateCreatedTevent
from System.Collections.Generic import IReadOnlyList_1
from JAStudio.Core.Note.Vocabulary import EnteredForm
from Compze.Abstractions import TaggregateId
from System import DateTime
from JAStudio.Core.Note import VocabId

class IVocabTevent_GenericClasses(abc.ABCMeta):
    Generic_IVocabTevent_GenericClasses_IVocabTevent_1_TTevent = typing.TypeVar('Generic_IVocabTevent_GenericClasses_IVocabTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_IVocabTevent_GenericClasses_IVocabTevent_1_TTevent]) -> typing.Type[IVocabTevent_1[Generic_IVocabTevent_GenericClasses_IVocabTevent_1_TTevent]]: ...

class IVocabTevent(IVocabTevent_0, metaclass =IVocabTevent_GenericClasses): ...

class IVocabTevent_0(INoteTevent, typing.Protocol):

    class CorpusAnswer_GenericClasses(abc.ABCMeta):
        Generic_CorpusAnswer_GenericClasses_CorpusAnswer_1_TTevent = typing.TypeVar('Generic_CorpusAnswer_GenericClasses_CorpusAnswer_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_CorpusAnswer_GenericClasses_CorpusAnswer_1_TTevent]) -> typing.Type[IVocabTevent.CorpusAnswer_1[Generic_CorpusAnswer_GenericClasses_CorpusAnswer_1_TTevent]]: ...

    CorpusAnswer : CorpusAnswer_GenericClasses

    CorpusAnswer_1_TTevent = typing.TypeVar('CorpusAnswer_1_TTevent', covariant=True)
    class CorpusAnswer_1(typing.Generic[CorpusAnswer_1_TTevent], IVocabTevent_0, IPublisherTevent_1[CorpusAnswer_1_TTevent], typing.Protocol):
        CorpusAnswer_1_TTevent = IVocabTevent_0.CorpusAnswer_1_TTevent
        pass


    class Created(IVocabTevent.Question.TextChanged, ITaggregateCreatedTevent, typing.Protocol):
        pass


    class Explanation_GenericClasses(abc.ABCMeta):
        Generic_Explanation_GenericClasses_Explanation_1_TTevent = typing.TypeVar('Generic_Explanation_GenericClasses_Explanation_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_Explanation_GenericClasses_Explanation_1_TTevent]) -> typing.Type[IVocabTevent.Explanation_1[Generic_Explanation_GenericClasses_Explanation_1_TTevent]]: ...

    Explanation : Explanation_GenericClasses

    Explanation_1_TTevent = typing.TypeVar('Explanation_1_TTevent', covariant=True)
    class Explanation_1(typing.Generic[Explanation_1_TTevent], IVocabTevent_0, IPublisherTevent_1[Explanation_1_TTevent], typing.Protocol):
        Explanation_1_TTevent = IVocabTevent_0.Explanation_1_TTevent
        pass


    class ExplanationLong_GenericClasses(abc.ABCMeta):
        Generic_ExplanationLong_GenericClasses_ExplanationLong_1_TTevent = typing.TypeVar('Generic_ExplanationLong_GenericClasses_ExplanationLong_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_ExplanationLong_GenericClasses_ExplanationLong_1_TTevent]) -> typing.Type[IVocabTevent.ExplanationLong_1[Generic_ExplanationLong_GenericClasses_ExplanationLong_1_TTevent]]: ...

    ExplanationLong : ExplanationLong_GenericClasses

    ExplanationLong_1_TTevent = typing.TypeVar('ExplanationLong_1_TTevent', covariant=True)
    class ExplanationLong_1(typing.Generic[ExplanationLong_1_TTevent], IVocabTevent_0, IPublisherTevent_1[ExplanationLong_1_TTevent], typing.Protocol):
        ExplanationLong_1_TTevent = IVocabTevent_0.ExplanationLong_1_TTevent
        pass


    class Forms(IVocabTevent_0, typing.Protocol):

        class Added(IVocabTevent.Forms, typing.Protocol):
            @property
            def Form(self) -> str: ...


        class NoLongerOwned(IVocabTevent.Forms, typing.Protocol):
            @property
            def Form(self) -> str: ...


        class Owned(IVocabTevent.Forms, typing.Protocol):
            @property
            def Form(self) -> str: ...


        class Removed(IVocabTevent.Forms, typing.Protocol):
            @property
            def Form(self) -> str: ...


        class Replaced(IVocabTevent.Forms, typing.Protocol):
            @property
            def EnteredForms(self) -> IReadOnlyList_1[EnteredForm]: ...



    class Mnemonic_GenericClasses(abc.ABCMeta):
        Generic_Mnemonic_GenericClasses_Mnemonic_1_TTevent = typing.TypeVar('Generic_Mnemonic_GenericClasses_Mnemonic_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_Mnemonic_GenericClasses_Mnemonic_1_TTevent]) -> typing.Type[IVocabTevent.Mnemonic_1[Generic_Mnemonic_GenericClasses_Mnemonic_1_TTevent]]: ...

    Mnemonic : Mnemonic_GenericClasses

    Mnemonic_1_TTevent = typing.TypeVar('Mnemonic_1_TTevent', covariant=True)
    class Mnemonic_1(typing.Generic[Mnemonic_1_TTevent], IVocabTevent_0, IPublisherTevent_1[Mnemonic_1_TTevent], typing.Protocol):
        Mnemonic_1_TTevent = IVocabTevent_0.Mnemonic_1_TTevent
        pass


    class Question(IVocabTevent_0, typing.Protocol):

        class DisambiguationChanged(IVocabTevent.Question, typing.Protocol):
            @property
            def Disambiguation(self) -> str: ...


        class TextChanged(IVocabTevent.Question, typing.Protocol):
            @property
            def Text(self) -> str: ...



    class References_GenericClasses(abc.ABCMeta):
        Generic_References_GenericClasses_References_1_TTevent = typing.TypeVar('Generic_References_GenericClasses_References_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_References_GenericClasses_References_1_TTevent]) -> typing.Type[IVocabTevent.References_1[Generic_References_GenericClasses_References_1_TTevent]]: ...

    References : References_GenericClasses

    References_1_TTevent = typing.TypeVar('References_1_TTevent', covariant=True)
    class References_1(typing.Generic[References_1_TTevent], IVocabTevent_0, IPublisherTevent_1[References_1_TTevent], typing.Protocol):
        References_1_TTevent = IVocabTevent_0.References_1_TTevent
        pass


    class TechnicalNotes_GenericClasses(abc.ABCMeta):
        Generic_TechnicalNotes_GenericClasses_TechnicalNotes_1_TTevent = typing.TypeVar('Generic_TechnicalNotes_GenericClasses_TechnicalNotes_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_TechnicalNotes_GenericClasses_TechnicalNotes_1_TTevent]) -> typing.Type[IVocabTevent.TechnicalNotes_1[Generic_TechnicalNotes_GenericClasses_TechnicalNotes_1_TTevent]]: ...

    TechnicalNotes : TechnicalNotes_GenericClasses

    TechnicalNotes_1_TTevent = typing.TypeVar('TechnicalNotes_1_TTevent', covariant=True)
    class TechnicalNotes_1(typing.Generic[TechnicalNotes_1_TTevent], IVocabTevent_0, IPublisherTevent_1[TechnicalNotes_1_TTevent], typing.Protocol):
        TechnicalNotes_1_TTevent = IVocabTevent_0.TechnicalNotes_1_TTevent
        pass



IVocabTevent_1_TTevent = typing.TypeVar('IVocabTevent_1_TTevent', covariant=True)
class IVocabTevent_1(typing.Generic[IVocabTevent_1_TTevent], INoteTevent_1[IVocabTevent_1_TTevent], typing.Protocol):
    pass


class VocabTevent_GenericClasses(abc.ABCMeta):
    Generic_VocabTevent_GenericClasses_VocabTevent_1_TTevent = typing.TypeVar('Generic_VocabTevent_GenericClasses_VocabTevent_1_TTevent')
    def __getitem__(self, types : typing.Type[Generic_VocabTevent_GenericClasses_VocabTevent_1_TTevent]) -> typing.Type[VocabTevent_1[Generic_VocabTevent_GenericClasses_VocabTevent_1_TTevent]]: ...

class VocabTevent(VocabTevent_0, metaclass =VocabTevent_GenericClasses): ...

class VocabTevent_0(NoteTevent, IVocabTevent_0):
    @property
    def TaggregateId(self) -> TaggregateId: ...
    @property
    def TaggregateVersion(self) -> int: ...
    @property
    def UtcTimeStamp(self) -> DateTime: ...

    class CorpusAnswer_GenericClasses(abc.ABCMeta):
        Generic_CorpusAnswer_GenericClasses_CorpusAnswer_1_TTevent = typing.TypeVar('Generic_CorpusAnswer_GenericClasses_CorpusAnswer_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_CorpusAnswer_GenericClasses_CorpusAnswer_1_TTevent]) -> typing.Type[VocabTevent.CorpusAnswer_1[Generic_CorpusAnswer_GenericClasses_CorpusAnswer_1_TTevent]]: ...

    CorpusAnswer : CorpusAnswer_GenericClasses

    CorpusAnswer_1_TTevent = typing.TypeVar('CorpusAnswer_1_TTevent')
    class CorpusAnswer_1(typing.Generic[CorpusAnswer_1_TTevent], VocabTevent_0, IVocabTevent.CorpusAnswer_1[CorpusAnswer_1_TTevent]):
        CorpusAnswer_1_TTevent = VocabTevent_0.CorpusAnswer_1_TTevent
        def __init__(self, tevent: CorpusAnswer_1_TTevent) -> None: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def Tevent(self) -> CorpusAnswer_1_TTevent: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...


    class Created(VocabTevent_0, IVocabTevent.Created):
        def __init__(self, vocabId: VocabId, question: str) -> None: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def Text(self) -> str: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...


    class Explanation_GenericClasses(abc.ABCMeta):
        Generic_Explanation_GenericClasses_Explanation_1_TTevent = typing.TypeVar('Generic_Explanation_GenericClasses_Explanation_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_Explanation_GenericClasses_Explanation_1_TTevent]) -> typing.Type[VocabTevent.Explanation_1[Generic_Explanation_GenericClasses_Explanation_1_TTevent]]: ...

    Explanation : Explanation_GenericClasses

    Explanation_1_TTevent = typing.TypeVar('Explanation_1_TTevent')
    class Explanation_1(typing.Generic[Explanation_1_TTevent], VocabTevent_0, IVocabTevent.Explanation_1[Explanation_1_TTevent]):
        Explanation_1_TTevent = VocabTevent_0.Explanation_1_TTevent
        def __init__(self, tevent: Explanation_1_TTevent) -> None: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def Tevent(self) -> Explanation_1_TTevent: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...


    class ExplanationLong_GenericClasses(abc.ABCMeta):
        Generic_ExplanationLong_GenericClasses_ExplanationLong_1_TTevent = typing.TypeVar('Generic_ExplanationLong_GenericClasses_ExplanationLong_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_ExplanationLong_GenericClasses_ExplanationLong_1_TTevent]) -> typing.Type[VocabTevent.ExplanationLong_1[Generic_ExplanationLong_GenericClasses_ExplanationLong_1_TTevent]]: ...

    ExplanationLong : ExplanationLong_GenericClasses

    ExplanationLong_1_TTevent = typing.TypeVar('ExplanationLong_1_TTevent')
    class ExplanationLong_1(typing.Generic[ExplanationLong_1_TTevent], VocabTevent_0, IVocabTevent.ExplanationLong_1[ExplanationLong_1_TTevent]):
        ExplanationLong_1_TTevent = VocabTevent_0.ExplanationLong_1_TTevent
        def __init__(self, tevent: ExplanationLong_1_TTevent) -> None: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def Tevent(self) -> ExplanationLong_1_TTevent: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...


    class Forms(abc.ABC):

        class Added(VocabTevent_0, IVocabTevent.Forms.Added):
            def __init__(self, form: str) -> None: ...
            @property
            def Form(self) -> str: ...
            @property
            def TaggregateId(self) -> TaggregateId: ...
            @property
            def TaggregateVersion(self) -> int: ...
            @property
            def UtcTimeStamp(self) -> DateTime: ...


        class NoLongerOwned(VocabTevent_0, IVocabTevent.Forms.NoLongerOwned):
            def __init__(self, form: str) -> None: ...
            @property
            def Form(self) -> str: ...
            @property
            def TaggregateId(self) -> TaggregateId: ...
            @property
            def TaggregateVersion(self) -> int: ...
            @property
            def UtcTimeStamp(self) -> DateTime: ...


        class Owned(VocabTevent_0, IVocabTevent.Forms.Owned):
            def __init__(self, form: str) -> None: ...
            @property
            def Form(self) -> str: ...
            @property
            def TaggregateId(self) -> TaggregateId: ...
            @property
            def TaggregateVersion(self) -> int: ...
            @property
            def UtcTimeStamp(self) -> DateTime: ...


        class Removed(VocabTevent_0, IVocabTevent.Forms.Removed):
            def __init__(self, form: str) -> None: ...
            @property
            def Form(self) -> str: ...
            @property
            def TaggregateId(self) -> TaggregateId: ...
            @property
            def TaggregateVersion(self) -> int: ...
            @property
            def UtcTimeStamp(self) -> DateTime: ...


        class Replaced(VocabTevent_0, IVocabTevent.Forms.Replaced):
            def __init__(self, enteredForms: IReadOnlyList_1[EnteredForm]) -> None: ...
            @property
            def EnteredForms(self) -> IReadOnlyList_1[EnteredForm]: ...
            @property
            def TaggregateId(self) -> TaggregateId: ...
            @property
            def TaggregateVersion(self) -> int: ...
            @property
            def UtcTimeStamp(self) -> DateTime: ...



    class Mnemonic_GenericClasses(abc.ABCMeta):
        Generic_Mnemonic_GenericClasses_Mnemonic_1_TTevent = typing.TypeVar('Generic_Mnemonic_GenericClasses_Mnemonic_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_Mnemonic_GenericClasses_Mnemonic_1_TTevent]) -> typing.Type[VocabTevent.Mnemonic_1[Generic_Mnemonic_GenericClasses_Mnemonic_1_TTevent]]: ...

    Mnemonic : Mnemonic_GenericClasses

    Mnemonic_1_TTevent = typing.TypeVar('Mnemonic_1_TTevent')
    class Mnemonic_1(typing.Generic[Mnemonic_1_TTevent], VocabTevent_0, IVocabTevent.Mnemonic_1[Mnemonic_1_TTevent]):
        Mnemonic_1_TTevent = VocabTevent_0.Mnemonic_1_TTevent
        def __init__(self, tevent: Mnemonic_1_TTevent) -> None: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def Tevent(self) -> Mnemonic_1_TTevent: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...


    class Question(abc.ABC):

        class DisambiguationChanged(VocabTevent_0, IVocabTevent.Question.DisambiguationChanged):
            def __init__(self, disambiguation: str) -> None: ...
            @property
            def Disambiguation(self) -> str: ...
            @property
            def TaggregateId(self) -> TaggregateId: ...
            @property
            def TaggregateVersion(self) -> int: ...
            @property
            def UtcTimeStamp(self) -> DateTime: ...


        class TextChanged(VocabTevent_0, IVocabTevent.Question.TextChanged):
            def __init__(self, text: str) -> None: ...
            @property
            def TaggregateId(self) -> TaggregateId: ...
            @property
            def TaggregateVersion(self) -> int: ...
            @property
            def Text(self) -> str: ...
            @property
            def UtcTimeStamp(self) -> DateTime: ...



    class References_GenericClasses(abc.ABCMeta):
        Generic_References_GenericClasses_References_1_TTevent = typing.TypeVar('Generic_References_GenericClasses_References_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_References_GenericClasses_References_1_TTevent]) -> typing.Type[VocabTevent.References_1[Generic_References_GenericClasses_References_1_TTevent]]: ...

    References : References_GenericClasses

    References_1_TTevent = typing.TypeVar('References_1_TTevent')
    class References_1(typing.Generic[References_1_TTevent], VocabTevent_0, IVocabTevent.References_1[References_1_TTevent]):
        References_1_TTevent = VocabTevent_0.References_1_TTevent
        def __init__(self, tevent: References_1_TTevent) -> None: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def Tevent(self) -> References_1_TTevent: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...


    class TechnicalNotes_GenericClasses(abc.ABCMeta):
        Generic_TechnicalNotes_GenericClasses_TechnicalNotes_1_TTevent = typing.TypeVar('Generic_TechnicalNotes_GenericClasses_TechnicalNotes_1_TTevent')
        def __getitem__(self, types : typing.Type[Generic_TechnicalNotes_GenericClasses_TechnicalNotes_1_TTevent]) -> typing.Type[VocabTevent.TechnicalNotes_1[Generic_TechnicalNotes_GenericClasses_TechnicalNotes_1_TTevent]]: ...

    TechnicalNotes : TechnicalNotes_GenericClasses

    TechnicalNotes_1_TTevent = typing.TypeVar('TechnicalNotes_1_TTevent')
    class TechnicalNotes_1(typing.Generic[TechnicalNotes_1_TTevent], VocabTevent_0, IVocabTevent.TechnicalNotes_1[TechnicalNotes_1_TTevent]):
        TechnicalNotes_1_TTevent = VocabTevent_0.TechnicalNotes_1_TTevent
        def __init__(self, tevent: TechnicalNotes_1_TTevent) -> None: ...
        @property
        def TaggregateId(self) -> TaggregateId: ...
        @property
        def TaggregateVersion(self) -> int: ...
        @property
        def Tevent(self) -> TechnicalNotes_1_TTevent: ...
        @property
        def UtcTimeStamp(self) -> DateTime: ...



VocabTevent_1_TTevent = typing.TypeVar('VocabTevent_1_TTevent')
class VocabTevent_1(typing.Generic[VocabTevent_1_TTevent], NoteTevent_1[VocabTevent_1_TTevent], IVocabTevent_1[VocabTevent_1_TTevent]):
    def __init__(self, tevent: VocabTevent_1_TTevent) -> None: ...
    @property
    def Tevent(self) -> VocabTevent_1_TTevent: ...

