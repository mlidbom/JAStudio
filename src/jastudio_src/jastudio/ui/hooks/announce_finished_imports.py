"""Tells JAStudio when Anki finished importing a file, such as an .apkg, so that the import profiles that run automatically can run.

Anki's importer adds the notes in its Rust backend, so note_will_be_added doesn't fire for them: the finished operation is the only signal."""
from __future__ import annotations

from typing import TYPE_CHECKING

from aqt import gui_hooks
from aqt.import_export.import_dialog import ImportDialog

from jastudio.ankiutils import app

if TYPE_CHECKING:
    from anki.collection import OpChanges


def _announce_a_finished_import(_changes: OpChanges, handler: object | None) -> None:
    if not _is_the_import_dialogs_import(handler): return
    from jastudio.ui import dotnet_ui_root
    dotnet_ui_root().AnkiImportedAFile()

def _is_the_import_dialogs_import(handler: object | None) -> bool:
    """The import dialog's page runs the import as an operation, whose handler is the active window: the import dialog, verified in Anki 26.9.3.
    While Anki isn't the active application, the handler is the main window instead, but the dialog is still modal until its page reports the import done."""
    return isinstance(handler, ImportDialog) or isinstance(app.main_window().app.activeModalWidget(), ImportDialog)

def init() -> None:
    gui_hooks.operation_did_execute.append(_announce_a_finished_import)  # pyright: ignore[reportUnknownMemberType]  # Anki's generated hook names its parameter "anki.collection.OpChanges" in a module that never runs `import anki.collection`, so pyright can't resolve it
