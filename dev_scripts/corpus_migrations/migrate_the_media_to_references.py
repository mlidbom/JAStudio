"""Slice 9, step 3: migrates the corpus's notes from their media fields, Anki's markup in fixed fields, to lists of references to their files.

Each reference holds a file's name and the facts known about the file: whether it's ours, whether it's TTS audio and by which voice, the
voice's gender, an image's alt text. And each file goes either into the note's source data, as what its source gave it, or into the note's own
lists, as what the corpus added since. The rules are in dev_docs/corpus-data-model.md, Media, "The plan", step 3; they come from the data's
file names, since the fields never recorded those facts. A fact is set only when it's proven: a file is someone else's, and audio a recording,
unless we know for sure (corpus-data-model.md, "Whether a file is ours" and "TTS audio").

    venv\\Scripts\\python.exe dev_scripts\\corpus_migrations\\migrate_the_media_to_references.py <corpus folder> <report file>

The corpus folder holds vocab/, sentences/ and kanji/. The notes are rewritten in place, in the new format; JAStudio then rewrites every note in
its own JSON formatting (the MCP's save_every_note), so this script's formatting doesn't matter. The report says what went where.

Proving that a HyperTTS file is TTS needs the voices HyperTTS was set up with: they're read from the presets of HyperTTS's meta.json in Anki's
add-on folder. That file also holds the services' keys: only its presets are read, and nothing of it is printed.
"""
from __future__ import annotations

import collections
import dataclasses
import hashlib
import html
import json
import os
import re
import sys
from typing import TYPE_CHECKING, Any

if TYPE_CHECKING:
    from collections.abc import Iterator

JsonObject = dict[str, Any]


@dataclasses.dataclass(frozen=True)
class Voice:
    """A text-to-speech voice as the corpus names it, its tts-provider then its name, such as "Azure ja-JP-NanamiNeural", and its gender when known."""
    name: str
    gender: str | None


GENDERS_OF_THE_VOICES = {
    "Azure ja-JP-NanamiNeural": "female", "Azure ja-JP-KeitaNeural": "male", "Azure ja-JP-AoiNeural": "female", "Azure ja-JP-DaichiNeural": "male",
    "Azure ja-JP-MayuNeural": "female", "Azure ja-JP-NaokiNeural": "male", "Azure ja-JP-ShioriNeural": "female",
    "Google ja-JP-Neural2-B": "female", "Google ja-JP-Neural2-C": "male", "Google ja-JP-Neural2-D": "male",
    "Google ja-JP-Wavenet-A": "female", "Google ja-JP-Wavenet-B": "female", "Google ja-JP-Wavenet-C": "male", "Google ja-JP-Wavenet-D": "male",
}


def voice_named(name: str) -> Voice:
    return Voice(name, GENDERS_OF_THE_VOICES.get(name))


@dataclasses.dataclass
class AudioReference:
    file_name: str
    is_ours: bool = False
    is_text_to_speech: bool = False
    text_to_speech_voice: str | None = None
    voice_gender: str | None = None

    def as_json(self) -> JsonObject:
        facts: JsonObject = {"fileName": self.file_name}
        if self.is_ours: facts["isOurs"] = True
        if self.is_text_to_speech: facts["isTextToSpeech"] = True
        if self.text_to_speech_voice is not None: facts["textToSpeechVoice"] = self.text_to_speech_voice
        if self.voice_gender is not None: facts["voiceGender"] = self.voice_gender
        return facts


@dataclasses.dataclass
class ImageReference:
    file_name: str
    is_ours: bool = False
    alt_text: str | None = None

    def as_json(self) -> JsonObject:
        facts: JsonObject = {"fileName": self.file_name}
        if self.is_ours: facts["isOurs"] = True
        if self.alt_text is not None: facts["altText"] = self.alt_text
        return facts


# Anki's markup, read as JAStudio's AnkiMediaMarkup reads it.
SOUND_TAG = re.compile(r"\[sound:([^\]]+)\]")
IMG_TAG = re.compile(r"""<img\b((?:[^>"']|"[^"]*"|'[^']*')*)>""", re.IGNORECASE)
ATTRIBUTE = re.compile(r"""([\w-]+)\s*=\s*(?:"([^"]*)"|'([^']*)'|([^\s"'>]+))""")


