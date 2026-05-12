from __future__ import annotations

from concurrent.futures.thread import ThreadPoolExecutor
from typing import TYPE_CHECKING

from jaspythonutils.sysutils.typed import non_optional
from jastudio.ankiutils import app
from PyQt6.QtCore import QCoreApplication, QThread

if TYPE_CHECKING:

    from jaspythonutils.sysutils.standard_type_aliases import Action

pool = ThreadPoolExecutor()

def current_is_ui_thread() -> bool:
    return bool(QCoreApplication.instance() and non_optional(QCoreApplication.instance()).thread() == QThread.currentThread())

def run_on_ui_thread_fire_and_forget(func: Action) -> None:
    if app.is_testing or current_is_ui_thread():
        func()

    return app.main_window().taskman.run_on_main(func)
