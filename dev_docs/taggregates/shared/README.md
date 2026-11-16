# The notes as Compze Taggregates: the shared conclusions, for JAStudio

> **Status: exploration, 2026-11-16. Nothing is built, in JAStudio or on any branch of Compze that anyone uses.**
> Agreed by the four sessions that explored the question as two teams (`taggregates-fable` and `taggregates-opus` in
> JAStudio, `jastudio-rep-fable` and `jastudio-rep-opus` in Compze), then **reviewed by two reviewers who took no part
> in it, one Opus and one Fable, and changed by their review.** Kept by `taggregates-fable`, which represents JAStudio.
> Compze's side is in Compze's `dev_docs/taggregate-improvements/shared/`. It was reviewed by two reviewers of its
> own and revised, and the two folders were brought in step on 2026-11-16.

> **Since these conclusions were written, Magnus has set a new direction in Compze: a taggregate store.** It changes
> what the Taggregates option is and what it takes of JAStudio: see the next section. The conclusions below are as
> they were before it, and conclusions 1 to 4 stand.

## What the taggregate store changes in these conclusions

**The direction (Magnus, 2026-11-16).** A Compze component holds a process's live taggregates and routes their
tevents: an `ITaggregateStore` that keeps state and no history, with a session for each unit of work, and the unit of
work a System.Transactions transaction. Compze's two sessions have designed it and prototyped it in memory:
Compze's `dev_docs/taggregate-improvements/shared/the-taggregate-store.md` (commit 4450f76db). What JAStudio's code
says to it, and what JAStudio's side says to its recommendation, is in
[answers-about-jastudio-for-the-taggregate-store.md](answers-about-jastudio-for-the-taggregate-store.md).

**One decision of Magnus's is reopened by JAStudio's facts: how a unit of work's changes are kept from other code.**

- **He chose a private copy of a note for each unit of work.** Every commit then puts a new object in place of the
  note, so every place in JAStudio that edits a note changes: the UI, the MCP server's tools, the importers, the
  batches and nearly every specification. Not counted, and larger than the 51 `NoteGuard.Update` sites.
- **Compze's two sessions now recommend shared live instances,** and JAStudio's side agrees. The note object stays
  for good and the places that edit stay as written. Other threads read changes that are not yet committed, as they
  do in JAStudio today. A unit of work that fails restores each note it changed, in place.

**What becomes of what this document says below:**

| Below | With the store |
|---|---|
| Conclusion 5, the two flaws in Compze.Teventive | Can't arise for a taggregate in a store: nothing calls out from inside a publish or a commit |
| Conclusion 6, nothing moves before routing for each class has landed | Stands, and the store comes before it in Compze's order |
| A queue in the collection's dispatching | Compze's: tevents are dispatched when the unit of work commits, in the order published |
| Loading one note from its file, to replace a note that refuses use | Gone: a publish that fails rolls its unit of work back, and no note is replaced |
| How a note is loaded | Decided by Magnus: from a snapshot type of the note class's own, with no tevents. JAStudio's DTOs are such types |
| Two threads changing one note | They wait for each other, on a lock for each note |
| A save that fails | Rolls the unit of work back, where today the note stays changed in memory and counts as saved |
| The stored note | Each file written whole, and several as all or nothing, where today a crash can leave half a file |

**New for JAStudio with the store,** each in the answers document:

- **Its indexes become derived state that the store keeps in step with the notes.** How is open between the two
  sides. Compze's design changes them at the commit, with a unit of work reading its own changes in them. JAStudio's
  side asks, under shared live instances, that they follow each change as today, and are put right after a rollback.
- **What Anki says of a note's cards moves out of the note.** It is not stored, and changes without an edit.
- **What can't be undone happens after the commit:** telling Anki to delete a study-note or suspend cards, and
  deleting media files. Today it happens in the middle of the edit.
