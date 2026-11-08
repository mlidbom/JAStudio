# Corpus Data Model — Tentative Draft

> **Status: tentative draft (2026-11-05, updated 2026-11-06). Only what is listed under "Settled so far" is
> decided.** It was written from a survey of the code and the corpus, plus Magnus's answers to questions. It is
> meant to be argued with. Sections marked **Open** have no proposal yet, or only a leaning.

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
  example, brackets around a form (gone 2026-11-08), `question:disambiguation` (gone from the question 2026-11-08),
  `<primary>` HTML around a kanji reading, or `[sound:file.mp3]`.
- **persisted-derived-data** — a value computed from other data that is stored anyway, for example
  `activeAnswer` or `sentenceCount`.
- **Anki-projection** — the mapping from the domain model to Anki note fields and tags. It runs on every flush.
- **Anki-ingestion** — the mapping from an Anki note into the domain model. It runs when a note is added in
  Anki and when an Anki note is saved.
- **source data** — what a note's source (WaniKani, a dictionary, Immersion Kit, a JLab deck) gave it when it was
  imported: `VocabNote.SourceData`, `KanjiNote.SourceData`, `SentenceNote.SourceData`. *(Named 2026-11-08: WaniKani is a
  source; `{ 行く, go }` is source data.)*
- **corpus answer**, **corpus question** — the corpus's own answer or question for a note: the canonical value, not
  student data. While a note has none, its source data's is shown. (`CorpusAnswer`, `CorpusQuestion`.) The corpus's
  other own texts, such as comments, explanations and mnemonics, have no prefix. *Until 2026-11-08 these were "user"
  fields, a misnomer: they were never the student's.*
- ~~**overridable-text**~~ — *replaced 2026-11-08 by source data and the corpus's own fields: how the two combine is
  decided per field. The answer and the sentence question show the corpus's, else the source's; comments show both,
  side by side; the source's mnemonics and hints are never shown.*
