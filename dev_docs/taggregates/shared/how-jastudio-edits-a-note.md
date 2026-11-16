# How JAStudio code edits a note, once the state store keeps the notes

> **Status: agreed, 2026-11-16: by Magnus ("Looks good to me"), and by `jastudio-rep-opus`, whose review is in it.** By
> `taggregates-opus`. Step 7 of
> [the migration](the-migration.md), increments 3 and 4, which are one step. It decides how every edit in JAStudio
> is written, so it's settled before the change that converts them all. The model it fits is Magnus's, in Compze's
> `dev_docs/taggregate-improvements/shared/the-taggregate-store.md`, "The programming model":
> - a committed instance never changes;
> - a change outside a transaction is a bug, refused by construction;
> - what holds a taggregate across units of work holds its reference.

## The headline

**Every edit is one call: `note.Edit(it => ...)`.** It joins the unit of work that is running, or begins one, gets the
note's private copy for update, and runs the edit on the copy. When the unit of work commits, the copy becomes the
note everyone reads, the note's file is written, and `JPCollection.NoteUpdated` is raised.

```csharp
vocab.Edit(it => it.CorpusAnswer.Set("to run"));
```

- **Inside `Edit`, read and change only `it`.** `Edit` gets the copy by the note's id, of the note as it is committed
  now, so calling it on a stale instance is safe. But a lambda that reads the instance `Edit` was called on, such as
  `vocab.Edit(it => it.CorpusAnswer.Set(vocab.CorpusAnswer.Value + "!"))`, computes from that instance's state, and when
  it is stale, writes old-derived data over a newer commit: a lost update (`jastudio-rep-opus`).
- **Changing the instance instead of `it` fails before it changes anything,** such as
  `vocab.Edit(it => vocab.CorpusAnswer.Set(...))`: `NoteGuard.Update` asserts first, through Compze's
  `Taggregate.AssertCanBeChanged` (done, `1089ee460`).

## The order it is built in

Each a commit that builds and passes the whole suite, so the switch to the store at the end is small.

1. **Done (`f706ab16e`). `note.Edit(it => ...)` before the store:** `EditingANote.Edit`, which until the store keeps
   the notes runs the edit on the note itself. Its contract is already the final one: change only `it`.
2. **Done (`edf320634`). Every edit site calls `Edit`,** an area at a time: commands and editable fields, the UI's
   direct changes, the MCP tools, the batches, the importer, the shortcuts, text-to-speech, the shared conditions, the
   cross-note changes in the domain, then the specifications.
   - **Checked since, by `JPNote`'s guard:** a change to a note of the collection outside an edit of it fails
     (`EditingANote.AssertIsBeingEdited`, specified by `When_a_note_is_changed`). It goes when the store's read-only
     committed notes refuse such changes themselves. With it, the whole suite passes, a full rebuild of the corpus and
     saving every note change no file, and copies lose only `JPNote`'s record of what was saved (step 4).
   - **`Edit` is a classic extension method,** not a C# 14 extension block: the Python stub generator writes an
     extension block's compiler-generated types into the stub, and loses the whole namespace's types.
   - **The specifications (`3c9a67540`, `a6ffb7350`, `604131b4b`)** edit the notes they change, as product code does.
   - **The product code (`f706ab16e`, `3cc42bf2e`).** What changed in how it reads:
     - The editors that changed a part of the note directly hold an `EditedNotePart<TPart>`: the part read on the
       current note, and changed in an edit of it. A flag's check box writes its editable field.
     - `IHasTextToSpeechAudio.Edit`, for code that knows the note only as one with TTS audio.
     - What creating a note does to it after adding it runs in an edit of it, so the behavior is unchanged: moving it
       before the note is added would skip filling a clone's parts of speech from the dictionary.
     - A cross-note change edits the other note: the mutual relationships, the perfect synonyms, the forms, renaming the
       word references and the shared conditions' rewrites.
   - **The sites were found by a check, not by reading.** In the working tree, `JPNote`'s guard asserted that a note of
     the collection changes only in an edit of it (`EditingANote.AssertIsBeingEdited`), and a run of the whole suite
     that recorded each failing stack instead of throwing listed every site at once, grouped by the code that should
     edit. Code no specification runs was found by reading: the menus and editors of the survey.
   - **For the switch:** renaming a vocab and using a shared condition wherever its definition is edit every vocab, and
     renaming also edits every kanji. Each edit copies its note once the store keeps them, about 28,000 copies: measure
     it then. And some operations are several edits in a row, such as the MCP server's `edit_a_note`, which writes the
     fields, reparses and updates the generated data: one unit of work around each operation would commit it whole.
