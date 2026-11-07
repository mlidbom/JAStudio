# Media Storage — Design & Architecture

## Goals & Motivations

We are building a corpus of analyzed Japanese sentences/vocab for language study. Most sentences come from copyrighted material (anime, textbooks, learning platforms). Using them for study with linguistic breakdowns is fair use, but rights holders can be aggressive, so we need to:

1. **Isolate media from the core corpus.** The core corpus is pure language analysis — text, breakdowns, grammar, readings. No media files, no media references, no source-identifying data.
2. **Partition media by copyright regime.** Media lives in separate git repositories organized so that a takedown for one source (e.g. one anime) can be handled by deleting files without touching the core data or other media.
3. **Keep git repos at manageable size.** Binary media files (audio, images) are large. Separating them from the text corpus keeps the core repo small and fast.

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│  Core Corpus (git repo)                             │
│  jas_database/                                      │
│    kanji/{bucket}/{id}.json     ← pure language     │
│    vocab/{bucket}/{id}.json        analysis data    │
│    sentences/{bucket}/{id}.json    NO media refs    │
│    media/                       ← media layer       │
│      metadata/                  ← import rules etc  │
│      {repo}/                    ← media files +     │
│        {source}/                   sidecars         │
│          {bucket}/{guid}.*                          │
└─────────────────────────────────────────────────────┘
```

### Key Principles

- **Core corpus stores no media data.** Not even MediaFileIds. The note JSON is purely about language analysis.
- **All note↔media association lives in the media layer.** Each media file's sidecar JSON declares which note it belongs to.
- **Directory structure is organizational, not semantic.** The routing config determines where files land at import time, but the path encodes no meaning — all metadata is in the sidecar JSON.
- **Sidecar JSON per media file.** Every media file has a companion `.json` file with the same GUID name containing all metadata about that file.
- **Note scanner is scoped to note directories.** `FileSystemNoteRepository` scans only `kanji/`, `vocab/`, and `sentences/` — it does not walk `media/` or any other subdirectory of `jas_database/`. This avoids both performance issues (thousands of media files) and GUID-parsing failures on non-note JSON files.

## Sidecar Metadata Format

Each media file is stored as a pair: `{guid}.{ext}` + `{guid}.json`. Different media types have **separate sidecar schemas** — audio and image are fundamentally different data and will diverge further over time (e.g. audio has duration, TTS engine; image has resolution; video will have frame range, subtitles track, etc.).

### JSON Serialization

The serializer is configured with `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault`. This means properties with `null`, `0`, `false`, or other default values are omitted from the JSON output. Benefits:

- **Smaller files** — most sidecar fields are optional; no need to write `"tts": null` on every non-TTS audio file
- **Clean git diffs** — when new optional properties are added to the schema, existing files don't change (no new `"newField": null` lines appearing across thousands of files)
- **Readable** — opening a sidecar shows only what's relevant to that specific file

The `noteSourceTag` field is a `SourceTag` value — a structured, `::` -separated hierarchical identifier (e.g. `source::anime::natsume::s1::01`). `SourceTag` is a domain type that supports hierarchical containment checks (`IsContainedIn`, `Contains`), proper equality, and rejects invalid values (empty strings, empty segments). This enables filtering, bulk updates, and re-routing by source without re-parsing note data.

### Shared media files

A single media file (e.g. pronunciation audio for 走る) may be referenced by multiple notes — the vocab note for 走る and every sentence note containing 走る. The sidecar uses `noteIds` (plural) to track all associated notes. During import, when a file is encountered that already exists (by original filename), its sidecar is updated to append the new note ID rather than creating a duplicate file.

### Audio Sidecar (`{guid}.audio.json`)

**Copyrighted anime audio (sentence):**
```json
{
  "noteIds": ["9f8e7d6c-5b4a-3210-fedc-ba9876543210"],
  "noteSourceTag": "source::anime::natsume::s1::01",
  "originalFileName": "natsume_ep01_03m22s.mp3",
  "copyright": "Commercial"
}
```

**TTS-generated audio (wani sentence):**
```json
{
  "noteIds": ["9f8e7d6c-5b4a-3210-fedc-ba9876543210"],
  "noteSourceTag": "source::wani::level05",
  "copyright": "Free",
  "tts": {
    "engine": "azure-neural",
    "voice": "ja-JP-NanamiNeural",
    "version": "2025.1"
  }
}
```

**WaniKani vocabulary audio:**
```json
{
  "noteIds": ["abc12345-6789-0abc-def0-123456789abc"],
  "noteSourceTag": "source::wani::level05",
  "originalFileName": "走る_audio_b.mp3",
  "copyright": "Commercial"
}
```

**Shared pronunciation audio (vocab + multiple sentences):**
```json
{
  "noteIds": [
    "abc12345-6789-0abc-def0-123456789abc",
    "9f8e7d6c-5b4a-3210-fedc-ba9876543210",
    "11223344-5566-7788-99aa-bbccddeeff00"
  ],
  "noteSourceTag": "source::core2000::step01",
  "originalFileName": "走る_core2k.mp3",
  "copyright": "Commercial"
}
```

### Image Sidecar (`{guid}.image.json`)

**Screenshot from anime:**
```json
{
  "noteIds": ["9f8e7d6c-5b4a-3210-fedc-ba9876543210"],
  "noteSourceTag": "source::anime::natsume::s1::01",
  "originalFileName": "natsume_ep01_03m22s.png",
  "copyright": "Commercial"
}
```

### File naming convention

The sidecar extension encodes the media type, so the media type is knowable from the filename alone without reading the JSON:

```
a1b2c3d4-e5f6-7890-abcd-ef1234567890.mp3          ← audio file
a1b2c3d4-e5f6-7890-abcd-ef1234567890.audio.json   ← audio sidecar