- **word-reference** and **note-id-reference** — the two ways one entity can refer to another. See
  [References between entities](#references-between-entities-word-references).
- **vertical-slice** — one concept changed end to end in a single increment: the domain value type, the JSON
  shape, a one-time corpus migration, and the editor control.
- **study-note** — the trimmed Anki note. It exists so that Anki can schedule cards, HyperTTS can generate
  audio (vocab and sentences), and cards can be reviewed on mobile, where the add-on doesn't run. See
  [The minimal Anki note type](#the-minimal-anki-note-type-tentative).
- **mobile-review** — reviewing cards in an Anki mobile app. The JAStudio add-on doesn't run there, so the card
  is exactly what the installed card template renders from the note's fields.
- **raw note** — an Anki note an import takes, in a note type of its own, such as "Immersion Kit Sentence" or a
  subs2srs deck's note type. It arrives through a deck import or AnkiConnect. JAStudio ingests it into the corpus
  through an import profile (`import-profiles.md`, the word's home), tags included, and the raw note is then deleted
  or tagged as imported. Deck imports happen in a separate Anki profile (2026-11-06; see
  [Import and organization](#import-and-organization-needs-its-own-design)).
- **generated-html-field** — the one Anki field a study-note keeps for mobile-review, besides the question and
  the audio. JAStudio fills it with HTML generated from the note, and nothing reads it back. See
  [The minimal Anki note type](#the-minimal-anki-note-type-tentative).
- **student-data** — data about one student's studying rather than about the language: the studying status,
  priority, and the student-tags. It doesn't belong in the corpus.
- **student-tag** and **corpus-tag** — a tag that is student-data, such as a priority tag, and a tag that
  describes the language data itself, such as its source. See
  [Student-tags and corpus-tags](#student-tags-and-corpus-tags).

## Settled so far (Magnus, 2026-11-05)

| Question | Answer |
|---|---|
| Anki's role from now on | The review scheduler, a way to import notes, and a way to update audio. |
| Consumers of `jas_database` | Only Magnus, for the foreseeable future. One-time migrations are enough. |
| Origin of `-::priority::…` and `-::sentence::difficulty::…` tags | Priority: tagged by hand in Anki, usually a whole deck or season at a time. Difficulty, *corrected 2026-11-06*: written by JAStudio code that sorted sentences into decks by difficulty, which was deleted the same day (2024-10-30). See [Tags](#tags). |
| Renaming "note" (`JPNote`, `VocabNote`, …) now that JAStudio owns the data | Probably yes, but as a separate effort. Out of scope here. |
| The "unresolved design concerns" about removing media fields (in `media-storage-and-import.md`) | Not remembered. See [Media](#media-open). |
| Ids or text for references between entities | **2026-11-06:** for now, the word itself (word-reference), not ids, because of the problems ids cause when notes are deleted. Whether renames propagate is still open. See [References between entities](#references-between-entities-word-references). |
| Anki fields | Direction: remove every Anki field except those Anki needs to work, **plus what mobile-review needs**, which since 2026-11-06 is one generated-html-field (rows below). See [The minimal Anki note type](#the-minimal-anki-note-type-tentative). |
| Mobile-review (2026-11-06) | Basic review must work on mobile, for kanji, vocab and sentences, by reading or by listening. The card templates are designed to render well on mobile, wherever the add-on doesn't replace them. The mobile app is AnkiDroid. *Later the same day:* a single generated-html-field replaces the plan to keep fields such as `A` (next rows). |
| Work incrementally, not up front (2026-11-06) | Focus on eliminating the need to use Anki at all, one capability at a time. That makes it clear what has to stay in Anki and what doesn't. Questions about deleting or trimming fields, and the template work, are deferred until they are actually in the way. The existing installed templates stay for now. |
| Sync direction (2026-11-06) | No two-way sync except where it can't be avoided. |
| Tagging, organizing, batch import | Need their own design. Partly decided 2026-11-06 (rows below). See [Import and organization](#import-and-organization-needs-its-own-design). |
| Navigation and editing | A large part of this work is building proper navigation and editing into JAStudio itself, so Anki isn't needed for either. |
| Overall approach | Build out JAStudio's own UI until Anki is only used for TTS, reviewing, and importing sentences and Yomitan-created vocab. The model changes come after that, when they no longer have to keep the Anki format working. *2026-11-08: TTS no longer needs Anki: JAStudio generates it itself (`text-to-speech.md`). Yomitan no longer creates vocab: Magnus creates it in JAStudio (`import-profiles.md`, "Decided since").* |
| Study approach | Magnus studies sentences, but studying vocab and kanji stays fully supported. That includes mnemonics, studying status and everything else around them. Only the Anki notes become minimal (study-notes). The one removal is the complex kanji audio generation (primary-readings TTS): it's in the way of minimal notes and doesn't help much. Anything else is removed only if it is actually in the way of the refactoring. |
| Edits in Anki (2026-11-06) | Magnus doesn't edit questions or answers in Anki, on desktop or on AnkiDroid. JAStudio ignores edits made in Anki to the fields it writes, and its next write overwrites them. |
| Mobile-review content (2026-11-06) | Magnus's proposal: one generated-html-field instead of a set of fields. The back templates are the committed minimal ones (`JAStudio.Web/Styles/notes/`) plus that one field. See [The minimal Anki note type](#the-minimal-anki-note-type-tentative). |
| Corpus and Anki (2026-11-06) | Eventually the corpus holds far more sentences than are studied, and only what is studied has Anki cards. **For now** every note stays in Anki, and whether a note is in Anki and whether it's studied stay separate questions. Studied means unsuspended, using the existing suspend and unsuspend actions. Notes that aren't in Anki are dealt with later. |
| Import (2026-11-06) | Through Anki notes, not by JAStudio reading deck files: Anki imports the decks, and its batch editing gets the notes into shape first. Imports happen in a separate Anki profile, which keeps the decks' media out of the synced profile; that needs the corpus properly separated from the Anki collection. An import takes all the notes' tags along. |
| Organizing (2026-11-06) | Navigating and editing hierarchical tags in JAStudio is central. They can do most of what folders, tags and decks do in other programs. After import, JAStudio manages the corpus-tags. |
| Student-data (2026-11-06) | Doesn't belong in the corpus. That covers priority and every other tag about one student rather than the corpus. Priority most likely stays, read from Anki the way the studying status is. Student-tags might still be managed in JAStudio, but stored only in the Anki notes' tags. See [Tags](#tags). |
| Difficulty tags (2026-11-06) | Deleted, from the corpus and from Anki. Nothing uses them. |
| The workspace (2026-11-06) | Its remaining steps (multiple workspaces, reverting a note) are parked. They free nothing from Anki. |

## Anki's role and its consequences

Until 2026-11-07 Anki was a full two-way partner:

- **Outbound:** every domain flush writes all of a note's old Anki fields and all of its tags to the Anki note.
  The path is `NoteFlushGuard.Flush` → `NoteCache` update listeners → Python `_update_anki_note` →
  `JPNoteDataShim.sync_note_to_anki_note`, which writes `JPNote.GetData()`
  (= `ToDto().ToNoteData()`).
- **Inbound:** every save on the Anki side ran `MergeAnkiData` on the corpus data types. For vocab, that pulled
  `Question`, `ActiveAnswer`, the audio fields, `Image`, `UserImage` and **all tags** back into the corpus.
  Kanji and sentences pulled similar sets. *Since 2026-11-08 a save in Anki takes nothing: see point 2.*

With Anki reduced to scheduler, note import and audio updates, the tentative consequences are:

1. **The Anki-projection becomes one-way, and the Anki note types shrink to the fields Anki needs plus the
   generated-html-field.** This is Magnus's direction. Most fields disappear, so they can't be edited in Anki.
   Nothing reads the generated-html-field back, and the next Anki-projection of a note overwrites any edit made
   to it in Anki. *(Settled 2026-11-06: Magnus doesn't edit in Anki.)*
2. **The Anki-ingestion has one job, importing new notes.** *It had two until 2026-11-08.*
   - **New note import:** build a domain note from a new Anki note, tags included. *Since 2026-11-08* import
     profiles do this, from notes of note types of their own (`import-profiles.md`). Notes of JAStudio's own types
     added in Anki aren't taken in any more.
   - ~~**Audio update:**~~ *Gone 2026-11-08.* It took in the audio HyperTTS generated into an Anki note. JAStudio
     generates its own TTS audio now (`text-to-speech.md`), HyperTTS isn't used any more, and nothing else writes
     audio in Anki.

   Everything else Anki sends back is ignored: question, answer and corpus-tags. Student-tags, such as priority,
   are read from the Anki notes instead of being merged into the corpus (see [Tags](#tags)).

   **Done (2026-11-08): a note saved in Anki changes nothing in the corpus.** On 2026-11-07 the take-in was cut to
   the audio HyperTTS writes, and tags went with it, since JAStudio now edits them (`tags.md`). On 2026-11-08 the
   audio went too.
   - **Images** (Magnus, 2026-11-07) only come with an import: there's no point in editing them in Anki. JAStudio has
     no image editor, so a note's images are fixed after import.
   - **The code:** JAStudio doesn't listen to Anki's `note_will_flush` hook any more. *Since 2026-11-08* it doesn't
     take in notes of its own types added in Anki either: `NoteCache.ExternalNoteAdded` is gone, and the corpus-validity
     checks report such a note (`corpus-validity.md`).
   - **A note deleted in Anki stays in the corpus** (Magnus, 2026-11-08). JAStudio forgets its Anki note and cards
     (`NoteCache.ExternalNoteRemoved`), so it's no longer studied in that Anki profile, and nothing recreates the Anki
     note. Anki holds the notes being studied; the corpus holds them all. Deleting a note for good is JAStudio's own
     delete (`JPCollection.Delete`). The media is part of the note (Magnus, 2026-11-08), so that delete also deletes its
     media files from the media layer, unless another note uses the same file. Their copies in Anki's media folder are
     left to Anki's "Check Media". *Until 2026-11-08 a note deleted in Anki left JAStudio's in-memory collection,
     though not the corpus files, and came back, without an Anki note, at the next reload.*
   - **What an edit made in Anki does now:** nothing in the corpus, and JAStudio's next write of the note undoes
     it in Anki.
   - The "Force flush all Anki notes by ID" batch, which made Anki save every note for the take-in, is gone.
3. **Tagging a whole deck in Anki doesn't reach the corpus, and already doesn't today** (found 2026-11-06).
   - Anki's tag operations go straight to Anki's backend and skip `note_will_flush`, the hook the
     Anki-ingestion listened to until 2026-11-08. That covers adding or removing tags in the browser, renaming or deleting a tag in
     the sidebar, and find and replace on tags. Only a note saved through Anki's Python API reached the corpus,
     and then its whole tag list did: the note editor, HyperTTS, and the "Force flush all Anki notes by ID"
     batch save notes that way. *Since 2026-11-07 no tags are taken from a save in Anki (point 2).*
   - Worse, the Anki-projection replaces the Anki note's whole tag list (`JPNoteDataShim.sync_note_to_anki_note`),
     so the next time JAStudio writes such a note it undoes the tag change made in Anki.
   - That is how priority tags were applied. Organizing gets its own design (see
     [Import and organization](#import-and-organization-needs-its-own-design)), centered on hierarchical tags in
     JAStudio.
4. **Both mappings move to `JAStudio.Anki`.** Today they live inside the corpus data types (`ToNoteData`,
   `PopulateAnkiNoteFields`, `FromAnki` in `VocabDto`, `KanjiDto`, `SentenceDto`), and
   `AnkiNoteData` and `NoteFieldsConstants` sit in `JAStudio.Core.Note`. That breaks the anti-corruption-layers
   code standard (foreign formats only at the boundary) and Core's own rule of having no Anki dependency.

### The minimal Anki note type (tentative)

Each note type keeps the fields Anki itself needs, plus the generated-html-field for mobile-review.

**Decided for the trim (Magnus, 2026-11-08).** Mobile-review is deferred, so the generated-html-field isn't added yet,
and HyperTTS isn't used any more, so its reasons below no longer apply. The study-notes keep:

| Note type | Fields |
|---|---|
| Vocab | `Q`, `Audio_b`, `Audio_g`, `Audio_TTS`, `jas_note_id` |
| Kanji | `Q`, `jas_note_id` |
| Sentence | `Q`, moved to be the first field, `Audio Sentence`, `jas_note_id` |

JAStudio writes only these since 2026-11-08 (`AnkiFieldNames`, `CorpusNoteDto.ToNoteData`), and reads only
`jas_note_id`. The installed templates already reference no other field, except the kanji answer template's
`{{_primary_readings_tts_audio}}` and the sentences' browser column, `{{source_question}}`, which become empty and
`{{Q}}`. Magnus trims the note types in Anki by hand, with syncing to AnkiWeb turned off until he has tested the result.

**Fields Anki itself needs:**

| Field | Why Anki needs it |
|---|---|
| `jas_note_id` | Links the Anki note to its corpus entry (`AnkiNoteIdMapLoader.Load`). |
| The question (`Q`; for kanji, the kanji character) | Reasons:<br>• Vocab and sentence HyperTTS presets read it.<br>• The reading card's front template is `{{Q}}`, and Anki generates a card only when its front side renders non-empty.<br>• I believe Anki also rejects a front template that references no field at all, but I haven't verified it.<br>• The first field is Anki's sort field in its browser. |
| Audio (vocab and sentences only) | `Audio_b`, `Audio_g`, `Audio_TTS` for vocab; `Audio Sentence` for sentences. Reasons:<br>• The listening card's front template is the audio fields, so a listening card exists only when the note has audio.<br>• Anki's own playback (auto-play and replay) comes from these fields (`web-rendering-architecture.md`, "Key Discovery: Anki Audio Behavior"). That is also the only audio playback on mobile.<br>• HyperTTS writes new audio into them. |

**The generated-html-field, for mobile-review (Magnus's proposal, 2026-11-06):**

On mobile the card is exactly what the template renders from the note's fields. Instead of keeping the fields
mobile-review would read (`A`, `Reading`, `Reading_On` and so on, as an earlier draft proposed), each study-note
keeps one field that JAStudio fills with HTML generated from the note. The back templates become the committed
minimal ones in `JAStudio.Web/Styles/notes/` plus that field. What the HTML shows is open (question 1).

Points to get right when it's built:
- **A vocab's reading and listening cards share the field.** Anki gives each card a class for its card type
  (`card1`, `card2`), so CSS can show or hide parts per card type. I believe AnkiDroid does the same; to verify
  on Magnus's phone.
- **The CSS goes in the note type's styling, not in each note's HTML,** so a visual change touches no notes. A
  change to the HTML itself means regenerating the field on every study-note, and the next sync uploads all of
  them.
- **To start with, the HTML holds only the note's own data.** If it showed other notes' data, such as a vocab's
  kanji meanings, editing those notes would leave it stale.
- **Images in the HTML still reach the phone.** Anki finds media through `<img src>` in any field.
- **No sentence reading.** A reading that guesses is worse than useless (Magnus, 2026-11-05).
- **On desktop it renders invisibly** under the add-on's iframe, as the old templates' bodies do today.

No tags are needed in Anki for Anki to work. Suspension and scheduling live on Anki's cards, not in fields.
Student-tags, such as priority, are stored in the Anki notes' tags (see [Tags](#tags)). Whether Magnus's own Anki
searches or filtered decks need any other tags is a separate question (see [Open questions](#open-questions)).

**What HyperTTS reads and writes** (from its preset configuration in the `Magnus` profile, 2026-11-05):

- **Vocab:** every preset reads `Q` and writes `Audio_TTS` or `Audio_g`.
- **Sentences:** every preset reads `Q` and writes `Audio Sentence`.
- **Kanji:** the presets use HyperTTS's advanced Python template. It reads `Reading_On`, `Reading_Kun` and
  `__reading_Nan`, parses the `<primary>` in-string-micro-syntax itself, and writes `_primary_readings_tts_audio`.
  **Decided (Magnus, 2026-11-05): this goes.** The kanji presets, the `_primary_readings_tts_audio` field, and
  any JAStudio code that exists for it are removed.

HyperTTS's automatic rules are keyed by note type id and deck id. Trimming the existing note types in place
keeps their ids, so the rules keep matching. I haven't verified whether automatic rules fire for notes created
by code rather than through Anki's Add dialog.

An earlier draft floated letting the JAStudio page play card audio, so that the audio fields could shrink to a
single marker field. Mobile-review rules that out: on mobile, Anki's playback from the audio fields is the only
playback there is.

### Anki dependencies to replace before their fields can go

| Done in Anki today | What it depends on | Replacement in JAStudio |
|---|---|---|
| ~~Searches that `JAStudio.Anki.QueryBuilder` builds for Anki's browser~~ | ~~Fields `F`, `Reading`, `A`, `Reading_On`, `Reading_Kun`, `__parsing_result`~~ | *Done 2026-11-06:* the note menus' "Open" items list their notes in the workspace's navigation pane, the old text searches into Anki are deleted, and what still opens Anki's browser passes it a `nid:` search of the Anki note ids. |
| The card templates installed in the Anki collection | Many fields, in the back templates | Install the minimal templates plus the generated-html-field, then remove the other fields. See [What the card templates actually use](#what-the-card-templates-actually-use). |
| Browsing and editing notes | All fields | *Done 2026-11-06:* JAStudio's workspace, navigation pane and editors. |
| Tagging whole decks and seasons | Tags | Navigating and editing hierarchical tags in JAStudio (own design) |
| ~~Yomitan adding vocab~~ | ~~The fields Yomitan fills in~~ | *Gone 2026-11-08:* Yomitan no longer adds notes. Vocab is created in JAStudio. |
| Searching on generated tags | `-::…` tags | JAStudio search |

### What the card templates actually use

Checked against the collections in the `Magnus` and `Anki-Development` profiles, 2026-11-05.

**The rendering is all Blazor.**

- Vocab, kanji and sentence cards all render through `AppendingPrerenderer`, sentences included:
  `SentenceNoteRenderer` builds the Blazor iframe, and `SentenceCard.razor` renders it.
- The "Sentence Next" status in `web-rendering-architecture.md` is out of date.
- **Question side:** Anki's rendered HTML is thrown away and replaced by the iframe.
- **Answer side:** the iframe (fixed, full-window) is appended on top of Anki's rendered HTML.

**What that leaves the installed templates doing:**

- deciding which cards exist (`{{Q}}` on reading fronts, the audio fields on listening fronts);
- queuing audio for Anki to play;
- rendering an invisible body under the answer-side iframe.

**The installed templates are not the stubs in the repo.**

- `JAStudio.Web/Styles/notes/` has the stubs (`{{Q}}` and audio only). Both profiles still have the old full
  templates, and their back templates reference about 10 to 15 fields each, plus `{{Tags}}`.
- The old full templates look like the ones in `src/web/notes/`.
- Nothing in the code reads either folder.

**Consequences:**

- **The invisible body still costs something.** It loads images such as `{{Screenshot}}` and runs its
  `<script>`. I haven't checked what that script does there.
- **The stubs alone aren't enough any more.** They render only `{{Q}}` and audio, so a mobile card would show no
  answer. The stubs plus the generated-html-field are (2026-11-06). On desktop the add-on still replaces them
  (the question side) or covers them (the answer side).
- **Install the new templates before removing fields.** I don't know how Anki handles a template that
  references a field which has been removed, so the safe order is templates first, then removing fields.

### Removing fields from Anki note types

Two cautions:

- **A full sync.** Removing a field from an Anki note type changes the collection schema. As far as I know,
  Anki then requires a one-way full sync to AnkiWeb (to be verified). The removals should be batched into as
  few schema changes as possible.
- **Check for data first.** Before a field is removed, check that no Anki note holds a value in it that the
  corpus doesn't have.

## Current state

### In-string-micro-syntaxes

| Concept | Where | Encoding |
|---|---|---|
| ~~Owned form~~ | `VocabNoteForms.ComputeOwnedForms` | ~~`[form]` in the `forms` list~~ *Gone 2026-11-08 (slice 1): each form is `{ "text", "isOwned" }`.* |
| ~~Alternative compound part~~ | `VocabNoteCompoundParts` | ~~`[part]` in `userCompounds`~~ *Gone 2026-11-08 (slice 1): `compoundParts` is `{ "primary", "alternative" }`.* |
| Disambiguation | `VocabNoteQuestion` (`DisambiguationMarker`) | ~~`question:disambiguation` (32 vocab)~~ *Gone from the question 2026-11-08 (slice 2): it's `{ "text", "disambiguation" }`, 34 vocab with one. References to a vocab still name it `text:disambiguation` (`VocabNoteQuestion.DisambiguationName`), 115 of them in 111 vocab, until slice 8.* |
| ~~Invalid / empty question~~ | `VocabNoteQuestion` | ~~Sentinel strings `InvalidQuestionMessage` and `"[EMPTY]"` stored as the question~~ *Gone 2026-11-08 (slice 2). None was stored in the corpus: the code made them up while it ran. A question can't be set empty, or to text containing `:`, now.* |
| Prefix / suffix marker | `Mine.VocabPrefixSuffixMarker` | `〜` in the question and forms, stripped as a "noise character". This is also standard dictionary notation, so it may rightly stay text. |
| Parts of speech | `VocabNotePartsOfSpeech`, `POSSetManager.InternAndHarmonize` | One comma-separated string, with duplicate spellings on disk (`"noun, suru verb"` 1,436 times, `"noun, する verb"` 774 times) |
| Primary kanji readings | `KanjiNote.PrimaryReadingPattern`, plus a duplicate regex in `LocalNoteUpdater` | Comma-separated HTML: `<primary>ひ</primary>, ひつ` in `readingOnHtml` / `readingKunHtml` / `readingNanHtml` |
| Primary kanji meaning | kanji `sourceAnswer` | `<primary>Stab</primary>, Thrust` |
| Media references | `MediaFieldParsing`, `WritableAudioValue`, `WritableImageValue` | Anki's `[sound:x.mp3]` and `<img src="…">` |
| Word-break hints | `SentenceQuestionField` | `<wbr>` inserted into the question text by `SentenceQuestionField.SplitTokenWithWordBreakTag`, to force the tokenizer to split there. It writes the source question when there is no user question. 252 sentence files have one: 233 in `sourceQuestion`, 19 in `userQuestion`. |

The brackets caused two bugs: `a921f09cb` (alternative compound parts lost when editing a vocab) and `51a2ab4ae`
(owned forms lost when editing a vocab). About 217 vocab files used them. Two more turned up when they were removed
(2026-11-08):
- `VocabNoteKanji.ExtractAllKanji` cut `[…]` out of the forms *with its content*, so an owned form's kanji never counted
  among the vocab's kanji (`VocabCache`'s kanji-in-any-form index, and the kanji tags the full rebuild sets from it);
- `くれよ`'s alternative parts were typed as `[くれる, よ]` into the comma separated box, which split them into `[くれる`
  and `よ]`, so `よ]` was a primary part.

Two encodings look like in-string-micro-syntaxes but are probably fine as text:

- **WaniKani markup** in mnemonics (`<radical>`, `<kanji>`, `<reading>`, `<ja>`) is real rich text from the
  source.
- **Answer text** uses ` | ` between senses. This is a display convention produced by `DictEntry`, and nothing
  parses it back. **Open:** should an answer one day become a list of senses?

### Configuration stored as tags

These are settings, not labels, kept as `::`-path strings in the tag list (`Note/Tagging/Tags.cs`):

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
  turns it into CSS classes. It's student-data, not configuration of the corpus (2026-11-06; see [Tags](#tags)).

Until 2026-11-07, `MergeAnkiData` replaced the tag list with Anki's, so Anki co-owned all of this. The workspace's vocab
editors (`VocabMatchingConfigEditorViewModel` and `VocabMetadataEditorViewModel`) are already structured
(checkboxes over these flags); only the storage behind them is wrong.

*2026-11-08 (slice 4a): the requires/forbids flags, the boolean matching flags and the register are the vocab's own data
now, not tags (see [Vocab](#vocab)). What's left as tags is `_uk`, `_disable_uk`, `_tts_audio` and the source.*
*Slice 4b: `_uk` and `_disable_uk` became the vocab's `usuallyWrittenInKana`. What's left is `_tts_audio` and the source.*

### Persisted-derived-data

- **Active answer and question:** `activeAnswer`, and `activeQuestion` on sentences, are just "user value, else
  source value". They exist to fill Anki's display fields.
- **Sentence count:** `sentenceCount` on vocab. The vocab editor even lets you edit it.
- **Generated tags:** `LocalNoteUpdater.TagNoteMetadata` writes about 25 computed tags, such as
  `-::kanji::in-any-vocab-form` and `-::vocab::matching::uses::prefix-is-not`.
- **Study state:** some generated tags are one user's Anki card state saved into the corpus, that is
  student-data, for example `-::kanji::studying-vocab`. The card status itself (`JPNote._cardStatus`) is
  runtime-only, which is right. The tags derived from it are not.
- **Orphaned tags:** nothing produces `-::kanji::not-in-any-vocab-form` (148),
  `-::kanji::not-in-any-vocab-main-form` (162) or `-::vocab::has-no-studying-sentences` (17,382 vocab) any more.
- **`parsingResult`:** derived, but it's an expensive analysis cache that carries vocab ids. The tentative plan
  is to keep it, named as a cache.

### Overridable-text without a type

`sourceAnswer`/`userAnswer`/`activeAnswer`, `sourceQuestion`/`userQuestion`/`activeQuestion`,
`sourceComments`/`userComments`, and the kanji meaning mnemonic (`sourceMeaningMnemonic`/`userMnemonic`, with
`KanjiNote.ActiveMnemonic` falling back) are all one concept. In code it's sibling strings plus
`FallbackStringField`.

*2026-11-08 (slice 3), decided with Magnus:* they're not one concept but two, and "user" was a misnomer. See
[source data and the corpus answer](#terms-used-in-this-document). The source's mnemonics, and the kanji's
meaning and reading hints, are WaniKani's and never shown: the kanji card shows the corpus's own mnemonic, or else a
default one made from the kanji's readings (`KanjiNote.ShownMnemonic`), and the option that showed WaniKani's
instead is gone.

### A domain model shaped like Anki fields

- `KanjiNote` joins the JSON lists `radicals`, `primaryVocab`, `similarMeaning` and `confusedWith` back into
  comma strings in memory, then re-parses them on every read.
- `VocabNotePartsOfSpeech` holds one raw string.
- `RelatedVocabDataSerializer`, `VocabNoteMatchingRulesSerializer` (*deleted 2026-11-08: nothing used it*),
  `ISentenceConfigurationSerializer` and `ParsingResultSerializer` exist only to produce Anki field strings and feed editor text boxes.
- There are two tables of Anki field names: `AnkiFieldNames` in `JAStudio.Anki`, and
  `NoteFieldsConstants` / `SentenceNoteFields` / `MyNoteFields` in `JAStudio.Core.Note`.
- `AnkiNoteData` is declared in `Note/JPNoteData.cs`, where the file name and the type name disagree.

### Editors

The editors are in the workspace (`VocabTextEditorViewModel`, `KanjiEditorSectionViewModel`,
`SentenceEditorSectionViewModel`). They still mostly copy Anki's note editor: one text box per stored string.

- *(Until 2026-11-08 the brackets and `question:disambiguation` were typed by hand. Each form now has an "owned" check
  box, compound parts have a box each for the primary and the alternative ones, and the question and its
  disambiguation have a box each.)*
- A vocab's forms and parts of speech are lists, with entries added and removed one at a time. Its readings and
  compound parts, and the kanji's lists, are still comma separated text.
- Parts of speech are free text, and audio is raw `[sound:…]`.
- Related vocab can't be edited: its raw JSON text box went with the vocab editor dialog, and a proper editor is
  still to come.
- Kanji readings are edited as HTML.
- The source fields are editable and get saved back.

## Target model principles (tentative)

1. **The JSON mirrors the domain model.** Every concept with meaning has its own JSON structure. No
   in-string-micro-syntax for anything the code interprets. Rich text that is only displayed stays text.
2. **Store facts, not conclusions.** No persisted-derived-data, except caches that are expensive to recompute
   and named as caches (`parsingResult`).
3. **Tags hold only labels the domain does not interpret.** Anything the code reads or reasons about is a
   typed field.
4. **Nothing in the corpus is there for Anki.** The Anki-projection derives what Anki needs, in `JAStudio.Anki`.
5. **The corpus is shared language data; student-data stays out.** That covers the studying status, priority and
   every other student-tag. Student-data belongs with the scheduler (Anki) or in a per-student store.
6. **Editors edit the model, not the storage.** Each concept gets a control that fits it: a list with an
   "owned" checkbox, a vocab picker, a reading list with a primary toggle. Derived values are shown read-only.

## Target shapes (sketches, not specifications)

The key names are placeholders that follow the current camelCase serializer. Each domain type's final name
should drive its JSON key, not the reverse.

### Vocab

```json
{
  "id": "00b4a087-8057-41b4-9172-c17752e65455",
  "question": { "text": "剥ぐ", "disambiguation": "" },
  "corpusAnswer": "",
  "readings": ["はぐ"],
  "forms": [
    { "text": "剝ぐ", "isOwned": true }
  ],
  "compoundParts": { "primary": [], "alternative": [] },
  "partsOfSpeech": ["godan verb", "transitive verb"],
  "register": [],
  "usuallyWrittenInKana": null,
  "explanation": { "short": "", "long": "" },
  "mnemonic": "",
  "matching": {
    "requiredOrForbidden": { "godan": "required", "ichidan": "forbidden" },
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
  "sourceData": { "answer": "to tear off; to peel off", "mnemonic": "…", "readingMnemonic": "…" },
  "tags": []
}
```

`forms` holds only the forms besides the question. The question is always a form, and always owned, so it isn't
repeated there. *(Already so in today's files, since 2026-11-06.)*

**`forms` and `compoundParts` are as above since 2026-11-08 (slice 1).** A form's text is unique within a vocab. The
primary compound parts are the vocab split into words, in order, and a word may repeat (`どいつもこいつも`). The
alternative ones are a set of words from other ways of understanding the vocab (Magnus, 2026-11-08): stored in a fixed
(ordinal) order, and never a word that's also a primary part, since it would add nothing. `VocabNoteCompoundParts.Alternative`
applies that rule when read, so typing the primary parts can't drop an alternative part on the way.

What disappears from vocab JSON:

- `activeAnswer` and `sentenceCount` (persisted-derived-data);
- `audioB`, `audioG`, `audioTTS`, `image`, `userImage` (media; see [Media](#media-open));
- every configuration tag, which moves into `matching`, `register` and `source`.

**`usuallyWrittenInKana` is as above since 2026-11-08 (slice 4b):** true, false, or null while undecided, placed after
`register`. It replaced the tags `_uk` (true) and `_disable_uk` (false); the three vocab with both, つづり, ていく and いく,
became true, since `_disable_uk` had no effect once `_uk` was set. While it's null, generating the vocab's data may set it
to true, and once it's decided it's never changed automatically (Magnus, 2026-11-08). The editor shows it as Undecided,
Yes and No, and true shows as "uk" on the vocab page and in the sentence breakdown. **Found while doing it:** the
dictionary half of the automatic decision never runs. Its condition is that the vocab has no parts of speech, but an
empty parts of speech parses as `{""}`, never an empty set, so only a kana-only question without readings sets it. That's
been so since before the slice, which kept it as it was.

**`register` and `matching` are as above since 2026-11-08 (slice 4a).** `register` is placed after `partsOfSpeech`.
`requiredOrForbidden` holds only the require/forbid rules that are set, each under its name (`RequireForbidFlagField.Name`),
so a rule can't be both required and forbidden. One vocab had both tags, which the old code read as failing every match:
笊, the noun, now forbids irrealis, which the auxiliary `ざる:masu` requires. The flags, the register and the rules' sets are
written in ordinal order. A name JAStudio doesn't know fails loudly when the vocab is read, since saving would lose it.
`isCompositionallyTransparentCompound` is a matching flag because only matching reads it, though the editor shows it
under Metadata. `matchWithPrecedingVowel` is a to-do flag that nothing reads and no vocab has.

Parts of speech are harmonized to one spelling per part during migration.

**Checked (2026-11-06):** vocab `sourceMnemonic` and `userMnemonic` are not overridable-text. The vocab card
(`VocabMnemonic.razor`) shows only the user mnemonic, with no fallback to the source mnemonic, which appears
only in the editor. So they're two separate fields, unlike the kanji mnemonic (`KanjiNote.ActiveMnemonic`
falls back). *2026-11-08: no mnemonic falls back to the source's any more; see
[Overridable-text without a type](#overridable-text-without-a-type).*

### Kanji

```json
{
  "id": "…",
  "kanji": "突",
  "corpusAnswer": "sudden/abrupt | stab/thrust/pierce/prick",
  "readings": {
    "on":  [{ "kana": "とつ", "primary": true }],
    "kun": [{ "kana": "つ",   "primary": true }],
    "nan": []
  },
  "radicals": ["穴", "大"],
  "mnemonic": "",
  "primaryVocab": [],
  "similarMeaning": [],
  "confusedWith": ["空"],
  "references": "",
  "sourceData": { "answer": "Stab, Thrust, Sudden", "meaningMnemonic": "…", "readingMnemonic": "…", "meaningInfo": "…", "readingInfo": "…" },
  "tags": []
}
```

**Checked (2026-11-06):** `<primary>` also marks the primary meaning in the source answer, but no code reads
it there. Only the readings are parsed (`KanjiNote.PrimaryReadingPattern`). So the source answer is rich text
from the source, like the WaniKani markup in mnemonics, and stays text.

### Sentence

```json
{
  "id": "…",
  "corpusQuestion": "",
  "corpusAnswer": "",
  "comments": "",
  "reading": "…",
  "sourceData": { "question": "…", "answer": "…", "comments": "", "externalId": "…" },
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
- `<wbr>` word-break hints move out of the question text into `configuration`, for example as a list of
  positions where the tokenizer must split (tentative). They're Magnus's own instructions to the tokenizer,
  set through `SentenceQuestionField.SplitTokenWithWordBreakTag`, not part of the sentence. Today they are even
  written into the source question;
- the parsing result is named as a cache.

## References between entities (word-references)

**Decided (Magnus, 2026-11-06): for now, references store the word itself, not ids,** because of the problems
ids cause when notes are deleted. That rules out option A below. **Open:** option B or C, that is, whether a
rename propagates.

The three options as they were weighed:

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

**Tentative leaning: C.** In my first report I leaned towards ids. I changed my mind, for two reasons:

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

After the configuration and the student-data move out, `tags` holds only corpus-tags. Navigating and editing
hierarchical tags in JAStudio is central to organizing (Magnus, 2026-11-06).

- **Difficulty** (`-::sentence::difficulty::N`, was on 48,550 sentences): **deleted 2026-11-06**, from the corpus
  and from Anki. Nothing read it.
  - It was written by JAStudio code from 2024-10-30 that sorted sentences into decks by difficulty. That code was
    deleted the same day in favor of custom auto-advance timings. The `DifficultyCalculator` still in Core is
    that replacement: it computes the auto-advance time from a card's character counts and doesn't read the tag.
  - The data agrees that it's a computed value, not a hand-tagged one. Within every episode it ranges from 1 to
    6, and it roughly follows sentence length: the median length is 5 characters at difficulty 1, 23 at 5 and
    67 at 8, with wide overlap.
- **Generated tags:** removed from the corpus. If any of them matter for searching in the Anki browser, the
  Anki-projection can still write them to Anki only. The study-state ones are student-data and are computed in
  memory from the studying status.
- **Orphaned tags:** deleted by the migration.
- **Source** (`source::…`) is a branch of the hierarchical tags.
  - The code reads it: the source is shown next to each sentence in a vocab's sentence list
    (`VocabSentencesList.razor`). The media import rules picked a note's media folder by it until they were retired
    on 2026-11-08.
  - The source tags are a tree written out as separate tags. 17,304 sentences carry several levels of one branch,
    such as both `source::wani` and `source::wani::current`, and 680 carry two unrelated sources, such as JLab
    booster plus inter.
  - **Open (question 9):** does the code keep reading it from the tag, or does it become a typed field? Moving
    source information out of the core corpus entirely (`media-storage-and-import.md`, "Source-Revealing Tags")
    remains a separate, deferred decision.
- **Priority** (`-::priority::…`, about 48,500 sentences): **decided 2026-11-06, student-data, not corpus.** It
  records Magnus's preferences, such as preferring sentences from Natsume's Book of Friends, not facts about the
  sentences. Priority most likely stays.
  - **What it's used for:** only a vocab's sentence list. Its number is one of the keys the list is sorted by,
    after highlighted, studying, displayed and has-an-answer, and the list shows only the first 30 sentences.
    "high" or "low" in the tag colors the sentence's source label. As the corpus grows, this ordering decides
    which sentences a vocab shows.
  - **Leaning:** read it from the Anki notes' tags when JAStudio loads, as the studying status is. The loader
    already reads every Anki note's `jas_note_id` (`AnkiNoteIdMapLoader.Load`), and the tags are in the
    same row. That needs the Anki-projection to stop replacing the Anki note's whole tag list: it writes only
    corpus-tags and leaves student-tags alone.
  - **For later:** a corpus note with no Anki note has nowhere to carry a priority tag. The alternative then is a
    per-student priority table keyed by source tag prefix, resolved by longest prefix, the way
    `MediaImportRuleSet` resolves import rules. The data supports that: every note with a priority tag also has
    a source tag, 17 distinct priority tags are spread over 143 source tags, and only 4 source tags carry more
    than one priority tag. The name part of a priority tag is just a label, and is sometimes wrong:
    `source::anime::girls-last-tour` notes carry `-::priority::high::011_Natsume_01`.

### Student-tags and corpus-tags

How today's tags split, as read on 2026-11-06 (the numbers count notes):

- **Student-tags:**
  - priority (about 48,500 sentences);
  - `legacy_scheduling` (9,355);
  - `_only_studying_listening` (1,065 sentences);
  - Anki's `marked` (2);
  - the kanji tags generated from what is studied: `-::kanji::studying-vocab` (1,670),
    `has-studying-vocab-for-each-primary-reading`, `studying-vocab-with-primary-on-reading`,
    `has-primary-reading-with-no-studying-vocab` and `has-studying-vocab-with-no-matching-primary-reading`.
    These are computed in memory, not moved to Anki.
- **Not yet classified:** `active_kanji` (1,531 kanji), `preparing-cards` (20), `tmp` (18), `-::TODO…` (about 40).
- **Corpus-tags:**
  - `source::…`;
  - labels about the words, such as `-::counting`, `-::formal-noun`, ~~`_uk`~~ *(the vocab's `usuallyWrittenInKana` since slice 4b)*;
  - `_tts_audio` and `fixed_primary_readings`;
  - ~~the matching configuration~~, *moved out of tags 2026-11-08 (slice 4a), with the register*;
  - the generated tags that don't depend on studying, which go in slice 6.

## Media (Open)

The direction is already set in `media-storage-and-import.md`: the core corpus stores no media data, and all
media association lives in the media layer's sidecars, which already record `noteIds`. What's missing is
removing `audioB`/`audioG`/`audioTTS`/`image`/`userImage` (vocab), `audio`/`screenshot` (sentences) and the
kanji media fields from the corpus data types.

Magnus doesn't remember the "unresolved design concerns". These are my **guesses** at what they might have
been. They are not reconstructions:

1. **Field roles.** The corpus field says which slot a file fills (`audioB` versus `audioG` versus `audioTTS`,
   `image` versus `userImage`). The sidecars don't record that role. `VocabMediaField` existed only in the
   import rules, retired on 2026-11-08. Dropping the fields loses the role, unless the sidecar or a preference layer
   records it.
2. **The Anki-projection still has to fill Anki's audio fields** so listening cards can auto-play. It needs a
   rule for which attachment goes into which Anki field, and the file has to exist in Anki's media folder.
3. **The audio-update ingestion path changes.** Today new audio arrives as a changed Anki field string, lands in
   the corpus, and is later picked up by the media import scan (`MediaImportAnalyzer`). Without corpus fields,
   the Anki-ingestion has to store the file into the media layer directly (`MediaStorageService`). *Moot since
   2026-11-08:* a save in Anki takes nothing, JAStudio's TTS stores its audio in the media layer itself, and the media
   import is retired.

Tentatively, media is the last vertical-slice, after these points are settled.

## Import and organization (needs its own design)

Magnus: tagging, organizing and batch import need more thinking and design. These are notes towards that
design, not a proposal. *The design of importing itself, import profiles, is in `import-profiles.md`
(2026-11-08).*

**Decided so far (2026-11-06):**
- **Import goes through Anki notes,** not through JAStudio reading deck files. Anki already reads every .apkg
  format version, brings in the media, and has the batch editing to get notes into shape before they're imported
  (find and replace, change note type with a field mapping, tagging). JAStudio has no batch editing at all yet.
- **Imports happen in a separate Anki profile, the Anki profile for importing.** Importing a big subs2srs deck into the profile that syncs would
  put all its audio and images into that profile's media folder, and I believe Anki's media sync uploads new
  files whether or not a note still uses them. A separate profile works only if the corpus is properly separated
  from the Anki collection. JAStudio loads the corpus from the JSON files and takes only the Anki note ids and
  the studying status from the open profile, so the leak is the student-data saved in the corpus. In the Anki
  profile for importing nothing is studied, so a full rebuild there would rewrite the generated study-state tags as
  if nothing were studied. Taking student-data out of the corpus removes that leak.
- **An import takes all the notes' tags along,** so work done while preparing the notes in Anki isn't lost. After
  import, JAStudio manages the corpus-tags.
- **The decks** are downloaded subs2srs decks in Magnus's Dropbox. `…\Anki\SUBS2SRS\Unprocessed` hasn't been
  touched; `…\Anki\SUBS2SRS\Processed` was polished to the standard of the time. Magnus last imported a deck years
  ago. Their fields tend to be similar to the corpus sentences' fields. The files are online-only, and I believe
  reading even the note types inside an .apkg downloads the whole file, media included, so pick small ones to
  inspect.
- **The import path in the code** was the Anki-ingestion's note-added hook (`ExternalNoteAdded`), which Yomitan's
  notes went through. *Verified 2026-11-08 in Anki 26.9.3:* the .apkg importer doesn't run that hook; it runs in
  Anki's Rust backend, and Anki only announces the finished import (`operation_did_execute`, with the
  `ImportDialog` as source). The Immersion Kit conversion that ran after an import was an empty TODO until import
  profiles replaced it on 2026-11-08 (`import-profiles.md`). They run on both signals, and `ExternalNoteAdded` is
  gone.

**Where new data comes from, and how each source could reach JAStudio directly** (written before import through
Anki was decided; kept for reference):

- **Yomitan** adds notes through AnkiConnect, a local HTTP JSON API. *Moot since 2026-11-08: Yomitan no longer adds
  notes.*
  - JAStudio already runs a Kestrel server (`JAStudio.Web/CardServer.cs`), so it could serve the part of that
    API Yomitan uses, and Yomitan would talk to JAStudio instead of Anki.
  - Unverified: exactly which actions Yomitan calls. They probably include `addNote`, `canAddNotes` and
    `storeMediaFile`, and Yomitan's source would settle it. Also unverified: that Yomitan's AnkiConnect server
    address can be configured.
- **Shared decks (`.apkg`)** are ZIP files with an SQLite database inside. There are three format generations:
  - In the two older ones, `collection.anki21` is plain SQLite and the media map is JSON. Reading that from
    .NET is easy (`System.IO.Compression` plus `Microsoft.Data.Sqlite`).
  - The current one, since Anki 2.1.50, uses `collection.anki21b`: zstd-compressed SQLite, with configuration
    and the media map in Protobuf. That needs a zstd library and the Protobuf schemas. I believe the schemas
    are in Anki's open-source repository, but I haven't checked.
  - Anki's developers say `.apkg` isn't meant to be read by other software, so the format can change between
    Anki versions. For a one-time import, where any breakage is immediately visible, that risk looks
    acceptable.
- **HyperTTS** is an Anki add-on that writes generated audio into an Anki field. Calling the TTS services from
  .NET directly would remove that dependency.
- **The raw source material** (subtitle files plus audio cuts) might be importable without any deck, but this
  is speculative.

**Corpus entries without Anki notes.** Eventually the corpus holds far more sentences than are studied, and only
what is studied has Anki cards (Magnus, 2026-11-06): Anki holds study handles for a subset of the corpus. That
matches `web-rendering-architecture.md`, which plans a corpus browser for 50K+ entries, most of them not in any
Anki collection. Importing in a separate profile is where such entries first appear, so that complexity arrives
with import. Until then every note stays in Anki, and studied means unsuspended.

**Organizing.** Navigating and editing hierarchical tags in JAStudio is central (Magnus, 2026-11-06). They can do
most of what folders, tags and decks do in other programs. The source tree is the `source::` branch of the tags.

**How Magnus chooses what to study** (2026-11-06; "much of this is up for debate and/or refinement"):
- **Big batches:** the notes of the next episode of an anime, found by their source tags and sorted by their
  subs2srs id or media file name, which tends to be the order of the lines in the episode. The subs2srs id is
  the sentence's `externalId`, for example `ny4-nodup_11_0.15.23.540-0.15.25.620`: the deck, episode 11, and the
  line's start and end times.
- **Then, while studying,** more sentences that contain the same vocab as the new ones, to see the vocab in more
  usages and senses.
- **Studying** a note means its cards aren't suspended, so they turn up in the decks Magnus studies.

## Migration approach (tentative)

- **One-time migrations, no permanent dual-format readers, no format version field.** This is justified by
  Magnus being the only consumer of `jas_database`.
- Each vertical-slice ships as:
  - a JAStudio commit that changes the domain type, the corpus data type and the editor (by then, the
    Anki-projection only writes the study-note fields, so a slice rarely touches it);
  - a migration run once over the corpus, whose code is deleted in the same or the next commit;
  - a `jas_database` commit with the migrated files.

  The JAStudio commit moves the `src/jas_database` submodule pointer to the migrated data, so every JAStudio
  commit pairs with corpus data it can read.
- **The on-disk MemoryPack snapshot** (`JsonFilesystemObjectRepository.SaveSnapshot`) can't be trusted across a
  format change, so each migration deletes it. I haven't checked whether anything already detects a stale
  snapshot.
- **Anki browser searches** no longer search Anki's fields (done 2026-11-06): what still opens Anki's browser
  searches by Anki note id. They don't constrain the field trim or the vertical-slices.

## Order of work (tentative)

The order follows the overall approach in [Settled so far](#settled-so-far-magnus-2026-11-05): build
JAStudio's own UI until Anki's role is minimal, and only then change the model.

An earlier draft started by moving the whole Anki-projection and Anki-ingestion into `JAStudio.Anki` unchanged.
That step is dropped. Under this order most of that code gets deleted rather than moved:
- `MergeAnkiData` shrinks to picking up audio changes. *(Done 2026-11-07, and gone entirely on 2026-11-08: a
  save in Anki takes nothing.)*
- `PopulateAnkiNoteFields` shrinks to the study-note fields. *(Done 2026-11-08.)*
- `FromAnki` was to survive as the raw note mapping. *Gone 2026-11-08:* an import profile's field mapping maps raw
  notes (`import-profiles.md`), and `FromAnki` went with the take-in of notes added in Anki.

What's left lives in `JAStudio.Anki` from the start.

**Current focus (Magnus, 2026-11-06): step 3, one capability at a time.** Steps 1, 2 and 4 are deferred until
they are in the way. Questions are settled when the work reaches them, not up front.

Progress on step 3:
- **Viewing and navigating notes** (2026-11-06): JAStudio's workspace replaces Anki's previewer for note links,
  with the same right-click menu and editors. See `web-rendering-architecture.md`, "Notes in JAStudio's
  Workspace", and `jastudio-workspace.md` for where the workspace is heading.
- **Lists of notes** (2026-11-06): the note menus' "Open" items list their notes in the workspace's navigation
  pane instead of Anki's browser, and the old text searches into Anki's browser are deleted.
- **Editing notes** (2026-11-06): every note type is edited live in the workspace; the editor dialogs are gone.

**Where that leaves us (2026-11-06):**
- Navigation, lists and editing are done. Organizing and import are the open design areas.
- The workspace's remaining steps (multiple workspaces, reverting a note) are parked, because they free nothing
  from Anki.
- ~~**Proposed, not started:** cut the Anki-ingestion down to audio, images and tags~~ *Done 2026-11-07, cut
  further, to the audio TTS writes (see [Anki's role and its consequences](#ankis-role-and-its-consequences)).*
- The difficulty tags are deleted, from the corpus and from Anki (2026-11-06).
- What comes next isn't chosen yet (Magnus, 2026-11-06).
- *2026-11-07:* tags come next: navigating, editing and tagging, specified in `tags.md`, with a search language
  and a results grid, proposed in `note-search.md`.
- *2026-11-07 and 08:* JAStudio generates its own TTS audio (`text-to-speech.md`), normalized as mp3gain normalized
  HyperTTS's (`mp3-loudness-normalization.md`), so Anki is no longer needed for TTS, and HyperTTS isn't used any more.
- *2026-11-08:* checking the whole corpus is wanted (`corpus-validity.md`, a draft).
- *2026-11-08:* the import flow is done (`import-profiles.md`), and JAStudio's own UI is complete enough for the model
  phase (Magnus): what turns out missing gets added as it's found, without holding the model phase up. Mobile-review
  is deferred (step 2): Magnus doesn't review on mobile now, and adding it later should be simple. The checks before
  the trim are done (see [Before the trim](#before-the-trim-2026-11-08)).

1. **Remove the kanji primary-readings TTS** (deferred). This happens almost entirely on the Anki side:
   - delete the kanji HyperTTS presets;
   - delete the `_primary_readings_tts_audio` field.

   On the JAStudio side the only trace is one line in `JAStudio.Web/Styles/notes/kanji/kanji.mustache`.
2. **Install the mobile-review card templates** in Anki (deferred, 2026-11-08: Magnus doesn't review on mobile now; the existing templates stay for now): the
   committed minimal templates plus the generated-html-field. On desktop the add-on still replaces or covers
   them. This step needs open question 1. See
   [What the card templates actually use](#what-the-card-templates-actually-use).
3. **JAStudio capabilities that replace Anki:** navigation and search, editing, organizing, and the import
   flow (see [Import and organization](#import-and-organization-needs-its-own-design)). Each one frees the Anki
   fields and tags it replaces (see
   [Anki dependencies to replace](#anki-dependencies-to-replace-before-their-fields-can-go)).
   - Building editors properly is part of each vertical-slice in step 5, not this step, so they aren't built
     twice. The existing editors are crude but work.
   - Fields and tags are separate channels. Tag ingestion can stay until JAStudio's organizing exists, without
     holding up the field trim in step 4.
4. **Trim the existing notes into study-notes** in as few schema changes as possible. Trimming in place keeps
   review history and note type ids. *Done 2026-11-08:* the checks before it ([Before the trim](#before-the-trim-2026-11-08)),
   JAStudio writing only the fields the study-notes keep ([The minimal Anki note type](#the-minimal-anki-note-type-tentative)),
   and Magnus removing the other fields in Anki by hand, then testing the result.
5. **Vertical-slices, one concept each,** now free of the Anki format:
   1. Brackets: owned forms and alternative compound parts. This one has caused real bugs. *Done 2026-11-08: see
      [In-string-micro-syntaxes](#in-string-micro-syntaxes) and [Vocab](#vocab). The migration also merged 25 forms
      entered twice within a vocab and dropped the empty question's sentinel, `[EMPTY]`, from the forms of `げ` and `極まる`.*
   2. Question and disambiguation, dropping the sentinel strings. *Done 2026-11-08: see
      [In-string-micro-syntaxes](#in-string-micro-syntaxes). References keep naming a vocab `text:disambiguation` until
      slice 8 (Magnus, 2026-11-08).*
   3. Overridable-text as a type, dropping `activeAnswer`/`activeQuestion`. *Done 2026-11-08, re-modeled with Magnus: no
      overridable-text type, but each note's `sourceData` and the corpus's own fields (see
      [Terms](#terms-used-in-this-document)). `activeAnswer` and `activeQuestion` aren't stored: the notes compute them, and
      the Anki study-note is built from the note (`JPNote.GetData`), not from its DTO. The migration found 433 stored ones
      that differed from the computed only by surrounding whitespace, and none that differed otherwise. `sourceName` stays
      where it is until slice 4.*
   4. Configuration out of tags: requires/forbids flags, boolean flags, register, `_uk`/`_disable_uk`, source.
      - 4a, the vocab's matching configuration and register. *Done 2026-11-08: see [Vocab](#vocab). The register came
        along because it's the same mechanism (`FlagField`), which retired the tag-backed `TagFlagField`. The full rebuild
        changed nothing. The matching configuration's own design is up for a major refactoring later, to a flexible tree
        structure resembling the note search's (Magnus, 2026-11-08), so its type names weren't polished here.*
      - 4b, `_uk` and `_disable_uk`. *Done 2026-11-08, as a pure refactoring: see [Vocab](#vocab). The workspace can set
        both again: the tag pickers refused them as jastudio-defined-tags, and the Metadata tab now has Undecided, Yes and No
        (Magnus: it must stay possible to stop JAStudio insisting that a vocab is usually written in kana).* `_tts_audio`
        moved to slice 6.
      - 4c, source, with the TODO below.
      **TODO, decided to do here (Magnus, 2026-11-08): untangle the provider from the work in the data.** "Source" mixes
      two levels today: the *provider* a note was imported from (WaniKani, jamdict, the JLab deck, Immersion Kit) and the
      *work* its content comes from (Shirokuma Cafe, Girls' Last Tour). For WaniKani vocab and the subs2srs anime decks
      they're the same; for JLab and Immersion Kit sentences they differ, which is why a JLab sentence has a tag naming
      the deck (`source::jlab::booster::1`) and a `sourceName` naming the anime. Decide whether they're one concept or
      two, and how a note's source is stored (a path, or fields), then move `sourceName` once, to where that puts it.
      Also fix the sentences tagged with two different sources, most likely editing mistakes: 291 in both
      `source::jlab::booster::1` and `source::jlab::inter`, 233 in `source::anime::violet::07` and `::08`. (A parent
      tag together with its child, as `source::wani` with `source::wani::current` on 14,567 sentences, is one source.)
   5. Priority out of the corpus, as student-data (see [Tags](#tags)).
   6. Persisted-derived-data and study state out: `sentenceCount`, generated tags, orphaned tags. Also `_tts_audio`, if
      the media decides it: nothing sets the tag, not even JAStudio's own text-to-speech, while the vocab page reads it to
      mark sentences and to list those with text-to-speech audio after those with native audio. Check against the 14,607
      sentences and 245 vocab tagged whether the audio file (a HyperTTS file name, or JAStudio's text-to-speech folder)
      says the same.
   7. Kanji readings and parts of speech.
   8. References between entities as typed word-references (with or without rename propagation), plus vocab
      pickers to replace the related-vocab JSON text box.
   9. Media, once the points under [Media](#media-open) are settled.

### Before the trim (2026-11-08)

Before step 4 deletes the Anki fields, the corpus was checked against Anki, so that anything only Anki held could still
be saved (Magnus: anything we need the Anki data to fix has to turn up before the Anki data is deleted). The checks ran
once, from throwaway code that isn't kept, on a copy of the Anki collection.
- **Every Anki note has its corpus note,** the tags are identical, and every media reference in the corpus is in the
  media layer.
- **The fields JAStudio writes match the corpus,** but for formatting (list separators, spacing, JSON layout, line
  breaks) and old forms of the same content, such as the question repeated among the forms, or dictionary-form
  endings expanded where the corpus keeps the placeholder. Two vocab differ for real: `でいられる` has the alternative
  compound parts `[でいる]` and `[られる]` in Anki only, and one vocab's answer is reordered in the corpus.
- **Fields only Anki holds,** and what becomes of them:
  - kanji `__audio` (1,657 kanji): *dropped with the trim* (Magnus). JAStudio doesn't target studying kanji in
    isolation, and the audio can be computed from the kanji's vocab;
  - kanji `_image` (71 kanji, 80 images): *kept,* as the kanji's `Image`, a field JAStudio only carries (Magnus: keep
    for now whatever is easy to keep, to be safe). Unlike the audio, an image can't be made again. Its files count as
    used by the kanji (`KanjiNote.MediaReferences`), so deleting another note never deletes them;
  - kanji `_primary_readings_tts_audio`: dropped, as planned (step 1);
  - sentence `Source` (5,772 sentences): the work or deck a sentence came from, such as JLab's "Shirokuma Cafe", which
    no tag records for JLab's 3,009 sentences. *Kept,* as the sentence's `SourceName`, a field JAStudio only carries
    (Magnus): source tags are for whole imported seasons, and source comments are the note creator's comments, which
    are on their way out. Import profiles can map it, and it was filled from Anki's `Source` field on 2026-11-08.
- **Audio that isn't MP3:** the four known Ogg files, and two Yomitan WAV files on vocab (`corpus-validity.md`).

**Watch-outs for the import flow (step 3):**
- ~~**Yomitan's "already added" check.**~~ *Moot since 2026-11-08:* Yomitan no longer adds notes.
- **HyperTTS automatic rules** are keyed by note type id and deck id. Not verified whether they fire for notes
  that code creates.

## Open questions

**Deferred (Magnus, 2026-11-06).** None of these is to be settled up front. Each one is decided when the work
reaches it, if it still matters by then. Answered so far: question 2 (AnkiDroid), question 3 (Magnus doesn't
edit in Anki; no two-way sync except where it can't be avoided), questions 7 and 8 (2026-11-06), and part of
question 4.

### About the Anki notes and templates

1. **What does the generated-html-field show on mobile?** *(Reworded 2026-11-06: the single generated-html-field
   replaced the question of which fields to keep.)* The answer, and for kanji the readings with the primary ones
   marked, are the obvious start. Candidates beyond that:
   - Vocab: the reading, the user mnemonic, the parts of speech, the image.
   - Kanji: the mnemonic, the radicals, the readings that are neither on nor kun.
   - Sentences: the screenshot.

   Unlike a field, content can be added to the generated HTML later without a schema change. Each change does
   regenerate the field on every study-note, though. See
   [The minimal Anki note type](#the-minimal-anki-note-type-tentative).
2. **Which mobile app does Magnus use: AnkiDroid (Android) or AnkiMobile (iPhone and iPad)?** The new templates
   have to be tested in that app, because its rendering and night mode differ from desktop Anki's.
3. ~~What happens to edits made in Anki to the fields kept for mobile-review?~~ *Answered 2026-11-06:* Magnus
   doesn't edit questions or answers in Anki, on desktop or on AnkiDroid. JAStudio ignores such edits, and its
   next Anki-projection of the note overwrites them, so all editing happens in JAStudio.
4. **What does Magnus use Anki's browser and Anki's tags for today?** The answer decides what JAStudio's
   navigation and search (step 3) must replace first. It also decides which tags the Anki-projection keeps
   writing to Anki. In particular:
   - ~~The JAStudio menu items that open Anki's browser on a search.~~ *Answered 2026-11-06:* the note menus'
     lists moved to the workspace, and the text searches into Anki ("Anki" in the Lookup menus) were unused and
     are deleted.
   - *Partly answered 2026-11-06:* Magnus finds the next episode's notes by their source tags, sorts them by
     their subs2srs id, and unsuspends them. See
     [Import and organization](#import-and-organization-needs-its-own-design).
   - Searches on tags, for example to build filtered decks or to suspend or move cards in bulk. That could be the
     generated tags (`-::kanji::…`, `-::vocab::has-no-studying-sentences` and so on), or the priority and source
     tags.
   - Anything else the browser is opened for.
5. ~~**Should an image added or changed in Anki reach the corpus?**~~ *Settled 2026-11-07 (Magnus): no. Images
   only come with an import, and since 2026-11-08 a save in Anki takes nothing (see
   [Anki's role and its consequences](#ankis-role-and-its-consequences), point 2).*

### Needed for later vertical-slices

6. **Rename propagation for word-references (slice 8).** References store the word (decided 2026-11-06). Say a
   vocab's question is renamed, or gets a disambiguation. Should every reference to it in other notes be
   rewritten (option C)? Without that (option B), references that use the old spelling silently stop resolving.
   Leaning: C.
7. ~~Priority (slice 5): a per-source table or a per-note field?~~ *Answered 2026-11-06:* priority is
   student-data and doesn't belong in the corpus. It most likely stays. The leaning is to read it from the Anki
   notes' tags when JAStudio loads; how it's stored for notes without an Anki note is for later. See
   [Tags](#tags).
8. ~~Difficulty: a label or a typed field?~~ *Answered 2026-11-06:* deleted. Nothing uses it. See [Tags](#tags).
9. **Source (slice 4).** See the TODO under slice 4 in [Order of work](#order-of-work-tentative): the provider and
   the work have to be untangled first. Every note has a `source::…` tag, such as `source::anime::girls-last-tour`, and JAStudio
   reads it (`JPNote.SourceTag`). Since 2026-11-06, organizing builds on hierarchical tags, and the source tree is
   the `source::` branch. Does the code keep reading the source from that tag? Target model principle 3 says the
   code should only interpret typed fields, not tags. Or does the source become a typed field? Separately,
   `media-storage-and-import.md` ("Source-Revealing Tags") defers a decision to move source information out of
   the core corpus entirely.
10. **The media concerns (slice 9).** `media-storage-and-import.md` mentions "unresolved design concerns" about
    removing the media fields from the corpus, and Magnus doesn't remember what they were. My three guesses are
    under [Media](#media-open). Does any of them ring a bell, or was it something else? Mobile-review confirms
    that the second guess matters. The Anki-projection must keep filling Anki's audio fields, and the files must
    be in Anki's media folder so they sync to the phone. This isn't urgent, since media is the last slice.

### Needs its own design, later

11. **Tagging, organizing and batch import.** Partly decided 2026-11-06. See
    [Import and organization](#import-and-organization-needs-its-own-design).
12. **Which of these tags are student-tags:** `active_kanji`, `preparing-cards`, `tmp` and `-::TODO…`? See
    [Student-tags and corpus-tags](#student-tags-and-corpus-tags).

### For me to verify (not questions for Magnus)

- Whether HyperTTS's automatic rules fire for notes created by code.
- Whether removing fields forces a full AnkiWeb sync. With mobile-review in the picture, a full sync also means
  the phone downloads the whole collection again, so batching the field changes matters more.
- How Anki handles a template that references a removed field. This is moot if the new templates are installed
  first.

### Resolved from the code (2026-11-06)

- **Vocab mnemonic fallback:** there is none. The vocab card shows only the user mnemonic, so the two are
  separate fields. See [Vocab](#vocab).
- **Kanji primary meanings:** no code reads `<primary>` in the source answer, so it stays text. See
  [Kanji](#kanji).
- **Where `<wbr>` comes from:** Magnus's own tokenizer split hints, proposed to move into `configuration`. See
  [Sentence](#sentence).
- **Final names for the new types and JSON keys:** chosen in each vertical-slice, not up front.

## Out of scope

- **Renaming "note"** (`JPNote`, `VocabNote`, `KanjiNote`, `SentenceNote`, `NoteCache`, …) now that JAStudio
  owns the data. It's agreed as probably right, but it's a separate effort.
- **Moving source information out of the core corpus.** This is deferred in `media-storage-and-import.md`.