3. **The holders, and the specifications' fields, hold references:** `ITaggregateReference<TNote>`, made over the
   collection until the store keeps the notes.
4. **The switch:** the store is composed with `CorpusNoteFiles`, the three note classes and their indexes; the
   collections read through it; `Edit` gets the copy; `NoteUpdated` is raised after the commit, and its four subscribers
   move to the UI thread; JAStudio's caches go; a restored note starts from its snapshot's hash (the `wip:` at
   `JPNote._persisted`). Then the corpus checks: saving every note, a full rebuild, and
   `find_what_copies_of_the_notes_lose`.

## The three decisions

### 1. Where a unit of work begins: at the edit, with `Edit`, not at a few central places

There is no single place every edit goes through (the survey, below). Commands and editable fields have central
methods, but about 20 UI classes change a note's parts directly, as do the MCP tools for tags, suspending, deleting and
creating, the batches, the shortcuts the Anki add-on runs, text-to-speech, the shared conditions and the importer.

- **`note.Edit(it => ...)` at each of them,** and in `NoteChangingCommand.Run`, `EditableField.Write` and
  `IFieldEditedOneItemAtATime.Add`/`Remove` once for all their commands and fields.
- **Creating, deleting and reloading** run in a unit of work of the collection's: `JPCollection.InAUnitOfWork(...)`,
  which `Edit` uses too.
- **The full rebuild:** a unit of work for each note, on the threads it uses today. That is safe while each unit of
  work changes only its own note, as `UpdateGeneratedData` does.
- **Parallel units of work that change each other's notes would deadlock** until the store's lock times out, after
  30 s, naming the note (`jastudio-rep-opus`). None do: `LocalNoteUpdater`'s batches on many threads, the generated
  data, the metadata tags and the reparse, each change only their own note. `RegenerateJamdictVocabAnswers`, whose
  answers `PerfectSynonyms` pushes into every synonym, runs on one thread. A batch that changes other notes keeps to one.
- **Units of work nest:** Compze's `ExecuteUnitOfWork` joins an ambient transaction, so a command that runs another
  command's edit, or a domain method that edits another note, is one unit of work.

### 2. A lookup gives the note, not its reference. What holds a note across edits holds its reference

Compze's lookups and indexes return references, so that the types say "don't hold this". JAStudio's collections keep
their query API, made over the store's indexes, and return `reference.Current`, the note as it is now.

- **Recommended, because readers outnumber holders by far:** parsing, rendering, searching and the MCP server's
  queries read notes within one call. Making each of them write `.Current` adds noise at hundreds of places and
  catches nothing, since `Edit` is safe on a stale note.
- **The holders, about 25 classes, hold references, or ids they look up again:** the view models that keep a note,
  such as `SentencePageViewModel`, and the MCP server's `NoteVersions`. Many UI classes already keep only an id, or look
  the note up on each use, as the editors do. The caches' indexes go: the store's replace them.
- **The alternative:** the collections return references, as Compze's API does. Every reader then writes `.Current`,
  and the compiler finds every holder. That's safer by construction, and much more churn.

### 3. Specifications: a note in a field becomes a reference, read through a property

95 specification files hold a note in a field, and 60 of them change it and then assert on it. Each such field becomes
the note's reference, with a property of the old name that reads it:

```csharp
readonly ITaggregateReference<VocabNote> _vocabReference;
VocabNote _vocab => _vocabReference.Current;
```

Every assertion stays as it is, and each edit becomes `_vocab.Edit(it => ...)`. `CreateVocab` and the other helpers
return the note, and its `Reference()` gives the reference.

## What reaches another note

**Until step 11 makes them participation handlers, they edit the other note with `Edit`,** which joins the unit of work
that is running. 11 places:

