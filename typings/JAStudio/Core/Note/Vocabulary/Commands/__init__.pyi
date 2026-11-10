import abc
from JAStudio.Core.Note.Commands import NoteChangingCommand_1, NoteChangingCommandTakingAText_1, INoteCommand_1
from JAStudio.Core.Note.Vocabulary import VocabNote, VocabCloner, CreatedOrExistingVocab
from System.Collections.Generic import IReadOnlyList_1
from System import Func_2, Func_3

class VocabCommands(abc.ABC):
    AcceptMeaning : NoteChangingCommand_1[VocabNote]
    AddSynonymsTransitivelyOneLevel : NoteChangingCommandTakingAText_1[VocabNote]
    AutogenerateCompoundParts : NoteChangingCommand_1[VocabNote]
    CloneToForm : VocabCreatingCommandTakingAText
    CloneWithDisambiguation : VocabCreatingCommandTakingAText
    CreateBaForm : VocabCreatingCommand
    CreateCausativeForm : VocabCreatingCommand
    CreateEStem : VocabCreatingCommand
    CreateImperative : VocabCreatingCommand
    CreateKaSuffixed : VocabCreatingCommand
    CreateKuForm : VocabCreatingCommand
    CreateMasuForm : VocabCreatingCommand
    CreateNaAdjective : VocabCreatingCommand
    CreateNaiForm : VocabCreatingCommand
    CreateNiAdverb : VocabCreatingCommand
    CreateNoAdjective : VocabCreatingCommand
    CreateNSuffixed : VocabCreatingCommand
    CreateOPrefixed : VocabCreatingCommand
    CreatePotentialGodan : VocabCreatingCommand
    CreateReceptiveForm : VocabCreatingCommand
    CreateSaForm : VocabCreatingCommand
    CreateShimasuVerb : VocabCreatingCommand
    CreateSuruVerb : VocabCreatingCommand
    CreateTaForm : VocabCreatingCommand
    CreateTeForm : VocabCreatingCommand
    CreateTePrefixed : VocabCreatingCommand
    CreateToAdverb : VocabCreatingCommand
    GenerateAnswer : NoteChangingCommand_1[VocabNote]
    PrefixOntoChopping1 : VocabCreatingCommandTakingAText
    PrefixOntoChopping2 : VocabCreatingCommandTakingAText
    PrefixOntoChopping3 : VocabCreatingCommandTakingAText
    PrefixOntoDictionaryForm : VocabCreatingCommandTakingAText
    RepopulatePartsOfSpeech : NoteChangingCommand_1[VocabNote]
    SuffixOntoAStem : VocabCreatingCommandTakingAText
    SuffixOntoChopping1 : VocabCreatingCommandTakingAText
    SuffixOntoChopping2 : VocabCreatingCommandTakingAText
    SuffixOntoChopping3 : VocabCreatingCommandTakingAText
    SuffixOntoChopping4 : VocabCreatingCommandTakingAText
    SuffixOntoDictionaryForm : VocabCreatingCommandTakingAText
    SuffixOntoEStem : VocabCreatingCommandTakingAText
    SuffixOntoIStem : VocabCreatingCommandTakingAText
    SuffixOntoTeStem : VocabCreatingCommandTakingAText
    @classmethod
    @property
    def All(cls) -> IReadOnlyList_1[INoteCommand_1[VocabNote]]: ...


class VocabCreatingCommand(INoteCommand_1[VocabNote]):
    def __init__(self, name: str, label: str, create: Func_2[VocabCloner, CreatedOrExistingVocab]) -> None: ...
    @property
    def CreatesAVocab(self) -> bool: ...
    @property
    def Label(self) -> str: ...
    @property
    def Name(self) -> str: ...
    @property
    def TextTaken(self) -> str: ...
    def CreateFrom(self, vocab: VocabNote) -> CreatedOrExistingVocab: ...
    def RunOn(self, note: VocabNote, text: str) -> None: ...
    def ToString(self) -> str: ...


class VocabCreatingCommandTakingAText(INoteCommand_1[VocabNote]):
    def __init__(self, name: str, label: str, textTaken: str, create: Func_3[VocabCloner, str, CreatedOrExistingVocab]) -> None: ...
    @property
    def CreatesAVocab(self) -> bool: ...
    @property
    def Label(self) -> str: ...
    @property
    def Name(self) -> str: ...
    @property
    def TextTaken(self) -> str: ...
    def CreateFrom(self, vocab: VocabNote, text: str) -> CreatedOrExistingVocab: ...
    def RunOn(self, note: VocabNote, text: str) -> None: ...
    def ToString(self) -> str: ...

