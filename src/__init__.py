from __future__ import annotations

import os
import sys

is_testing = "pytest" in sys.modules
if not is_testing:
    from pathlib import Path

    top_dir = Path(os.path.dirname(os.path.abspath(__file__)))
    sys.path.append(str(top_dir / "jastudio_src"))
    sys.path.append(str(top_dir / "jastudio_src" / "_lib"))
    sys.path.append(str(top_dir / "jaspythonutils_src"))

    from jastudio.ankiutils import app  # noqa

    app.addon_name = os.path.basename(str(top_dir))
    from jastudio import ui  # noqa

    ui.init()

    if app.config().EnableAutomaticGarbageCollection.Value:
        import gc

        gc.enable()
