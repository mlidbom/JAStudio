# The notes as Compze Taggregates: the shared conclusions, for JAStudio

> **Status: exploration, 2026-11-16. Nothing is built, in JAStudio or on any branch of Compze that anyone uses.**
> Agreed by the four sessions that explored the question as two teams (`taggregates-fable` and `taggregates-opus` in
> JAStudio, `jastudio-rep-fable` and `jastudio-rep-opus` in Compze), then **reviewed by two reviewers who took no part
> in it, one Opus and one Fable, and changed by their review.** Kept by `taggregates-fable`, which represents JAStudio.
> Compze's side is in Compze's `dev_docs/taggregate-improvements/shared/`, written before the review and not changed
> by it: where the two differ, this folder is the later one.

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
| **The notes as Taggregates,** after Compze's changes | All of the above, and state that changes only in tevent appliers, loading through the code that edits, and one part following another by a tevent | Everything under "What the Taggregates option costs JAStudio", and Compze's changes first |

**The middle option is a step toward the third, and may prove to be where to stop.** What carries over, if it uses
the Taggregate tevent types from the start: the tevent interfaces, which are the description of what can happen to a
note, and the subscribers. What is done twice: the tevent classes, and all the change points, which go from "change,
then report" to "publish, and the tevent applier changes". Run by the Fable reviewer against 0.6.0-alpha: a
subscription to a shared meaning works on such a dispatcher, and a dispatch allocates 132 B.

**The size of the conversion, counted by both reviewers:** 51 `NoteGuard.Update` sites in 22 files, 27
`WritableStringValue`s, and about 21 slots. What it serves today: four subscribers to `JPCollection.NoteUpdated` in
product code besides the save, `PerfectSynonyms`' two subscriptions, and six places that reparse by hand.

## What the Taggregates option costs JAStudio

Found by the review. None of it was in the conclusions before.

- **Starting takes longer than with plain objects.** Loading through tevents is about 20 times slower than building
  plain objects: for the about 570,000 tevents of a start, roughly 0.35 s of one core and 0.4 GB of garbage, by the
  Fable reviewer's Release figures. JAStudio's load time today was not measured, so whether that matters isn't known.
  The "15 to 40 times faster" of the teams' documents is against 0.6.0-alpha, not against plain objects.
- **The heap against today's notes was not measured.** The 20 to 30 MB is against plain objects. Today's notes carry
  objects that would go, of which the notifiers' 17 MB is measured. Likely about even or lower.
- **No-change publishes.** Today `JPNote.Flush` saves and notifies only when the note's hash changed. As tevents, each
  publishing method has to compare first, which needs value equality on generated values such as a parsing result.
  Otherwise a full rebuild saves and dispatches every sentence, and its second run updates every note, which the
  specification `When_running_a_full_rebuild` forbids.
- **Order across notes.** Compze's queue keeps the order of one note's tevents. A subscriber that reacts to note A's
  tevent by changing note B makes later subscribers hear B's tevents before the rest of A's: run by the Fable reviewer.
  JAStudio's collection needs a queue of its own.
- **One writer at a time in each note, enforced.** The assertion of one thread is held until the hook returns, so it
  covers the save and every subscriber: run. A full rebuild whose subscribers reach other notes, or an edit while a
  reparse updates the same vocab, would throw. That is the assertion doing its job, and JAStudio has to give each note
  one writer at a time and defer reactions that reach other notes during a full rebuild.
- **Failures.** A save that fails inside the commit leaves the note changed, nothing uncommitted and no subscriber
  told: the prototype's fixed `Commit` hands a tevent over once, where 0.6.0-alpha hands it to the next commit again.
  And a taggregate that refuses use after a throw can't be loaded again by itself: JAStudio can load only the whole
  collection, while view models hold the notes.
- **More to write for each note type:** a creation tevent for each way a note is built, and a third form of a stored
  note beside its DTO and the note: the snapshot tevent.
- **Wiring inside a note doesn't go away. It moves into the tevent types:** a rename's tevent inherits a tevent of the
  forms.

## How far the evidence goes

- **Reproduced by both reviewers:** every figure of bytes kept, the eight behaviors the notes rely on, and both flaws,
  the stack overflow included.
- **The prototypes in Compze's base classes are additive.** One test model declares its routing for each class, beside
  the registration for each instance, which stays. Compze's own taggregates, 35 or so files, are not ported, so that
  porting them is mechanical is not shown. In no prototype: `ForWrapped`, `ForGenericTevent`, `BeforeHandlers` and
  `AfterHandlers` in a class's declaration, the query models, and the assertion that a declaration is the instance's
  own class's.
- **The branches hold the earlier loading API,** public on `ITaggregate` with a version argument, not the protected one
  the teams then agreed on.
- **"About 936 B" and "about 520 B in a final design" are subtractions,** not measurements.
- **Compze's full suite ran on the combined branch only.** On the branch with slots of the class, only its unit tests.
- **The Compze-side figures of bytes allocated and of time are from Debug builds of Compze.** In Release a
  vocab-shaped note loads in 3.0 µs and allocates 3,800 B, not 4.8 µs and 4,800 B. Bytes kept are the same.

## What Magnus has to decide

- **Which of the three options,** or the next step first: `VocabNoteForms` written both ways, and the conversion sized.
- **In Compze,** as Compze's shared folder lists: routing for each class, which changes how every taggregate is
  written, and whether it is worth doing for Compze's own sake if JAStudio doesn't use it; declaration or convention;
  `ITaggregateTevent` as a plain `ITevent`; the names; a source generator later; and whether a taggregate refuses use
  after a throw.
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
