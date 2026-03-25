from __future__ import annotations


def init() -> None:
    from aqt import gui_hooks
    from JAStudio.Core.Note import KanjiNote

    from jastudio.ui import dotnet_ui_root
    from jastudio.ui.web.web_utils.dotnet_rendering_content_renderer_anki_shim import DotNetPrerenderingContentRendererAnkiShim
    net_renderer = dotnet_ui_root().Services.AnkiHTMLRenderers.KanjiNoteRenderer
    renderer = DotNetPrerenderingContentRendererAnkiShim(KanjiNote, net_renderer.CreateRenderer())
    gui_hooks.card_will_show.append(renderer.render)
