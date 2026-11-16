# What the notes require of Compze.Teventive

> **Status: findings, 2026-11-16.** Each requirement with the fact about JAStudio that causes it, and where
> Compze.Teventive 0.6.0-alpha stands on it. The conclusions are in [README.md](README.md).

## The consumer: the facts that set the requirements

| Fact about JAStudio | What it requires |
|---|---|
| The whole corpus is held in memory as live objects: 79,720 notes (49,162 sentences, 28,004 vocab, 2,554 kanji), and it is meant to grow | A taggregate must cost an instance little more than its own data |
| Every note is loaded at each start, from a file that holds its state | Loading from stored state, without the tevent store, and cheaply |
| Every change is written when it is made, with no Save button | The note's class must hear of a publish without an observer for each instance |
| What follows a change may change the same note, or other notes, on the publishing thread | Publishing and committing must stand a publish from inside them |
| The full rebuild publishes from many threads at once, each on notes of its own | What instances share must be safe for several threads, without a lock on the usual path |
| A background reparse may change a vocab while the UI edits it | A race on one taggregate should fail at its cause |
| The same class of part occurs several times in one note | Shared tomponents and their slots are on the hottest path |
| `JPNote` is a base class with state of its own, under three note classes | A base class's tevent appliers, with each subclass's tevents told apart |
| A note's tevents never leave the process | A tevent's type shouldn't declare a delivery it doesn't get |

## 1. A taggregate must cost an instance little more than its own data

**Today it costs about 1 GB for the corpus.** Measured, for each instance
([measurements-and-checks.md](measurements-and-checks.md)):

| | Kept |
|---|---|
| A Taggregate with 1 tevent applier and no parts | 1,111 B |
| A Taggregate with 4 tevent appliers and no parts | 1,632 B |
| Each component with 2 tevent appliers | about 1,000 B |
| Each shared tomponent, before it has seen a tevent | 552 B |
| Each shared tomponent, after its first tevent | 971 B |
| The first tevent of a new type published on an instance | 528 B more |
| The same data as plain objects, a root with four parts | 350 B |

What a note would be made of, counted from today's constructors, text values aside, which become properties with a
tevent each. Which part becomes a component and which a shared tomponent is my judgment:

| Note type | Notes | Components | Shared tomponents | For each note | For all |
|---|---|---|---|---|---|
| Vocab | 28,004 | about 12 | 10: six word sets, four media reference lists | about 23 KB | about 650 MB |
| Sentence | 49,162 | about 3 | 4 to 6: four media reference lists, two word exclusion sets | about 9.5 KB | about 465 MB |
| Kanji | 2,554 | about 5 | 6: three reading lists, two mutual relationships, a media reference list | about 12 KB | about 32 MB |

**About 1.1 GB, on a heap of 733 MB right after a start** (`dev_docs/memory-use.md`). Loading would also allocate about
as much again as garbage, and take about 2.5 seconds of processor time, by the same figures.

**Why:** how a tevent is routed depends only on the class of the teventive and the type of the tevent, and it is worked
out for every instance. Each taggregate creates two tevent dispatchers, each component and each shared tomponent one,
each with its lists, a registration and a closure for each tevent applier, and a dictionary of the handlers resolved
for each tevent type it has seen. A slot is one more object, with a closure registered on its owner.

**What is needed:** routing decided once for each teventive class. An instance then keeps a reference to its class's
declaration, a component its parent, a shared tomponent its owner and its slot.

**Why it matters this much here:** the corpus's memory was worked on in November 2026, and 300 MB went on a change
notifier with a lock in each text value, nine to a note. The same shape of cost, at three times the size, would come
back.

## 2. Loading from stored state, without the tevent store

**A note's stored state, read back, is a compacted-history:** its history rewritten to the fewest tevents that give
the state. A creation tevent that is every property update at once loads the root and its components, so that loading
and changing run the same tevent appliers.

- **It is a list of tevents, not one.** One tevent can't carry the state of two shared tomponents of one class: both
  adopting wrapper tevents close the same `IPublisherTevent<T>`, and a class has one `Tevent` property for it. A vocab
  loads from its creation tevent and a tevent adopted through each slot, about eleven.
- **It works today, through a member for internal use** (run): `ITaggregate.LoadFromHistory` applies such a list, when
  each tevent's version is set with `IMutableTaggregateTevent.SetTaggregateVersionInternal` and the adopting wrapper
  tevents are constructed by hand. Without the versions the taggregate stays at version 0 and refuses its next tevent:
  "The first published tevent … did not implement ITaggregateCreatedTevent".
- **What is needed:** a supported member that stamps id and versions, applies, publishes nothing and leaves nothing
  uncommitted; and a slot that adopts a tevent for loading, so that wrapping stays in one place.
- **Loading is JAStudio's start:** about 570,000 tevents. Today each adopted tevent allocates 1,220 bytes and takes
  1.2 microseconds to load. It should be measured in Compze beside construction.

## 3. Publishing and committing must stand a publish from inside them

