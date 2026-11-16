# What the notes require of Compze

> **Status: agreed by the two JAStudio sessions, 2026-11-16.** One list, merged from the two teams' own. Each
> requirement with the fact about JAStudio behind it. The conclusions are in [README.md](README.md). How Compze would
> meet each is Compze's to design: see Compze's `dev_docs/taggregate-improvements/shared/`.

## JAStudio's use

- **The whole corpus is held in memory as live objects:** 79,720 notes (49,162 sentences, 28,004 vocab, 2,554 kanji),
  on a managed heap of about 733 MB right after a start. It is meant to grow.
- **Each note is stored as its state, in a JSON file,** never as a history. There is no tevent store, and won't be.
- **Every note is loaded at each start.**
- **Every change is written when it is made,** with no Save button. Edits are rare and one at a time, from the UI.
- **A full rebuild recomputes every note's generated data on many threads at once,** each thread on notes of its own.
- **`JPNote` is a base class with state of its own,** under `VocabNote`, `SentenceNote` and `KanjiNote`.
- **The same class of part occurs several times in a note, and across note types:** six word sets in a vocab, three
  reading lists and two mutual relationships in a kanji, two word exclusion sets in a sentence, and media reference
  lists in all three.
- **Several JAStudios run in one process** in the specifications.
- **Only Compze.Teventive would be used:** the Taggregate base classes, and one `IMutableTeventDispatcher` for the
  whole collection, to which every note's committed tevents go in their wrappers.

## The requirements

1. **A taggregate must cost an instance little more than its own data.** Tevent routing decided once for each
   teventive class: an instance keeps its state, its id, and a reference to its parent or its owner. Measured today:
   a root 1.1 to 1.6 KB, each component about 1.0 KB, each shared tomponent 0.55 to 0.97 KB.
2. **Shared tomponents and their slots cost as little as components, from the first note type.** Every note type
   needs them. Counted from today's constructors, a vocab is about 12 components and 10 shared tomponents, a sentence 3
   and 4 to 6, a kanji 5 and 6, most shared tomponents inside a component. A slot is a static of its owner's class, not
   an object for each instance.
3. **Loading from stored state, through the tevent appliers, without the tevent store.** A snapshot is a short list of
   tevents, not one: the root's, which also loads its components, and one adopted through each slot. A vocab loads
   from about eleven, and the corpus from about 570,000 at each start.
   - **The first tevent carries a snapshot marker of its own,** not the creation marker. A note is created in several
     ways, each a creation tevent that says what that creation set, and none of them is that it was loaded. And the
     snapshot tevent alone must set every property, which can then be checked of its type.
   - **Called from the note class's own factory:** protected, and typed on the class's own tevents.
   - **The slot adopts a tevent for loading as it does for publishing.**
   - **The whole snapshot is version 1.** JAStudio stores no version in a note's file.
4. **A note's class hears of a publish without an observer for each note.** A protected hook, called when a publish
   and everything the tevent handlers published in response is applied and the invariants hold. `JPNote` overrides it
   once, and reaches its collection through the note's own services: nothing static.
5. **Committing from inside that hook, and publishing from inside the commit.** JAStudio commits at once: it saves the
   note's file and dispatches each tevent on the collection's dispatcher, whose subscribers may change the same note or
   other notes. So:
   - **A note's tevents reach subscribers in the order published,** also when a subscriber publishes on that note
     meanwhile. A subscriber that keeps data derived from the tevents, as an index does, otherwise ends with an older
     value than the note holds.
   - **`Commit` hands over each tevent once,** and a tevent published from inside its callback waits for the next
     commit.
   - **What a throw from the hook, an applier or a handler leaves behind is defined.**
6. **What instances share is safe for several threads,** without a lock on the usual path, for the full rebuild.
7. **Two threads publishing on one taggregate fail at once,** where today they would corrupt it quietly. JAStudio has
   that race: a background reparse updates a vocab's generated data while the UI may edit the vocab, and the editors are
   disabled during a reparse by hand because of it.
8. **A base class's tevent appliers, and a subclass's parts typed on the subclass.** A subclass's declaration starts
   from its base class's. A component of `VocabNote` gets a `VocabNote` as its parent, not a `JPNote`.
9. **A tevent's type shouldn't declare a delivery it doesn't get.** `ITaggregateTevent` is an `IExactlyOnceTevent`
   today; a note's tevents never leave the process. Not a blocker: nothing enforces it in memory.

## What the notes don't require

- **Tevent handlers in components.** Where one part of a note follows another, it is one tevent that both apply,
  which works today. Reactions that reach other notes belong to the collection's subscribers, after the commit. Where
  a part makes its note do work, as a change to a sentence's configuration reparses the sentence, the root's tevent
  handler does it, in the same publish. Roots have tevent handlers today.
- **Entities.** Every list a note holds is of immutable values that its part adds, removes or replaces.
- **A tevent's version, time stamp or `TessageId`.**
- **A second base class beside `Taggregate`,** the tevent store, or the bus.
- **Anything of `IMutableTeventDispatcher`:** it serves as the collection's dispatcher as it is.

## What holds today, and should be pinned by specifications in Compze

Run against Compze.Teventive 0.6.0-alpha by the fable team (`../fable/measurements-and-checks.md`), and in part by
the opus team's checks in Compze:

- one tevent that two components apply, because its interface inherits an interface of each;
- a subscription to a meaning that several tevents share;
- a base class's tevent applier in a subclass, its tevent told apart by the subclass's wrapper;
- two shared tomponents of one class in a component, told apart by their slots, and a subclass of a shared tomponent;
- a subscription to an interface of the owner's that a slot's adopting wrapper tevent implements, which gets what is
  adopted through that slot;
- a subscription to a shared tomponent's tevents in every slot of every owner, by covariance, with nothing declared on
  the owners;
- a generic shared tomponent with generic tevents, and a generic tevent interface matched by covariance;
- a component typed on the subclass that declares it, through the top-level base classes.