def audio_in(field: str) -> list[str]:
    return [name for name in (it.strip() for it in SOUND_TAG.findall(field)) if name != ""]


def images_in(field: str) -> list[tuple[str, str | None]]:
    """The images an <img> tag of the field describes, as (file name, alt text): an attribute given twice has its first value, as in HTML."""
    images: list[tuple[str, str | None]] = []
    for attributes in IMG_TAG.findall(field):
        values: dict[str, str] = {}
        for name, double_quoted, single_quoted, bare in ATTRIBUTE.findall(attributes):
            values.setdefault(name.lower(), html.unescape(double_quoted or single_quoted or bare))
        if values.get("src", "").strip() != "":
            images.append((values["src"].strip(), values.get("alt")))
    return images


@dataclasses.dataclass
class TtsVoiceId_v3:
    """HyperTTS's voice id: its repr is part of the text whose hash names a HyperTTS file, so it must be HyperTTS's exactly."""
    voice_key: Any
    service: str


class HyperTtsProof:
    """Proves a HyperTTS file TTS audio by recomputing its name: HyperTTS names a file by a hash of the text, the voice and the options it was made with."""

    def __init__(self) -> None:
        self._voices: list[tuple[TtsVoiceId_v3, Voice | None]] = [
            (self._azure_voice_id(name), voice_named(f"Azure ja-JP-{name}Neural")) for name in ("Daichi", "Nanami", "Mayu")]
        self._voices += [(TtsVoiceId_v3({"language_code": "ja-JP", "name": f"ja-JP-Neural2-{letter}"}, "Google"), voice_named(f"Google ja-JP-Neural2-{letter}"))
                         for letter in "BCD"]
        self._option_sets: list[Any] = [{"rate": 1.0}, {}, {"rate": 1.0, "format": "mp3"}, {"rate": 1.0, "format": "mp3_hq"}]
        self._add_the_voices_of_the_presets()

    @staticmethod
    def _azure_voice_id(name: str) -> TtsVoiceId_v3:
        return TtsVoiceId_v3({"name": f"Microsoft Server Speech Text to Speech Voice (ja-JP, {name}Neural)"}, "Azure")

    def _add_the_voices_of_the_presets(self) -> None:
        with open(os.path.join(os.environ["APPDATA"], "Anki2", "addons21", "111623432", "meta.json"), encoding="utf-8") as file:
            presets: JsonObject = json.load(file)["config"]["presets"]
        for preset in presets.values():
            selection: JsonObject = preset["voice_selection"]
            chosen: JsonObject | list[JsonObject] = selection.get("voice") or selection.get("voice_list") or []
            voices: list[JsonObject] = [chosen] if isinstance(chosen, dict) else chosen
            for voice in voices:
                voice_id = TtsVoiceId_v3(voice["voice_id"]["voice_key"], voice["voice_id"]["service"])
                if all(voice_id != known for known, _ in self._voices):
                    self._voices.append((voice_id, self._our_voice(voice_id)))
                if voice.get("options") not in self._option_sets:
                    self._option_sets.append(voice.get("options"))

    @staticmethod
    def _our_voice(voice_id: TtsVoiceId_v3) -> Voice | None:
        """The voice as the corpus names it, or None for a service whose files are recordings, as Forvo's."""
        if voice_id.service == "Azure":
            match = re.fullmatch(r"Microsoft Server Speech Text to Speech Voice \((ja-JP), (\w+)\)", voice_id.voice_key["name"])
            assert match is not None, voice_id
            return voice_named(f"Azure {match.group(1)}-{match.group(2)}")
        if voice_id.service == "Google":
            return voice_named(f'Google {voice_id.voice_key["name"]}')
        assert voice_id.service == "Forvo", voice_id.service
        return None

    def voice_of(self, file_name: str, texts: set[str]) -> tuple[str, Voice | None] | None:
        """The service and voice that made the HyperTTS file from one of the texts, or None when no voice and text recompute its name."""
        for text in texts:
            for voice_id, voice in self._voices:
                for options in self._option_sets:
                    source = str({"source_text": text, "voice_id": voice_id, "options": options})
                    if "hypertts-" + hashlib.sha224(source.encode("utf-8")).hexdigest() + ".mp3" == file_name:
                        return voice_id.service, voice
        return None


