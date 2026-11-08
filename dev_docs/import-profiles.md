# Import profiles

> **Status: design agreed with Magnus on 2026-11-08, and increment 2 built the same day:** import profiles for
> sentences, with the editor, validation and running by hand (see [As built](#as-built-2026-11-08)). Not built yet:
> automatic runs, vocab, and subs2srs decks. What import profiles replace is described in
> `media-storage-and-import.md` ("Importing from Anki") and `corpus-data-model.md` ("Import and organization").

## What an import profile is

An **import profile** is a saved description of one kind of import, such as Immersion Kit's sentence cards or one
subs2srs deck. It says:
- which Anki notes to take;
- how their fields become a corpus note's fields;
- which tags the notes get;
- where their media goes in the media layer.

An **import run** applies an import profile. It validates first, reporting every problem before anything changes.
Then it turns the raw notes into corpus notes and stores their media in the media layer, with the audio normalized.

*"Profile" here isn't an Anki profile. The separate Anki profile that imports happen in is called **the Anki
profile for importing**.*

**Raw notes** are the Anki notes an import takes: notes of a note type of their own, such as "Immersion Kit
Sentence" or a subs2srs deck's note type.

## Why

Two things built for the February 2026 move out of Anki don't fit what comes next:
- **The Immersion Kit conversion** was special code. It has been an empty placeholder since the C# port on
  2026-02-07, so Immersion Kit's sentence cards stopped reaching JAStudio.
- **The rules-based media import** (`media-storage-and-import.md`, "Importing from Anki") has several problems:
  - It's a separate manual step after the notes arrive, and until someone runs it, JAStudio can't play or show their
    media.
  - It picks the folder indirectly, from a note's first source tag and the field name.
  - A reference no rule covers is skipped without an error.
  - Every rule names its repository, so when a repository fills up, every rule has to change.

Both did their job. Every media reference in the corpus resolved in the media layer on 2026-11-08. Since the
February move, only about 300 vocab and 10 sentences have been added. Import profiles are for the imports ahead:
- subs2srs decks;
- Immersion Kit's sentence cards, through its "Download sentence card" .apkg files;
- Yomitan.

## Principles (Magnus, 2026-11-08)

1. **A note's media comes with it.** The import run stores the media in the media layer as it imports the note.
   There's no separate media step.
2. **The person importing decides, and nothing guesses.** The import profile names the media folders, and with
   them the copyright regime. The notes can't tell you: Yomitan, for example, picks its audio source automatically,
   and the note doesn't say which source it used.
3. **In Anki, only JAStudio writes notes of JAStudio's own note types.** Everything else arrives as raw notes in a
   note type of its own, and import profiles are the only way from Anki into the corpus. Importing from JAStudio's
   own note types was rejected as iffy. So the take-in of JAStudio-type notes added in Anki
   (`NoteCache.ExternalNoteAdded`) retires once Yomitan has a note type and an import profile of its own.
4. **Anki holds the notes being studied, and the corpus holds them all.** An import doesn't need to create Anki
   notes, and in the Anki profile for importing there's no point. A note deleted in Anki stays in the corpus; that
   was done on 2026-11-08 (`corpus-data-model.md`, "Anki's role and its consequences").

## What an import profile holds

It's edited in an editor window from the start (Magnus: not in a JSON file): **Local Actions > Import profiles**,
where "Convert Immersion Kit sentences" used to be. Every change is saved at once.

| Setting | |
|---|---|
| Name | |
| Anki note type | Mandatory, chosen from a dropdown: the raw notes' note type. It gives the list of fields for the field mapping. JAStudio's own note types aren't offered. |
| Anki search | Optional: narrows the selection within the note type, such as to a deck or tags. It runs through Anki's own search. |
| What it creates | Sentence or vocab. *Sentences only so far: vocab comes with Yomitan's import profile (increment 5).* |
| Field mapping | For each sentence field, the Anki field it takes its value from, chosen from the note type's fields: source question, source answer, reading, external id, source comments, audio and screenshot. |
| Tags | Tags to add, such as `source::immersion_kit`. The raw notes' own tags come along too. |
| Media folders | A folder in the media layer for each mapped media field, such as audio to one folder and screenshots to another. Typed in, and checked to be `free/` or a commercial repository, or inside one (`MediaLayerFolders`). |
| Delete the raw notes afterwards | Yes or no. Magnus wants both options to start with, to compare and check while he gets imports going. |
| Create Anki notes to study | Yes or no: whether each new corpus note gets a note in the open Anki profile. |
| Runs automatically | Yes or no: see [When a run starts](#when-a-run-starts). *Not built yet: increment 3.* |

## An import run

1. **Select** the notes of the note type that match the search and don't carry the tag `jastudio::imported`.
   The tag name is tentative.
2. **Validate,** and show the report. Any error stops the whole run.
3. **For each note that isn't already imported:**
   - create the corpus note from the field mapping, with the tags;
   - store its media in the media layer, in the profile's folders. Audio is normalized to ReplayGain's reference
     (`Mp3LoudnessNormalization`), and the same normalized file replaces Anki's copy.
4. **Then** delete the raw notes, or tag them `jastudio::imported`. The tag keeps later runs from finding them again
   and is visible in Anki. Removing it makes a note importable again. This includes the notes skipped as already
   imported, so that later runs don't warn about them again.

The window's Run executes the run its Check prepared, with the profile as it was then (`ImportProfile.Copy`): a run
does what its report said, even if the profile was edited in between.

## Validation

- How many notes the selection finds.
- **Problems with the profile itself,** while there are any, no notes are selected:
  - no note type, or one Anki doesn't have, or one of JAStudio's own;
  - the source question isn't mapped;
  - a mapped Anki field that the note type doesn't have. The dropdowns guarantee most of this;
  - a media folder that isn't `free/` or a commercial repository, or inside one. That catches the kind of mistake
    that put files in the stray `media/vocab/TTS`.
- **Problems with a note:**
  - its source question is empty;
  - a media file it references isn't in Anki's media folder;
  - its audio isn't MP3. The corpus holds only MP3 (`corpus-validity.md`);
  - another note of the run references the same media file.
- **Already imported:**
  - A note counts as already imported when its external id is already in the corpus, or when any of its media file
    names is already in the media layer.
  - Both are always checked (Magnus). Some decks have no external ids, and decks differ in which of the two is
    reliable. Media file names must be unique across the media layer anyway, so a match on a name means either a
    duplicate or a collision that couldn't be stored.
  - Such notes are skipped, under a clear warning that lists each note and what matched.

## When a run starts

- **By hand,** from the import window.
- **Automatically,** for import profiles marked to run that way, on either of two signals from Anki:
  - **an import finished.** Anki's .apkg importer runs in its Rust backend and doesn't fire `note_will_be_added`.
    It only announces the finished operation: `operation_did_execute`, with the `ImportDialog` as source. Verified
    on 2026-11-08 in Anki 26.9.3. This is the signal for Immersion Kit's .apkg sentence cards and for decks.
  - **a note was added** (`note_will_be_added`). AnkiConnect adds notes this way, and Immersion Kit and Yomitan
    can both use it.

  Both signals do the same thing: run every automatic import profile over its selection. An automatic run that
  fails validation opens the import window with its report instead of running.

## Anki knows nothing of particular imports

Anki's side stays generic:
- it announces the two signals (increment 3);
- it finds notes by an Anki search (`notes_find` in `anki_facade_backend.py`);
- it tags and deletes notes (`notes_add_tag`, `notes_remove`).

C# reads note types, fields and notes straight from Anki's database, as it does at startup (`AnkiRawNotes`, like
`NoteBulkLoader`). Everything specific to an import, Immersion Kit's included, is an import profile.

## The first import profile: Immersion Kit

It's made in the import profiles window like any other. The mapping is the one the 781 Immersion Kit sentences
already in the corpus were imported with:

| Immersion Kit field | Corpus sentence |
|---|---|
| Expression | source question |
| English | source answer |
| Reading | reading, furigana as it is |
| ID, such as `anime_the_garden_of_words_000000346` | external id |
| Audio Sentence | audio |
| Screenshot | screenshot |

The tag is `source::immersion_kit`. The media goes to `commercial-01/anime/immersion-kit/audio` and
`commercial-01/anime/immersion-kit/screenshots`. commercial-02, where the earlier Immersion Kit media is, is about 2
GB, so new commercial media goes to commercial-01. Files are found by name anywhere in the media layer, so the split
doesn't matter to lookups.

## What it replaces

- **Immersion Kit,** *all removed 2026-11-08:*
  - the Python stub `local_note_updater.convert_immersion_kit_sentences`, with its package `jastudio.batches`;
  - the hook `convert_immersion_kit_sentences_on_import.py`;
  - `AnkiFacade.Batches.ConvertImmersionKitSentences`, which called a Python function removed on 2026-03-25;
  - the Immersion Kit constants, kept for the restoration: `AnkiFieldNames.ImmersionKit`,
    `NoteTypes.ImmersionKit` and `QueryBuilder.ImmersionKitSentences`. The field names are now an import profile's
    data.

  The menu item "Convert Immersion Kit sentences" became "Import profiles".
- **The rules-based media import:** `MediaImportDialog`, `NoteMediaFieldScanner`, `MediaImportPlan`,
  `MediaImportExecutor`, `ImportRule`, and `media/metadata/media-import-rules.json`. They retire once subs2srs decks
  go through import profiles. Its missing-files report becomes one of the corpus-validity checks.
- **The take-in of JAStudio-type notes added in Anki** (`NoteCache.ExternalNoteAdded`), once Yomitan has its own
  import profile.

## Increments

1. *Done 2026-11-08:* a note deleted in Anki stays in the corpus and in JAStudio. Its Anki note and cards are
   forgotten.
2. *Built 2026-11-08:* import profiles: the editor, validation, and running by hand, with Immersion Kit as the
   first import profile, imported through .apkg.
3. Automatic runs on both signals.
4. subs2srs decks, and retiring the rules-based media import.
5. Yomitan through a note type of its own and an import profile, and retiring `ExternalNoteAdded`'s take-in.

## Decided since (Magnus, 2026-11-08)

- **Import profiles are saved in the corpus repository.** They name the corpus's sources and media folders, and they
  replace `media/metadata/media-import-rules.json`, which lives there too.
- **A validation error blocks the whole run,** as the simple version: the notes are fixed in Anki, and the profile is
  run again. Already-imported notes aren't errors: they get the warning and are skipped.

## As built (2026-11-08)

- **`JAStudio.Core.Importing`:**
  - `ImportProfile`, with its `SentenceFieldMapping`;
  - `SavedImportProfiles`, which keeps them in `import-profiles.json` in the corpus repository;
  - `Importer.Prepare`, which selects and validates (`ImportRunValidation`) and returns an `ImportRun`. The run holds
    its `Problems` and `AlreadyImported` notes, and `Execute` runs it.
- **`JAStudio.Core.Importing.RawNotes`:** `RawNote`, `AnkiNoteType`, and `IAnkiRawNotes`, which is Anki's side.
  `AnkiRawNotes` in `JAStudio.Anki` implements it; specifications use `InMemoryAnkiRawNotes`.
- **Corpus only:** a sentence imported without an Anki note is added with `SentenceCollection.AddWithoutAnAnkiNote`.
- **Dependencies:** `JAStudio.Core` references `Mp3LoudnessNormalization` to normalize the audio.
- **UI:** `ImportProfilesWindow`, with `ImportProfilesWindowViewModel` and an `ImportProfileEditorViewModel` per profile.
- **Specifications:** `AICreatedSpecs/Importing` in `JAStudio.Core.Specifications`. They check, among other things,
  that the stored audio is byte for byte what mp3gain makes of it.

## Open questions

- **The vocab fields the mapping offers.** To decide when vocab is imported.
- **Magnus's mp3gain script** (`normalize-anki-collection.ps1`) would tag Anki's copy of the files an import run has
  normalized. They'd then differ from the media layer's copies by that tag, unless the script leaves them out.
- **Yomitan:** it shows whether a word is "already in Anki" by looking for the note in Anki. Its import profile
  should therefore keep the raw notes.
- **Studying corpus notes in an Anki profile that doesn't have them yet** means creating their Anki notes and copying
  their media from the media layer into that profile's media folder. That's for later. It belongs to "Corpus entries
  without Anki notes" in `corpus-data-model.md`.
