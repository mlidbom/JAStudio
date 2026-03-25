from __future__ import annotations

from jastudio import mylog
from jastudio.ankiutils import app
from jastudio.sysutils import app_thread_pool


def collect_on_ui_thread_and_display_message(message: str = "Garbage collecting") -> None:
    def collect_with_progress() -> None:
        mylog.info("collect_with_progress")
        import gc
        app.get_ui_utils().tool_tip(message, 6000)
        gc.collect()

    app_thread_pool.run_on_ui_thread_synchronously(collect_with_progress)
