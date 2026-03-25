from __future__ import annotations

from aqt import gui_hooks
from JAStudio.Core.Note.Vocabulary import VocabNote

from jastudio.ui import dotnet_ui_root
from jastudio.ui.web.web_utils.dotnet_rendering_content_renderer_anki_shim import DotNetPrerenderingContentRendererAnkiShim


def init() -> None:
    net_renderer = dotnet_ui_root().Services.AnkiHTMLRenderers.VocabNoteRenderer
    renderer = DotNetPrerenderingContentRendererAnkiShim(VocabNote, net_renderer.CreateRenderer())
    gui_hooks.card_will_show.append(renderer.render)

