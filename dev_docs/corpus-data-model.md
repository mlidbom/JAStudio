# Corpus Data Model — Tentative Draft

> **Status: tentative draft (2026-11-05). Nothing here is decided.** It was written from a survey of the code
> and the corpus, plus Magnus's answers to a first round of questions. It is meant to be argued with.
> Sections marked **Open** have no proposal yet, or only a leaning.

## Why this document exists

JAStudio used to keep its data in Anki notes. The corpus now lives in JSON files in `src/jas_database`, but the
move changed where the data lives, not what shape it has. Each corpus file is close to a one-to-one copy of the
old Anki fields. It keeps:

- the same fields;
- the same ways of hiding meaning in the characters of a string;
- the same values that are computed from other data yet stored anyway;
- Anki tags that hold configuration.

The domain model (`JPNote`, `VocabNote`, `KanjiNote`, `SentenceNote` and their components) and the editors still
work on those strings. The goal is a clear, well-designed object model with JSON storage that mirrors it, and
nothing stored only for Anki's sake.

## Terms used in this document

- **in-string-micro-syntax** — meaning hidden in a string's characters, which code has to parse back out. For
  example, brackets around a form, `question:disambiguation`, `<primary>` HTML around a kanji reading, or
  `[sound:file.mp3]`.
- **persisted-derived-data** — a value computed from other data that is stored anyway, for example
  `activeAnswer` or `sentenceCount`.
- **Anki-projection** — the mapping from the domain model to Anki note fields and tags. It runs on every flush.
- **Anki-ingestion** — the mapping from an Anki note into the domain model. It runs when a note is added in
  Anki and when an Anki note is saved.
- **overridable-text** — text that comes from a source (WaniKani, a dictionary, a subtitle file) and that
  Magnus can override with his own text. Examples are `sourceAnswer`/`userAnswer` and
  `sourceQuestion`/`userQuestion`.
