from __future__ import annotations

import atexit
import traceback
from typing import TYPE_CHECKING

from jaspythonutils.sysutils.typed import non_optional

from jastudio import mylog

if TYPE_CHECKING:
    from JAStudio.Anki.PythonInterop import JAStudioAnkiAppRoot

# The JAStudioAppRoot composition root instance, set during init().
# Use dotnet_ui_root() to access — never import the backing field directly.
_dotnet_ui_root_instance: JAStudioAnkiAppRoot | None = None

def dotnet_ui_root() -> JAStudioAnkiAppRoot:
    return non_optional(_dotnet_ui_root_instance)

def init() -> None:
    _init_dot_net_app()
    from jastudio.ui import hooks, jastudio_menu, menus, timing_hacks, web
    hooks.init()
    timing_hacks.init()
    jastudio_menu.init()
    web.init()
    menus.init()

def _init_dot_net_app() -> None:
    global _dotnet_ui_root_instance
    from JAStudio.Anki.PythonInterop import JAStudioAnkiAppRoot

    try:
        _dotnet_ui_root_instance = JAStudioAnkiAppRoot.Initialize()
    except Exception:
        # Anki shows the failure in a dialog only, and every later use of JAStudio then fails with an error that doesn't say why: the log keeps the reason.
        mylog.error(f"JAStudio's .NET side failed to start:\n{traceback.format_exc()}")
        raise

    def shutdown_dot_net() -> None:
        mylog.info("Shutting down DotNet UI")
        dotnet_ui_root().ShutDown()

    atexit.register(shutdown_dot_net)