AWESOME_TTS = re.compile(r"^ATTS .* \(azure microsoft server speech text to speech voice \(ja-jp (\w+)neural\)\)\.mp3$")
JASTUDIO_TTS = re.compile(r"^jastudio-tts-(azure|google)-([a-z0-9-]+?)-[0-9a-f]{32}\.mp3$")
WANIKANI_NAME = re.compile(r"_(b|g|male|female)\.mp3$")
WANIKANIS_OWN_AUDIO = re.compile(r"(_(b|g)|^Wani_\d+(_.+)?_(male|female))\.mp3$")
GENDERS_OF_WANIKANI_NAMES = {"b": "male", "male": "male", "g": "female", "female": "female"}
CORE_2000_AUDIO = re.compile(r"^[0-9a-f]{32}(-[0-9a-f]{40})?\.mp3$")
CORE_2000_IMAGE = re.compile(r"^[0-9a-f]{32}(_\d{13})?\.(jpg|jpeg|png)$")
YOMITAN_AUDIO = re.compile(r"^(yomitan|yomichan)_audio_")
SUBS2SRS_TIMESTAMPED = re.compile(r"_\d+\.\d+\.\d+\.\d+(-\d+\.\d+\.\d+\.\d+)?\.\w+$")
JLAB_NUMBERED = re.compile(r"^\d{13}(_\d{13})?\.(mp3|jpg)$")
RECORDING = re.compile(r"^rec\d+\.mp3$")
PASTED = re.compile(r"^paste-[0-9a-f]+\.\w+$")
OUR_AI_IMAGES = {"file-aNbdQAFEtF3DrSYO3IT37AfN.webp", "OIG3.LbmpkxItYdQ4ri_FToRK"}
TTS_AUDIO_TAG = "_tts_audio"


class Report:
    """What the migration put where: counts by note type, list and kind of file, and the cases no rule fits."""

    def __init__(self) -> None:
        self.counts: collections.Counter[tuple[str, str, str, str]] = collections.Counter()
        self.voices: collections.Counter[tuple[str, str, str]] = collections.Counter()
        self.no_rule_fits: list[str] = []
        self.dropped_duplicates: list[str] = []
        self.notes_changed: collections.Counter[str] = collections.Counter()

    def count(self, note_type: str, sources: str, where: str, kind: str) -> None:
        self.counts[(note_type, sources, where, kind)] += 1

    def write(self, path: str) -> None:
        with open(path, "w", encoding="utf-8") as file:
            file.write("# The media migration of slice 9, step 3\n\n")
            file.write(f"Notes: {dict(self.notes_changed)}\n\n## References by note type, source, list and kind of file\n\n")
            file.write("| count | note type | source | list | kind |\n|---:|---|---|---|---|\n")
            file.writelines(f"| {count} | {note_type} | {sources} | {where} | {kind} |\n" for (note_type, sources, where, kind), count in sorted(self.counts.items()))
            file.write("\n## TTS audio by voice\n\n| count | note type | kind | voice |\n|---:|---|---|---|\n")
            file.writelines(f"| {count} | {note_type} | {kind} | {voice} |\n" for (note_type, kind, voice), count in sorted(self.voices.items()))
            file.write(f"\n## Files no rule fits: the corpus's own, as not proven the source's ({len(self.no_rule_fits)})\n\n")
            file.writelines(f"- {it}\n" for it in self.no_rule_fits)
            file.write(f"\n## Second references to a file the note already has, dropped ({len(self.dropped_duplicates)})\n\n")
            file.writelines(f"- {it}\n" for it in self.dropped_duplicates)


