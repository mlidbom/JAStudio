# Corpus validity — Design

> **Status: draft (2026-11-08), almost empty.** Magnus wants JAStudio to be able to inspect the validity of the whole
> corpus, in various ways. Until the design is worked out, this holds the checks known to be needed. Problems already
> found in the corpus's data, to be fixed by hand, are listed in `corpus-data-issues.md`.

## Why

The corpus is JAStudio's own data now (`corpus-data-model.md`): edited in JAStudio, imported through Anki, with a media
layer of tens of thousands of files (`media-storage-and-import.md`). Mistakes in it, JAStudio's own or left over from
Anki, go unnoticed until something trips on them. Checking the whole corpus finds them first.

## Checks to build

- **Orphaned media files:** files in the media layer, and in Anki's media folder, that no note references. Keeping a TTS
  attempt leaves the audio it replaces behind (`text-to-speech.md`, open question 4), as HyperTTS regenerating audio
  did. Deleting one safely needs a check that no other note references the same name. Deleting a note in JAStudio
  takes its unused files with it since 2026-11-08, but notes deleted before then left theirs.
- **Missing media files:** references to files the media layer doesn't have. *One on 2026-11-10, found after the survey for
  slice 9 (`corpus-data-model.md`, "Media"): 甘ったるい's web image `zhoti610522.jpg`, whose `img` tag's alt text holds
  `>`, so the media import missed its file name. Added from Anki's media folder.* The check has to parse the markup
  properly, or, after slice 9, read the references.
- **A file in `free/` that isn't ours** (Magnus, 2026-11-10): the media layer's `free/` holds only files a note marks
  `isOurs` (`corpus-data-model.md`, "Whether a file is ours"), so a file there that no note marks, or that one note marks
  and another doesn't, belongs in a commercial folder. Only that way round: a file of ours in a commercial folder is safe.
- **A file name in more than one folder of the media layer** (Magnus, 2026-11-10: a check rather than a one-off fix). The
  media layer finds a file by its name alone, so it keeps the first copy it finds and logs a warning
  (`MediaFileIndex`). On 2026-11-10, 824 names were in two folders, each pair with the same content: 431 Core 2000
  images in both `commercial-01/vocab/core2000/images` and `commercial-02/studying/core2000/images`, 45 images in both
  `commercial-01/vocab/wani/user-image` and `free/AI/kanji/images`, and smaller groups. Which folder is right decides the
  copyright partition, so the check reports them by pair of folders, and which copy goes is decided per group. *Later that
  day, the 186 with a copy in `free/` not proven ours lost that copy.*
- **Anki notes of JAStudio's note types that no corpus note has.** In Anki, only JAStudio writes notes of its own note
  types (`import-profiles.md`, principle 3), so such a note is a mistake: one added through Anki's Add dialog, by a tool
  through AnkiConnect, or on the phone and brought in by a sync. JAStudio ignores them: the reload matches Anki notes
  to corpus notes by their `jas_note_id` field, and nothing takes in a note added in Anki since 2026-11-08. The check
  only reports them, and they're fixed by hand (Magnus, 2026-11-08): it should never happen, so JAStudio needs no
  support for it.
- **Audio that isn't MP3.** The corpus holds only MP3 (Magnus, 2026-11-08). Supporting other formats might happen, but
  not to rescue a few files.
  - Four imported sentences have Ogg Vorbis audio, which JAStudio can't play or compare: `0x0005a48b.wav.ogg`,
    `0x00066667.wav.ogg`, `0x000f63fb.wav.ogg` and `0x000f9437.wav.ogg`, in `commercial-02/unknown`. They're to be
    converted to MP3, and their notes and Anki's media folder to reference the MP3s.
  - Two vocab have Yomitan WAV audio too (found 2026-11-08): `yomitan_audio_2025-03-13-09-03-56-195.wav` and
    `yomitan_audio_2025-03-04-06-42-32-975.wav`.
  - Nothing but MP3 comes in any more: an import run's check makes audio that isn't MP3 a problem.

## Open questions

- How a check reports what it finds, and whether it offers to fix it.
- Whether checks run only on demand, or also as part of something else, such as an import run.