- **word-reference** and **note-id-reference** — the two ways one entity can refer to another. See
  [References between entities](#references-between-entities-open).
- **vertical-slice** — one concept changed end to end in a single increment: the domain value type, the JSON
  shape, a one-time corpus migration, and the editor control.

## Settled so far (Magnus, 2026-11-05)

| Question | Answer |
|---|---|
| Anki's role from now on | The review scheduler, a way to import notes, and a way to update audio. |
| Consumers of `jas_database` | Only Magnus, for the foreseeable future. One-time migrations are enough. |
| Origin of `-::priority::…` and `-::sentence::difficulty::…` tags | Tagged by hand in Anki, usually a whole deck or season at a time. |
| Renaming "note" (`JPNote`, `VocabNote`, …) now that JAStudio owns the data | Probably yes, but as a separate effort. Out of scope here. |
| The "unresolved design concerns" about removing media fields (in `media-storage-and-import.md`) | Not remembered. See [Media](#media-open). |
| Ids or text for references between entities | Undecided. See [References between entities](#references-between-entities-open). |

## Anki's role and its consequences

Today Anki is a full two-way partner:

- **Outbound:** every domain flush writes all of a note's old Anki fields and all of its tags to the Anki note.
  The path is `NoteFlushGuard.Flush` → `NoteCache` update listeners → Python `_update_anki_note` →
  `JPNoteDataShim.sync_note_to_anki_note`, which writes `JPNote.GetData()`
  (= `ToCorpusData().ToNoteData()`).
- **Inbound:** every save on the Anki side runs `MergeAnkiData` on the corpus data types. For vocab, that pulls
  `Question`, `ActiveAnswer`, the audio fields, `Image`, `UserImage` and **all tags** back into the corpus.
  Kanji and sentences pull similar sets.

With Anki reduced to scheduler, note import and audio updates, the tentative consequences are:

1. **The Anki-projection becomes one-way and shrinks.** It writes only what Anki's card templates and the Anki
   browser searches still need. `dev_docs/web-rendering-architecture.md` already plans for trivial templates,
   so the projection should shrink over time.
2. **The Anki-ingestion has exactly two jobs:**
   - **New note import:** build a domain note from a new Anki note (Yomitan, imported decks). This is the one
     place where Anki-format strings are parsed into the domain model.
   - **Audio update:** when an audio field changes in Anki (for example HyperTTS generating audio), take the new
     audio into the media layer.

   Everything else Anki sends back is ignored: question, answer, images (**Open**: images too, or audio only?)
   and tags.
3. **Edits made in Anki's own note editor would be silently overwritten** on the next flush. **Open:** is that
   acceptable, or should the Anki editor be redirected to the JAStudio editors?
4. **Tagging a whole deck in Anki would stop reaching the corpus.** That is how priority and difficulty tags
   were applied. **Open:** before tag ingestion is cut, does JAStudio need its own bulk tagging, or is
   priority handled as in [Tags](#tags)?
5. **Both mappings move to `JAStudio.Anki`.** Today they live inside the corpus data types (`ToNoteData`,
   `PopulateAnkiNoteFields`, `FromAnki`, `MergeAnkiData` in `VocabData`, `KanjiData`, `SentenceData`), and
   `AnkiNoteData` and `NoteFieldsConstants` sit in `JAStudio.Core.Note`. That breaks the anti-corruption-layers
   code standard (foreign formats only at the boundary) and Core's own rule of having no Anki dependency.

## Current state

### In-string-micro-syntaxes

| Concept | Where | Encoding |
|---|---|---|
| Owned form | `VocabNoteForms.ComputeOwnedForms` | `[form]` in the `forms` list |
| Alternative compound part | `VocabNoteUserCompoundParts` | `[part]` in `userCompounds`: the same syntax as owned forms, with a different meaning |
| Disambiguation | `VocabNoteQuestion` (`DisambiguationMarker`) | `question:disambiguation` (32 vocab) |
| Invalid / empty question | `VocabNoteQuestion` | Sentinel strings `InvalidQuestionMessage` and `"[EMPTY]"` stored as the question |
| Prefix / suffix marker | `Mine.VocabPrefixSuffixMarker` | `〜` in the question and forms, stripped as a "noise character". This is also standard dictionary notation, so it may rightly stay text. |
| Parts of speech | `VocabNotePartsOfSpeech`, `POSSetManager.InternAndHarmonize` | One comma-separated string, with duplicate spellings on disk (`"noun, suru verb"` 1,436 times, `"noun, する verb"` 774 times) |
| Primary kanji readings | `KanjiNote.PrimaryReadingPattern`, plus a duplicate regex in `LocalNoteUpdater` | Comma-separated HTML: `<primary>ひ</primary>, ひつ` in `readingOnHtml` / `readingKunHtml` / `readingNanHtml` |
| Primary kanji meaning | kanji `sourceAnswer` | `<primary>Stab</primary>, Thrust` |
| Media references | `MediaFieldParsing`, `WritableAudioValue`, `WritableImageValue` | Anki's `[sound:x.mp3]` and `<img src="…">` |
| Word-break hints | sentence questions | `<wbr>` HTML, stripped by `StripHtmlOnReadFallbackStringField` |

The brackets have already caused two bugs: `a921f09cb` (alternative compound parts lost when editing a vocab)
and `51a2ab4ae` (owned forms lost when editing a vocab). About 217 vocab files use them.

Two encodings look like in-string-micro-syntaxes but are probably fine as text:

- **WaniKani markup** in mnemonics (`<radical>`, `<kanji>`, `<reading>`, `<ja>`) is real rich text from the
  source.
- **Answer text** uses ` | ` between senses. This is a display convention produced by `DictEntry`, and nothing
  parses it back. **Open:** should an answer one day become a list of senses?

### Configuration stored as tags

These are settings, not labels, kept as `::`-path strings in the tag list (`Note/Tags.cs`):

- **Requires/forbids flags:** 18 pairs such as `-::vocab::matching::requires::godan` /
  `…::forbids::godan`, read through `RequireForbidFlagField` in
  `VocabMatchingRulesConfigurationRequiresForbidsFlags`.
- **Boolean matching flags:** through `TagFlagField` in `VocabMatchingRulesConfigurationBoolFlags`.
- **Register:** 14 values (`VocabNoteRegister`).
- **Other system tags:** `_uk`, `_disable_uk`, `_tts_audio`.
- **Source:** `source::…`. A `SourceTag` domain type already exists, but `JPNote.SourceTag` derives it from the
  tag list.
- **Priority:** `-::priority::high::011_Natsume_01`. `JPNote.PriorityTagValue` parses out the number with
  `StringExtensions.FirstNumber`, and `VocabSentencesList.razor` orders sentences by it. `JPNote.GetMetaTags`
  turns it into CSS classes.

Because `MergeAnkiData` replaces the tag list with Anki's, Anki co-owns all of this today. The `VocabFlagsDialog`
editor is already structured (checkboxes over these flags); only the storage behind it is wrong.

### Persisted-derived-data

- **Active answer and question:** `activeAnswer`, and `activeQuestion` on sentences, are just "user value, else
  source value". They exist to fill Anki's display fields.
- **Sentence count:** `sentenceCount` on vocab. The vocab editor even lets you edit it.
- **Generated tags:** `LocalNoteUpdater.TagNoteMetadata` writes about 25 computed tags, such as
  `-::kanji::in-any-vocab-form` and `-::vocab::matching::uses::prefix-is-not`.
- **Study state:** some generated tags are one user's Anki card state saved into the corpus, for example
  `-::kanji::studying-vocab` and `-::vocab::has-no-studying-sentences` (17,382 occurrences). The card status
  itself (`JPNote._cardStatus`) is runtime-only, which is right. The tags derived from it are not.
- **Orphaned tags:** nothing produces `-::kanji::not-in-any-vocab-form` (148) or
  `-::kanji::not-in-any-vocab-main-form` (162) any more.
- **`parsingResult`:** derived, but it's an expensive analysis cache that carries vocab ids. The tentative plan
  is to keep it, named as a cache.

### Overridable-text without a type

`sourceAnswer`/`userAnswer`/`activeAnswer`, `sourceQuestion`/`userQuestion`/`activeQuestion`,
`sourceComments`/`userComments`, and the kanji meaning mnemonic (`sourceMeaningMnemonic`/`userMnemonic`, with
`KanjiNote.ActiveMnemonic` falling back) are all one concept. In code it's sibling strings plus
`FallbackStringField`.

### A domain model shaped like Anki fields

- `KanjiNote` joins the JSON lists `radicals`, `primaryVocab`, `similarMeaning` and `confusedWith` back into
  comma strings in memory, then re-parses them on every read.
- `VocabNotePartsOfSpeech` holds one raw string.
- `RelatedVocabDataSerializer`, `VocabNoteMatchingRulesSerializer`, `ISentenceConfigurationSerializer` and
  `ParsingResultSerializer` exist only to produce Anki field strings and feed editor text boxes.
- There are two tables of Anki field names: `AnkiFieldNames` in `JAStudio.Anki`, and
  `NoteFieldsConstants` / `SentenceNoteFields` / `MyNoteFields` in `JAStudio.Core.Note`.
- `AnkiNoteData` is declared in `Note/JPNoteData.cs`, where the file name and the type name disagree.

### Editors

`VocabEditorViewModel`, `KanjiEditorViewModel` and `SentenceEditorViewModel` copy Anki's note editor: one text
box per stored string.

- Brackets and `question:disambiguation` are typed by hand.
- Lists are comma-split, and `SplitCommaSeparated` is duplicated in two view models.
- Parts of speech is free text, and audio is raw `[sound:…]`.
- Related vocab is a raw JSON text box (`VocabEditorDialog.axaml`).
- `sentenceCount` is editable.
- Kanji readings are edited as HTML.
- The "read-only reference fields" are editable and get saved back.

## Target model principles (tentative)

1. **The JSON mirrors the domain model.** Every concept with meaning has its own JSON structure. No
   in-string-micro-syntax for anything the code interprets. Rich text that is only displayed stays text.
2. **Store facts, not conclusions.** No persisted-derived-data, except caches that are expensive to recompute
   and named as caches (`parsingResult`).
3. **Tags hold only labels the domain does not interpret.** Anything the code reads or reasons about is a
   typed field.
4. **Nothing in the corpus is there for Anki.** The Anki-projection derives what Anki needs, in `JAStudio.Anki`.
5. **The corpus is shared language data; per-user study state stays out.** Study state belongs with the
   scheduler (Anki) or in a per-user store.
6. **Editors edit the model, not the storage.** Each concept gets a control that fits it: a list with an
   "owned" checkbox, a vocab picker, a reading list with a primary toggle. Derived values are shown read-only.

## Target shapes (sketches, not specifications)

The key names are placeholders that follow the current camelCase serializer. Each domain type's final name
should drive its JSON key, not the reverse.

### Vocab

```json
{
  "id": "00b4a087-8057-41b4-9172-c17752e65455",
  "question": { "text": "剥ぐ", "disambiguation": null },
  "answer": { "source": "to tear off; to peel off", "user": "" },
  "readings": ["はぐ"],
  "forms": [
    { "text": "剥ぐ", "owned": false },
    { "text": "剝ぐ", "owned": true }
  ],
  "compoundParts": { "primary": [], "alternative": [] },
  "partsOfSpeech": ["godan verb", "transitive verb"],
  "register": [],
  "explanation": { "short": "", "long": "" },
  "mnemonic": { "source": "", "user": "" },
  "readingMnemonic": "",
  "matching": {
    "requiresOrForbids": { "godan": "requires", "ichidan": "forbids" },
    "flags": ["isInflectingWord"],
    "rules": { "prefixIsNot": [], "suffixIsNot": [], "surfaceIsNot": [], "yieldToSurface": [], "requiredPrefix": [] }
  },
  "relatedVocab": {
    "ergativeTwin": "剥がれる",
    "derivedFrom": null,
    "perfectSynonyms": [], "synonyms": ["剥ぐ"], "antonyms": [], "confusedWith": [], "seeAlso": []
  },
  "technicalNotes": "",
  "references": "",
  "tags": []
}
```

What disappears from vocab JSON:

- `activeAnswer` and `sentenceCount` (persisted-derived-data);
- `audioB`, `audioG`, `audioTTS`, `image`, `userImage` (media; see [Media](#media-open));
- every configuration tag, which moves into `matching`, `register` and `source`.

Parts of speech are harmonized to one spelling per part during migration.

**Open:** I haven't verified that `sourceMnemonic`/`userMnemonic` on vocab follow the same fallback as the
kanji mnemonic. If they do, they're overridable-text. If both are shown side by side, they're two separate
fields.

### Kanji

```json
{
  "id": "…",
  "kanji": "突",
  "answer": { "source": "Stab, Thrust, Sudden", "user": "sudden/abrupt | stab/thrust/pierce/prick" },
  "readings": {
    "on":  [{ "kana": "とつ", "primary": true }],
    "kun": [{ "kana": "つ",   "primary": true }],
    "nan": []
  },
  "radicals": ["穴", "大"],
  "meaningMnemonic": { "source": "…", "user": "" },
  "meaningInfo": "…",
  "readingMnemonic": "…",
  "readingInfo": "…",
  "primaryVocab": [],
  "similarMeaning": [],
  "confusedWith": ["空"],
  "references": "",
  "tags": []
}
```

**Open:** `<primary>` also marks the primary meaning in the source answer. If anything other than rendering
uses that marking, source meanings could become a list with a primary flag, like readings.

### Sentence

```json
{
  "id": "…",
  "question": { "source": "…", "user": "" },
  "answer": { "source": "…", "user": "" },
  "comments": { "source": "", "user": "" },
  "reading": "…",
  "externalId": "…",
  "configuration": {
    "highlightedWords": ["強制"],
    "incorrectMatches": [{ "word": "思える", "index": 11 }],
    "hiddenMatches": []
  },
  "parsingResultCache": { "sentence": "…", "parserVersion": "…", "parsedWords": [] },
  "tags": []
}
```

The sentence shape is already close. What changes:

- `activeQuestion` and `activeAnswer` go (persisted-derived-data);
- `audio` and `screenshot` go (media);
- `<wbr>` word-break hints move out of the question text, or are generated at render time (**Open:** where do
  they come from?);
- the parsing result is named as a cache.

## References between entities (Open)

Magnus is undecided, and both sides have real costs. Three options:

**A. note-id-reference.** Store the referenced note's id.

- Exact, and it survives renames.
- A delete leaves a dangling id that carries no information: you can't even display what it pointed to. That
  needs either cascading removal on delete or tombstones.
- It can't refer to a word that has no vocab yet.
- Merging duplicate vocab must rewrite every incoming reference.

**B. word-reference.** Store the word: the question text, plus a disambiguation when needed, as a typed value
instead of today's bare string.

- A delete is harmless. The word still exists, the reference simply resolves to nothing, and it resolves again
  if the vocab is re-created.
- It can name words that aren't in the corpus yet.
- It's readable.
- It breaks silently when a vocab's question is renamed.
- It's ambiguous when several vocab share a question and no disambiguation is given.

**C. word-reference plus rename propagation.** Option B, with renaming a vocab's question (or adding a
disambiguation) as a domain operation that rewrites every incoming word-reference.

- `NoteCache` holds the whole corpus in memory, so finding incoming references is cheap.
- It keeps B's harmless deletes and fixes B's rename weakness.
- Cost: a rename becomes a write to several notes, and shared questions still need disambiguation discipline.

**Tentative leaning: C.** In my first report I leaned towards ids. I've changed my mind, for two reasons:

- Magnus's point about deletes.
- A second look at what these relationships *are*. "Synonym", "antonym", "ergative twin" and "confused with"
  are relationships between **words**. A word exists whether or not the corpus has a vocab note for it, so the
  truthful reference identifies the word, and finding the note is a lookup.

That reasoning gives one rule for the other references too:

- Kanji references (`radicals`, `similarMeaning`, `confusedWith`) use the kanji character itself, which never
  changes. That's a natural key, so text is simply right there.
- Kanji `primaryVocab` refers to words, so it gets the same treatment as vocab relationships.
- Forms and compound parts are spellings and words, not links to notes, so they stay text.
- Sentence `configuration` entries are words at positions, so they stay text.
- `parsingResult` keeps vocab ids, because it's a cache: a re-parse repairs any dangling id.

## Tags

After the configuration moves out, `tags` holds only labels the domain doesn't interpret.

- **Difficulty** (`-::sentence::difficulty::N`, 48,552 occurrences): nothing in the code reads it, so it stays a
  plain user label. **Open:** should it become a typed field if something will use it?
- **Generated tags and study-state tags:** removed from the corpus. If any of them matter for searching in the
  Anki browser, the Anki-projection can still write them to Anki only.
- **Orphaned tags:** deleted by the migration.
- **Source:** tentatively a typed `source` field holding a `SourceTag`, because the code reads it. Moving source
  information out of the core corpus entirely (`media-storage-and-import.md`, "Source-Revealing Tags") remains
  a separate, deferred decision.
- **Priority:** the corpus data says priority belongs to a source, not to a note:
  - 48,545 notes carry a priority tag, and every one of them also has a source tag.
  - There are 17 distinct priority tags spread over 143 source tags.
  - Only 4 source tags carry more than one priority tag: the JLab inter/booster overlap, and 30 WaniKani notes
    tagged `130_tts_removed_on_wany`.
  - The name part of the tag is just a label, and is sometimes wrong: `source::anime::girls-last-tour` notes
    carry `-::priority::high::011_Natsume_01`.

  **Tentative proposal:** a priority table keyed by `SourceTag` prefix, resolved by longest prefix (the way
  `MediaImportRuleSet` resolves import rules), instead of tens of thousands of per-note tags. Changing the
  priority of a season becomes one edit. The alternative is a typed per-note `priority` number. **Open.**

## Media (Open)

The direction is already set in `media-storage-and-import.md`: the core corpus stores no media data, and all
media association lives in the media layer's sidecars, which already record `noteIds`. What's missing is
removing `audioB`/`audioG`/`audioTTS`/`image`/`userImage` (vocab), `audio`/`screenshot` (sentences) and the
kanji media fields from the corpus data types.

Magnus doesn't remember the "unresolved design concerns". These are my **guesses** at what they might have
been. They are not reconstructions:

1. **Field roles.** The corpus field says which slot a file fills (`audioB` versus `audioG` versus `audioTTS`,
   `image` versus `userImage`). The sidecars don't record that role. `VocabMediaField` exists only in the
   import rules. Dropping the fields loses the role, unless the sidecar or a preference layer records it.
2. **The Anki-projection still has to fill Anki's audio fields** so listening cards can auto-play. It needs a
   rule for which attachment goes into which Anki field, and the file has to exist in Anki's media folder.
3. **The audio-update ingestion path changes.** Today new audio arrives as a changed Anki field string, lands in
   the corpus, and is later picked up by the media import scan (`MediaImportAnalyzer`). Without corpus fields,
   the Anki-ingestion has to store the file into the media layer directly (`MediaStorageService`).

Tentatively, media is the last vertical-slice, after these points are settled.

## Migration approach (tentative)

- **One-time migrations, no permanent dual-format readers, no format version field.** This is justified by
  Magnus being the only consumer of `jas_database`.
- Each vertical-slice ships as:
  - a JAStudio commit that changes the domain type, the corpus data type, the editor, and the Anki-projection
    (which keeps producing the old Anki strings);
  - a migration run once over the corpus, whose code is deleted in the same or the next commit;
  - a `jas_database` commit with the migrated files.

  The JAStudio commit moves the `src/jas_database` submodule pointer to the migrated data, so every JAStudio
  commit pairs with corpus data it can read.
- **The on-disk MemoryPack snapshot** (`JsonFilesystemObjectRepository.SaveSnapshot`) can't be trusted across a
  format change, so each migration deletes it. I haven't checked whether anything already detects a stale
  snapshot.
- **Anki searches keep working throughout** (`JAStudio.Anki.QueryBuilder` searches `F`, `Reading`, `A`,
  `Reading_On`/`Reading_Kun`, `__parsing_result`), because the Anki-projection keeps writing those fields from
  the new model.

## Order of work (tentative)

0. **This document:** settle the open questions that block step 1 and the first vertical-slice.
1. **Move the Anki-projection and the Anki-ingestion into `JAStudio.Anki`, with no format change.**
   - The corpus data types lose `ToNoteData`, `PopulateAnkiNoteFields`, `FromAnki` and `MergeAnkiData`.
   - `AnkiNoteData` and `NoteFieldsConstants` leave `JAStudio.Core.Note`, and the two field-name tables become
     one.
   - Tag and question ingestion stops here, or at a later step, depending on open questions 1 and 2.

   This follows the re-home-concepts-before-restructuring code standard. After it, the JSON can change freely
   without touching Anki's format.
2. **Vertical-slices, one concept each:**
   1. Brackets: owned forms and alternative compound parts. This one has caused real bugs.
   2. Question and disambiguation, dropping the sentinel strings.
   3. Overridable-text as a type, dropping `activeAnswer`/`activeQuestion`.
   4. Configuration out of tags: requires/forbids flags, boolean flags, register, `_uk`/`_disable_uk`, source.
   5. Priority (per-source table or per-note field).
   6. Persisted-derived-data and study state out: `sentenceCount`, generated tags, orphaned tags.
   7. Kanji readings and parts of speech.
   8. References between entities (option A, B or C), plus vocab pickers to replace the related-vocab JSON text
      box.
   9. Media, once the points under [Media](#media-open) are settled.

## Open questions

1. **Anki's note editor:** edits made there would be overwritten by the one-way Anki-projection. Is that
   acceptable, should the JAStudio editor open instead, or should something else happen?
2. **Bulk tagging:** with tags no longer ingested from Anki, where do whole-deck or whole-season labels get
   applied? Does that wait for a JAStudio-side feature?
3. **Images:** does the Anki-ingestion take image updates as well as audio?
4. **References:** option A, B or C?
5. **Priority:** a per-source table or a per-note field?
6. **Difficulty:** a plain label, or will something use it?
7. **Source:** a typed field now, or wait for the decision to move source information out of the corpus?
8. **Anki fields to keep:** which fields and tags must the Anki-projection keep writing? This depends on the
   card templates and on which Anki browser searches Magnus still uses.
9. **Media:** what do the media fields need to keep doing that the sidecars don't yet do? See the guesses under
   [Media](#media-open).
10. **Smaller details:** vocab mnemonic fallback, kanji primary meanings, the origin of `<wbr>`, and the final
    names for the new types and JSON keys.

## Out of scope

- **Renaming "note"** (`JPNote`, `VocabNote`, `KanjiNote`, `SentenceNote`, `NoteCache`, …) now that JAStudio
  owns the data. It's agreed as probably right, but it's a separate effort.
- **Moving source information out of the core corpus.** This is deferred in `media-storage-and-import.md`.
