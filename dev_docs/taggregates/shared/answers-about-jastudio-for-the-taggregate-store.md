# Answers about JAStudio, for the taggregate store that Compze is designing

> **Status: 2026-11-16, by `taggregates-fable`, from JAStudio's code as it is on the branch `compze-taggretates`.**
> Answers to five questions from `jastudio-rep-opus`, which represents Compze. **Read by `taggregates-opus`, which
> agrees with the facts and the recommendations, and added two things that are taken in and marked as its:** under
> question 1, the note object that stays, and under question 3, lookups that read a unit of work's own writes.
> What is a guess, or was not traced, is marked. The recommendations are the two JAStudio sessions', and nothing here
> is decided.
>
> **The answers were given to a design in which a note got for update is a private copy. They changed Compze's
> recommendation:** its two sessions now recommend shared live instances, under which most of the answer to question
> 1 is no longer needed. See [the last section](#after-the-answers-compze-now-recommends-shared-live-instances).

**What is being designed in Compze (Magnus's direction, 2026-11-16).** A taggregate store, `ITaggregateStore`, that
holds state and no history, with an `ITaggregateStoreUpdater` session for each unit of work. A unit of work is a
System.Transactions transaction. Tevents reach subscribers through Tessaging's in-process engine: participation
handlers inside the unit of work, observers after the commit and off the publishing thread. A note that is got for
update is a private copy; the commit puts the copy in place of the committed instance, which is read only. A lock for
each note makes writers wait for each other. Each note class has a snapshot type of its own, with no tevents in it.
The hook and the queue inside the taggregate go.

## The headlines

1. **Identity: a handle that the store keeps for each note, not bare ids, and not copying back.** Few things hold a
   note across edits, and all of them can follow a swap. What can't follow one cheaply is JAStudio's indexes, and what
   an edit needs is that a lookup inside a unit of work gives that unit of work's own copy.
2. **The larger cost of read-only committed instances is not who holds notes. It is every place that edits one.**
   Today an edit is a call on the note one has, which one then goes on reading. That stops being true, unless the
   note object that JAStudio's code holds stays for good and reads through its handle. Whether it can decides the
   cost of the whole direction, more than handles or ids do.
3. **Files: one JSON file for each note, written in place, not atomically.** A crash that leaves part of a unit of work
   written matters little, and no more than today. A half-written file matters more: the next start fails on it.
4. **Subscribers: the indexes must change with the commit, before the edit returns. Everything in the UI can be an
   observer.** Nothing needs the UI thread synchronously. And yes: code later in an edit reads what earlier steps of
   the same edit changed, in other notes and in the indexes.
5. **A version in each file: nothing in JAStudio needs one, and I'd leave it out.**
6. **Depending on Compze.Tessaging and a store package is no problem.** Four things to check are listed.

## 1. Identity

### What holds a note across edits

| Holder | What it holds | Can it follow a swap? |
|---|---|---|
| The caches: `NotesById`, and every `NoteIndex` | The note objects themselves. An index's sets are `HashSet<TNote>`, and `JPNote.Equals` compares ids | See below: this is the costly one |
| A note's own parts | Their note (`_vocab`, `_kanji`, `_sentence`, `_note`) | Yes: a copy builds its parts, as loading does |
| Other notes | Nothing. A note refers to another by a `WordReference` or an id, never by the object | Nothing to follow |
| The note tabs of the workspace | Ids (`NoteTabHistory`) | They look the note up each time |
| Six classes in `JAStudio.UI` | A note in a field: `SentencePageViewModel`, `SentenceParsingCaseViewModel`, `VerdictChoiceViewModel`, `SentenceElementMenus`, `ListedNoteViewModel`, `SearchResultViewModel` | Yes. `SearchResultViewModel` already replaces the note it holds |
| The view models of the card and note pages (`JAStudio.Core/UI/Web`), and a few result objects in Core | A note, for one rendering of a page or one call. **Not traced** one by one | Built again each time |
| JAStudio's MCP server | `EditableNote`, for one tool call. `NoteVersions`, see below | Yes |
| Parsing: `VocabMatch.Vocab`, `VocabWordInfoEntry` | A vocab, for the analysis of one sentence. **Not traced** whether anything keeps an analysis | Probably nothing to follow |
| The Python add-on | Looks notes up for each use, by a grep. It is to stop calling .NET at all | Nothing to follow |

**JAStudio already has the rule that a note object can be replaced.** `JPCollection.ReloadFromBackend` replaces
every note object, and the documentation of `JPCollection.ReloadedFromBackend` says that whoever holds one must look
it up again by its `NoteId`. Every holder in the UI follows that notification today. A swap at each commit makes the
rare case the everyday one, for about ten classes.

**`NoteVersions` builds notes that are in no store:** it reads two versions of a note's file from git into two notes
and compares them. So a note must be restorable from its snapshot, and readable, outside any store.

### The indexes are the costly holder

`NoteIndex.Update` changes only the sets of the keys that a note gained or lost. Under every other key the note
stays in its set, as the same object. With a new instance at every commit there are two ways, and both cost:

- **Replace the instance in every set it is in.** Lookups read the sets without a lock, so a set is never changed in
  place: the index copies it, changes the copy and publishes it. A sentence is filed under each of its words, and
  the set of a common word holds thousands of sentences. Every commit of a sentence would copy each of those sets.
- **File ids, and resolve each at the lookup.** A lookup in the dictionary of notes for each note returned, on the
  path that parsing runs all the time from many threads. Not measured.

### What an edit needs: its own copies back from a lookup

An edit looks notes up in its middle, and expects to see what it has changed so far. The clearest case is
`PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym` (`PerfectSynonyms.cs`, lines 64 to 72). It adds a word
to this vocab, then walks the whole web of perfect synonyms through lookups by question, reading each vocab's own
perfect synonyms as it goes, and then changes every vocab it found. `PushAnswerToOtherSynonyms`, `Remove` and
`JPCollection.RenameTheWordReferences` change the notes that lookups return in the same way.

So inside a unit of work, **a lookup must give the unit of work's own copy of a note it has got for update, and the
committed instance of every other note.** A lookup that always gives committed instances makes such an edit read
stale notes, and change objects that are read only.

### What I recommend

**A handle for each note, kept by the store, which the indexes and the long-lived holders hold.** It is the second of
the two ways `jastudio-rep-fable` names. A handle is an id with the lookup already done:

- **The indexes hold handles, so they change only when a note's keys change,** as today.
- **A handle resolves to the committed instance with one read,** and inside a unit of work to that unit of work's copy
  when it has one. That is the rule above, in one place. Asking whether there is a unit of work, at each resolve, is
  a cost on the path that parsing runs. Not measured. A reader that knows it is in none could skip the question.
- **Whoever holds a handle follows no swap.** The six classes in the UI hold a handle, or go on looking up by id.

**Not copying the state back into the same instance.** I agree with both reasons against it.

**Bare ids work too,** at a lookup for each use. The resolving inside a unit of work is then needed in the store's
lookup by id instead.

### The larger cost: every place that edits a note

**Today an edit is a call on the note one has: `vocab.Question.SetText("x")`, and the same object then shows the
change.** The UI's editors and menus, the MCP server's tools, the importers, the batches and nearly every
specification are written so. With committed instances that are read only, each of them becomes: in a unit of work,
get the note for update, change that, and afterwards treat what was held before as stale.

- **Not counted.** It comes on top of the 51 `NoteGuard.Update` sites inside the notes, which was the size given so
  far, and it is larger than they are.
- **What removes most of it, and so decides the cost of the whole direction** (`taggregates-opus`'s addition, which I
  take): the note object that JAStudio's code holds stays for good and reads through its handle. A change made on it
  joins the unit of work under way, found through the ambient transaction, and is a unit of work of its own when
  there is none, as `IIndependentTeventPublisher` is to `IUnitOfWorkTeventPublisher`.
  `vocab.Question.SetText("x")` then stays as written at every call site, and an edit across notes, such as the walk
  through the perfect synonyms, opens one unit of work around itself and nothing more.
- **It asks two things of Compze:** the handle that resolves as described, and that resolving and getting a note for
  update both work from the ambient transaction alone, with no session object passed down to the call sites.
- **Not worked out, and it has one hard part: a note's parts.** `vocab.Question` is an object of one instance. Either
  a part reads through its note's handle too, or a part of a committed instance that is asked to change finds the
  same part in the unit of work's copy and makes the change there. The declaration of tevent routing for each class
  says how each component is reached from its root, which is what the second way needs. This is JAStudio's design,
  and the first thing to prototype if the direction is taken.

### Three more facts the copy has to live with

- **A note holds state that is not in its file:** the studying statuses of its cards and its tags in Anki
  (`JPNote._cardStatus`, `JPNote.AnkiTags`). They are read from Anki at a load and at a sync, and set on the note
  without a note update: `JPCollection.StudyNoteChanged` notifies them, not `NoteUpdated`. This confirms what
  `jastudio-rep-fable` could not: the DTOs don't hold them. A sync sets them for up to every note at once, which as
  taggregate state would be a unit of work and a copy for each. **I would move them out of the note,** into a table
  by note id, as the ids of the study-notes already are (`StudyNoteIds`). Compze then needs no state that is in a
  snapshot and not stored, for JAStudio's sake.
- **A note computes values lazily and keeps them:** seven in `VocabNoteForms`, one in `RelatedVocab`. Committed
  instances are read from many threads, so they must be safe for that. Some are computed from other notes, and go
  stale when those change: the stale set in `VocabNoteForms` that the shared README describes.
- **A full rebuild gets every note for update,** and its second run changes none
  (`When_running_a_full_rebuild`). A copy for each is then about the cost of building every note once more. Not
  measured. JAStudio can compute on the committed instance and get the note for update only when the result differs.

## 2. Files

**One JSON file for each note:** `<corpus>/<vocab|kanji|sentences>/<the first two hex digits of the id>/<id>.json`
(`JsonFilesystemObjectRepository.FilePath`). Indented, camelCase, Japanese written as it is and not escaped,
unmapped members refused (`NoteSerializer.JsonOptions`). The file is the note's DTO: `VocabDto`, `SentenceDto` or
`KanjiDto`.

**Written in place, not atomically:** `File.WriteAllText` (`JsonFilesystemObjectRepository.cs`, line 40). No
temporary file, no rename, no flush to the disk. A crash in the middle of a write leaves a truncated file, and the
next start then fails on it: the load reads every changed file and throws at one it can't deserialize. The recovery
is git, where the corpus lives.

**A binary cache for a fast start, beside the files.** This confirms `jastudio-rep-fable`'s guess. Each note type's
folder has a `snapshot.bin`, a MemoryPack file of all its DTOs, which git ignores. `LoadAll` reads it, then reads only
the JSON files whose last-write time is later than the cache file's, drops the ids that have no file, and writes the
cache again if anything changed. If it fails in any way, every JSON file is read. So:

- **A store that reads 80,000 JSON files at each start would slow JAStudio's start.** The storage seam has to leave
  room for this cache, as the consumer's own or as something Compze's JSON store offers.
- **The cache goes by last-write times.** A file renamed into place keeps the time it was written at, which is still
  later than the cache's, so a temporary file and a rename fit.

**Does a partly written unit of work matter?** Little, and no more than today:

- **Generated data:** no. A full rebuild computes it again, and its second run changes nothing.
- **What a person entered that spans notes:** yes, and no rebuild repairs it, since `UpdateGeneratedData` changes
  its own note only. A vocab renamed without the word-references that named it; one perfect synonym with the new
  answer and the others with the old; one side of a mutual relationship.
- **Today is the same or worse:** each note is saved by itself as it changes, one after the other, in the middle of
  the edit.
- **Everything is in git,** so a partial write shows as a diff and can be reverted.

**So, in order of what JAStudio gains:** a single file written atomically, first. Several files as all or nothing is
a gain too, and nothing JAStudio has today.

**Three things the store has to leave room for:**

- **Loading everything again while running.** `JPCollection.ReloadFromBackendAfter` empties the collection, lets a
  change be made to the corpus checkout, such as a git fast-forward, loads again, and passes on as updated the notes
  whose stored data differs from before. It tells them apart by a 128-bit hash of each note's stored form
  (`JPNote.HasTheSameStoredDataAs`). No tevent describes such a change.
- **Only one JAStudio works on a corpus checkout.** It holds a lock file beside the checkout (`CorpusCheckoutLock`).
  The store's own files, the journal and the temporary files, belong beside the checkout too, or in a folder that the
  corpus repository's `.gitignore` names.
- **A rename can fail on Windows while another program has the file open.** Git, a diff viewer and a virus scanner
  all read the corpus. `jastudio-rep-fable`'s design makes a failure after the journal is written fatal: I'd retry
  the renames for a while first. That such collisions happen often enough to matter is a guess.

## 3. Subscribers

**Today little is a reaction to a notification.** What must follow a change is mostly a direct call in the middle
of the edit. A note update has five followers in product code: the save, which follows each cache's `NoteUpdated`, and
four that show the change, which follow `JPCollection.NoteUpdated`.

| What follows a change | How it is done today | When it must happen |
|---|---|---|
| The indexes are refiled | Inside every `NoteGuard.Update`, under the cache's lock, before `NoteUpdated` | **With the commit:** before the edit returns, and not visible before the notes are |
| The note is saved | The first subscriber, on the thread that made the change | The store's commit |
| Other notes that must agree: the answer of the perfect synonyms, the word-references after a rename, the other side of a mutual relationship, another vocab's forms | Direct calls in the edit | **Inside the unit of work** |
| A sentence is parsed again when its configuration changes | A direct call on the same note (`CachingSentenceConfigurationField`) | Inside the note's own cascade |
| The sentences of a changed vocab are parsed again | Asked for by the UI after the edit, as a batch on many threads, with progress (`LocalNoteUpdater.ReparseSentencesForVocab`) | After the commit, in units of work of its own. Stays as it is |
| Anki is told: a study-note deleted, cards suspended | A call to Anki in the middle of the edit (`JPCollection.Delete`, `JPNote.SuspendAllCards`) | **After the commit,** before the edit returns: it can't be rolled back |
| A deleted note's media files are deleted | In the middle of `JPCollection.Delete` | The same |
| The UI shows the change: `NoteTabViewModel`, `SearchResultsViewModel`, `ParsingCasesViewModel`, `ChangesThatOutdateNotePages` | Subscribers of `NoteUpdated` | **After the commit, off the thread: observers** |

**Nothing needs to see a change synchronously on the UI thread.** `NoteUpdated` is documented as notified "on the
thread that updated it", and updates come from background threads already: a rebuild, a reparse, a tool call of the
MCP server. Each of the four either posts to the UI thread itself or only raises that a page is outdated. The editor
that made an edit reads the note when its edit returns: what it needs is the commit, not an observer.

**Code later in the same edit does read what earlier steps changed.** Two kinds:

- **Other notes changed earlier in the edit:** the walk through the web of perfect synonyms, under question 1.
- **The indexes:** today a note is refiled at the end of each `NoteGuard.Update`, so the next step finds it by its
  new keys. `VocabCollection.NamedExactlyByOrCreated` looks a vocab up by its question and creates it when there is
  none, and `VocabCollection.Add` asserts that no vocab has the question. Two such steps in one unit of work would
  not find the first one's vocab, and create a second with the same question.

### What this asks of Compze

- **Derived state that changes with the swap,** the third of the three ways `jastudio-rep-fable` lists. Its cost
  there is that the consumer's code runs at commit and must not fail. **Split it in two, as the storage seam is:**
  a step that may fail, in which the consumer prepares its changes, and a step that can't, in which it puts them in
  place. `NoteIndex` works so already: a change is made to copies of the sets, and `PublishChanges` assigns them.
- **From that preparing to that publishing, one unit of work at a time.** Two that prepared copies of the same set
  would lose one's change. Today the cache changes one change at a time under its lock, so a rebuild on many threads
  already waits there.
- **Lookups inside a unit of work read its own writes** (`taggregates-opus`'s addition, which I take in place of what
  I first wrote: that keys changed inside a unit of work would simply not be in the indexes until it commits). With
  handles that resolve to the unit of work's copies, that limit makes lookups disagree with the notes both ways.
  After a vocab is renamed from x to y, a lookup of x still finds it, as a copy that is no longer named x, and a
  lookup of y finds nothing. The rule instead: inside a unit of work, a lookup takes the committed index's handles
  for the key, resolves them, drops those whose copy no longer has the key, and adds the unit of work's own copies
  that have it.
  - **It asks of Compze** that the session can list the notes it holds, by type: those got for update, those created
    and those deleted.
  - **JAStudio's `NoteIndex` can do the rest,** with the function it has that gives a note's keys. A unit of work
    holds few copies, so it costs little, and nothing outside a unit of work.
  - **Creating vocab in a loop can then be one unit of work,** and so all or nothing.
  - **Two units of work at once can still each create a vocab with the same question:** neither sees the other's.
    Today has the same gap: `VocabCollection.Add` asserts that no vocab has the question, and then adds, with no
    lock around the two.
- **A change made outside any unit of work is a unit of work of its own,** committed when the call returns. The UI,
  the MCP server's tools and the specifications call a method and expect it done.
- **A way to act after the commit and before the edit returns,** for what can't be rolled back, such as telling Anki.
- **A wait for the observers to be at rest, without a testing host or an endpoint.** A specification of the UI
  edits a note and then asserts on a view model. The delivery model says the testing host's wait covers the
  observation queues: JAStudio would run the engine with no endpoint.
- **A second writer waits,** with a timeout that throws with diagnostics, as Compze.Threading's locks do. My
  recommendation, for Magnus to confirm. It holds only if units of work are short, so a rebuild is a unit of work for
  each note, which fits its rule that `UpdateGeneratedData` changes its own note only.

## 4. A version in each note's file

**Nothing in JAStudio needs one, and I'd leave it out.** Acceptable if something does come to need it, but it has
costs that are particular to a corpus kept in git:

- **Nothing would compare it.** One JAStudio works on a corpus checkout (`CorpusCheckoutLock`), and inside it the lock
  for each note serializes writers.
- **A line of noise in every diff.** A rebuild's changes are analyzed as diffs, by eye and through the MCP server's
  comparison of two versions of a note, which would have to skip the property.
- **A note changed and changed back would still differ from what is committed,** by its version alone.
- **A conflict in every merge that touches a note on both sides,** where git today merges changes to different
  lines. The corpus is kept to one line of history, so this is rare, but a version line makes it certain.
- **If a change from outside ever has to be detected,** JAStudio already does it without a version: by the hash of
  each note's stored form, at a reload.

In memory a restored note can be version 1, as `jastudio-rep-fable`'s design has it.

## 5. Depending on Compze.Tessaging and a taggregate store package

**No problem.** JAStudio is an application and adds packages freely, any project in it may depend on Compze
(Magnus, 2026-11-12), and it composes its services with Compze.DependencyInjection already (`IRootResolver`).

- **Already referenced:** Compze.Abstractions, Contracts, DependencyInjection, Internals.Logging,
  Internals.SystemCE, Threading, TypeIdentifiers, Underscore and Unit.
- **New, from Compze.Tessaging's project file:** Compze.Tessaging, Compze.Tessaging.Abstractions,
  Compze.TypeIdentifiers.DependencyInjection and Microsoft.Extensions.Caching.Memory. No SQL, no ASP.NET and no
  Newtonsoft come with it. Then Compze.Teventive and the store's package.
- **JAStudio uses no unit of work and no transaction today:** no `ExecuteUnitOfWork` and no `TransactionScope`
  anywhere in its product code.

**Four things to check, none of them a reason against:**

1. **Nothing static.** The specifications run several JAStudios in one process (the opus team's fact, not checked by
   me), so the engine, the store and anything they register must belong to a container.
2. **Type identifiers.** Whether the engine needs an identifier mapped for each tevent type I don't know. A note's
   tevents never leave the process, and there would be hundreds of types.
3. **Synchronous handlers.** JAStudio's domain code is synchronous throughout. `jastudio-rep-fable` has this as
   `ITaggregateTevent` becoming a plain `ITevent`.
4. **What a unit of work costs,** since a full rebuild would run 80,000 of them: a scope, a transaction, one get for
   update and a commit. Not measured. And System.Transactions on macOS and Linux, which JAStudio is to run on: fine
   for transactions within one process as far as I know, not run.

## After the answers: Compze now recommends shared live instances

**Compze's design for the store is in its `dev_docs/taggregate-improvements/shared/the-taggregate-store.md`** (commit
4450f76db). After these answers its two sessions recommend, as Magnus's decision to make, a different isolation
from the one he chose:

| | (a) A private copy for each unit of work, which Magnus chose | (b) Shared live instances, which Compze's sessions now recommend |
|---|---|---|
| The note object JAStudio's code holds | Replaced at every commit | The same object for good |
| The places that edit a note | Every one changes | Stay as written: a change joins the unit of work under way, or is one of its own |
| What other threads read meanwhile | Committed state only | Changes not yet committed, some of them later undone |
| A unit of work that fails | Its copies are discarded | Each note it changed is restored in place, from a snapshot taken at its first change |

A third way, (c), keeps the object and shows other threads committed state only: a note's state held as one
immutable value for each transaction. It is how every taggregate is written that changes, and it is not designed.

### What becomes of the answers above

- **Question 1:** the handles, the indexes following a swap, and the cost of every place that edits a note all
  belong to (a). Under (b) none of them arises. What stays: what Anki says of a note's cards moves out of the note;
  values a note computes lazily and keeps must be safe to read from many threads; and a note must be restorable
  outside any store.
- **Question 3:** unchanged, and taken up: the indexes as derived state that is itself transactional, written
  inside the unit of work and applied at its commit; lookups inside a unit of work that read its own writes, for
  which the session lists the notes it holds, by type; and what can't be undone, such as telling Anki, after the
  commit and before the edit returns (`IUnitOfWorkResolver.OnCommittedSuccessfully`).
- **Questions 2, 4 and 5:** unchanged, and taken up. The version is kept in memory only. No type identifiers are
  needed for tevents that stay in the process. A unit of work that gets a note, changes it and commits was measured
  by Compze at 8.5 to 12.5 µs and 9.5 KB.
- **Not designed yet in Compze:** loading everything again while running, and a wait for the observers to be at
  rest that needs no testing host.

### What JAStudio's side says to (b)

By `taggregates-fable`. `taggregates-opus` has been told of it, and its reply, if any, is taken in here.

1. **I agree with recommending (b).** It is how JAStudio's notes are read today: a note is changed in place, and
   threads that parse read it meanwhile. (b) adds what today lacks: writers of one note wait for each other, an edit
   that fails is undone in every note it touched, and the files are written whole.
2. **(b) keeps (c) open, and (a) doesn't.** Under both (b) and (c) the note object stays and the places that edit
   stay as written. Going from (b) to (c) later would change Compze and the insides of the notes, not their callers.
3. **Restoring in place should be the only restoring, used by loading too:** build the note empty, then restore it
   in place from its snapshot. Compze's document gives (b)'s price as a second and harder restoring that the
   implementor writes for every part, and that runs rarely, when something has already gone wrong. As the only
   restoring it is written once for each part, and every start runs it 80,000 times. It asks that a part's state can
   be set after the part is built, which the tevent appliers need anyway.
4. **Restoring in place must also reset what a note computed and kept:** the seven lazy values in `VocabNoteForms`
   and the one in `RelatedVocab`. The specification helper that compares two snapshots doesn't see them.
5. **A unit of work must not wait for the UI thread while it holds a note's lock.** JAStudio's progress presenters
   call the UI thread and wait for it when a task or a batch starts (`DialogProgressPresenter`,
   `ScopeProgressPresenter`, through `IUIThreadDispatcher.InvokeSynchronouslyOnUIThread`). A background unit of work
   that did so while holding a note, with the UI thread waiting for that note, stands still until the lock times
   out. Today's batches start their progress before they touch a note, so a unit of work for each note is safe. A
   whole rebuild as one transaction is not.
6. **In a rebuild, a unit of work for each note before batches of hundreds.** A batch holds every note it changed
   until it commits. So a UI edit that needs two notes can wait for a batch that waits for it, until the timeout,
   and other threads look up the batch's notes by their old keys until it commits. A unit of work for each note
   never waits while it holds one. By Compze's figures that costs about 0.8 s of one core and 0.76 GB of garbage for
   80,000 notes. For JAStudio to measure and choose.
