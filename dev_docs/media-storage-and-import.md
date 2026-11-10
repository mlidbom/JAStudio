# Media storage and import

> **Status: as built (rewritten 2026-11-07).** This document used to describe a design with a JSON sidecar per media
> file. That design was dropped on 2026-05-15, in "Migrate to fully using the file name to handle media files and
> import missing media" (`2353634c3`), and the media layer has worked by file name alone since. The document wasn't
> updated then. What the sidecars were is summarized under [Dropped: sidecars](#dropped-sidecars-2026-05-15).

## Goals

The corpus is analyzed Japanese sentences and vocab for study. Most sentences come from copyrighted material:
anime, textbooks and learning platforms. Using them for study with linguistic breakdowns is fair use, but rights
holders can be aggressive, so:

1. **Media is partitioned by copyright regime,** in git repositories of its own, so that a takedown for one source,
   such as one anime, means deleting files without touching the corpus or the other media. *(Done: see
   [Where the files are](#where-the-files-are).)*
2. **The git repositories stay a manageable size.** Audio and images are large, so they're kept out of the corpus's
   repository. *(Done.)*
3. ~~**The corpus holds no media references.**~~ *Dropped (Magnus, 2026-11-10). It came from the sidecar design: with
   media working by file name alone, a note is the only place that says which files are its. The notes keep their media
   references, as data rather than Anki's markup: slice 9 in `corpus-data-model.md`, "Media".*

## Where the files are

`jas_database/media` is a folder of the corpus repository, `jas_database`. It used to be a repository of its own,
`JAStudioData-media`, but that made creating vocab by hand a pain, so its files moved into the corpus repository.
*A leftover of that: the git data of the `commercial-01` and `commercial-02` submodules is still inside the old
repository's git data, in `.git/modules/src/jas_database/modules/media/modules/` of the main repository. That
folder holds about 3.9 GB, of which about 1.8 GB is the old repository's own data, but deleting it breaks both
submodules. A file `media/.git` that made git run inside `media/` show the old repository was deleted on
2026-11-08, and `media/free/.git`, pointing at a former `free` repository's data, on 2026-11-10.* In it:

| Folder | What | Files (2026-11-07) |
|---|---|---|
| `commercial-01` | A submodule of the corpus repository, `JAStudioData-media-commercial-01`: commercial vocab audio and images, such as Core 2000's and WaniKani's. New commercial media goes here, since commercial-02 is near GitHub's recommended size | about 15,000 |
| `commercial-02` | A submodule of the corpus repository, `JAStudioData-media-commercial-02`: anime audio and screenshots, per show. About 2 GB | about 69,000 |
| `free` | Freely redistributable media, in the corpus repository itself, among it the TTS audio in `free/TTS/vocab` and `free/TTS/sentences/<source>` | about 35,000 |

The folders are organizational: they say nothing about a file that the code reads. **A file keeps the name it had in
Anki,** such as `hypertts-0001168b….mp3`, so file names must be unique across the whole tree.

## How a note finds its media

**The note holds the link.** A note's media fields hold Anki's markup in the corpus JSON: `[sound:…]` for audio and
`<img src="…">` for images. That file name is the only connection between a note and a file. The files don't know
their notes.

| Note type | Media fields (`VocabDto`, `SentenceDto`) |
|---|---|
| Vocab | `AudioB` (`Audio_b`), `AudioG` (`Audio_g`), `AudioTTS` (`Audio_TTS`), `Image`, `UserImage` |
| Sentence | `Audio` (`Audio Sentence`), `Screenshot` |
| Kanji | none |

**`MediaFileIndex` maps file names to files.** On first use it scans the whole media tree, leaving out `.json` and
`.txt` files. It maps each file name, ignoring case, to an `AudioAttachment` or an `ImageAttachment`, by the file's
extension. Each attachment holds only the original file name and the path. For a duplicate name, the first file
found wins, and a warning is logged.

`MediaFileIndex.GetAudioAttachments` and `GetImageAttachments` parse a field's markup and look up each name. A
reference whose file isn't in the media layer finds nothing. The card server serves the files to the note pages at
`/media?name={file name}`, through the index.

## Importing from Anki

An import profile stores a note's media in the media layer when it imports the note, in the folders the profile names,
with the audio normalized (`import-profiles.md`).

**Retired 2026-11-08: the rules-based media import.** It was built to move everything out of Anki in February 2026,
and did: on 2026-11-08 every media reference in the corpus resolved in the media layer. It was a separate step in
Config > Media import, after the notes had arrived. It scanned the notes' media references, picked each file's folder
by rules matching the note's first source tag and the field (`media/metadata/media-import-rules.json`), and copied the
files from Anki's media folder. A reference no rule covered was skipped without an error, and every rule named its
repository. Its code (`MediaImportDialog`, `NoteMediaFieldScanner`, `MediaImportPlan`, `MediaImportExecutor`,
`ImportRule`, `MediaImportRulePersistence`, `MissingFilesDialog`) and the rules file are gone. Its report of missing
files is to become a corpus-validity check (`corpus-validity.md`, "Missing media files").

## What happens when a note's audio changes

- **The field gets the new reference** when a speech is kept in JAStudio's TTS dialog (`text-to-speech.md`). Until
  2026-11-08 a save in Anki also brought in audio HyperTTS generated; now it brings in nothing
  (`corpus-data-model.md`, "Anki's role and its consequences").
- **The new file is in the media layer at once:** JAStudio's TTS stores it there itself, and copies it into Anki's
  media folder. Files that come with notes imported through Anki reach the media layer with them, through the import
  run that imports the notes (`import-profiles.md`).
- **The old file stays where it was,** in the media layer and in Anki's media folder. Nothing deletes it. It's just no
  longer referenced: an orphan. Nothing in JAStudio finds orphans yet (`corpus-validity.md`). Anki's "Check Media"
  finds the unused files in its own folder.

## Not done

- **The corpus's media references are still Anki's markup** in the note fields. Slice 9 makes them data: see
  `corpus-data-model.md`, "Media".
- **Removing orphaned files.** Checking the whole corpus could find them, by comparing every note's references with the
  index (`corpus-validity.md`).
- **Access gating:** offering a commercial source's media only to someone entitled to it.
- **Source-revealing tags:** a sentence's `source::…` tags reveal where it comes from. Moving them out of the corpus
  is deferred.

## Specifications

In `JAStudio.Core.Specifications/Storage/Media`: `When_building_a_MediaFileIndex`,
`When_querying_MediaFileIndex_by_original_filename`, `When_storing_a_media_file` and `When_working_with_SourceTag`.
An import run's storing of media is specified with the import profiles, in `AICreatedSpecs/Importing`.

## Dropped: sidecars (2026-05-15)

The earlier design gave each media file a JSON sidecar with the ids of the notes using it, the source tag, a
copyright status and, for TTS audio, the engine and voice. Files were renamed to GUIDs in GUID-named buckets, and the
index could find a note's media by its id. The commit that dropped it (`2353634c3`) removed `SidecarSerializer`,
`MediaFileId`, `TtsInfo` and the media snapshot, about a thousand lines in all. It doesn't record why. **Why (Magnus,
2026-11-10):** complexity, of the storage code and of managing it, not least of understanding changes to the corpus: when
a note's JSON holds everything about it, a human reading a diff in git understands it far more easily.