- **Under shared live instances, each part of a note is restorable in place.**
- **Not designed yet in Compze:** loading the whole corpus again while running, as JAStudio does around a git
  fast-forward, and a wait for observers to be at rest, which JAStudio's specifications need.

**The next step that conclusion 4 names stands, and now needs the store:** `VocabNoteForms` written both ways.

**The question (Magnus, 2026-11-16).** Should JAStudio's notes be Compze Taggregates, so that every change to a note is
a tevent, and whatever follows a change subscribes to exactly the tevents it concerns? The storage stays as it is: a
note's file holds its state, not its history. The exploration was narrowed to what that needs of Compze.

## The conclusions

1. **Compze can be made to carry the notes.** That is established. Compze.Teventive 0.6.0-alpha can't: each instance
   builds its own tevent dispatchers, and the 80,000 notes would add 0.6 to 1.1 GB to a heap of 733 MB. Tevent routing
   decided once for each class brings a note to within 170 to 300 bytes of the same data as plain objects, 20 to 30 MB
   for the corpus. Both reviewers reproduced every figure of bytes kept.
2. **Whether JAStudio should make its notes Taggregates is not established.** The exploration was narrowed to Compze,
   so JAStudio's side was never designed, sized or compared with anything smaller, and no real part of a note was
   written as a teventive. Both reviewers found this, separately.
3. **There are three options, and the middle one was never examined:** see below.
4. **The next step that would settle it:** one real part of a note, `VocabNoteForms`, written both ways, and the
   conversion sized. Then decide.
5. **Two flaws in Compze.Teventive should be fixed whatever is chosen,** both run by the teams and again by both
   reviewers: an observer that publishes on the taggregate it observes can kill the process with a stack overflow, and
   a tevent published from inside `ITaggregate.Commit`'s callback is applied and never committed.
6. **If the notes do become Taggregates, nothing in JAStudio moves before routing for each class, slots included, has
   landed in Compze.**

## The three options, the smallest first

| | What it gives | What it costs |
|---|---|---|
| **Nothing** | | The hand-written wiring stays, and "this note changed, somehow" stays all that listeners hear |
| **Tevents from today's change points:** each `NoteGuard.Update` site also dispatches a tevent, after today's check that the note did change, on one `IMutableTeventDispatcher` for the whole collection | Listeners across the corpus subscribe to exactly what changed, by meaning. No change in Compze, nothing kept for each note, no snapshot to get wrong, no generic base class under `JPNote`. It can be done one part at a time | State can still change without a tevent, since nothing enforces it. Loading and editing stay two code paths. One part of a note following another stays wired by hand |
| **The notes as Taggregates,** after Compze's changes | All of the above, and state that changes only in tevent appliers, loading through the code that edits, and one part following another by a tevent | Everything under "What the Taggregates option takes of JAStudio", and Compze's changes first |

**The middle option is a step toward the third, and may prove to be where to stop.** What carries over, if it uses
the Taggregate tevent types from the start: the tevent interfaces, which are the description of what can happen to a
note, and the subscribers. What is done twice: the tevent classes, and all the change points, which go from "change,
then report" to "publish, and the tevent applier changes". Run by the Fable reviewer against 0.6.0-alpha: a
subscription to a shared meaning works on such a dispatcher, and a dispatch allocates 132 B.

**The size of the conversion, counted by both reviewers:** 51 `NoteGuard.Update` sites in 22 files, 27
`WritableStringValue`s, and about 21 slots. What it serves today: four subscribers to `JPCollection.NoteUpdated` in
product code besides the save, `PerfectSynonyms`' two subscriptions, and six places that reparse by hand.

## What the Taggregates option takes of JAStudio

Found by the review, and corrected on 2026-11-16 after Magnus's questions: the review's list held things that are no
cost of Taggregates.

**What it takes:**

