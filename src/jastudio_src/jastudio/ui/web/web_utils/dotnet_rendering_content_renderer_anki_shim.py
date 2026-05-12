
from __future__ import annotations

from typing import TYPE_CHECKING

from autoslot import Slots
from JAStudio.Core.Note import JPNote, Mine

if TYPE_CHECKING:
    from anki.cards import Card
    from JAStudio.Core.UI.Web import AppendingPrerenderer_1


class DotNetPrerenderingContentRendererAnkiShim[TNote: JPNote](Slots):
    def __init__(self, cls: type[TNote], renderer: AppendingPrerenderer_1[TNote]) -> None:
        self._cls: type[TNote] = cls
        self._renderer: AppendingPrerenderer_1[TNote] = renderer

    def render(self, html: str, card: Card, type_of_display: str) -> str:
        from jastudio.ui import dotnet_ui_root
        if not dotnet_ui_root().IsInitialized:
            return Mine.AppStillLoadingMessage

        note = dotnet_ui_root().Services.CoreApp.Collection.NoteFromExternalId(card.nid or card.note().id)

        if not isinstance(note, self._cls):
            return html

        card_template_name = str(card.template()["name"])
        return self._renderer.Render(note, html, type_of_display, card_template_name)