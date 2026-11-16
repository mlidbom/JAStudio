# Shared live instances: recommended for a while, and dropped

> **Dropped. Magnus chose the private copy for each unit of work (2026-11-16).** This is the record of the
> recommendation that the four sessions held between JAStudio's answers and his decision, kept to show how the
> decision was reached. It reads as it was written while shared live instances were recommended. What is current
> is in [answers-about-jastudio-for-the-taggregate-store.md](answers-about-jastudio-for-the-taggregate-store.md).

**Compze's design for the store is in its `dev_docs/taggregate-improvements/shared/the-taggregate-store.md`** (commit
4450f76db at the time). After these answers its two sessions recommended, as Magnus's decision to make, a different
isolation from the one he chose:

| | (a) A private copy for each unit of work, which Magnus chose | (b) Shared live instances, which Compze's sessions now recommend |
|---|---|---|
| The note object JAStudio's code holds | Replaced at every commit | The same object for good |
| The places that edit a note | Every one changes | Stay as written: a change joins the unit of work under way, or is one of its own |
| What other threads read meanwhile | Committed state only | Changes not yet committed, some of them later undone |
| A unit of work that fails | Its copies are discarded | Each note it changed is restored in place, from a snapshot taken at its first change |

A third way, (c), keeps the object and shows other threads committed state only: a note's state held as one
immutable value for each transaction. It is how every taggregate is written that changes, and it is not designed.

## What becomes of the answers

- **Question 1:** the handles, the indexes following a swap, and the cost of every place that edits a note all
  belong to (a). Under (b) none of them arises. What stays: what Anki says of a note's cards moves out of the note;
  values a note computes lazily and keeps must be safe to read from many threads; and a note must be restorable
  outside any store.
- **Question 3:** taken up as asked: the indexes as derived state that is itself transactional, written inside the
  unit of work and applied at its commit; lookups inside a unit of work that read its own writes, for which the
  session lists the notes it holds, by type; and what can't be undone, such as telling Anki, after the commit and
  before the edit returns (`IUnitOfWorkResolver.OnCommittedSuccessfully`). **Under (b) JAStudio's side now asks for
  something simpler for the indexes than what it asked for under (a): see point 7 below.**
- **Questions 2, 4 and 5:** unchanged, and taken up. The version is kept in memory only. No type identifiers are
  needed for tevents that stay in the process. A unit of work that gets a note, changes it and commits was measured
  by Compze at 8.5 to 12.5 µs and 9.5 KB.
- **Not designed yet in Compze:** loading everything again while running, and a wait for the observers to be at
  rest that needs no testing host.

## What JAStudio's side said to (b)

By `taggregates-fable`. `taggregates-opus` agrees with recommending (b) and with points 1 to 6, and added point 7.

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
7. **Under (b) the indexes should follow the live notes, not the commit** (`taggregates-opus`'s addition, which I
   take, with the point about timing added). Compze's document recommends derived state that is itself
   transactional: changed at the commit, with a unit of work reading its own changes. Under (a) that fits, since
   readers see committed notes only. Under (b) it gives every other thread two kinds of read at once: a note's
   uncommitted state, found through an index that still shows its committed keys. During a rename a lookup finds
   the vocab under its old question while the vocab itself says the new one, for as long as the unit of work lasts.
   That pairing is new: today a note is refiled at each change, so a reader's notes and indexes agree.
   - **The rule instead:** a note is refiled at each change, inside the unit of work, under the cache's lock, one
     change at a time, as `NoteGuard.Update` has it today. When a unit of work is rolled back, the store restores
     its notes in place and then tells the consumer which, and each is refiled from what it holds again.
   - **Lookups then need no reading of a unit of work's own writes,** inside one or out: the indexes show the live
     notes. The session needn't list what it holds, for this.
   - **No copies of index sets are held across a unit of work,** so two units of work can't lose each other's change.
   - **What readers see:** uncommitted notes with indexes that agree with them, and now and then keys that are
     later rolled back. (b) accepts that for the notes already. Today a failed edit is never undone at all.
   - **It can't be a participation handler,** since Compze's design dispatches those at the commit, which is too
     late for a lookup later in the same edit. It has to be told at each publish, once the cascade is settled, on the
     publishing thread. That is a call out of a publish, which the store's design removed because such calls
     published on the taggregate they were told of. This one only reads the note to compute its keys, so the rule
     can be asserted: publishing during it throws.
   - **It asks two things of Compze under (b):** that the consumer's derived state is told of each settled publish
     of a note the store holds, under that rule; and that it is told which notes a rollback restored in place,
     with the notes that were created in the unit of work and are gone again, and those deleted in it that are back.
     The second call must not fail, as restoring in place must not.
   - **For the transactional way under (b):** the indexes never show a key that is later rolled back, and no code of
     the consumer's runs at a rollback. Against it: a transactional map to build, lookups that overlay a unit of
     work's changes, and the two kinds of read above.

## What Compze made of the seven points

**All seven are in Compze's design as of its commit 9defe6c46,** agreed by the four sessions:

- **(b) keeps (c) open** is given as a reason for recommending it.
- **Restoring in place is the only restoring:** loading builds the taggregate empty and restores it in place, and
  restoring in place resets what the taggregate computed lazily and kept.
- **A unit of work must not wait for the UI thread while it holds a taggregate's lock.**
- **Under (b), derived state follows the live instances.** The session tells the consumer at the end of each settled
  cascade which taggregates changed, through a narrow notification that may change none, and after a rollback
  which instances it restored in place. JAStudio refiles under its own lock, as `NoteGuard.Update` does today.

What is still Magnus's to decide is listed in Compze's shared folder: first of all which isolation, (a) or (b).