- **The conversion itself,** sized above.
- **No tevent without a change.** The rule is Magnus's: setting a value to what it already is publishes nothing. Most
  setters compare today. Some rely on the note's hash instead, such as `SentenceNote.SetParsingResult`, and
  `ParsingResult` and `ParsedMatch` are classes without value equality. So the generated values need equality. Without
  it a full rebuild publishes for every sentence, and its second run updates every note, which the specification
  `When_running_a_full_rebuild` forbids.
- **A queue in the collection's dispatching,** for subscribers that build something from the sequence of tevents. It
  is a first-in-first-out list: a tevent that arrives while another is being dispatched waits until that one has
  reached every subscriber. Without it, when a subscriber reacts to note A's tevent by changing note B, the later
  subscribers hear of B's change before A's: run by the Fable reviewer. Today's `NoteUpdated` nests the same way,
  harmlessly, since its subscribers read the note again.
- **Loading one note from its file,** so that a note that refuses use after a throw can be replaced. `INoteRepository`
  has `LoadAll` and nothing for one note. A small addition: whoever holds the old object has to get the new one, and
  the note tabs already look notes up by id after a reload.
- **A creation tevent for each way a note is built.**
- **Wiring inside a note doesn't go away. It moves into the tevent types:** a rename's tevent inherits a tevent of the
  forms.

**A choice, not a cost: how a note is loaded.**

- **Through the tevent appliers,** as both teams chose, so that loading and editing are one code path. It needs a
  snapshot tevent, a third form of a stored note beside its DTO and the note. In the prototype in Compze's base classes
  it is about 20 times slower than building plain objects: roughly 0.35 s of one core and 0.4 GB of garbage for the
  about 570,000 tevents of a start, by the Fable reviewer's Release figures. It needn't cost that much: the fable
  team's standalone prototype loaded a root with four components in 0.6 µs, against 0.2 µs for plain objects.
- **By setting each part's state directly,** as the constructors do today. About what loading costs today, by
  assumption, not measured. It gives up the one code path, so the round-trip specification matters more.
- **JAStudio's load time today was not measured,** so what either costs of a start isn't known. The teams' "15 to 40
  times faster" was against 0.6.0-alpha, not plain objects, and from Debug builds: in Release, by Compze's revised
  figures, a vocab with components and shared tomponents loads 9 to 12 times faster than with 0.6.0-alpha.

**The same as today, so no cost of Taggregates:**

- **Two threads changing one note.** It is unsafe today, and nothing detects it. The assertion of one thread makes it
  throw. It is held until the hook returns, so a second thread that arrives while the first is still saving and
  notifying throws too, where today it gets through: run by the Fable reviewer.
- **A save that fails.** Today `JPNote.Flush` records the note's new hash before the save runs, so after a failed save
  the note is changed in memory and counts as saved, and the next flush doesn't try again. The prototype's fixed
  `Commit` leaves the same. The reviewer's finding compared 0.6.0-alpha, which hands the tevent to the next commit
  again, with the prototype.

**Not measured:** the heap against today's notes. The 20 to 30 MB is against plain objects. Today's notes carry objects
that would go, of which the notifiers' 17 MB is measured. Likely about even or lower.

## How far the evidence goes

- **Reproduced by both reviewers:** every figure of bytes kept, the eight behaviors the notes rely on, and both flaws,
  the stack overflow included.
- **The prototypes in Compze's base classes are additive.** One test model declares its routing for each class, beside
  the registration for each instance, which stays. Compze's own taggregates, 35 or so files, are not ported, so that
  porting them is mechanical is not shown. In no prototype: `ForWrapped`, `ForGenericTevent`, `BeforeHandlers` and
  `AfterHandlers` in a class's declaration, the query models, and the assertion that a declaration is the instance's
  own class's.
- **The branches hold the earlier loading API,** public on `ITaggregate` with a version argument and a snapshot
  marker. What is agreed now is protected, typed on the class, and given the note's id, with no marker in Compze.