**JAStudio's reactions publish from inside both.** Known cases: a vocab's rename renames the word-references to it in
other notes; a perfect synonym's answer is pushed to the other vocab of its web, whose tevent sets the first vocab's
answer again, to the same value; a change to a sentence's configuration reparses the sentence. And since every change
is written at once, the commit runs from inside the publish.

Both flaws were run against 0.6.0-alpha:

- **An observer of `ITaggregate.TeventStream` that publishes on the taggregate it observes.** The inner publish
  notifies the outer call's tevents again.
  - *If the observer publishes once:* it is notified of `first, first, second`, the tags are applied, and the outer
    publish throws `InvalidOperationException`, "Collection was modified", after the state has changed.
  - *If the observer publishes each time it is notified of the tevent it reacts to:* each inner publish notifies that
    tevent again, without end. The process exits with a stack overflow, code -1073741571, which nothing can catch.
    Reading the code had predicted only the first.
- **A tevent published from inside `ITaggregate.Commit`'s callback.** The callback was handed one tevent, the tevent
  published inside it was applied, and the next `Commit` was handed none: it was never committed.

**What is needed:** the notification as a queue, so that a publish from inside it is delivered after what was published
before it, in the order published; `Commit` taking its list out before it calls back; and a defined outcome when what
is notified throws, since the taggregate must not be left believing that it is still notifying.

## 4. Hearing of a publish without an observer for each instance

The only way to hear that a taggregate has published is an observer on that instance's tevent stream: 80,000 observers.
**What is needed:** a protected hook that `Publish` calls when its outermost call completes, which `JPNote` overrides
once to tell the collection. The collection then commits: it saves the note's file and dispatches each tevent, in its
wrapper, on its one `IMutableTeventDispatcher`.

- **Published is not committed:** by default JAStudio commits at once, and a command that makes several changes commits
  at its end.
- **The uncommitted tevents themselves are needed at the commit,** not only that there are some: whether the file is
  saved depends on what they are, and subscribers subscribe by their types.
- **The lists and the tevent stream of a taggregate can be created on first use.**

## 5. Several threads

- **What instances share must be safe for several threads.** The full rebuild publishes from many threads at once,
  each on notes of its own. A class's declaration and its resolved routes are read by all of them.
- **A race on one taggregate should fail at its cause.** The reentrancy level and the flag that tevents are being
  applied are plain fields. An assertion that one thread at a time is in `Publish` turns JAStudio's latent race into a
  failure where it happens. It is not thread affinity, and a read against a publish stays JAStudio's to prevent.

## 6. Shared tomponents throughout

**The same class of part several times in one note is the normal case,** so the notes would use shared tomponents in
all three note types, most of them in a component and not in the root: the word sets in a vocab's related vocab, the
media reference lists in each note's source data, the reading lists in a kanji's readings. From that:

- **A slot must cost an instance nothing for routing:** see 1.
- **A slot in a component, a shared tomponent that is subclassed, and one that is generic** all work today (run).
- **A subscription to an interface of the owner's that the slot's adopting wrapper tevent implements** gets the tevents
  adopted through that slot (run). It says the same of every tevent through the slot. JAStudio's subscribers would
  mostly subscribe so. The Compze session found no specification that pins it.
- **Each slot costs two declarations,** an adopting wrapper tevent interface and its class, besides the slot. That is
  what tells two shared tomponents of one class apart, and it stays.

## 7. A base class with tevent appliers

`JPNote` holds the note's tags and what Anki holds of its study-note, under the vocab, sentence and kanji notes. A base
class's tevent appliers run in a subclass, and a subscriber tells a tag added to a vocab from one added to a sentence by
the subclass's wrapper (run). **What is needed of the new routing:** a subclass's declaration starts from its base
class's, and a subclass's components are entries in the subclass's declaration.

## 8. What a tevent's type says about delivery

`ITaggregateTevent` is an `IExactlyOnceTevent`. Compze's own rule is that a tevent's type states its delivery contract
(`src/Compze.Tessaging/dev_docs/tevent-delivery-model.md` in Compze), and that a plain `ITevent` is what never leaves
the process. A note's tevents would declare a contract nothing honors. Nothing enforces it on an
`IMutableTeventDispatcher`, so it works today. **What is wanted:** `ITaggregateTevent` as a plain `ITevent`, with no
`TessageId` made for a tevent that needs none.

## What the notes don't require

- **Tevent handlers in components.** A part of a note follows another part in three places, each of which is one
  tevent that both parts apply, which works today (run). Where a part makes its note do work, a change to a sentence's
  configuration reparsing the sentence, a tevent handler on the root does it, and roots have them.
- **Entities.** Every list a note holds is of immutable values that its part adds, removes or replaces.
- **A declared order between a teventive's own tevent appliers and its children's.** Nothing in the notes depends on
  it. What they rely on is that every tevent applier in the tree runs before any tevent handler.
- **A tevent's version, time stamp or `TessageId`.** Nothing in JAStudio wants them.
- **A second base class beside `Taggregate`,** the tevent store, or the bus.

## What a plain property costs to declare

A text value in a note is one line today. As a tevent it is an interface, a class, an entry in the declaration, the
property and the method that publishes when the value differs. The notes have about 34 text values. That count, taken
when one note type is written, is what a source generator's worth should be judged by.
