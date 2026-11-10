"""Verifies slice 9, step 3's migration of the media: every note references the same files after it as before, but the second references dropped.

    venv\\Scripts\\python.exe dev_scripts\\corpus_migrations\\verify_the_media_references.py <corpus checkout>

Compares each note's media fields at the checkout's git HEAD, Anki's markup as the corpus stored it before the migration, with the lists in its
file now: its source data's and its own, in that order, should hold the files of its fields in their order, each file once. The files a field
holds keep their order within whichever list they went to. Prints the notes that differ, and how many notes and references it compared.
"""
from __future__ import annotations

import json
import os
import subprocess
import sys
from typing import Any

from migrate_the_media_to_references import audio_in, images_in

JsonObject = dict[str, Any]

OLD_FIELDS = {"vocab": (("audioB", "audioG", "audioTTS"), ("image", "userImage")), "sentences": (("audio",), ("screenshot",)), "kanji": ((), ("image",))}
NEW_LISTS = {"vocab": (("sourceData.audio", "audio"), ("sourceData.images", "images")), "sentences": (("sourceData.audio", "audio"), ("sourceData.screenshots", "screenshots")),
             "kanji": ((), ("images",))}


def files_in_the_old_fields(note: JsonObject, audio_fields: tuple[str, ...], image_fields: tuple[str, ...]) -> list[str]:
    names = [name for field in audio_fields for name in audio_in(note[field])] + [name for field in image_fields for name, _ in images_in(note[field])]
    return list(dict.fromkeys(names))


def files_in_the_new_lists(note: JsonObject, audio_lists: tuple[str, ...], image_lists: tuple[str, ...]) -> list[str]:
    def listed(path: str) -> list[str]:
        holder = note["sourceData"] if path.startswith("sourceData.") else note
        return [it["fileName"] for it in holder[path.removeprefix("sourceData.")]]
    return [name for path in audio_lists + image_lists for name in listed(path)]


def keeps_each_lists_order(old: list[str], new_lists: list[list[str]]) -> bool:
    position = {name: index for index, name in enumerate(old)}
    return all([position[it] for it in listed] == sorted(position[it] for it in listed) for listed in new_lists)


def main() -> None:
    checkout = sys.argv[1]
    paths = [os.path.relpath(os.path.join(folder, it), checkout).replace(os.sep, "/")
             for note_type in OLD_FIELDS for folder, _, files in os.walk(os.path.join(checkout, note_type)) for it in sorted(files) if it.endswith(".json")]
    batch = subprocess.run(["git", "-C", checkout, "cat-file", "--batch"], input="".join(f"HEAD:{it}\n" for it in paths).encode("utf-8"), capture_output=True, check=True).stdout
    differing: list[str] = []
    references = 0
    offset = 0
    for path in paths:
        header_end = batch.index(b"\n", offset)
        size = int(batch[offset:header_end].split()[2])
        old: JsonObject = json.loads(batch[header_end + 1:header_end + 1 + size].decode("utf-8"))
        offset = header_end + 1 + size + 1
        with open(os.path.join(checkout, path), encoding="utf-8") as file:
            new: JsonObject = json.load(file)
        note_type = path.split("/")[0]
        old_files = files_in_the_old_fields(old, *OLD_FIELDS[note_type])
        audio_lists, image_lists = NEW_LISTS[note_type]
        new_files = files_in_the_new_lists(new, audio_lists, image_lists)
        references += len(new_files)
        each_list = [files_in_the_new_lists(new, (it,), ()) for it in audio_lists + image_lists]
        if sorted(old_files) != sorted(new_files) or len(set(new_files)) != len(new_files) or not keeps_each_lists_order(old_files, each_list):
            differing.append(f"{path}: before {old_files}, after {new_files}")
    print(*differing[:20], sep="\n")
    print(f"Compared {len(paths)} notes and {references} references: {len(differing)} notes differ.")


if __name__ == "__main__":
    main()
