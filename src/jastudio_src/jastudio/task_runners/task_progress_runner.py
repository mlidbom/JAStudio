from __future__ import annotations

from contextlib import contextmanager
from typing import TYPE_CHECKING

from autoslot import Slots
from jaspythonutils.sysutils.typed import non_optional
from jastudio.ankiutils import app
from jastudio.task_runners.invisible_task_progress_runner import InvisibleTaskRunner

if TYPE_CHECKING:
    from collections.abc import Callable, Iterator

    from jastudio.task_runners.i_task_progress_runner import ITaskRunner

class TaskRunner(Slots):
    _ui_task_runner_factory: Callable[[str, str, bool, bool], ITaskRunner] | None = None

    # noinspection PyUnusedFunction
    @classmethod
    def set_ui_task_runner_factory(cls, factory: Callable[[str, str, bool, bool], ITaskRunner]) -> None:
        if cls._ui_task_runner_factory is not None: raise RuntimeError("UI task runner factory already set.")
        cls._ui_task_runner_factory = factory

    @classmethod
    def create(cls, window_title: str, label_text: str, visible: bool | None = None, allow_cancel: bool = True, modal: bool = False) -> ITaskRunner:
        if visible is None: visible = not app.is_testing
        if not visible:
            return InvisibleTaskRunner(window_title, label_text)

        if cls._ui_task_runner_factory is None: raise RuntimeError("No UI task runner factory set. Set it with TaskRunner.set_ui_task_runner_factory().")

        return cls._ui_task_runner_factory(window_title, label_text, allow_cancel, modal)

    _depth: int = 0
    _current: ITaskRunner | None = None

    @classmethod
    @contextmanager
    def current(cls, window_title: str, label_text: str | None = None, force_hide: bool = False, allow_cancel: bool = True, modal: bool = False) -> Iterator[ITaskRunner]:
        cls._depth += 1
        if cls._depth == 1:
            visible = (not app.is_testing) and not force_hide
            cls._current = cls.create(window_title, label_text or window_title, visible, allow_cancel, modal)
        else:
            non_optional(cls._current).set_label_text(label_text or window_title)

        try: yield non_optional(cls._current)
        finally:
            cls._depth -= 1
            if cls._depth == 0:
                non_optional(cls._current).close()
                cls._current = None
