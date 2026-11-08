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
3. **The corpus holds no media references.** *(Not done: a note's media fields still hold Anki's markup. See
   [Not done](#not-done).)*

## Where the files are

`jas_database/media` is a folder of the corpus repository, `jas_database`. It used to be a repository of its own,
`JAStudioData-media`, but that made creating vocab by hand a pain, so its files moved into the corpus repository.
*A leftover of that: `media/.git` is a small file that still points at the old repository's git data, so git run
inside `media/` shows the old repository, last committed 2026-03-26, instead of the corpus repository.* In it:

| Folder | What | Files (2026-11-07) |
|---|---|---|
| `commercial-01` | A submodule of the corpus repository, `JAStudioData-media-commercial-01`: commercial vocab audio and images, such as Core 2000's and WaniKani's. New commercial media goes here, since commercial-02 is near GitHub's recommended size | about 15,000 |
| `commercial-02` | A submodule of the corpus repository, `JAStudioData-media-commercial-02`: anime audio and screenshots, per show. About 2 GB | about 69,000 |
| `free` | Freely redistributable media, in the corpus repository itself, among it the TTS audio in `free/TTS/vocab` and `free/TTS/sentences/<source>` | about 35,000 |
| `metadata` | The import rules, `media-import-rules.json` | |

The folders are organizational: they say nothing about a file that the code reads. **A file keeps the name it had in
Anki,** such as `hypertts-0001168b….mp3`, so file names must be unique across the whole tree.

## How a note finds its media

**The note holds the link.** A note's media fields hold Anki's markup in the corpus JSON: `[sound:…]` for audio and
`<img src="…">` for images. That file name is the only connection between a note and a file. The files don't know
their notes.

| Note type | Media fields (`VocabMediaField`, `SentenceMediaField`) |
|---|---|
| Vocab | `AudioFirst` (`Audio_b`), `AudioSecond` (`Audio_g`), `AudioTts` (`Audio_TTS`), `Image`, `UserImage` |
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

*To be replaced (decided 2026-11-08): an import profile stores a note's media when it imports the note, in folders
chosen in the import profile (`import-profiles.md`). This rules-based media import was built to move everything out
of Anki in February 2026, and it did: on 2026-11-08 every media reference in the corpus resolved in the media layer.*

A new file reaches the media layer by being copied from Anki's media folder (`collection.media`) by a media import,
in Config > Media import (`MediaImportDialog`). An import is batch, incremental, and per note type:

1. **Scanning** (`NoteMediaFieldScanner`): one pass over the notes makes a `NoteMediaFieldImportState` for each
   media reference. It records the note's source tag (`JPNote.SourceTag`, from its `source::…` tags), the field, the
   file name, and whether the file is already in the index or in Anki's media folder.
2. **Rules** (`ImportRule`): each rule says that a field of the notes with a source tag in a branch goes to a folder:
   `(source tag prefix, field) → target folder`. The rule with the longest matching prefix wins
   (`ImportRulesCE.TryResolve`). Rules are a positive selection: a field no rule covers isn't imported, with no
   error. They're kept in `media/metadata/media-import-rules.json` (`MediaImportRulePersistence`).
3. **Planning** (`MediaImportPlan`): a file a rule covers is either *to import*, when it's in Anki's media folder,
   *already stored*, when the index has its name, or *missing*, when it's in neither. The dialog shows the counts, and
   lists the missing files in a `MissingFilesDialog`.
4. **Executing** (`MediaImportExecutor`): each file to import is copied by `MediaStorageService.StoreFile` to
   `{media root}/{target folder}/{file name}`, and registered in the index.

## What happens when a note's audio changes

- **The field gets the new reference** when a speech is kept in JAStudio's TTS dialog (`text-to-speech.md`). Until
  2026-11-08 a save in Anki also brought in audio HyperTTS generated; now it brings in nothing
  (`corpus-data-model.md`, "Anki's role and its consequences").
- **The new file is in the media layer at once:** JAStudio's TTS stores it there itself, and copies it into Anki's
  media folder. Files that come with notes imported through Anki reach the media layer through a media import.
- **The old file stays where it was,** in the media layer and in Anki's media folder. Nothing deletes it. It's just no
  longer referenced: an orphan. Nothing in JAStudio finds orphans yet (`corpus-validity.md`). Anki's "Check Media"
  finds the unused files in its own folder.

## Not done

- **The corpus still holds the media references,** as Anki's markup in the note fields. Moving them out has
  unresolved design questions: see `corpus-data-model.md`, "Media (Open)".
- **Removing orphaned files.** Checking the whole corpus could find them, by comparing every note's references with the
  index (`corpus-validity.md`).
- **Access gating:** offering a commercial source's media only to someone entitled to it.
- **Source-revealing tags:** a sentence's `source::…` tags reveal where it comes from. Moving them out of the corpus
  is deferred.

## Specifications

In `JAStudio.Core.Specifications/Storage/Media`: `When_building_a_MediaFileIndex`,
`When_querying_MediaFileIndex_by_original_filename`, `When_storing_a_media_file`, `When_configuring_media_routing`,
`When_working_with_SourceTag`, and `When_importing_media_from_anki` (in `AnkiSync/When_syncing_media_from_anki.cs`).

## Dropped: sidecars (2026-05-15)

The earlier design gave each media file a JSON sidecar with the ids of the notes using it, the source tag, a
copyright status and, for TTS audio, the engine and voice. Files were renamed to GUIDs in GUID-named buckets, and the
index could find a note's media by its id. The commit that dropped it (`2353634c3`) removed `SidecarSerializer`,
`MediaFileId`, `TtsInfo` and the media snapshot, about a thousand lines in all. It doesn't record why.
