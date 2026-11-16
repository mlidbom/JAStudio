"""The user's own folder for JAStudio's data, whichever checkout a JAStudio was started from: where the Anki add-on and JAStudio find each
other, by the two access files.
"""
from __future__ import annotations

import os
import sys
from pathlib import Path


def the_users_folder_for_jastudio() -> Path:
    """The folder, on each platform where .NET has it (Environment.SpecialFolder.LocalApplicationData), since JAStudio names it by that
    (ThisUsersFolderForJAStudio)."""
    if sys.platform == "win32":
        the_users_local_data = Path(os.environ["LOCALAPPDATA"])
    elif sys.platform == "darwin":
        the_users_local_data = Path.home() / "Library" / "Application Support"
    else:
        the_users_local_data = Path(os.environ.get("XDG_DATA_HOME") or Path.home() / ".local" / "share")
    return the_users_local_data / "JAStudio"
