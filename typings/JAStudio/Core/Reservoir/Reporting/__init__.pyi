from System.IO import DirectoryInfo
from System.Collections.Generic import IReadOnlyList_1
from JAStudio.Core.Reservoir.Sources.AnkiPackages import WhatADeckGives
from JAStudio.Core.Reservoir import ReservoirFile
from JAStudio.Core.Reservoir.Measures import ReservoirMeasures, WhatTheCorpusHas
from JAStudio.Core.Note.Collection import JPCollection
from JAStudio.Core.Reservoir.Picking import SentencesWorthTakingIn

class DeckCheckReport:
    @staticmethod
    def WriteTo(reportFolder: DirectoryInfo, decksFolder: DirectoryInfo, decks: IReadOnlyList_1[WhatADeckGives]) -> None: ...


class ReservoirReport:
    @staticmethod
    def WriteTo(folder: DirectoryInfo, reservoir: ReservoirFile, measures: ReservoirMeasures, corpus: WhatTheCorpusHas, collection: JPCollection, picks: SentencesWorthTakingIn) -> None: ...

