from System.Collections.Generic import IReadOnlyList_1
from JAStudio.Core.Note.ConditionTrees import ConditionPosition

class CheckedCondition:
    @property
    def Children(self) -> IReadOnlyList_1[CheckedCondition]: ...
    @property
    def IsMet(self) -> bool: ...
    def At(self, position: ConditionPosition) -> CheckedCondition: ...

