"""One-time migration of the corpus's kanji (2026-11-10): cleans up their radicals, similarMeaning and confusedWith lists, so that each value
is one kanji, before KanjiNote holds them as lists rather than comma separated text.

- A value of several kanji run together, each one the corpus has, such as 佐's confused-with `祐怖`, becomes a value per kanji.
- `&nbsp;`, left over from Anki's fields, is dropped from a value, such as 元's confused-with `先&nbsp;`.
- A kanji is never among its own radicals: 13 kanji listed themselves, which every reader of the radicals skipped.
- A value of several characters that holds the kanji itself, such as 跡's similar meaning `奇跡`, is a word, not a kanji: it stays, and is
  reported, for Magnus to decide.

Usage: python -X utf8 clean_up_the_kanji_lists.py <corpus checkout>
Afterwards, reload the collection in the headless JAStudio and save every note, so that JAStudio writes each file in its own format.
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

LISTS = ["radicals", "similarMeaning", "confusedWith"]


def main() -> None:
    corpus = Path(sys.argv[1])
    files = {path: json.loads(path.read_text(encoding="utf-8")) for path in (corpus / "kanji").rglob("*.json")}
    corpus_kanji: set[str] = {note["kanji"] for note in files.values()}
    words_left_for_magnus: list[str] = []

    for path, note in files.items():
        kanji: str = note["kanji"]
        changed = False
        for list_name in LISTS:
            old: list[str] = note[list_name]
            new = cleaned(kanji, list_name, old, corpus_kanji, words_left_for_magnus)
            if new != old:
                print(f"{kanji} {list_name}: {old} -> {new}")
                note[list_name] = new
                changed = True
        if changed:
            path.write_text(json.dumps(note, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print("\nWords left as they are, for Magnus:")
    for word in words_left_for_magnus:
        print(f"  {word}")


def cleaned(kanji: str, list_name: str, values: list[str], corpus_kanji: set[str], words_left_for_magnus: list[str]) -> list[str]:
    result: list[str] = []
    for value in values:
        for part in parts_of(kanji, list_name, value.replace("&nbsp;", "").strip(), corpus_kanji, words_left_for_magnus):
            if part == kanji and list_name == "radicals":
                continue
            if list_name != "radicals" and part in result:
                continue
            result.append(part)
    return result


def parts_of(kanji: str, list_name: str, value: str, corpus_kanji: set[str], words_left_for_magnus: list[str]) -> list[str]:
    if len(value) <= 1:
        return [value]
    if not all(character in corpus_kanji for character in value):
        words_left_for_magnus.append(f"{kanji} {list_name}: {value} (holds a character no kanji has)")
        return [value]
    if kanji in value and list_name != "radicals":
        words_left_for_magnus.append(f"{kanji} {list_name}: {value}")
        return [value]
    return list(value)


main()