- `PerfectSynonyms`: the answer pushed to every synonym, and leaving or joining the web.
- `MutualWordRelationship`, `MutualKanjiRelationship` and `Synonyms.AddTransitivelyOneLevel`: the other side.
- `VocabNoteForms.Add` and `Remove`: the vocab whose question the form is.
- Renaming a vocab: `RenameTheWordReferences`, over every vocab and kanji.
- `VocabCloner`; `VocabNoteCompoundParts.AutoGenerate`, which creates vocab; and the shared conditions, which rewrite
  every vocab that uses one.

`PerfectSynonyms` follows its own vocab's `CorpusAnswer` through a subscription made in its constructor. On a copy that
still works, since a copy builds its parts.

**Participation handlers run as each publish cascade ends, not at the commit** (Magnus, 2026-11-16; spike `b5d9ae9ed`).
A tevent reaches them once its note is at rest, inside the domain call, as these changes happen today: code later in
the unit of work sees what they did, and a handler's failure comes out of the call, not out of the commit. So step 11
moves each of these places into a handler without changing when it happens.

**A note looked up inside a unit of work that then rolls back is the discarded copy,** holding changes that never
committed. Only a holder of instances could keep one, and the holders hold references (`jastudio-rep-opus`).

## `JPCollection.NoteUpdated`, after the commit

Raised for each note a unit of work changed, with the new committed note, once it has committed. Today it is raised
inside the change, on the changing thread, often the UI thread. Its subscribers are four UI and web classes that show
changed notes, and the saving, which the store's storage takes over. **Raised on the engine's thread, the four must
move to the UI thread themselves** (`jastudio-rep-opus`). Compze's observers run after the commit, on the engine's own thread,
so a specification that asserts on what an observer does waits for the observers: the wait the plan lists last.

## What `jastudio-rep-opus` answered, 2026-11-16

1. **Failing before the change:** `Taggregate.AssertCanBeChanged()`, which `NoteGuard.Update` calls first (spike
   `3d3410a5d`, JAStudio `1089ee460`). A copy that publishes nothing is neither written nor swapped in, so every change
   must reach `Flush`: `NoteTags` and `WordExclusionSet`, which changed state outside the guard, now go through it.
2. **`Edit` from an instance stays JAStudio's,** for now: where units of work begin is the application's policy, and an
   instance-based `ForUpdate` in Compze would invite holding instances. A candidate for Magnus's naming and API pass once
   the migration has shown how it works.
3. **The observers' wait:** `ILocalTessagingEngine.AwaitObserversAsync()`, which leaves the engine running (spike
   `26990b991`).

## The survey, 2026-11-16

- **Commands:** 50 definitions, with 42 of them the vocab's, through `NoteChangingCommand`,
  `NoteChangingCommandTakingAText`, `VocabCreatingCommand` and `VocabCreatingCommandTakingAText`. The UI's menu items run
  them, and so does the MCP server's `CommandOfANote`.
- **Editable fields:** 14 for a vocab, 7 for a kanji and 11 for a sentence, through `EditableField.Write` and
  `IFieldEditedOneItemAtATime`. The editors' view models and the MCP server's `EditableFieldOfANote` write them.
- **UI changing parts directly:** `EnteredFormsEditorViewModel`, `PartsOfSpeechEditorViewModel`,
  `AudioListEditorViewModel`, `ImageListEditorViewModel`, `KanjiReadingListEditorViewModel`, `FlagEditorViewModel`,
  `UsuallyWrittenInKanaEditorViewModel`, `OneNoteTagging`, the menus of `VocabStringMenus`, `SentenceStringMenus`,
  `SentenceElementMenus`, `KanjiStringMenus` and `KanjiNoteMenus`, `BulkNoteChanges`, `ActionsOnSelectedResults`,
  `TextToSpeechForANoteViewModel`, the shared conditions' windows, and creating notes in `NoteContextMenu`.
- **Elsewhere:**
  - `LocalNoteUpdater`;
  - `ImportRun`;
  - `NoteViewShortcuts`, which the Anki add-on runs over HTTP;
  - `UsingASpeech`;
  - `SharedConditions`;
  - `JPCollection`'s `Delete`, `Add`, the reloads and `DiscardTheTextToSpeechAudioOf`;
  - the MCP server's tools for tags, notes, creating, deleting, reloading, rebuilding and shared conditions.
- **Anki sync** changes no note: what Anki holds of a note's cards is in `WhatTheSRSHoldsOfTheStudyNotes`.
