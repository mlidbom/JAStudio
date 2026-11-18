# Migrating the notes to Compze's state store

> **Status: started 2026-11-16.** Kept by `taggregates-opus`, which does the migration in JAStudio, with
> `jastudio-rep-opus`, which builds the state store as a spike in Compze. The plan as proposed to `jastudio-rep-opus`,
> and where it stands. It changes as the work does.

**The goal (Magnus, 2026-11-16):** all of JAStudio's notes kept in Compze's state store, every change to a note a
tevent, to the point where everything the sessions can test themselves works. Magnus looks once the basic structure is
there, such as how a note's tevent interface hierarchy is built, before the rest is built on it. Done when Magnus,
`taggregates-opus` and `jastudio-rep-opus` all sign off.

**Where:** JAStudio's worktree `C:\Dev\JAStudio.wt2`, branch `migrating-the-notes-to-the-state-store`, from `main`.
Ideas tried more than one way get branches of their own, kept as a record. Compze's side: branch
`taggregate-store-spike`, in Compze's main checkout. What Magnus decided, which the migration codes against, is in
Compze's `dev_docs/taggregate-improvements/shared/README.md`, "What Magnus decides", and `the-taggregate-store.md`, "The
programming model".

## How JAStudio builds against the spike

**Project references to the spike's projects, instead of Compze's packages, on the migration branch only.** A change
in Compze is then in JAStudio's next build, with no packing, and no locally built package reaches the global NuGet
cache.

- **Today:** central package management with transitive pinning, in `src/src_dotnet/Directory.Packages.props`.
  Sixteen projects reference fourteen Compze packages, and three more are pinned because they come through the others.
  Each pinned version is the version in Compze's source, so the switch brings no unreleased changes besides the spike's.
- **The switch, in one file:** `src/src_dotnet/Directory.Build.targets` turns each Compze package reference into a
  reference to the project of the same name, under a folder given by a property. None of the sixteen project files
  changes, and switching back is deleting that block. Every Compze package must switch at once: a project built from
  source beside the same library from a package gives two copies of one assembly.
- **The folder:** a Compze worktree of JAStudio's own, `C:\Dev\Compze.worktrees\taggregate-store-spike-for-jastudio`,
  created by `jastudio-rep-opus` and moved only to commits it names, never committed in. Building straight from
  Compze's main checkout would build in the folders `jastudio-rep-opus` builds in, at the same time, and would build its
  unfinished edits.
- **`JAStudio.slnx` lists the Compze projects too,** 21 of them, Teventive and Tessaging among them. Compze's
  FlexRef (`build/FlexRef.props` there) turns a Compze project's reference to another into a package reference when the
  solution being built doesn't list that other project, and the build then fails looking for the package. Listed, they
  are built from source, and Rider navigates into their source. A new Compze project the migration needs is added to the
  list too: a missing one fails the restore with NU1101, never silently.
- **The central pins of Compze's packages are removed** on this branch, in the same block: a pin collides with the
  project of the same name (NU1109).
- **Checked, 2026-11-16:** at Compze's `e29fd6673`, which is `main`, JAStudio builds clean and all 2,823 of its .NET
  specifications pass.