f7e6d5c4-b3a2-1098-7654-321fedcba098.png           ← image file
f7e6d5c4-b3a2-1098-7654-321fedcba098.image.json    ← image sidecar
```

Each sidecar schema is independently extensible — adding audio-specific fields (duration, bitrate) or image-specific fields (resolution, format) doesn't affect the other type.

## Import-Time Routing

Import rules are **flat, atomic instructions**: each rule says "for notes of this type, with this source tag prefix, take media from this specific field, store it in this directory with this copyright." One rule = one (note type, source tag, field) combination.

**Rules are a positive selection.** They specify exactly what to import. Fields without a matching rule are simply not imported — no error, no warning. This enables incremental workflows: configure a few rules, run the import, inspect results, add more rules, run again. Files already in storage are not affected.

**Rules are disposable.** After a batch runs, its rules have done their job. They can be deleted — no files will match them again since the media is already stored.

### Rule types

```csharp
// Field enums — typed per note type, compile-time safe
enum VocabMediaField    { AudioFirst, AudioSecond, AudioTts, Image, UserImage }
enum SentenceMediaField { Audio, Screenshot }
enum KanjiMediaField    { Audio, Image }

// One rule = one (note type, source tag, field) → (directory, copyright)
record VocabImportRule(SourceTag Prefix, VocabMediaField Field, string TargetDirectory, CopyrightStatus Copyright);
record SentenceImportRule(SourceTag Prefix, SentenceMediaField Field, string TargetDirectory, CopyrightStatus Copyright);
record KanjiImportRule(SourceTag Prefix, KanjiMediaField Field, string TargetDirectory, CopyrightStatus Copyright);
```

### Example: importing WaniKani vocab media

```csharp
// Commercial audio goes to one directory, free TTS to another
new VocabImportRule(SourceTag.Parse("source::wani"), VocabMediaField.AudioFirst,  "commercial-002/wanikani", CopyrightStatus.Commercial)
new VocabImportRule(SourceTag.Parse("source::wani"), VocabMediaField.AudioTts,    "free-vocab/tts",          CopyrightStatus.Free)
new VocabImportRule(SourceTag.Parse("source::wani"), VocabMediaField.Image,       "commercial-002/wanikani", CopyrightStatus.Commercial)
// AudioSecond and UserImage are not configured — they won't be imported in this batch
```

### Resolution

`MediaImportRuleSet` holds all rules, grouped by note type. Resolution returns `null` for unconfigured combinations:

```csharp
var ruleSet = new MediaImportRuleSet(vocabRules, sentenceRules, kanjiRules);

