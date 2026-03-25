from __future__ import annotations  # noqa: I001

# noinspection Annotator
from jastudio.dotnet import load_dotnet_runtime  # pyright: ignore [reportUnusedImport]  # noqa: F401

import os
from typing import TYPE_CHECKING

from jastudio import mylog
from jaspythonutils.sysutils.typed import checked_cast, non_optional
from jastudio.note.collection.anki_collection_synchronizer import AnkiCollectionSynchronizer
from jastudio.testutils import ex_pytest

from JAStudio.Anki import AnkiLifecycleEvent
from aqt import gui_hooks

is_testing = ex_pytest.is_testing

if TYPE_CHECKING:
    from anki.collection import Collection
    from anki.dbproxy import DBProxy
    from anki.scheduler.v3 import Scheduler  # pyright: ignore[reportMissingTypeStubs]
    from aqt import AnkiQt  # type: ignore[attr-defined]  # pyright: ignore[reportPrivateImportUsage]
    from JAStudio.Core.Configuration import JapaneseConfig

    from jastudio.anki_extentions.config_manager_ex import ConfigManagerEx
    from jastudio.ankiutils.ui_utils_interface import IUIUtils

_synchronizer: AnkiCollectionSynchronizer | None = None

addon_name: str = "should_be_replaced_by_init"

_profile_open = False

def _notify_dotnet(event: AnkiLifecycleEvent) -> None:
    from jastudio.ui import dotnet_ui_root
    dotnet_ui_root().HandleAnkiLifecycleEvent(event)

def config() -> JapaneseConfig:
    from jastudio.ui import dotnet_ui_root
    return dotnet_ui_root().Services.ConfigurationStore.Config()

def is_initialized() -> bool:
    return _synchronizer is not None

def _ensure_initialized() -> None:
    global _synchronizer
    if _synchronizer: return
    _synchronizer = AnkiCollectionSynchronizer()

def anki_config() -> ConfigManagerEx:
    from jastudio.anki_extentions.config_manager_ex import ConfigManagerEx
    return ConfigManagerEx(anki_collection().conf)

def anki_collection() -> Collection:
    from aqt import mw
    if mw.col is None: raise AssertionError("Anki collection not available")
    return mw.col

def anki_db() -> DBProxy: return non_optional(anki_collection().db)

def anki_scheduler() -> Scheduler:
    from anki.scheduler.v3 import Scheduler  # pyright: ignore[reportMissingTypeStubs]
    return checked_cast(Scheduler, anki_collection().sched)

def main_window() -> AnkiQt:
    from aqt import mw
    return non_optional(mw)

def get_ui_utils() -> IUIUtils:
    from jastudio.ankiutils.ui_utils import UIUtils
    return UIUtils(main_window())

def _profile_opened() -> None:
    global _profile_open
    _profile_open = True
    mylog.info("profile_opened")
    _ensure_initialized()
    _notify_dotnet(AnkiLifecycleEvent.ProfileOpened)
    non_optional(_synchronizer).start()

def _profile_closing() -> None:
    non_optional(_synchronizer).stop()
    _notify_dotnet(AnkiLifecycleEvent.ProfileClosing)

def _sync_will_start(_col: object | None = None) -> None: _notify_dotnet(AnkiLifecycleEvent.SyncStarting)
def _sync_did_finish() -> None: _notify_dotnet(AnkiLifecycleEvent.SyncCompleted)

gui_hooks.profile_will_close.append(_profile_closing)
gui_hooks.profile_did_open.append(_profile_opened)

gui_hooks.sync_will_start.append(_sync_will_start)
gui_hooks.sync_did_finish.append(_sync_did_finish)

user_files_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..\\user_files")