- **The same SDK** in both repositories' `global.json`, 11.0.100, and both are `net11.0`.
- **Merged into `main` building Compze from the worktree** (Magnus, 2026-11-17: "We can live without CI. Turn the CI auto
  build off for now. There are no other contributors and no other users. Building here is plenty during this major
  refactoring."), so that the changes to the corpus that follow the migration are made on `main`, in one line of the
  corpus's history. CI runs only by hand meanwhile (`.github/workflows/ci.yml`), since its machines have no Compze
  worktree. Once Compze publishes the store's packages, on Magnus's approval, JAStudio goes back to package references,
  and CI's triggers come back.

## Where it stands, 2026-11-17

- **Compze:** steps 2a and 3 are in.
  - Step 2a is the in-memory state store, with three additions JAStudio asked for:
    - a factory that takes a container service: `store.Taggregate((NoteServices services) => new VocabNote(services))`;
    - reloading while running: `IStateStore.ReloadFromStorage()`, which keeps every reference and reports the
      taggregates added, changed and removed;
    - a comparison of snapshots per class, `snapshotsAreEqual:`, for that report.
  - Step 3 is tevent routing declared once per class.

  JAStudio's worktree of Compze is at `eb7470c4a`, after the end review's fixes and Magnus's answers to its questions.
  Six changes reached JAStudio, and one change of terms: observers created through `ITeventObservation.ObserveTevents`
  as the application runs are *dynamic observers*, and those declared at composition *composed observers*
  (`dev_docs/taggregate-improvements/shared/dynamic-and-composed-observers.md`, in Compze).
  - **A note class is declared with a registration of the container** (end review Q3):
    `store.Taggregate<VocabNote, VocabDto>(TrackedTransient.For<VocabNote>().CreatedBy((NoteServices services) => ...), ...)`.
  - **Nothing is declared for the dynamic observers** (Q4): a plain container's dynamic observers may observe any tevent,
    so `DeclareWhatIsObservedAtRuntime` is gone.
  - **A new taggregate is saved only with the tevents it published from its creation on** (D5), so every note's creation
    is a tevent observers hear. An import run and a vocab's clone had created their notes by restoring them from a DTO;
    they now publish creation tevents carrying the note's starting state, `SentenceTevent.Imported` and
    `VocabTevent.Cloned` (Magnus, 2026-11-18: "This is what semantic tevents is all about").
  - **The files of a state store** (Q5, `dev_docs/taggregate-improvements/shared/the-files-of-a-state-store.md`, in
    Compze): a taggregate's file is `<its class's folder>/<first two hex digits of its id>/<id>.json`, JAStudio's own
    layout, with no `fileOf` to declare; a reload compares a file's text with what the store would write, so there is
    no `snapshotsAreEqual`, and `CorpusNoteDto.HoldTheSameState` is gone; and `IStateStore.WriteEverythingAnew` writes
    every note's file anew as one commit, which `save_every_note` and the menu item "Write every note's file anew" use,
    in place of `CorpusNoteFiles.Write`. On the real corpus it rewrote all 79,896 files, none of them changed, in 74 s.
  - **Tevent routing is a protected property** (Q9): each of JAStudio's 17 teventive classes gives its routing as
    `protected override TeventRouting TeventAppliers`, not through the explicit `ITeventRouted`.
  - **Ten trims of the public API** (Q10, `dev_docs/taggregate-improvements/shared/ten-trims-of-the-public-api.md`, in
    Compze): a unit of work adds a new note with `Add`, an index has no `HasAny`, and the one reload is
    `ReloadFromStorage(changeToTheStorage)`, whose added, changed and removed taggregates are collections in no order,
    as `ICollectionReloaded`'s now are. A reference only reads: `ITaggregateReference.ForUpdate` is gone, and so is
    JAStudio's `NoteReference.ForUpdate`, which only a specification called. The note for update comes from the unit of
    work, `NoteUnitOfWork.ForUpdate`.

  Before it, `628f05c1e`, the end of step 5's code
  (`dev_docs/taggregate-improvements/shared/step-5-the-rest-of-the-teventives.md`, on the `taggregate-improvements`
  branch): every teventive class must declare its tevent routing, and every component asserts, when it's constructed,
  that its parent's routing has an entry for its tevents. JAStudio needed no change for it. Before it, `37731c854`,
  step 5's second increment: a class's tevent routing is typed on its tevent root, such as
  `TeventRouting<JPNote, INoteTevent>`, and its first one comes from its Compze base class's `DeclareAppliers`. Before
  it, `c5c910e06`, where the storage seam is
  internal: JAStudio uses none of it, its
  specifications keeping the JSON store's files in memory (`KeptInMemory`, `77c3cdaf8`). Before it, `962a05221`, where the
  JSON store starts from a MemoryPack cache of its own
  package, `Compze.Teventive.StateStore.MemoryPack` (`CachedWithMemoryPack`). Before it, `4ccdfb6b7`, where Compze's JSON
  store (`StoredInFiles`), which JAStudio now keeps its notes in, reads a class's folder anew at each load: a store that started on an empty folder lost every taggregate at
  its next reload (found by `taggregates-opus`). Before it, `d248ebd66`, where only Compze's own file storage hands the
  store a restored taggregate: a storage that kept the snapshots it was given handed back live instances (also found by
  `taggregates-opus`). Before it, `3ff814272`, with observers created at runtime (`ITeventObservation`), each unit
  of work's tevents observed together (`ForEachUnitOfWork`), what may be observed at runtime declared at composition
  (`AtRuntime`), and the JSON store. Before it, `8695d4608`, with the first two. Before it, `5e9f293e4`, where deletion is a tevent: a taggregate that publishes an
  `ITaggregateDeletedTevent` is gone from its unit of work at once, and from the store when it commits, and the updater's
  `Delete` is gone (Magnus, item 42). Before it, `3df5ee7f8`, where the engine's await is `AwaitNoTessagesInFlightAsync`
  (Magnus, item 43). Before it, `53d24b46c`, where an index keeps no record of each taggregate's keys (step 12, below).
  Before it, `f0ed1ccf6`, where every taggregate of a class comes from a bulk reader,
  `ITaggregateStoreBulkReader`, and neither it nor an index lookup gives an order (Magnus, items 34, 36 and 37): JAStudio's
  `NoteIndex` sorts lookups by id. Before it, `63a5dcce5`, whose index lookups gave id order, and `21012570a`, after step 2b, the tevent store joining the shared `ITaggregateStoreUpdater`
  and `ITaggregateStoreReader`: the three additions JAStudio's switch asked for, specified under "Three gaps for JAStudio's
  switch, specified" in Compze's `the-taggregate-store.md`.
  - a storage built with a container service, `store.StoredIn((ITaggregateStorage storage) => storage)`, resolved at the
    store's first use;
  - every taggregate of a class, then `IStateStore.All<TTaggregate>()`, now the bulk reader's `All`, inside a unit of work
    as it has changed them;
  - a reload that runs a change to the storage first, `IStateStore.ReloadFromStorageAfter(change)`, while every unit of
    work waits.
- **JAStudio:** steps 1 to 6 below are done, and so are step 7's increments 1 to 4: **the state store keeps the notes**,
  every edit runs in a unit of work on the note's copy, and the holders hold references. Each passed the whole suite,
  with `full-build.ps1` clean, and is pushed. How it was built is in [how JAStudio code edits a note](how-jastudio-edits-a-note.md),
  "The order it is built in", step 4. The copies of all 79,896 notes lose nothing. Neither a full rebuild nor saving every note changes a file of the
  corpus. Magnus has looked at step 6, and it is revised by his word:
  [the-structure-of-the-notes-tevents.md](the-structure-of-the-notes-tevents.md). He also decided that text fields
  become plain strings, which changes step 10: [what-the-notes-hold.md](what-the-notes-hold.md).
- **Steps 8, 9 and 13 came with the switch:** every edit site has its unit of work, the caches' indexes are the store's,
  the kanji and the sentences are in the store too, and each unit of work writes its notes' files as all or nothing
  (`CorpusNoteFiles`). `FileSystemNoteRepository`, which wrote the same files, is gone (`b00768751`), its specifications
  rewritten against `CorpusNoteFiles`.
- **Step 10 is done,** as [below](#step-10-as-it-is-built): every part of the three notes publishes tevents of its
  own, and the interim tevent, `NoteGuard` and the flush's hash are gone.
- **Step 11 is done,** as [below](#step-11-as-it-is-built): what a change to one note does to other notes is a
  participation handler of the change's tevent, in the unit of work that made it. Magnus looks at the structure at the
  end, rather than holding the work up (2026-11-17: "I'd rather refactor at the end than hold everything up with you
  waiting for me. The events are not stored so the choices can be changed.").
- **Step 12 is done,** as [below](#step-12-as-measured): against `main`, the branch takes about 15% less memory, and
  about 1.7 seconds longer to load, since Compze's index keeps no keys (`53d24b46c`); before, 2% less and 2.4 s longer.
- **Every step of [the order](#the-order) is done,** 13 and the last with the switch.
- **Merged into `main`, and running in Anki** (2026-11-17). Magnus, after starting it: "Everything seems to be working
  perfectly." What is left is the end: Magnus's decisions, the spike's review in Compze, the release, and his end
  refactoring of the structure.
- **Compze's types are in JAStudio's Python stubs** since the notes derive from `Taggregate`. After each Compze step,
  `src/src_dotnet/regenerate-stubs.ps1 -OnlyTargetTypes -RefreshCompzeStubs` regenerates them: `full-build.ps1`
  regenerates JAStudio's alone.

## Step 7, as it will be built

Step 7 moves the notes into the store with every part publishing the interim tevent. It is cut into increments that
each build and pass:

1. **Done (`054451d5f`, and `f57e5c518` for step 3's routing). The notes are snapshotable taggregates, still kept by
   JAStudio's collection.**
   - `JPNote` derives from Compze's `Taggregate`, and each note class implements `ISnapshotable<TNote, TDto>` with
     its DTO. `RestoreFrom` calls `RestoreIdentity`, then rebuilds the parts from the DTO.
   - Each note class has three constructors:
     - an internal empty one, for the store's factory;
     - one for a new note, which publishes its `Created` tevent;
     - one that restores a stored note, through `RestoreFrom`.
   - A kanji and a sentence have tevent roots of their own, `IKanjiTevent` and `ISentenceTevent`, with `Created`
     only. A kanji's `Created` carries its question, which never changes.
   - `JPNote.Flush`, which every guarded change calls, publishes the interim tevent,
     `INoteTevent.PartWithoutTeventsChanged`, when the note's stored data changed.
   - Loading builds every part twice: empty, then from the DTO. Loading the corpus still takes 6.4 s.
   - Until increment 3, nothing commits the notes, so the tevents each note publishes pile up on it, one per edit.
   - An imported sentence and a cloned vocab are made from a DTO, so they publish no `Created`. Nothing needs them
     to yet. Increment 4 decides how a new note enters the store.
2. **Done (`206ee4ddd`). JAStudio's storage behind `ITaggregateStorage`: `Storage/CorpusNoteFiles`.**
   - `LoadAll` gives each note's DTO, through the binary snapshot cache as before.
   - `Prepare` writes each changed note's file under a temporary name beside it, `<id>.being-written`, and `Commit`
     renames it into place. A deletion waits for `Commit`.
   - `JsonFilesystemObjectRepository.Save` is the same two steps, committed at once, so the files stay byte for byte
     as they are: saving every note changes none.
   - **The cost:** saving every note takes about 50 s instead of 12, about 0.6 ms a file against 0.15. An edit
     writes one file.
   - Nothing uses it yet.
3. **Done. Increments 3 and 4 are one step:** once the store keeps the notes, its committed instances are read only, so
   every edit outside a unit of work fails. They are built in the order of
   [how JAStudio code edits a note](how-jastudio-edits-a-note.md), "The order it is built in", where the progress is.
   **The store keeps the notes.** Composed with the three note classes, each with its factory and a comparison of
   serialized DTOs. The caches' indexes become the store's indexes, each a function of a note's state, as today's
   `keysOf(snapshot)` already is. Loading and reloading go through the store.
4. **Done. Every edit runs in a unit of work, on the note's copy.** The UI, the MCP server's tools, the importers, the batches
   (the full rebuild, a unit of work per note) and the specifications. Whatever holds a note across edits holds its
   reference: about twelve classes outside the notes, and 92 specification files that hold a note in a field.
   `JPCollection.NoteUpdated` is raised once a unit of work has committed.

### Settled before the store keeps the notes

[How JAStudio code edits a note](how-jastudio-edits-a-note.md): agreed by Magnus and `jastudio-rep-opus`, 2026-11-16.
It decides how every edit in JAStudio is written, and the order increments 3 and 4 are built in.

## Step 10, as it is built

Each part converted publishes tevents of its own instead of the interim one, as
[the structure of the notes' tevents](the-structure-of-the-notes-tevents.md) has them, and becomes what
[what the notes hold](what-the-notes-hold.md) says. What reaches other notes stays a call from the domain method,
marked `wip:`, until step 11 makes it a participation handler. Each increment ends with a full rebuild and saving every
note in the headless JAStudio, neither changing a file, and the copies checked.

1. **Done (`f69c53948`). The vocab's six text fields are plain strings:** `CorpusAnswer`, `Explanation`,
   `ExplanationLong`, `Mnemonic`, `TechnicalNotes` and `References`. Each has a `Set` method that trims and publishes
   its `PropertyUpdated` tevent only when the value changes (`JPNote.PublishIfChanged`), and an applier sets it.
   `SetCorpusAnswer` pushes the answer to the perfect synonyms itself. The call sites were fixed, position by position,
   from the build's errors, by a script that the next three increments used again.
2. **Done (`48f1b25b4`). The vocab's source data:** its `Answer`, `Mnemonic` and `ReadingMnemonic`, under a group of
   their own, `IVocabTevent.SourceData`, since the source data holds more than one property. `VocabSourceData` is the
   first part that is a Compze component, `TeventiveComponent<VocabNote, ...>`, with appliers of its own, which the
   vocab's routing reaches with `.Component(vocab => vocab.SourceData)`. A part publishes through
   `JPNote.PublishIfChanged`. Changing the answer pushes it to the perfect synonyms as the corpus answer does, and
   `PerfectSynonyms` subscribes to nothing any more. The tevent classes are `SourceAnswerUpdated` and the like: a nested
   `MnemonicUpdated` hid the vocab's own.
3. **Done (`d9c5362c2`). The kanji's text fields and source data,** with `IKanjiTevent`'s `PropertyUpdated` tevents, and
   `KanjiSourceData` a component as the vocab's is.
4. **Done (`57df9e5b2`). The sentence's text fields and source data,** with `ISentenceTevent`'s, and
   `SentenceSourceData` a component. A new sentence's `Created` tevent carries its source question, as a vocab's carries
   its question. `SentenceQuestionField` reads and writes the sentence itself. `WritableStringValue` and its interfaces
   are gone: every text field of the notes is a plain string.
5. **Done (through `8fc76fcca`). The other simple parts,** one or a few to an increment, in the order of [what the notes hold](what-the-notes-hold.md):
   the sets of words and kanji, the media lists, the register and the flags, the readings, the tags.
   - **Done (`3d4b9b15b`):** the vocab's readings, a list property at its root (`JPNote.PublishIfTheListChanged`), and
     `VocabNoteMetaData` and `VocabNoteUsuallyWrittenInKana` as components. `JPNote.PublishIfChanged` is generic.
   - **Done (`2d16944dd`):** a sentence's parsing result and parsing case. `ParsingResult` and `ParsedMatch` are values,
     so a reparse that comes to the same result publishes nothing, and the full rebuild writes only what changed.
   - **Done (`2ebfc62ce`):** the vocab's register and the flags and rules of its matching configuration.
     `VocabNoteRegister` and `VocabNoteMatchingConfiguration` are components. A `FlagField` is a view of its kind's
     flags: `FlagFieldList` holds the names of those set, and a change publishes the whole new set, as the corpus
     stores it. Each of the two require/forbid rules publishes its own tevent, and the yield-to-surface rule its
     surfaces. Every tevent of the matching configuration also drops what the forms derived from it, a route of its
     own, which Compze's routing runs after the applier: it calls every route that matches, in declaration order. The
     match-condition still changes through the guard.
   - **Done (`caf8388bd`):** the sets of words and kanji. A vocab's `RelatedVocab` is a component: each of its
     relationships publishes the whole new set of words (`JPNote.PublishIfTheSetChanged`), and `DerivedFrom` its new
     word. A kanji's similar-meaning and confused-with kanji publish at the kanji's root. The other side of a mutual
     relationship still changes in an edit of the other note.
   - **Done (`d8856b46f`):** a kanji's radicals and primary vocab, ordered lists at the kanji's root.
   - **Done (`5ad520b73`):** the nine media lists, the audio, images and screenshots of the notes and of their source
     data. `MediaReferenceList` takes its note and its tevent instead of a guard, and the source data components no
     longer take the guard at all.
   - **Done (`c3f6c0fb8`):** a vocab's parts of speech, one list at its root, and `VocabNoteCompoundParts`, a component:
     the primary parts a list, the entered alternatives a set.
   - **Done (`29859c50d`):** every note's tags, one set at its root, `INoteTevent.PropertyUpdated.Tags`, which the appliers
     every note shares apply. The note classes' own `PropertyUpdated` levels are declared `new`.
   - **Done (`8fc76fcca`):** a kanji's readings. `KanjiNoteReadings` is a component, with a tevent for each kind of
     reading, and each list keeps its invariants in the methods that compute its new list.
6. **Done (through `1890701d8`). The parts with real behavior.** The perfect synonyms
   and the mutual relationships were done in the fifth increment, with the sets of words and kanji.
   - **Done (`8be0b6f5a`):** a vocab's forms and question, both components. Every change to the entered forms publishes
     the whole new list, and adding or removing a form also says which. Setting the question's text or disambiguation
     publishes `Question.Renamed`, with the old name; a new text, applied, drops the forms' caches, and the rename then
     drops the new question from the entered forms, a tevent of the forms of its own.
   - **Done (`e9f9c2256`):** a vocab's match-condition, `MatchingConfiguration.PropertyUpdated.MatchCondition`. Renaming
     a shared-condition a vocab uses publishes `MatchingConfiguration.SharedConditionRenamed`, which sets nothing: the
     vocab didn't change, but the corpus stores the shared-condition by its new name, so the vocab is written again. It
     replaces `JPNote.PublishThatWhatItStoresChanged`.
   - **Done (`b2cba9e2f`):** a sentence's configuration. `CachingSentenceConfigurationField`, which cached nothing, is
     `SentenceNoteConfiguration`, a component: its highlighted words, a list, and its incorrect and hidden matches,
     sets, each publish their whole new value, and only a change reparses the sentence.
   - **Done (`1890701d8`): the interim tevent is gone,** `INoteTevent.PartWithoutTeventsChanged`, with `NoteGuard`, `JPNote.Flush`
     and the hash of the note's stored data it compared, 16 bytes a note. A note is written when it published a tevent,
     and each part publishes only what changed.
   - **What reaches other notes is still a call from the domain method,** marked `wip:` for step 11: the mirrored forms,
     the renamed word-references, the perfect synonyms' answer and the other side of the mutual relationships.

## Step 11, as it is built

What a change to one note does to other notes leaves the domain method, into Compze participation handlers.

- **When a handler runs:** Compze runs it synchronously, inside the unit of work that made the change, as the note ends
  publishing the tevent it handles (Compze's `b5d9ae9ed`): inside the domain method's publish, before the method goes on.
  Not at the commit, as Compze's design first had it. So what a handler changes commits or rolls back with the change,
  and an exception it throws comes out of the domain call.
- **Where they are declared:** each by the part whose concept it is, as `PerfectSynonyms.ShareTheAnswerThrough`, given
  Compze's `TessageBusHandlerRegistrar`. `AppBootStrapper.DeclareWhatReachesOtherNotes` lists them, as it lists the
  indexes.
- **How a handler gets its notes:** the note that published, through the running unit of work,
  `NoteUnitOfWork.NoteThatPublished<TNote>(tevent)`, which is that unit of work's copy. The notes it changes, by `Edit`,
  which joins the running unit of work. So they are notified once it has committed, as `JPCollection.NoteUpdated`.
  `NoteUnitsOfWork` keeps the running one in an `AsyncLocal` until the body of the unit of work returns, which is late
  enough, since every handler runs inside the body.
- **What every part is specified by:** that a unit of work that fails after the change leaves the other notes as they
  were, and, checked by leaving the handlers unregistered, specifications that fail without them. Each was checked in
  the headless JAStudio as an edit through the MCP server, and undone by the reverse edit, every file back as committed.
- **Done (`ac8c88336`): the perfect synonyms' answer.** Handlers of `IVocabTevent.PropertyUpdated.CorpusAnswer` and
  `IVocabTevent.SourceData.PropertyUpdated.Answer` give each perfect synonym the answer the vocab then shows.
  `SetCorpusAnswer` and `SourceData.SetAnswer` only publish.
- **Done (`b8c32f7c5`): renaming word-references,** a handler of `Question.Renamed`, which takes both names from the
  tevent. The rename gets for update only the notes that hold the old name (`VocabNote.HoldsTheWordReference`,
  `RelatedVocab.HoldsTheWord`, `KanjiPrimaryVocab.Contains`), where it copied and locked every vocab and kanji.
- **Done (`9b35b394e`): the mirrored forms,** handlers of `Forms.PropertyUpdated.EnteredForms.Added` and `.Removed`. The
  other vocab's change is an `Add` or a `Remove` too, so it mirrors in turn, as before: a form is a text, so the vocab
  whose question it is are every vocab with that text.
- **Done (`2040ea4c1`, `d2491b032`): the other side of the mutual relationships,** a vocab's ergative twins, synonyms,
  antonyms and see-also, and a kanji's similar meaning and confused-with. Each relationship's tevent has four beneath it
  that name the word, each with the whole new set: `Added` and `Removed` for an edit of the note, whose handlers mirror
  it in the other note, and `AddedAsTheMirrorOf` and `RemovedAsTheMirrorOf` for that mirror, which no handler follows.
  `jastudio-rep-opus` reviewed the shape and named the mirror's tevents: a tevent says what happened, not who caused it,
  and the guard against mirroring back is in the type and the method. A vocab's mirror needs the guard: the word is a
  text, so mirroring back would resolve the vocab's name to vocab other than the one first named, and add a word no one
  gave. `MutualWordRelationshipTevents` and `MutualKanjiRelationshipTevents` hold each relationship's factories, once
  per relationship.
- **Done (`b2e491cd5`): the perfect synonyms' web.** Its tevent has `Added` and `Removed`. The domain methods change
  only the vocab; the handler of `Added` connects the whole web and gives it the vocab's answer, and the handler of
  `Removed` takes the word's vocab out of the web, every vocab in it keeping the word as a synonym. The web's own changes
  publish only the whole set, so they set off no handler of the web's.
- **One change in what JAStudio does:** adding what a note has already, such as a form typed into the forms editor that
  the vocab has, no longer mirrors anything. Before, it added the note to the other side when the other side lacked it.
- **Left as direct calls, on purpose:**
  - `Synonyms.AddTransitivelyOneLevel` and the collection's own operations, such as `SharedConditions.Rename` and
    `JPCollection.Delete`: operations meant to change several notes, each note changed by `Edit` in the one unit of work.
    A handler would react to nothing that happened to one note.
  - `PerfectSynonyms.LeaveTheWebAsTheVocabIsDeleted`, which `JPCollection.Delete` calls: deleting a note publishes no
    tevent.
- **What can't be undone is after the commit already:** `JPCollection.Delete` deletes the study-note and the media files
  in `NoteUnitOfWork.OnceCommitted`. Suspending a note's cards tells Anki inside an edit, in the workspace's bulk action,
  but it changes no note, so its unit of work has nothing to roll back.

## Step 12, as measured

Measured 2026-11-17: the branch at `237b5bca6` against `main` at `59cd15125`, whose code is the branch's base,
`4d12d9122`. Both run in the headless JAStudio as Debug builds, on this worktree's corpus checkout at `3949c927f`, which
both record, with the same add-on folder and Anki profile. Both write the same snapshot caches (`snapshot.bin`), byte for
byte, so a run of one loads what the other wrote.

- **At Compze's `f0ed1ccf6`, loading took the branch about 2.4 seconds longer.** `dev_scripts/compare-load-times.ps1` alternates the two, run by
  run. Other processes used under 8 processor-seconds in any run, on 32 hardware threads:

  | | main | the branch |
  |---|---|---|
  | Populating JAStudio's data, warm, from the snapshot caches: median of 5 | 2.73 s (2.66–2.83) | 5.08 s (4.90–5.31) |
  | The same, cold, from every note's file: 2 runs | 6.3 and 6.6 s | 9.0 and 9.3 s |
  | Until the MCP server listens, warm | 3.6 s | 6.0 s |
  | The process's processor time until then, warm | about 14.5 s | about 21 s |

  The session's very first cold run took `main` 19.6 s: the notes' files weren't in the operating system's file cache
  yet. The cold runs above found them there, as any start after an earlier one does.
- **Where the branch's time goes** (dotTrace, a warm start): Compze's `TaggregateStateStore.LoadEverything` takes 4.2 s,
  all of it on one thread:
  - filing JAStudio's indexes, one after another, `TaggregateIndex.FileExactly`: 2.8 s. Of it, grouping each index's
    taggregates by key into its `ConcurrentDictionary` takes 1.1 s, and making each taggregate's keys an
    `ImmutableHashSet` 0.8 s;
  - restoring the 79,896 taggregates from their snapshots, one after another: 0.75 s;
  - reading the snapshot caches: 0.5 s.

  `main` reads the caches in 0.9 s, then builds the vocab's and the sentences' caches at the same time, in 1.1 and 1.3 s.
- **Memory, at `f0ed1ccf6`: 2% less,** as [memory-use.md](../../memory-use.md#on-the-state-store-against-main-2026-11-17) details: after
  the most thorough collection, 850 MB on the managed heap against 878 MB, and a working set of 941 MB against 958 MB.
  What the text fields' notifiers and `main`'s index sets took is gone, and the state store's indexes take about as
  much. Their largest part, about 105 MB, is each taggregate's keys in each index, kept as an `ImmutableHashSet`.
- **Then the index keeps no keys at all** (`jastudio-rep-opus`, Compze's `53d24b46c`, on Magnus's yes). When a unit of
  work prepares its commit, the index computes a changed taggregate's old keys from its committed instance, which stays
  the old one until the commit, and its new keys from the unit of work's copy, and files by the difference. It asserts
  that the taggregate is filed under each old key, so a key function that reads anything but its note's own state fails
  the unit of work: none of JAStudio's does. Measured again, the same way, twice:

  | | main | the branch, before | the branch, now |
  |---|---|---|---|
  | Populating JAStudio's data, warm: median of 5, twice | 2.63 and 2.67 s | 5.08 s | 4.38 and 4.31 s |
  | The same, cold: the second run, twice | 7.1 and 6.2 s | 9.3 s | 9.1 and 7.8 s |
  | The process's processor time until the MCP server listens, warm | about 14.5 s | about 21 s | about 19 s |
  | The managed heap after the most thorough collection | 878 MB | 850 MB | 730 MB |
  | The process's working set then | 958 MB | 941 MB | 818 MB |

  So the branch loads about 1.7 s slower than `main`, and takes about 15% less memory. Another session kept one of the
  32 hardware threads busy all through both sets, alternating over both builds. The first cold run of each set was
  disturbed, `main`'s, after every note's file had just been written: 40 and 48 processor-seconds by other processes.
  Filing the indexes and restoring the taggregates in parallel would take off most of the rest, at the price of running
  JAStudio's key functions on several threads at once: optional, and not recommended unless Magnus wants the load time
  down further.
  - **The headless checks on `53d24b46c`:** a full rebuild, saving every note and copying all 79,896 notes change
    nothing, and edits that change a vocab's index keys and those of the sentences it matches, reversed, leave every
    file as committed.
- **Found while measuring: half the runs of `JAStudio.UI.Specifications` ended in a fatal error,** which `main`'s don't
  (`38e9cae9d`). Since step 4 moved a note's Anki tags out of it, a search reads them through the container, and a
  search the search editor started, superseded by a later one, read on after its specification had disposed the
  container. The specifications now wait for the work they started before disposing the app.

## After the order: what follows changes observes tevents

Decided by Magnus, 2026-11-17, on item 21: `JPCollection.NoteUpdated`, a plain observable raised after the outermost unit
of work commits, goes. "That NoteUpdated should most likely just disappear and the actual code that cares about changes
does `.For<INoteTevent>(it => ...)` if it truly wants absolutely any change. If it cares about something more specific,
then it should listen for that. If that tevent interface does not exist, we are probably missing an interface." And on
building messaging paths of JAStudio's own: "A subscriber that publishes something new, with a well designed inheritence
hierarchy lets you stay on the bus and use a consistent model and have only one place you need to think about subscribing
to anything that happens".

- **The publishing side, built:**
  1. **Done. A deleted note is a tevent:** `INoteTevent.Deleted : ITaggregateDeletedTevent`, and one per note type, like
     their `Created`. The note's own `Delete` publishes it, and that alone deletes it, on Magnus's word (item 42, Compze's
     `5e9f293e4`): the note is gone from its unit of work at once, and from the store when it commits. Leaving the web of
     perfect synonyms is a participation handler of `IVocabTevent.Deleted`, which carries the vocab's name, since the
     vocab can no longer be read by then (`291ea305a`).
  2. **Done, then taken out: one summary tevent per note per unit of work,** `INoteChangedInAUnitOfWork` (`60284d57e`).
     Magnus approved Compze's own instead: `observe.ForEachUnitOfWork<INoteTevent>(tevents => ...)`, called once for each
     committed unit of work that published any matching tevents, in publish order (`jastudio-rep-opus`, 2026-11-17). What
     follows changes to notes is built on it.
  3. **Done. What isn't a note's tevent, published on its own** through `IIndependentTeventPublisher` (`b2e012aa9`):
     `ICollectionReloaded`, with the notes a reload added, changed and removed; `IStudyNoteChanged { NoteId }`, when what
     Anki holds of a note's study-note changes; and `IConfigurationChanged`, which replaces `Config.Changed`. The
     independent publisher refuses to run inside a unit of work, which found the search results suspending cards in an
     edit of each note (`c18ec301f`).
- **Specifications see the tevents** as the test harness's observer records them (`TeventsObserved`, `d8eddcfee`).
- **Done. The observing side:** what follows changes observes the tevents, and `JPCollection`'s `NoteUpdated`,
  `NoteDeleted`, `StudyNoteChanged` and `ReloadedFromBackend`, and `JapaneseConfig.Changed`, are gone.
  - **Dynamic observers,** created through Compze's `ITeventObservation.ObserveTevents`, which Magnus approved, observation only: a
    note's tab, the search results, the parsing cases, the note list, the workspace showing its saved state once the
    notes have loaded, an editor window closing at a reload, and a card page in Anki (`ChangesThatOutdateNotePages`). The
    live MCP server started once the collection had loaded, until it started with JAStudio (2026-11-17). Nothing is declared for them: until Compze's `0255c7231`, what
    they could observe was declared at composition (`DeclareWhatIsObservedAtRuntime`).
  - **The view models observe on the UI thread** through `TeventsObservedOnTheUIThread` (Magnus's idea), over
    `IUIThreadDispatcher`: each call is posted there, and none runs once it is disposed, since Compze's disposal doesn't
    wait for a call already taken. A note's changes come as each unit of work's `INoteTevent`s, which also tell of its
    deletions (`INoteTevent.Deleted`).
  - **Composed observers:** the log of the notes saved (`SavedNotesLog`), which `NoteUnitOfWork` no longer tracks the notes
    for, and Anki's windows drawing again after a load, which Anki's environment declares.
  - **`Settings` reads the configuration as it is,** rather than caching it until a change is announced: the cache only
    saved a property read, and an observer of the change, off-thread, would leave it stale while a page is made anew.
  - **The specifications** read the workspace and the view models they hold once every tevent published has been
    observed (`SpecificationUsingAWorkspace.OnceCaughtUp`), and the specifications' `NoUIThreadDispatcher` runs what is
    posted one at a time, as a UI thread does, since observers post from threads of their own.
  - **What changed in what the user sees:** a view now follows a change once it has committed and been observed, a moment
    later, rather than on the thread that made it. A reload whose change to the corpus checkout fails publishes no
    `ICollectionReloaded`, where `ReloadedFromBackend` was raised anyway; so the live MCP server no longer starts after a
    first load that fails.
- **Done. Compze's JSON store keeps the notes' files** (Magnus's approval, through `jastudio-rep-opus`): the state store
  is `StoredInFiles` the corpus checkout, as `CorpusNoteFiles.DeclareIn` declares it, a folder for each class of note
  and each note's file where it always was. JAStudio's own storage, `JsonFilesystemObjectRepository` and its
  `snapshot.bin` caches, is gone.
  - **Each environment declares where the notes are kept** (`IEnvironmentSpecificDependenciesRegistrar.DeclareWhereTheNotesAreKept`):
    the hosts in the corpus checkout's files. The specifications keep the same files in memory (`NotesKept.InMemory`,
    Compze's `KeptInMemory`, `77c3cdaf8`), composed as the hosts compose theirs, MemoryPack cache included, and read
    and change them through Compze's `ITaggregateFilesInMemory` (`SpecificationUsingACollection.TheStoredFileOf` and
    `WriteTheFileOf`); on the disk only where a specification needs them there (`NotesKept.InTheirFiles`): the corpus
    repository's MCP specifications, which run git on them. `NotesStoredInMemory`, JAStudio's own storage for the
    specifications, is gone, and with it JAStudio's last use of Compze's storage seam (`ITaggregateStorage` and the
    rest). Before the files were kept in memory, with every specification on files, the suite took 149 to 186 s rather than 56 to 59 s:
    the 74 specifications that start with the specifications' collection, 1,035 notes, each added in a unit of work of
    its own, wrote about 5,000 files each, Compze making about five file operations of each note's commit. The other
    2,400 or so start empty and create the few notes they need.
  - **Done. The specifications load their mini corpus as the application loads the corpus** (Magnus, 2026-11-17: "The
    tests should load their mini corpus the same way as the real app if possible. I definitely agree").
    `TheSpecificationsCorpus` is made once per test process for each `DataNeeded`, by a JAStudio that creates the notes:
    the parsing cases' corpus's vocab, loaded from its files as below, and the sample kanji and sentences. Its files, the
    notes' and the shared-conditions', are then written into each specification's JAStudio, and the collection is loaded
    again, as a start loads it, rather than adding 1,035 notes in a unit of work each. The parsing cases' JAStudio loads
    the parsing cases' corpus's vocab the same way (`CorpusFolder.AddTheVocabTo`).
  - **Writing every note, changed or not** (`save_every_note`, and the workspace's "Write file system repository"), is
    `CorpusNoteFiles.Write`, which writes a note's file as Compze does, since Compze's storage is out of reach: a
    `todo:compze:` until Compze can rewrite every taggregate.
  - **On the corpus:** a full rebuild changes no file; saving every note, all 79,896, changes no file; and an edit made
    through Compze's storage and undone leaves the file byte for byte as it was.
  - **At the merge, in the corpus repository:** `.gitignore` lists `/.state-store/`, Compze's own folder, which holds the
    cache, and the journals while a commit is made, in place of `snapshot.bin`; and the `snapshot.bin` files go. A linked
    worktree's corpus clone ignores the folder in its `info/exclude` meanwhile.
  - **The start, from Compze's MemoryPack cache, is as fast as from `snapshot.bin`** ("Loading the notes" in the headless
    JAStudio, the machine's other processes using 0.5 to 0.8 cores): 3.95, 4.06 and 4.13 s from the cache, against 4.01,
    4.16 and 4.45 s from `snapshot.bin`. Without a cache it was 7.69 and 7.81 s once the system had the files cached, and
    37.65 s the first time they were read, so Magnus's conditional approval of a MemoryPack cache, a package of Compze's
    own, held: `Compze.Teventive.StateStore.MemoryPack` (`CachedWithMemoryPack`), whose cache is
    `.state-store/<class folder>.load-cache`. It takes each file whose length and last write time it has from the cache,
    and reads the others; a change to a DTO's fields makes the next start read every file once. A start that rebuilt the
    cache took 10.09 s. A cold start wasn't measured, since emptying the system's file cache takes administrator rights:
    it reads what `snapshot.bin`'s did, a MemoryPack file of each class (80 MB, against 74) and the folders' listings.
- **Specifications wait** with `ILocalTessagingEngine.AwaitNoTessagesInFlightAsync`, so named on Magnus's word (Compze's
  `3df5ee7f8`).

## The order

Each step is an increment that builds and passes JAStudio's whole suite, and is committed.

**Before Compze's step 2, needing nothing new from Compze:**

1. **A baseline:** a full rebuild of this worktree's corpus in the headless JAStudio, to compare every later rebuild
   with.
2. **The snapshot types round-trip:** specifications that a vocab, a kanji and a sentence come back unchanged through
   their DTOs, and a check of every note in the real corpus. Under the private copy every edit builds its copy through
   the DTO, so a property it misses would be lost at every edit.
3. **`NoteId` derives from `TaggregateId`,** which is in Compze.Abstractions, already referenced. `VocabId`, `KanjiId`
   and `SentenceId` then are taggregate ids.
4. **What Anki says of a note's cards leaves the note:** `_cardStatus` and `AnkiTags`. It isn't stored, so a copy
   through the DTO drops it.
5. **What a note computes from other notes and keeps leaves it:** the set of forms that other vocab don't own, in
   `VocabNoteForms`, which goes stale.

**After Compze's step 1, `ITaggregateTevent` as a plain `ITevent`:**

6. **The vocab's tevent interface hierarchy,** `VocabNoteForms` first, written to be read. **Magnus looks at it here.**

**After Compze's step 2, the in-memory state store:**

7. **The vocab moves into the store,** with `VocabNoteForms` the first part that publishes real tevents. The parts not
   converted yet go on changing state as today, and `NoteGuard.Update` publishes an interim tevent for them, so that the
   store counts the copy as changed. It publishes only when the note's DTO changed, as the hash decides today. Each later part converted removes one use of it, and the last removes it.
8. **The edit sites get units of work,** the holders of a note get taggregate-references, and the vocab caches' nine
   indexes become `ITaggregateIndex`es.
9. **The kanji, then the sentences,** the same way.
10. **Done. The remaining parts converted,** one at a time, until the interim tevent is gone.
11. **Done. What reaches other notes becomes participation handlers:** the perfect synonyms' answer, renaming
    word-references, and the other side of the mutual relationships and of `VocabNoteForms.Add` and `Remove`. What
    can't be undone, such as telling Anki to delete a study-note, moves to after the commit.
    - **A handler gets the notes it changes through JAStudio's running `NoteUnitOfWork`,** not straight from the store:
      only the notes the unit of work hands out are notified once it has committed. A handler runs inside the
      caller's transaction and unit of work, but not always on its thread. So `NoteUnitsOfWork` keeps the running
      unit of work in an `AsyncLocal`, which flows as the transaction does (`5eaa27b7d`, on `jastudio-rep-opus`'s
      word).
    - **After the commit is `NoteUnitOfWork.OnceCommitted`,** synchronous on the thread that made the change, not
      Compze's observers, which run off-thread.

**After Compze's step 3, routing per class:**

12. **Done. Memory and load time measured** with the whole corpus loaded, against `main`.

**After Compze's step 4, snapshots and the JSON store:**

13. **Done, with the switch. The notes stored through the store,** each file written whole, and several as all or
    nothing: the store is `StoredIn` JAStudio's `CorpusNoteFiles`, its `ITaggregateStorage`. Compze's step 4 has nothing
    more for JAStudio (`jastudio-rep-opus`, 2026-11-17): its snapshot API is `ISnapshotable`, which the notes implement,
    their DTOs the snapshots; and its JSON store isn't built, nor for JAStudio, whose corpus files are a format of its own.

**Last, and done: loading the whole corpus again while running,** as around a git fast-forward:
`IStateStore.ReloadFromStorageAfter`, which JAStudio's start and `fast_forward_the_corpus_checkout` load through.

**What is left is the end:**
- Magnus decides whether the JSON store waits for a consumer that needs it, as `jastudio-rep-opus` put to him. The index
  that keeps no keys, from step 12, is built and measured.
- The end of the spike's review in Compze: independent reviews of every public API change and of the store's design,
  then a pass with Magnus. What it changes may reach JAStudio's code.
- Compze's release, on Magnus's go-ahead; then this branch goes back to package references, and the three sign-offs.

Two more were on this list, and JAStudio doesn't need them from Compze (2026-11-17):
- **A wait until observers have caught up.** JAStudio uses no observers. What it does after a commit has to run
  synchronously, on the thread that made the change: the notifications the UI and caches follow, and calls into
  Anki. Compze's observers run off-thread, so that work stays in `NoteUnitOfWork.OnceCommitted`.
- **A short way for a specification to run an edit.** That is `note.Edit(it => ...)`, in JAStudio.

## What proves each step

- **JAStudio's whole suite,** .NET and Python, and `full-build.ps1`, since the API that Python sees changes.
- **The round-trip check over the real corpus,** from step 2 on.
- **A full rebuild compared with the baseline:** the migration should change nothing the corpus stores, so no file
  should differ. If the store needs the files changed, that is a change to the corpus, which the main checkout's
  session makes.
- **The MCP server's tools against the headless JAStudio,** which edit notes as the UI does.
- **What only Magnus can test:** JAStudio inside Anki, live.

## Put to `jastudio-rep-opus`, 2026-11-16, and agreed by it

**Its answers:**

- **Typed ids:** yes to all three, as requirements of steps 2 and 4. `Taggregate.Id` isn't sealed, and stays so.
- **Storage:** the seam, `ITaggregateStorage` (`LoadAll`, and `Prepare` returning a write to commit or discard), comes
  in step 2. JAStudio keeps its own storage behind it, its DTOs, settings and file layout byte for byte. Compze's JSON
  store is the default implementation for others, and JAStudio may borrow its atomic write. Step 4 brings JAStudio the
  snapshots and the round-trip helper, not a new file format.
- **Loading:** the store fills itself from `ITaggregateStorage.LoadAll` when it starts, outside any unit of work, in
  step 2. Loading everything again while running stays last.
- **The interim tevent:** works. A copy counts as changed when it has published, and that is what is written.

**The points as put:**

1. **Typed ids.** A note's id is a `VocabId`, `KanjiId` or `SentenceId`, each a `NoteId`. With `NoteId` derived from
   `TaggregateId`, JAStudio needs the store, the tevents and `RestoreIdentity` to keep the id instance they're given,
   never to make a new `TaggregateId` from its `Guid`, and `Taggregate.Id` to stay overridable, so that `VocabNote.Id`
   can be a `VocabId` by a covariant override.
2. **Storage before step 4.** The real JAStudio can't run on the store until its notes are written. A storage seam in
   step 2 would let JAStudio's repository write them until step 4. And at step 4: the corpus's files must stay exactly
   as they are, so either the JSON store lets its consumer decide the layout and the serialization, or JAStudio keeps
   its own storage behind the seam.
3. **Loading.** JAStudio loads all 80,000 notes at startup, from their DTOs, and again around a git fast-forward. The
   store needs a way to be filled with committed taggregates from their snapshots, outside any unit of work.
4. **The interim tevent of step 7** needs nothing from Compze, as far as I can see: a tevent whose applier changes
   nothing. Its only purpose is that the store counts the copy as changed.
