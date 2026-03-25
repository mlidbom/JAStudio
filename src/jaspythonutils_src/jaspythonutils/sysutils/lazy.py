from __future__ import annotations

from typing import TYPE_CHECKING

from autoslot import Slots

if TYPE_CHECKING:
    from jaspythonutils.sysutils.standard_type_aliases import Func

class Lazy[T](Slots):
    def __init__(self, factory: Func[T]) -> None:
        self.factory: Func[T] = factory
        self._instance: T | None = None

    def __call__(self) -> T:
        if self._instance is None:
            self._instance = self.factory()
        return self._instance