ruleSet.TryResolveVocab(SourceTag.Parse("source::wani::level05"), VocabMediaField.AudioFirst);
// → VocabImportRule("commercial-002/wanikani", Commercial)

ruleSet.TryResolveVocab(SourceTag.Parse("source::wani::level05"), VocabMediaField.AudioSecond);
// → null (not configured, skip)
```

Source-tag matching uses hierarchical containment, longest prefix first — same as before.

**This config is import-time only.** Once files are stored, the rules are not consulted again. `MediaStorageService` takes the resolved target directory directly — it has no dependency on routing.

## Storage Path Structure

Files are stored at:
```
{media-repo}/{routed-directory}/{guid-bucket}/{guid}.{ext|json}
```

The GUID bucket is the first two hex characters of the GUID, keeping any single directory to ~1/256th of total files:

```
commercial-001/natsume/a1/a1b2c3d4-e5f6-7890-abcd-ef1234567890.mp3
commercial-001/natsume/a1/a1b2c3d4-e5f6-7890-abcd-ef1234567890.audio.json
commercial-001/natsume/f7/f7e6d5c4-b3a2-1098-7654-321fedcba098.png
commercial-001/natsume/f7/f7e6d5c4-b3a2-1098-7654-321fedcba098.image.json
free-vocab/tts/3c/3c4d5e6f-a7b8-9012-cdef-123456789012.mp3
free-vocab/tts/3c/3c4d5e6f-a7b8-9012-cdef-123456789012.audio.json
```

## Runtime: Building the Domain Model

`MediaFileIndex` is lazy-initialized — it scans all media directories under `App.MediaDir` on first access, not at startup. The scan reads every `*.audio.json` and `*.image.json` sidecar to build an in-memory index. The `metadata/` subdirectory is excluded. This means the first operation that touches the index (typically clicking Scan in the import dialog) pays the full cost of the filesystem walk.

**Index lookups:**
- `NoteId → NoteMedia` — all media for a note, typed (a single media file appears in the results for every note in its `NoteIds` list)
- `MediaFileId → AudioAttachment | ImageAttachment` — single file by GUID
- `OriginalFileName → MediaAttachment` — for dedup during import

**Runtime domain objects (built from sidecar data at index time):**
```csharp
public enum CopyrightStatus
{
    Unknown = 0,   // not yet classified
    Commercial,    // requires verified account or license
    Free           // freely redistributable (TTS, CC-licensed, etc.)
}

// SourceTag — hierarchical, ::‑separated, validated domain type
// Supports containment: tag.IsContainedIn(ancestor), tag.Contains(descendant)
// Serialized to/from JSON as a plain string
public sealed class SourceTag : IEquatable<SourceTag>
{
    public static SourceTag Parse(string value);
    public IReadOnlyList<string> Segments { get; }
    public bool IsContainedIn(SourceTag ancestor);
    public bool Contains(SourceTag descendant);
}

// Base — common storage/identity data shared by all media types
public abstract record MediaAttachment
{
    // Persisted in sidecar JSON
    public required MediaFileId Id { get; init; }
    public required List<NoteId> NoteIds { get; init; }     // all notes that reference this file
    public required SourceTag NoteSourceTag { get; init; }   // full tag, e.g. "source::anime::natsume::s1::01"
    public string? OriginalFileName { get; init; }
    public CopyrightStatus Copyright { get; init; } = CopyrightStatus.Unknown;  // not required — defaults for backward compat

