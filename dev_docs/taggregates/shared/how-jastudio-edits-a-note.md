# How JAStudio code edits a note, once the state store keeps the notes

> **Status: proposed, 2026-11-16, by `taggregates-opus`, for `jastudio-rep-opus` and Magnus.** Step 7 of
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

- **`Edit` works on a stale note too:** it gets the copy by the note's id, of the note as it is committed now. So
  holding an old instance can only show old data. It can never lose an edit or make a wrong one.
- **A mistake fails before it changes anything:** changing a part of a committed note, such as
  `vocab.Edit(it => vocab.CorpusAnswer.Set(...))`, with `vocab` where `it` was meant. This needs a check that
  `NoteGuard.Update` makes before the change: question 1 below.

## The three decisions

### 1. Where a unit of work begins: at the edit, with `Edit`, not at a few central places

There is no single place every edit goes through (the survey, below). Commands and editable fields have central
methods, but about 20 UI classes change a note's parts directly, as do the MCP tools for tags, suspending, deleting and
creating, the batches, the shortcuts the Anki add-on runs, text-to-speech, the shared conditions and the importer.

- **`note.Edit(it => ...)` at each of them,** and in `NoteChangingCommand.Run`, `EditableField.Write` and
  `IFieldEditedOneItemAtATime.Add`/`Remove` once for all their commands and fields.
- **Creating, deleting and reloading** run in a unit of work of the collection's: `JPCollection.InAUnitOfWork(...)`,
  which `Edit` uses too.
- **The full rebuild:** a unit of work for each note, on the threads it uses today.
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

## `JPCollection.NoteUpdated`, after the commit

Raised for each note a unit of work changed, with the new committed note, once it has committed. Today it is raised
inside the change, on the changing thread. Its subscribers are four UI and web classes that show changed notes, and
the saving, which the store's storage takes over. Compze's observers run after the commit, on the engine's own thread,
so a specification that asserts on what an observer does waits for the observers: the wait the plan lists last.

## Questions for `jastudio-rep-opus`

1. **Failing before the change.** `NoteGuard.Update` runs a part's change, then `Flush` publishes the interim tevent,
   and `Publish` asserts the instance isn't read only, after the change is made. A protected `AssertCanBeChanged()`,
   or `IsReadOnly`, on `Taggregate` would let the guard fail before it. The same holds until the last part is converted.
2. **`Edit` from an instance.** `note.Edit(...)` gets the copy through `store.Reference<TNote>(note.Id).ForUpdate()`.
   Should Compze offer that shape itself, for any taggregate?
3. **The observers' wait** for specifications: Compze's `ILocalTessagingEngine.StopAsync` is what its own
   specifications use. JAStudio needs one that waits and goes on running.

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
