# What taggregates would give JAStudio's notes

Found 2026-11-16 by reading JAStudio's note model and its listeners. This is the case for the Compze changes, not a
design of JAStudio's taggregates.

## What JAStudio does today

- **Every edit serializes the whole note.** Each field holds the note's `NoteGuard`. A `Set` makes the change, then
  `JPNote.Flush()` serializes the note with MemoryPack and hashes it, to learn whether anything changed.
- **If it did, the cache refiles the whole note.** It rebuilds the note's snapshot (for a vocab, `VocabSnapshot`: its
  forms, every prefix of every form, its kanji, readings and stems) to refile it in all nine indexes. The repository
  saves the file, and `JPCollection.NoteUpdated` hands every listener the note.
- **Every listener works out for itself whether it cares,** and redoes everything it does.
- **One listener subscribes to a field:** `PerfectSynonyms`, to two of each vocab's `WritableStringValue`s. The rest of
  the 717,480 field notifiers (`dev_docs/memory-use.md`) have no subscriber.

So only the code that makes a change knows what changed, and whatever has to follow is called from there by hand.

## What listening to exactly what changed would clean up

### Inside a note

- **One part calling another.** `VocabNoteQuestion` calls `_vocab.Forms.OnQuestionChanged()`, and `VocabNote` gives
  the matching configuration a `NoteGuard` of its own (`FlushAfterTheMatchingConfigurationChanged`), only so that
  `VocabNoteForms` can drop its caches. With tevents, `VocabNoteForms` subscribes to the question's and the matching
  configuration's tevents itself, and neither knows it exists.
- **A dependency explained in a comment.** `PerfectSynonyms` subscribes to `CorpusAnswer` and to
  `SourceData.Answer`, with a remark that "the answer shown" depends on both. With semantic tevents the tevent types
  declare that dependency: both answer tevents implement one meaning, such as "the answer shown may have changed". One
  subscription then catches any later way of changing the answer too. That's Compze's unification of property-updated
  tevents and domain tevents, as it stands.

### Across the corpus

- **Reparsing after a vocab changes.** Six places call `LocalNoteUpdater.ReparseSentencesForVocab…` after an edit:
  `VocabSentencesReparserViewModel` (for three windows), four MCP tools, and a menu. One subscriber to the tevents that
  change what a vocab matches would make "sentences follow their vocab" true of JAStudio rather than of each editor.
- **Data derived from other notes.** A vocab's cached set of forms no other vocab owns goes stale when another vocab
  changes (see [found along the way](found-along-the-way.md)). A subscription to every vocab's form tevents is the
  natural fix.
- **The UI's listeners.** The parsing cases view compares a sentence's case again after any change to it, a comment
  edit included; it would subscribe to the parsing tevents only. `ChangesThatOutdateNotePages` says that a change to a
  vocab shown in a sentence's breakdown doesn't outdate the page; the page could subscribe to that vocab's tevents.
- **The changes view** (`dev_docs/changes-view.md`) must tell the notes edited by hand from those a rebuild only
  regenerated, which a diff can't. Tevent types can: tevents of generated data implement a meaning of their own.
- **Change tracking.** `NoteGuard`, `NoteFlushGuard`, the hash of the last saved snapshot and `MarkAsPersisted` become
  "the note has tevents not yet committed". The note is still saved as one file.

## How it fits without a tevent store

- **Loading:** a note's stored state is one tevent that implements every property-updated tevent of the note. Restoring
  it runs only the tevent appliers: no tevent handlers, no `TeventStream`, nothing left uncommitted. Loading and editing
  then go through the same appliers, as in Compze's documented pattern `IUserRegistered : IUserEmailPropertyUpdated`.
  Tried with 0.6.0-alpha through `ITaggregate.LoadFromHistory`. It works, but only by setting the version through a
  member marked for Compze's internal use: hence the first change in [what Compze needs](what-compze-needs.md).
- **Saving:** once per publish cascade, after the invariants are asserted, the collection refiles the note, commits its
  tevents, saves the file and dispatches the tevents across the corpus.
- **Inheritance:** `JPNote` is the root, with `VocabNote`, `SentenceNote` and `KanjiNote` below it, which is taggregate
  inheritance. Tags live on `JPNote`, so a subscriber can take the tag tevents of every note, or only of the vocab.
- **Identity:** a note's id is already a Guid. `NoteId` derives from Compze's `EntityId` today, and would derive from
  `TaggregateId`, which derives from `EntityId` too, with the same equality between a `NoteId` and a `VocabId`.