    // Runtime only — resolved by MediaFileIndex from the filesystem, not serialized
    [JsonIgnore] public string FilePath { get; internal set; } = string.Empty;
}

// Audio — extends with audio-specific fields
public record AudioAttachment : MediaAttachment
{
    public TtsInfo? Tts { get; init; }                    // non-null if TTS-generated
}

public record TtsInfo(string Engine, string Voice, string Version);

// Image — extends with image-specific fields (none yet, but will diverge)
public record ImageAttachment : MediaAttachment;

// Import rules — flat, atomic, per (note type, source tag, field)
enum VocabMediaField    { AudioFirst, AudioSecond, AudioTts, Image, UserImage }
enum SentenceMediaField { Audio, Screenshot }
enum KanjiMediaField    { Audio, Image }

record VocabImportRule(SourceTag Prefix, VocabMediaField Field, string TargetDirectory, CopyrightStatus Copyright);
record SentenceImportRule(SourceTag Prefix, SentenceMediaField Field, string TargetDirectory, CopyrightStatus Copyright);
record KanjiImportRule(SourceTag Prefix, KanjiMediaField Field, string TargetDirectory, CopyrightStatus Copyright);

public class MediaImportRuleSet(List<VocabImportRule>, List<SentenceImportRule>, List<KanjiImportRule>)
{
    // Returns null for unconfigured combinations — no error, just not imported
    public VocabImportRule? TryResolveVocab(SourceTag sourceTag, VocabMediaField field);
    public SentenceImportRule? TryResolveSentence(SourceTag sourceTag, SentenceMediaField field);
    public KanjiImportRule? TryResolveKanji(SourceTag sourceTag, KanjiMediaField field);
}
```

The base `MediaAttachment` gives shared storage/serialization/index logic a single type to work with. `MediaFileIndex` can store all attachments as `MediaAttachment` internally, while typed accessors return `AudioAttachment` or `ImageAttachment`.

**Domain notes expose typed media from the index:**
```csharp
// SentenceNote — assembled at load time
public NoteMedia Media { get; }    // .Audio, .Images

// VocabNote — same
public NoteMedia Media { get; }
```

Multiple audio sources per note is the norm — copyrighted original audio, TTS from multiple engines, human recordings from free sources. The access/preference layer selects which to present.

## Core Corpus Data (Source-Blind)

The note JSON stored in the core corpus contains **no media references at all**:

```csharp
// SentenceData — what's on disk in core corpus
// NO AudioId, NO ScreenshotId, NO MediaIds
// Just: text, breakdown, grammar, readings, translation, etc.

