from __future__ import annotations

from jaspythonutils.sysutils.typed import non_optional
from PyQt6.QtWidgets import QInputDialog, QLineEdit
from System import Func

from jastudio.ankiutils.app import main_window
from jastudio.ui.menus.menu_utils import shortcutfinger


def build_main_menu() -> None:
    menu = non_optional(main_window().form.menubar.addMenu(shortcutfinger.home1("Japanese")))
    from jastudio import mylog
    from jastudio.qt_adapters import qt_menu_adapter
    from jastudio.ui import dotnet_ui_root

    def get_user_text_input() -> str:
        text, ok = QInputDialog.getText(None, "input", "enter text", QLineEdit.EchoMode.Normal, "")
        return text if ok and text else ""

    try:
        menu_builder = dotnet_ui_root().Menus.CreateJapaneseMainMenu()
        clipboard_getter = Func[str](get_user_text_input)  # pyright: ignore [reportCallIssue]
        specs = menu_builder.BuildMenuSpec(clipboard_getter)
        qt_menu_adapter.add_to_qt_menu(menu, specs)
    except Exception as e:
        mylog.error(f"Failed to build C# main menu: {e}")
        import traceback
        mylog.error(traceback.format_exc())
        # Add fallback menu item
        menu.addAction("⚠️ C# Menu Error (check console)")  # pyright: ignore[reportUnknownMemberType]

def init() -> None:
    build_main_menu()