class Migration:
    def __init__(self, corpus_folder: str, report: Report) -> None:
        self._corpus_folder = corpus_folder
        self._report = report
        self._hyper_tts = HyperTtsProof()

    def run(self) -> None:
        for note_type, migrate in (("vocab", self._migrate_vocab), ("sentences", self._migrate_sentence), ("kanji", self._migrate_kanji)):
            for path in self._note_files(note_type):
                with open(path, encoding="utf-8") as file:
                    note: JsonObject = json.load(file)
                migrated = migrate(note)
                migrated["tags"] = [it for it in migrated["tags"] if it != TTS_AUDIO_TAG]
                with open(path, "w", encoding="utf-8", newline="") as file:
                    file.write(json.dumps(migrated, indent=2, ensure_ascii=False))
                self._report.notes_changed[note_type] += 1

    def _note_files(self, note_type: str) -> Iterator[str]:
        for folder, _, files in os.walk(os.path.join(self._corpus_folder, note_type)):
            yield from (os.path.join(folder, it) for it in sorted(files) if it.endswith(".json"))

    # Vocab

    def _migrate_vocab(self, vocab: JsonObject) -> JsonObject:
        sources = source_tags_of(vocab)
        texts = {vocab["question"]["text"], vocab["corpusAnswer"], vocab["sourceData"]["answer"], *vocab["readings"], *(it["text"] for it in vocab["forms"])}
        if vocab["question"]["disambiguation"]:
            texts |= {f"{vocab['question']['text']}:{vocab['question']['disambiguation']}", f"{vocab['question']['text']}[{vocab['question']['disambiguation']}]"}
        lists = NoteMedia(f"vocab {vocab['question']['text']} {vocab['id']}", self._report)
        for field in ("audioB", "audioG", "audioTTS"):
            for name in audio_in(vocab[field]):
                audio, kind = self._vocab_audio(name, texts)
                is_the_sources = self._is_the_vocabs_sources_audio(name, sources)
                lists.add_audio(audio, is_the_sources, kind, "vocab", sources)
        for name, alt_text in images_in(vocab["image"]):
            is_the_sources = has_source(sources, "core2000") and CORE_2000_IMAGE.match(name) is not None
            lists.add_image(ImageReference(name, name in OUR_AI_IMAGES, alt_text), is_the_sources, image_kind(name), "vocab", sources)
        for name, alt_text in images_in(vocab["userImage"]):
            lists.add_image(ImageReference(name, name in OUR_AI_IMAGES, alt_text), False, image_kind(name), "vocab", sources)
        source_data = {**vocab["sourceData"], "audio": lists.source_audio, "images": lists.source_images}
        return replaced_keys(vocab, {"audioB": ("audio", lists.own_audio), "audioG": None, "audioTTS": None, "image": ("images", lists.own_images),
                                     "userImage": None, "sourceData": ("sourceData", source_data)})

    def _vocab_audio(self, name: str, texts: set[str]) -> tuple[AudioReference, str]:
        """The vocab audio file's facts, and its kind for the report: TTS only when proven, which for HyperTTS means its name recomputed."""
        if name.startswith("hypertts-"):
            made_by = self._hyper_tts.voice_of(name, texts)
            if made_by is None:
                return AudioReference(name), "HyperTTS, not proven TTS"
            service, voice = made_by
            if voice is None:
                return AudioReference(name), f"HyperTTS from {service}, a recording"
            return self._text_to_speech(name, voice), "HyperTTS TTS"
        return self._proven_text_to_speech_by_its_name(name) or (AudioReference(name, voice_gender=wanikani_gender(name)), audio_kind(name))

    @staticmethod
    def _is_the_vocabs_sources_audio(name: str, sources: set[str]) -> bool:
        return (has_source(sources, "wani") and WANIKANIS_OWN_AUDIO.search(name) is not None) \
            or (has_source(sources, "core2000") and CORE_2000_AUDIO.match(name) is not None) \
            or (has_source(sources, "yomitan") and YOMITAN_AUDIO.match(name) is not None)

    # Sentences

    def _migrate_sentence(self, sentence: JsonObject) -> JsonObject:
        sources = source_tags_of(sentence)
        is_tagged_as_tts = TTS_AUDIO_TAG in sentence["tags"]
        texts = {sentence["corpusQuestion"], sentence["sourceData"]["question"]} - {""}
        lists = NoteMedia(f"sentence {sentence['sourceData']['question'][:30]} {sentence['id']}", self._report)
        for name in audio_in(sentence["audio"]):
            if is_tagged_as_tts or is_a_tts_file_name(name):
                audio, kind = self._sentence_text_to_speech(name, texts)
                lists.add_audio(audio, False, kind, "sentence", sources)
            elif RECORDING.match(name) and not has_source(sources, "lyrics"):
                lists.add_audio(AudioReference(name), False, "rec, recorded by hand", "sentence", sources)
            else:
                lists.add_audio(AudioReference(name), self._is_the_sentences_sources(name, sources), audio_kind(name), "sentence", sources)
        for name, alt_text in images_in(sentence["screenshot"]):
            is_the_sources = not PASTED.match(name) and self._is_the_sentences_sources(name, sources)
            lists.add_image(ImageReference(name, name in OUR_AI_IMAGES, alt_text), is_the_sources, image_kind(name), "sentence", sources)
        source_data = {**sentence["sourceData"], "audio": lists.source_audio, "screenshots": lists.source_images}
        return replaced_keys(sentence, {"audio": ("audio", lists.own_audio), "screenshot": ("screenshots", lists.own_images), "sourceData": ("sourceData", source_data)})

    def _sentence_text_to_speech(self, name: str, texts: set[str]) -> tuple[AudioReference, str]:
        """A sentence's TTS audio, proven by the _tts_audio tag or a TTS file's name: its voice when its name recomputes or names it."""
        if name.startswith("hypertts-"):
            made_by = self._hyper_tts.voice_of(name, texts)
            if made_by is None:
                return AudioReference(name, True, True), "HyperTTS TTS, voice unknown"
            service, voice = made_by
            assert voice is not None, f"The sentence audio {name} recomputes as made by {service}, which makes recordings."
            return self._text_to_speech(name, voice), "HyperTTS TTS"
        named = self._proven_text_to_speech_by_its_name(name)
        return named if named is not None else (AudioReference(name, True, True), "tagged as TTS")

    @staticmethod
    def _is_the_sentences_sources(name: str, sources: set[str]) -> bool | None:
        """Whether the sentence's file came with its source, the deck it was imported from: None when no rule says, so that it's listed."""
        if SUBS2SRS_TIMESTAMPED.search(name): return True
        if has_source(sources, "core2000") and (CORE_2000_AUDIO.match(name) or CORE_2000_IMAGE.match(name)): return True
        if has_source(sources, "jlab") and JLAB_NUMBERED.match(name): return True
        if has_source(sources, "immersion_kit"): return True
        if has_source(sources, "lyrics") and RECORDING.match(name): return True
        return None

    # Kanji

    def _migrate_kanji(self, kanji: JsonObject) -> JsonObject:
        lists = NoteMedia(f"kanji {kanji['kanji']} {kanji['id']}", self._report)
        for name, alt_text in images_in(kanji["image"]):
            lists.add_image(ImageReference(name, name in OUR_AI_IMAGES, alt_text), False, image_kind(name), "kanji", source_tags_text(source_tags_of(kanji)))
        return replaced_keys(kanji, {"image": ("images", lists.own_images)})

    # TTS

    def _proven_text_to_speech_by_its_name(self, name: str) -> tuple[AudioReference, str] | None:
        """AwesomeTTS's and JAStudio's own TTS files, whose names name their voices."""
        if (match := AWESOME_TTS.match(name)) is not None:
            return self._text_to_speech(name, voice_named(f"Azure ja-JP-{match.group(1).capitalize()}Neural")), "AwesomeTTS"
        if (match := JASTUDIO_TTS.match(name)) is not None:
            provider, voice = match.group(1).capitalize(), match.group(2)
            assert provider == "Azure", name
            return self._text_to_speech(name, voice_named(f'Azure ja-JP-{voice[:-len("neural")].capitalize()}Neural')), "JAStudio's own TTS"
        return None

    @staticmethod
    def _text_to_speech(name: str, voice: Voice) -> AudioReference:
        return AudioReference(name, is_ours=True, is_text_to_speech=True, text_to_speech_voice=voice.name, voice_gender=voice.gender)