// VocabData — same
// NO audio fields, NO image fields
// Just: word, readings, meanings, pitch accent, usage notes, etc.
```

The association `note ↔ media` exists only in the media layer's sidecar files (via `noteId`).

## Access Gating (Future)

Media access is controlled at runtime based on the user's verified accounts:

```
noteSourceTag.IsContainedIn("source::anime::natsume")  →  requires Crunchyroll (or relevant licensor)
noteSourceTag.IsContainedIn("source::wani")             →  requires verified WaniKani account
copyright == Free                                       →  always available
tts != null                                             →  always available
```

A note might have 5 audio attachments. The access layer filters to what the user is entitled to, then the preference layer selects the best one (community-ranked, student-overridable, curator-pinned).

## Takedown Handling

If a rights holder demands removal of media from a source:

1. Identify all sidecar files matching the source (e.g. `noteSourceTag.IsContainedIn(SourceTag.Parse("source::anime::natsume"))`)
2. Delete the media files and their sidecars
3. Commit the deletion to the media repo
4. Core corpus is untouched — notes still load, they just have fewer (or no) audio/image options

Because media repos are separate git repositories, an entire repo can be deleted without affecting the core corpus or other media repos.

## Source-Revealing Tags (Future)

Note tags like `source::anime::natsume::s1::01` on sentence notes in the core corpus reveal the source. This is a separate concern from media storage but related to the same copyright goal. Eventually these tags should move out of the core corpus — possibly into the media repo's sidecar data or a separate metadata layer. Deferred for now.

## Deduplication Strategy

During import, `MediaFileIndex.TryGetByOriginalFileName()` checks if a file with the same original name is already stored:

- **First encounter:** file is copied, sidecar created with `noteIds: [currentNoteId]`
- **Subsequent encounters (same filename, different note):** file is NOT copied again; the existing sidecar's `noteIds` list is updated to include the new note ID
- The check is case-insensitive

This is name-based dedup, not content-based. Acceptable because Anki media files are essentially immutable once created.

## Current Implementation Status

The storage layer (`JAStudio.Core.Storage.Media`) is implemented and tested:

### Implemented

| Component | Status |
|---|---|
| `SourceTag` | Domain type with `::` hierarchy. `Parse()`, `IsContainedIn()`, `Contains()`, equality, JSON serialization. Rejects empty/invalid values. |
| `MediaAttachment` / `AudioAttachment` / `ImageAttachment` | Classes holding the original file name and the file's path. *Corrected 2026-11-07: this row said they have a `SourceTag NoteSourceTag` field and that audio has `TtsInfo?`, but the code has neither. The sidecar format above is the design; see `text-to-speech.md`, "Open questions".* |
| `SidecarSerializer` | Writes/reads typed sidecar JSON (`*.audio.json` / `*.image.json`). Omits defaults. Custom JSON converters for `NoteId`, `MediaFileId`, `SourceTag`. |
| `MediaFileId` | GUID-based value type with `Parse`, `TryParse`, `New`. |
| `MediaFileIndex` | Builds from filesystem scan of sidecar files. Lookups by ID, original filename (case-insensitive), note ID. In-memory `Register()` for newly stored files. |
| `MediaStorageService` | Stores files with GUID-bucket paths, writes typed sidecars, updates existing sidecars for shared files (dedup by original filename). Takes target directory as parameter — no routing dependency. |
| `VocabMediaField` / `SentenceMediaField` / `KanjiMediaField` | Per-note-type enums for media fields. Compile-time safe — no string-based field names. |
| `VocabImportRule` / `SentenceImportRule` / `KanjiImportRule` | Flat atomic rules: one rule = one (source tag, field) → (directory, copyright). |
| `MediaImportRuleSet` | Holds all rules, resolves by (source tag, field). Returns `null` for unconfigured combinations — fields without rules are silently skipped. Longest prefix match. |
| `MediaImportPlan` | Output of analysis: `FilesToImport` (ready to execute), `AlreadyStored` (sidecar noteId updates), `Missing` (files not found in Anki). Pure data — no side effects. |
| `MediaImportAnalyzer` | Analyzes notes against rules. Per-note-type methods: `AnalyzeVocab`, `AnalyzeSentences`, `AnalyzeKanji`. Produces a `MediaImportPlan`. No side effects — reads filesystem and index to classify each media reference. |
| `MediaImportExecutor` | Executes a `MediaImportPlan` — copies files, writes sidecars, updates noteIds. Note-type agnostic — works only with the flat plan. No analysis, no decision-making. Uses `TaskRunner.RunBatch` for visible progress. |
| `MediaImportRulePersistence` | Saves/loads rules to `jas_database/media/metadata/media-import-rules.json`. JSON with enum and `SourceTag` converters. Rules are stored in the media layer, not user files. |
| `MediaImportDialog` | Avalonia dialog (View + ViewModel). Scan discovers un-imported media grouped by source/type. User configures rules (sorted by source tag prefix then target directory), clicks Analyze to see plan counts (`FilesToImport`, `AlreadyStored`, `Missing`), then executes. The Missing count is a clickable link that opens a `MissingFilesDialog` with per-note-type tabs (Vocab, Sentences, Kanji) showing question, note ID, field, and filename in a sortable DataGrid. Double-clicking a row opens the note in Anki's browser. Rules persist across sessions. Accessible from Config > Media import menu. |
| `MissingFilesDialog` | Avalonia dialog showing files referenced by notes but not found in Anki's media folder, organized in tabs by note type with sortable DataGrid columns. Supports double-click to open the note in Anki. |

### Completed milestones

- **Initial media import complete.** All media files from Anki have been imported into the structured media storage layer. The first full batch import ran successfully across all note types.

### Not yet implemented

| Component | Notes |
|---|---|
| `NoteMedia` aggregate | Per-note typed media view (`Audio`, `Images`). Not yet needed — media is currently accessed via the index directly. |
| Removal of media fields from corpus DTOs | Notes still contain Anki markup audio/image fields. Has unresolved design concerns — deferred for later discussion. |
| Access gating | Runtime filtering by user's verified accounts / copyright status. |

### Test coverage

All components have BDD-style tests:

- `When_working_with_SourceTag` — parsing, containment, equality
- `When_configuring_media_import_routing` — flat rule matching, longest prefix, null for unconfigured fields, per-field copyright verification
- `When_serializing_a_sidecar` — JSON roundtrip for audio/image, TTS, multiple note IDs, file read/write
- `When_building_a_MediaFileIndex` — filesystem scan, sidecar parsing, lookups
- `When_querying_MediaFileIndex_by_original_filename` — exact/case-insensitive lookup, registered attachments
- `When_storing_a_media_file` — target directory, GUID-bucket paths, sidecar writing, index rebuilding
- `When_importing_media_from_anki` — analyze → execute flow, plan inspection (files to import, already stored, missing), per-note-type batches, dedup, unconfigured field skipping, typed attachments
- `When_parsing_media_references` — audio and image regex parsing, apostrophe handling in filenames, multiple references, blank input

## Import Workflow

Import is **batch, incremental, and per note type**. Analysis and execution are separate concerns:

- **`MediaImportAnalyzer`** — knows note types, scans notes against rules, produces a `MediaImportPlan`. Pure analysis, no side effects.
- **`MediaImportPlan`** — flat data: files to import, files already stored, files missing from Anki. Counts for the UI to display.
- **`MediaImportExecutor`** — takes a plan, executes it. Note-type agnostic, no analysis, no decisions. Should Just Work or explode.

### Workflow

1. **Discovery:** The UI scans all notes, checks which media files exist in Anki but not yet in our storage, and presents what's un-imported — grouped by note type and source tag prefix.
2. **Configure rules:** The user creates import rules for the combinations they want to import now.
3. **Analyze & review:** `MediaImportAnalyzer.AnalyzeVocab(notes, rules)` → `MediaImportPlan`. UI shows: "2,341 to import, 3 missing from Anki, 847 already stored." The user reviews the plan and adjusts rules until the numbers look right.
4. **Execute:** User is satisfied with the plan, clicks Run. `MediaImportExecutor.Execute(plan)` — does exactly what the plan promised. No report needed.
5. **Repeat:** Add more rules, analyze again, review, execute again.

### Example

```csharp
var rules = new[]
{
    new VocabImportRule(SourceTag.Parse("source::wani"), VocabMediaField.AudioFirst, "commercial/wani", CopyrightStatus.Commercial),
    new VocabImportRule(SourceTag.Parse("source::wani"), VocabMediaField.AudioTts,   "free/tts",        CopyrightStatus.Free)
};

// 1. Analyze — no side effects
var plan = analyzer.AnalyzeVocab(allVocabNotes, rules);
// plan.FilesToImport.Count == 2341
// plan.AlreadyStored.Count == 847
// plan.Missing.Count == 3

// 2. Execute — copies files, writes sidecars
executor.Execute(plan);
```

### Plan review (presented to user before execution)

The `MediaImportPlan` contains all the information the user needs to decide whether to proceed:

- Files to import (audio / image breakdown)
- Files already in storage (noteIds will be updated)
- Files missing from Anki media

Execution does exactly what the plan says — no surprises, no separate report needed.
