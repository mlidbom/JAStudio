from __future__ import annotations

import re

html_bracket_noise_pattern = re.compile('<.*?>|\[.*?\]|[〜]') # noqa  # pyright: ignore[reportInvalidStringEscapeSequence] don't know what's going on here but it has been working for ages
def strip_html_and_bracket_markup_and_noise_characters(string: str) -> str:
    return html_bracket_noise_pattern.sub("", string)