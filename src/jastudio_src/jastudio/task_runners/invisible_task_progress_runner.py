from __future__ import annotations

from typing import TYPE_CHECKING, override

from autoslot import Slots
from jaspythonutils.sysutils.timeutil import StopWatch
from jastudio import mylog
from jastudio.task_runners.i_task_progress_runner import ITaskRunner

if TYPE_CHECKING:
    from collections.abc import Callable


class InvisibleTaskRunner(ITaskRunner, Slots):
    # noinspection PyUnusedLocal
    def __init__(self, window_title: str, label_text: str) -> None:  # pyright: ignore
        pass

    # noinspection PyUnusedFunction
    @override
    def process_with_progress[TInput, TOutput](self, items: list[TInput], process_item: Callable[[TInput], TOutput], message: str) -> list[TOutput]:
        result = [process_item(item) for item in items]
        total_items = len(items)
        watch = StopWatch()
        mylog.debug(f"##--InvisibleTaskRunner--## Finished {message} in {watch.elapsed_formatted()} handled {total_items} items")
        return result
    @override
    def set_label_text(self, text: str) -> None: pass  # pyright: ignore
    @override
    def close(self) -> None: pass

    # noinspection PyUnusedFunction
    @override
    def run_on_background_thread_with_spinning_progress_dialog[TResult](self, message: str, action: Callable[[], TResult]) -> TResult:  # pyright: ignore
        watch = StopWatch()
        result = action()
        mylog.debug(f"##--QtTaskProgressRunner--## Finished {message} in {watch.elapsed_formatted()}")
        return result