class NoteMedia:
    """A note's references as the migration gathers them: its source's and its own, a second reference to a file the note already has dropped."""

    def __init__(self, note: str, report: Report) -> None:
        self._note = note
        self._report = report
        self._file_names: set[str] = set()
        self.source_audio: list[JsonObject] = []
        self.own_audio: list[JsonObject] = []
        self.source_images: list[JsonObject] = []
        self.own_images: list[JsonObject] = []

    def add_audio(self, audio: AudioReference, is_the_sources: bool | None, kind: str, note_type: str, sources: set[str] | str) -> None:
        if self._is_a_second_reference(audio.file_name): return
        self._count(audio.file_name, is_the_sources, "audio", kind, note_type, sources)
        if audio.is_text_to_speech:
            self._report.voices[(note_type, kind, audio.text_to_speech_voice or "unknown")] += 1
        (self.source_audio if is_the_sources else self.own_audio).append(audio.as_json())

    def add_image(self, image: ImageReference, is_the_sources: bool | None, kind: str, note_type: str, sources: set[str] | str) -> None:
        if self._is_a_second_reference(image.file_name): return
        self._count(image.file_name, is_the_sources, "images", kind + (", ours" if image.is_ours else ""), note_type, sources)
        (self.source_images if is_the_sources else self.own_images).append(image.as_json())

    def _is_a_second_reference(self, file_name: str) -> bool:
        if file_name in self._file_names:
            self._report.dropped_duplicates.append(f"{self._note}: {file_name}")
            return True
        self._file_names.add(file_name)
        return False

    def _count(self, file_name: str, is_the_sources: bool | None, list_name: str, kind: str, note_type: str, sources: set[str] | str) -> None:
        if is_the_sources is None:
            self._report.no_rule_fits.append(f"{self._note}: {file_name}")
        where = ("sourceData." if is_the_sources else "") + list_name
        self._report.count(note_type, sources if isinstance(sources, str) else source_tags_text(sources), where, kind)


