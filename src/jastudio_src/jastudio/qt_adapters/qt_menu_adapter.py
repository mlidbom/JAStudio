"""
PyQt adapter for UI-agnostic menu specifications.
"""
from __future__ import annotations

from typing import TYPE_CHECKING

from aqt.qt import QAction, QKeySequence, QMenu
from jaspythonutils.sysutils.typed import non_optional

if TYPE_CHECKING:
    from collections.abc import Iterable

    from JAStudio.UI.Menus.UIAgnosticMenuStructure import SpecMenuItem, SpecMenuItemKind
else:
    from JAStudio.UI.Menus.UIAgnosticMenuStructure import SpecMenuItemKind

def add_to_qt_menu(target_qt_menu: QMenu, specs: Iterable[SpecMenuItem]) -> None:
    """
    Add C# menu specs to a parent QMenu. This is the single integration point for adding C#-defined menus to PyQt.
    """
    for spec in specs:
        if not spec.IsVisible:
            continue

        name_with_accelerator = _qt_name(spec.Name)

        if spec.Kind == SpecMenuItemKind.Submenu:
            qt_sub_menu = non_optional(target_qt_menu.addMenu(name_with_accelerator))
            qt_sub_menu.setEnabled(spec.IsEnabled)
            add_to_qt_menu(qt_sub_menu, spec.Children)
        elif spec.Kind == SpecMenuItemKind.Separator:
            target_qt_menu.addSeparator()
        elif spec.Kind == SpecMenuItemKind.Command:
            action = QAction(_qt_text(name_with_accelerator, spec.KeyboardShortcut), target_qt_menu)
            action.setEnabled(spec.IsEnabled)
            action.triggered.connect(lambda urdu, a=spec.Action: a.Invoke())  # pyright: ignore [reportUnknownMemberType, reportUnknownLambdaType]
            target_qt_menu.addAction(action)  # pyright: ignore [reportUnknownMemberType]
        else:
            raise Exception(f"Unknown menu spec type: {spec.Name}")

def _qt_text(name_with_accelerator: str, keyboard_shortcut: str | None) -> str:
    """The command's text, with its shortcut after a tab, which Qt shows beside it without binding it.
    Binding a menu's shortcuts is the job of whoever owns the menu, as in the workspace: in Anki, global_shortcuts.py."""
    if not keyboard_shortcut:
        return name_with_accelerator
    return f"{name_with_accelerator}\t{QKeySequence(keyboard_shortcut).toString(QKeySequence.SequenceFormat.NativeText)}"

def _qt_name(name: str) -> str:
    """The spec's name already carries its accelerator key, marked with '_' the way Avalonia marks it. Qt marks it with '&'."""
    return name.replace("_", "&")
