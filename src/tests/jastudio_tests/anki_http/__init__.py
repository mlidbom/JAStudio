from __future__ import annotations

# Anki's modules import each other in a circle, which only resolves when anki.collection is the first of them to be imported.
from anki.collection import Collection as Collection
