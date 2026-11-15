"""What the Anki add-on serves JAStudio over HTTP, once JAStudio is a program of its own: see dev_docs/moving-jastudio-out-of-anki.md.

Nothing here needs .NET, and only the part that drives Anki's windows needs Anki's window.
"""
from __future__ import annotations

# Anki's modules import each other in a circle, which only resolves when anki.collection is the first of them to be imported.
from anki.collection import Collection as Collection
