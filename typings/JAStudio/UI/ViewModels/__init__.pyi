import typing
from System import IDisposable, Action_1
from JAStudio.Core import TemporaryServiceCollection
from System.Collections.Generic import IReadOnlyList_1

class TeventsObservedOnTheUIThread(IDisposable):
    def __init__(self, services: TemporaryServiceCollection, observe: Action_1[TeventsObservedOnTheUIThread.Registrar]) -> None: ...
    def Dispose(self) -> None: ...

    class Registrar:
        # Skipped ForEachUnitOfWork due to it being static, abstract and generic.

        ForEachUnitOfWork : ForEachUnitOfWork_MethodGroup
        class ForEachUnitOfWork_MethodGroup:
            def __getitem__(self, t:typing.Type[ForEachUnitOfWork_1_T1]) -> ForEachUnitOfWork_1[ForEachUnitOfWork_1_T1]: ...

            ForEachUnitOfWork_1_T1 = typing.TypeVar('ForEachUnitOfWork_1_T1')
            class ForEachUnitOfWork_1(typing.Generic[ForEachUnitOfWork_1_T1]):
                ForEachUnitOfWork_1_TTevent = TeventsObservedOnTheUIThread.Registrar.ForEachUnitOfWork_MethodGroup.ForEachUnitOfWork_1_T1
                def __call__(self, observer: Action_1[IReadOnlyList_1[ForEachUnitOfWork_1_TTevent]]) -> TeventsObservedOnTheUIThread.Registrar:...


        # Skipped ForTevent due to it being static, abstract and generic.

        ForTevent : ForTevent_MethodGroup
        class ForTevent_MethodGroup:
            def __getitem__(self, t:typing.Type[ForTevent_1_T1]) -> ForTevent_1[ForTevent_1_T1]: ...

            ForTevent_1_T1 = typing.TypeVar('ForTevent_1_T1')
            class ForTevent_1(typing.Generic[ForTevent_1_T1]):
                ForTevent_1_TTevent = TeventsObservedOnTheUIThread.Registrar.ForTevent_MethodGroup.ForTevent_1_T1
                def __call__(self, observer: Action_1[ForTevent_1_TTevent]) -> TeventsObservedOnTheUIThread.Registrar:...




