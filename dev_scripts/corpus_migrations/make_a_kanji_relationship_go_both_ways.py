"""One-time migration of the corpus's kanji (2026-11-10): makes one of the kanji's relationships to other kanji, similarMeaning or
confusedWith, go both ways in the data, as Magnus chose: wherever a kanji names another the corpus has a note for, and that kanji doesn't
name it back, it's added at the end of that kanji's list. A kanji with no note has no other side, and is reported.

Usage: python -X utf8 make_a_kanji_relationship_go_both_ways.py <corpus checkout> similarMeaning|confusedWith
Afterwards, reload the collection in the headless JAStudio and save every note, so that JAStudio writes each file in its own format.
"""

from __future__ import annotations

import json
import sys
from pathlib import Path


def main() -> None:
    corpus = Path(sys.argv[1])
    relationship = sys.argv[2]
    assert relationship in ("similarMeaning", "confusedWith"), relationship

    files = {path: json.loads(path.read_text(encoding="utf-8")) for path in (corpus / "kanji").rglob("*.json")}
    notes_by_kanji = {note["kanji"]: note for note in files.values()}
    changed: set[str] = set()

    for kanji, note in notes_by_kanji.items():
        for other in note[relationship]:
            if other not in notes_by_kanji:
                print(f"{kanji} names {other}, which the corpus has no note for")
                continue
            other_side: list[str] = notes_by_kanji[other][relationship]
            if kanji not in other_side:
                other_side.append(kanji)
                changed.add(other)
                print(f"{other} {relationship}: added {kanji}, which names it")

    for path, note in files.items():
        if note["kanji"] in changed:
            path.write_text(json.dumps(note, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print(f"\n{len(changed)} kanji changed")


main()
