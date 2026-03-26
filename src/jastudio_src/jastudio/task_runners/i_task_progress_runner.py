from __future__ import annotations

from typing import TYPE_CHECKING

from jaspythonutils.sysutils.abstract_method_called_error import AbstractMethodCalledError

if TYPE_CHECKING:
    from collections.abc import Callable

# noinspection PyMethodMayBeStatic
class ITaskRunner:
    # noinspection PyUnusedFunction,Annotator
    def process_with_progress[TInput, TOutput](self, items: list[TInput], process_item: Callable[[TInput], TOutput], message: str) -> list[TOutput]: raise AbstractMethodCalledError()  # pyright: ignore[reportUnusedParameter]
    # noinspection Annotator
    def set_label_text(self, text: str) -> None: raise AbstractMethodCalledError()  # pyright: ignore[reportUnusedParameter]
    def close(self) -> None: raise AbstractMethodCalledError()
    # noinspection PyUnusedFunction, Annotator
    def run_on_background_thread_with_spinning_progress_dialog[TResult](self, message: str, action: Callable[[], TResult]) -> TResult: raise AbstractMethodCalledError()  # pyright: ignore[reportUnusedParameter]
    # noinspection PyUnusedFunction
    def is_hidden(self) -> bool: return True