- **"About 936 B" and "about 520 B in a final design" are subtractions,** not measurements.
- **Compze's full suite ran on the combined branch only.** On the branch with slots of the class, only its unit tests.
- **The Compze-side figures of bytes allocated and of time are from Debug builds of Compze.** In Release a
  vocab-shaped note loads in 3.0 µs and allocates 3,800 B, not 4.8 µs and 4,800 B. Bytes kept are the same.

## What Magnus has to decide

- **Which of the three options,** or the next step first: `VocabNoteForms` written both ways, and the conversion sized.
- **In Compze,** as Compze's shared folder lists: routing for each class, which changes how every taggregate is
  written, and whether it is worth doing for Compze's own sake if JAStudio doesn't use it; declaration or convention;
  `ITaggregateTevent` as a plain `ITevent`; the names; a source generator later; whether a taggregate refuses
  publishing and committing after any exception has escaped `Publish`, the hook's included; and how far the assertion
  of one thread reaches. JAStudio's race between a background reparse and an edit needs it to reach over the hook.
- **Two choices the teams settled by bytes, which the review puts back, the simple one first:**
  - *How an instance reaches its class's declaration:* a constructor argument, which can't be forgotten, at 8 B for
    each teventive, about 9 MB for the corpus; or a virtual property, at no bytes, which fails at the first tevent when
    a subclass forgets it.
  - *A slot:* an object for each instance, at 48 B a slot, about 26 MB; or a static of the owner's class, which must
    be declared above the routing that enters it. The compiler warns of the wrong order within one file.

## Where everything is

| What | Where |
|---|---|
| The shared conclusions for JAStudio, as changed by the review | `dev_docs/taggregates/shared/`, here |
| What the two reviews found, and what they changed | [what-the-two-reviews-changed.md](what-the-two-reviews-changed.md) |
| What the notes would require of Compze as Taggregates | [what-the-notes-require-of-compze.md](what-the-notes-require-of-compze.md) |
| How the two teams' measurements and positions compared | [how-the-two-teams-compared.md](how-the-two-teams-compared.md) |
| Answers about JAStudio for the taggregate store that Compze is designing | [answers-about-jastudio-for-the-taggregate-store.md](answers-about-jastudio-for-the-taggregate-store.md) |
| The shared conclusions for Compze, from before the review | Compze: `dev_docs/taggregate-improvements/shared/` |
| Each team's own documents | `dev_docs/taggregates/fable/` and `opus/`; Compze: `dev_docs/taggregate-improvements/fable/` and `opus/` |
| The prototypes in Compze's base classes | Compze: branches `taggregates-opus/combined` (604c00df4) and `taggregate-experiments-fable-static-slots` (fb75d60a0) |
| The measurement and check programs, the Fable reviewer's included | JAStudio: branch `taggregates-fable-experiments`, under `experiments/taggregates-fable/` |

No code is changed in this checkout, or on Compze's `main`.

## Left on JAStudio's side

- **`VocabNoteForms` written both ways, and the conversion sized:** the next step, above.
- **A round-trip specification,** if the notes become Taggregates: load each note, save it, and compare the files. It is
  the guard against a snapshot tevent that misses a property, which would lose data at the next save without a word.
- **The Python stub generator** would meet a generic base class from Compze under `JPNote`. Not checked.

## Found along the way, in JAStudio

- **A vocab's forms that no other vocab owns go stale** (`VocabNoteForms`, `_notOwnedByOtherVocab`). The set is computed
  from other vocab's questions and forms, but only the vocab's own changes reset it. When another vocab adds or drops
  this vocab's question as a form, `SentenceCollection.WithVocabOwnedForm` lists sentences by the stale set until the
  vocab changes or the collection is loaded again. Found by the opus team by reading the code, not reproduced, and not
  fixed or marked at the code, since this exploration changes no code. It needs no Taggregates: a subscription to
  `NoteUpdated` can reset the set today.
