# Corpus validity — Design

> **Status: draft (2026-11-08), almost empty.** Magnus wants JAStudio to be able to inspect the validity of the whole
> corpus, in various ways. Until the design is worked out, this holds the checks known to be needed.

## Why

The corpus is JAStudio's own data now (`corpus-data-model.md`): edited in JAStudio, imported through Anki, with a media
layer of tens of thousands of files (`media-storage-and-import.md`). Mistakes in it, JAStudio's own or left over from
Anki, go unnoticed until something trips on them. Checking the whole corpus finds them first.

## Checks to build

- **Orphaned media files:** files in the media layer, and in Anki's media folder, that no note references. Keeping a TTS
  attempt leaves the audio it replaces behind (`text-to-speech.md`, open question 4), as HyperTTS regenerating audio
  did. Deleting one safely needs a check that no other note references the same name. Deleting a note in JAStudio
  takes its unused files with it since 2026-11-08, but notes deleted before then left theirs.
- **Missing media files:** references to files the media layer doesn't have.
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
  - Then nothing but MP3 should come in, such as through a media import.

## Open questions

- How a check reports what it finds, and whether it offers to fix it.
- Whether checks run only on demand, or also as part of something else, such as a media import.