def source_tags_of(note: JsonObject) -> set[str]:
    return {it for it in note["tags"] if it.startswith("source::")}


def has_source(sources: set[str], source: str) -> bool:
    return any(it == f"source::{source}" or it.startswith(f"source::{source}::") for it in sources)


def source_tags_text(sources: set[str]) -> str:
    return ", ".join(sorted({"::".join(it.split("::")[:2]) for it in sources})) or "none"


def is_a_tts_file_name(name: str) -> bool:
    return name.startswith("hypertts-") or name.startswith("ATTS ") or name.startswith("jastudio-tts-")


def wanikani_gender(name: str) -> str | None:
    match = WANIKANI_NAME.search(name)
    return GENDERS_OF_WANIKANI_NAMES[match.group(1)] if match is not None else None


def audio_kind(name: str) -> str:
    for pattern, kind in ((WANIKANI_NAME, "WaniKani-named"), (CORE_2000_AUDIO, "Core 2000"), (YOMITAN_AUDIO, "Yomitan"), (SUBS2SRS_TIMESTAMPED, "subs2srs"),
                          (JLAB_NUMBERED, "JLab"), (RECORDING, "rec")):
        if pattern.search(name):
            return kind
    return "other"


def image_kind(name: str) -> str:
    for pattern, kind in ((CORE_2000_IMAGE, "Core 2000"), (SUBS2SRS_TIMESTAMPED, "subs2srs"), (JLAB_NUMBERED, "JLab"), (PASTED, "pasted")):
        if pattern.search(name):
            return kind
    return "AI" if name in OUR_AI_IMAGES else "other"


def replaced_keys(note: JsonObject, replacements: dict[str, tuple[str, Any] | None]) -> JsonObject:
    """The note with each old key replaced, in its place, by a new key and value, or dropped for None: every old key must be there."""
    missing = set(replacements) - set(note)
    assert not missing, (note["id"], missing)
    migrated: JsonObject = {}
    for key, value in note.items():
        if key not in replacements:
            migrated[key] = value
        elif (replacement := replacements[key]) is not None:
            migrated[replacement[0]] = replacement[1]
    return migrated


def main() -> None:
    corpus_folder, report_path = sys.argv[1], sys.argv[2]
    report = Report()
    Migration(corpus_folder, report).run()
    report.write(report_path)
    print(f"Migrated {dict(report.notes_changed)}; {len(report.no_rule_fits)} files no rule fits, {len(report.dropped_duplicates)} second references dropped.")


if __name__ == "__main__":
    main()
